# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.


## Simple Logging class
## This class should be regarded as static. 
## There are three logging methods, 'Verbose(...)', 'Informative(...)', and 'Failure(...)'. 
## Depending on the value of Logging.OutStream (default is 'stdout') the output locations of these
## three may be variable.

## TODO: Implement 'cloud logging'
 
class Logging:
    # used for print debugging
    # these behave similarly to static, btw
    CONFIG_VERBOSE_OUTPUT = True
    CONFIG_INFORMATIVE_OUTPUT = True
    
    OutStream = 'stdout'

    @staticmethod
    def Verbose(value):
        if CONFIG_VERBOSE_OUTPUT:
            print(str(datetime.now()) + ' VERBOSE: ' + value)
    @staticmethod
    def Informative(value):
        if CONFIG_INFORMATIVE_OUTPUT:
            print(str(datetime.now()) + ' INFORMATIVE: ' + value)

    @staticmethod
    def Failure(value, error_code = 1, exitPython = True):
        print(str(datetime.now()) + ' FAILURE: ' + value)
        if exitPython:
            exit(error_code)

## This is meant to be private to the module, do not touch.
_paths_dictionary = None

## 'paths' must be a dictionary. Key is a name for the corresponding path, usually something short and familiar to readers of the code.
## the Values are the FULL ABSOLUTE PATHS for the item being named. Items will need to exist on disk. 
def CheckTheseNamedPaths(paths):
    _paths_dictionary = paths
    # Verify that the paths we DEPEND on for normal operations are in existence.
    # These files MUST exist at the start of the script, AND MUST be absolute paths.
    _path_check_succeeded = True;
    
    for key, value in paths.iteritems():
        status = str(key) + ' (' + str(value) + ') ' + ' EXISTS: ' + str(path.exists(value)) + ' IS_ABSOLUTE: ' + str(path.isabs(value))
        if path.exists(value) and path.isabs(value):
            Logging.Informative(status)
            continue
        else:
            _path_check_succeeded = False
            Logging.Failure(status, 4)    
        
    if not _path_check_succeeded:
        Logging.Failure('configured paths need to exist, and must be absolute.', 8)

## Shorthands the common trend of checking if a key exists and handling failures when they do not exist.
def pathof(key):
    if _paths_dictionary.has_key(key):
        return str(_paths_dictionary[key])
    else:
        Logging.Failure('the key "%s" does not exist as a dependable path. - exiting.' % key, 2)
