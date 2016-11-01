# sudo apt-get install python-dev
# python setup.py build_ext --inplace

from distutils.core import setup, Extension

setup(ext_modules=[Extension("_core_dump", ["python_binding.cpp", "list_build_ids.cpp"])])
