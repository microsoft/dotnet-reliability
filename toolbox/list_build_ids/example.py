from sys import argv
from _core_dump import read_core_dump

build_ids, modules_missing_build_ids = read_core_dump(argv[1])

print("Modules:")
for module, id in build_ids:
    print("    %s %s", id, module)

print()
print("Modules without build ids:")
for module in modules_missing_build_ids:
    print("    " + module);
