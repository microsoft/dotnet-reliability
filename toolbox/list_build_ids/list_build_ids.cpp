/* list_build_ids
 * Copyright (c) 2016 Microsoft Corportation
 * MIT License
 * 
 * This program opens a core dump and enumerates all the modules contained within it.  For every
 * module that contains a build-id, this program will print out the build id and module name.
 * Currently only works for 64 bit core dumps.
 */

#include <elf.h>
#include <cerrno>
#include <cassert>
#include <cstring>
#include <cstdio>
#include <set>
#include <map>
#include <string>

// elf.h defines ELF_NOTE_GNU but not a length for it
#define ELF_NOTE_GNU_LEN 4

// build ids are actually 20 bytes, but we will allow a slightly larger buffer in case this grows in the future
#define BUILD_ID_BYTE_MAX 64

// Reads an elf header structure from the given offset, validating that the magic header matches.
int read_elf_header(FILE *fp, uint64_t offset, Elf64_Ehdr *elf_header);

// Gets the file offset of a given virtual memory address.
long get_file_offset(const Elf64_Ehdr &elf_header, Elf64_Phdr *program_headers, long vmaddr);

// Walks all program headers in an elf file.
int walk_program_headers(FILE *file, const Elf64_Ehdr &elf_hdr, const char *filename = NULL, long nestedOffset = 0);

// Walks all notes in a memory range (specified by a program/section header).
int walk_notes(FILE *file, const Elf64_Ehdr &elf_hdr, Elf64_Phdr *program_headers, const char *filename, long notesBegin, long notesEnd);

// Enumerates the next note in the sequence.
int next_note(FILE *file, long offset, bool *is_build_id, bool *is_file_list, long *data_offset, long *data_length);

// Walks a file table in a given note.
int walk_file_table(FILE *file, const Elf64_Ehdr &elf_hdr, Elf64_Phdr *program_headers, char *file_table);

// Walks a core dump building global state in encountered_modules and module_build_ids.
int walk_core_dump(const char *filename);

// Prints a table of modules that do not have a build id
void print_table();

// Global variables to track what modules we've encountered and which have build ids
std::set<std::string> encountered_modules;
std::map<std::string, std::string> module_build_ids;

int main(int argc, char ** argv)
{
    if (argc != 2)
    {
        printf("usage: %s core_dump\n", argv[0]);
        return 1;
    }

    const char *filename = argv[1];
    int result = walk_core_dump(filename);
    if (result == 0)
        print_table();

    return result;
}


int walk_core_dump(const char *filename)
{
    encountered_modules.clear();
    module_build_ids.clear();

    FILE *file = fopen(filename, "rb");
    if (file == NULL)
    {
        int error = errno;
        printf("Error loading file '%s': %x\n", filename, error);
        return error;
    }

    Elf64_Ehdr elf_header;
    if (read_elf_header(file, 0, &elf_header))
    {
        printf("'%s' is not an elf binary.\n", filename);
        fclose(file);
        return 1;
    }

    if (elf_header.e_machine != EM_X86_64)
    {
        printf("Error loading '%s': currently only x86_x64 is supported.\n", filename);
        fclose(file);
        return 1;
    }

    int returnCode = 0;
    if (elf_header.e_type == ET_EXEC || elf_header.e_type == ET_CORE)
    {
        walk_program_headers(file, elf_header);
    }
    else if (elf_header.e_type == ET_EXEC || elf_header.e_type == ET_DYN)
    {
        walk_program_headers(file, elf_header, filename);
    }
    else
    {
        printf("Unknown ELF file '%s'.\n", filename);
        returnCode = -1;
    }

    fclose(file);
    return returnCode;
}


inline int note_align(int value)
{
    return (value + 3) & ~3;
}

inline unsigned char get_hex(unsigned char c)
{
    assert(c >= 0 && c <= 0xf);

    if (c <= 9)
        return c + '0';

    return c - 10 + 'a';
}

int read_elf_header(FILE *fp, uint64_t offset, Elf64_Ehdr *elf_header)
{
    if (fseek(fp, offset, SEEK_SET) || fread(elf_header, sizeof(Elf64_Ehdr), 1, fp) != 1)
        return 1;

    if (elf_header->e_ident[EI_MAG0] != ELFMAG0 ||
        elf_header->e_ident[EI_MAG1] != ELFMAG1 ||
        elf_header->e_ident[EI_MAG2] != ELFMAG2 ||
        elf_header->e_ident[EI_MAG3] != ELFMAG3)
        return 1;

    return 0;
}

long get_file_offset(const Elf64_Ehdr &elf_header, Elf64_Phdr *program_headers, long vmaddr)
{
    // maps a virtual address back to a file offset within the core dump
    for (Elf64_Phdr *curr = program_headers; curr < program_headers + elf_header.e_phnum; curr++)
    {
        if (curr->p_type != PT_LOAD)
            continue;

        if (vmaddr >= (curr->p_vaddr & ~curr->p_align) && vmaddr <= curr->p_vaddr + curr->p_filesz)
            return vmaddr - curr->p_vaddr + curr->p_offset;
    }

    return 0;
}

int walk_program_headers(FILE *file, const Elf64_Ehdr &elf_hdr, const char *filename, long nested_offset)
{
    // Walks the p-headers of either the core dump itself or of a nested module in the core dump
    // filename is null when we are walking the core dump itself, and nestedOffset == 0
    // filename is the path of a loaded module when we are walking a module within the core dump, and nestedOffset != 0

    assert(elf_hdr.e_phentsize == sizeof(Elf64_Phdr));
    Elf64_Phdr *program_hdrs = new Elf64_Phdr[elf_hdr.e_phnum];

    if (fseek(file, elf_hdr.e_phoff + nested_offset, SEEK_SET) || fread(program_hdrs, sizeof(Elf64_Phdr), elf_hdr.e_phnum, file) != elf_hdr.e_phnum)
    {
        if (filename == NULL)
            fprintf(stderr, "Failed to walk program headers in core dump.\n");
        else
            fprintf(stderr, "Failed to read core dump headers for inner file '%s', skipping...\n", filename);

        delete[] program_hdrs;
        return 1;
    }

    for (int i = 0; i < elf_hdr.e_phnum; i++)
    {
        // We care about file lists and build ids, both of which are contained in NOTE sections.
        if (program_hdrs[i].p_type == PT_NOTE)
        {
            long begin = program_hdrs[i].p_offset + nested_offset;
            long end = program_hdrs[i].p_offset + program_hdrs[i].p_filesz + nested_offset;

            walk_notes(file, elf_hdr, program_hdrs, filename, begin, end);
        }
    }

    delete[] program_hdrs;
    return 0;
}

int walk_file_table(FILE *file, const Elf64_Ehdr &elf_hdr, Elf64_Phdr *program_headers, char *file_table)
{
    // A file table is laid out as a counted list of three pointers: VM Address Start, VM Address End, Page Offset.
    // The corresponding string table for file name is a list of null terminated strings at the end of that table.

    size_t offs = 0;

    // table entry count
    size_t count = *(size_t*)(file_table + offs);
    offs += sizeof(size_t);

    // Page size
    offs += sizeof(size_t);

    size_t filename_offset = offs + count * 3 * sizeof(size_t);
    for (size_t i = 0; i < count && filename_offset; i++)
    {
        // vmrange start
        size_t start = *(size_t*)(file_table + offs);
        offs += sizeof(size_t);

        // vmrange stop
        offs += sizeof(size_t);

        // page offset
        offs += sizeof(size_t);

        // get the corresponding file path in the string table
        char *full_path = file_table + filename_offset;
        filename_offset += strlen(full_path) + 1;

        // Add the file to the list of modules we've seen
        encountered_modules.insert(full_path);

        // Check to see if the mapped file section is an elf header (that is, the beginning of the image)
        Elf64_Ehdr inner_header;
        long image_offset = get_file_offset(elf_hdr, program_headers, (long)start);
        if (image_offset != 0 && read_elf_header(file, image_offset, &inner_header) == 0)
            walk_program_headers(file, inner_header, full_path, image_offset);
    }

    return 0;
}


int walk_notes(FILE *file, const Elf64_Ehdr &elf_hdr, Elf64_Phdr *program_headers, const char *filename, long notesBegin, long notesEnd)
{
    // This function could be walking either the core dump itself or a module loaded as a section in the core dump.
    // If filename is null then we are walking the core dump itself, and nestedOffset == 0.
    // If filename is the path of a loaded module then we are walking a module within the core dump, and nestedOffset != 0.

    // When walking the core dump itself (filename == NULL) we expect to find a file list somewhere in the notes.
    // When walking a loaded module (filename != NULL), we may find a build id.
    // We do NOT expect to find a build ID in the core dump itself, and we do NOT expect to find a file list within a loaded module.

    long offset = notesBegin;
    long data_offset = 0, data_len;
    bool is_build_id, is_file_list;
    while (offset < notesEnd && (offset = next_note(file, offset, &is_build_id, &is_file_list, &data_offset, &data_len)) != 0)
    {
        if (is_build_id && filename)
        {
            // '&& filename' ensures we are inspected a loaded module and not a floating build id in the core dump itself.
            assert(!is_file_list);
            assert(data_len > 0 && data_len < BUILD_ID_BYTE_MAX);

            if (data_len <= BUILD_ID_BYTE_MAX)
            {
                unsigned char build_id[BUILD_ID_BYTE_MAX];
                char hex_build_id[BUILD_ID_BYTE_MAX * 2 + 1];
                if (fseek(file, data_offset, SEEK_SET) == 0 && fread(build_id, 1, data_len, file) == data_len)
                {
                    for (int i = 0; i < data_len; i++)
                    {
                        hex_build_id[i * 2] = get_hex(build_id[i] >> 4);
                        hex_build_id[i * 2 + 1] = get_hex(build_id[i] & 0xf);
                        hex_build_id[i * 2 + 2] = 0;
                    }

                    module_build_ids[filename] = hex_build_id;
                }

                // We are enumerating an embedded module in the core.  Once we find the build id we can stop looking.
                return 0;
            }
        }

        if (is_file_list && !filename)
        {
            // '&& !filename' ensures we are only inspecting a file list within the core dump itself and not an
            // NT_FILE that happens to be within a loaded module.
            assert(!is_build_id);
            assert(data_len > 0);

            char *file_table = new char[data_len];
            if (fseek(file, data_offset, SEEK_SET) == 0 && fread(file_table, 1, data_len, file) == data_len)
                walk_file_table(file, elf_hdr, program_headers, file_table);

            delete[] file_table;
        }
    }

    return 0;
}

int next_note(FILE *file, long offset, bool *is_build_id, bool *is_file_list, long *data_offset, long *data_length)
{
    assert(!data_offset == !data_length);

    if (offset <= 0)
        return 0;

    // Read note header
    Elf64_Nhdr header;
    if (fseek(file, offset, SEEK_SET) || fread(&header, sizeof(Elf64_Nhdr), 1, file) != 1)
        return 0;

    offset += sizeof(header);

    // build ids are NT_PRPSINFO sections with a name of "GNU".
    if (is_build_id)
    {
        char name[ELF_NOTE_GNU_LEN];
        if (header.n_type == NT_PRPSINFO && header.n_namesz == ELF_NOTE_GNU_LEN)
            *is_build_id = fread(name, sizeof(char), ELF_NOTE_GNU_LEN, file) == ELF_NOTE_GNU_LEN && memcmp(name, ELF_NOTE_GNU, ELF_NOTE_GNU_LEN) == 0;
        else
            *is_build_id = false;
    }

    // file lists are a section of type NT_FILE
    if (is_file_list)
        *is_file_list = header.n_type == NT_FILE;

    // Move past name
    offset += note_align(header.n_namesz);
    if (data_offset && data_length)
    {
        *data_offset = offset;
        *data_length = header.n_descsz;
    }

    // Move past data
    offset += note_align(header.n_descsz);
    return offset;
}

void print_table()
{
    for (std::map<std::string, std::string>::iterator itr = module_build_ids.begin(); itr != module_build_ids.end(); ++itr)
        printf("%s %s\n", itr->second.c_str(), itr->first.c_str());

    printf("\n");

    bool first = true;
    for (std::set<std::string>::iterator itr = encountered_modules.begin(); itr != encountered_modules.end(); ++itr)
    {
        if (module_build_ids.find(*itr) == module_build_ids.end())
        {
            if (itr->find("/dev/") != 0)
            {
                if (first)
                {
                    printf("\nModules without build ids:\n");
                    first = false;
                }

                printf("    %s\n", itr->c_str());
            }
        }
    }
}
