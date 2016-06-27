#include "Python.h"

#include <string>
#include <set>
#include <map>

extern std::set<std::string> encountered_modules;
extern std::map<std::string, std::string> module_build_ids;
extern int walk_core_dump(const char *path);

extern "C"
PyObject *read_core_dump(PyObject *self, PyObject *args)
{
	if (!PyString_Check(args))
	{
		PyErr_SetString(PyExc_TypeError, "read_core_dump requires a full path as only argument.");
		return NULL;
	}

	const char *path = PyString_AsString(args);
	int core_result = walk_core_dump(path);
	if (core_result != 0)
	{
		PyErr_SetString(PyExc_Exception, "Error walking core dump.");
		return NULL;
	}

	// Fill the build id list.
	PyObject *build_ids = PyList_New(0);
	for (std::map<std::string, std::string>::iterator itr = module_build_ids.begin(); itr != module_build_ids.end(); ++itr)
	{
		PyObject *tmp = Py_BuildValue("(ss)", itr->first.c_str(), itr->second.c_str());
		PyList_Append(build_ids, tmp);
		Py_DECREF(tmp);
	}

	// Fill the no id list.
	PyObject *no_id = PyList_New(0);
	for (std::set<std::string>::iterator itr = encountered_modules.begin(); itr != encountered_modules.end(); ++itr)
	{
		if (module_build_ids.find(*itr) == module_build_ids.end())
		{
			PyObject *tmp = Py_BuildValue("s", itr->c_str());
			PyList_Append(no_id, tmp);
			Py_DECREF(tmp);
		}
	}

	// return a tuple of these two
	PyObject *result = Py_BuildValue("(OO)", build_ids, no_id);
	Py_DECREF(build_ids);
	Py_DECREF(no_id);

	return result;
}


extern "C"
PyMethodDef module_methods[] =
{
	{ "read_core_dump", (PyCFunction)read_core_dump, METH_O, NULL },
	{ NULL, NULL, 0, NULL }
};

PyObject *s_module = NULL;

extern "C"
PyMODINIT_FUNC init_core_dump(void)
{
	s_module = Py_InitModule("_core_dump", module_methods);
}
