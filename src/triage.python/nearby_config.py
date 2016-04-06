# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.

from dumpling_util import Logging

## A NearbyConfig is just a json config that is serialized in to a python object. The 'Nearby' comes from the assumption that the configuration file
## is sitting exactly next to this script! 
##
## Keys in the json are created as public fields of the class.
## for example, the following json:
## { "alpha" : "omega" } 

## import nearby_config
## nearby_config.named('foo.json')

## print nearby_config.values.alpha 
## should print 'omega'

class NearbyConfig(object):
    def __init__(self, jsonText):
        self.__dict__ = json.loads(jsonText)

# global variables
nearby_folder      = path.dirname(path.realpath(__file__))
nearby_fullpath    = None

# factory method
# The weird name is to hit the 'scenario' 

# I wanted to be able to instantiate so that it reads like:
# analysis_worker_config = nearby_config.named('analysis_worker_config.json')
def named(name):
    _configPath = path.join(nearby_folder, name)

    if not path.exists(_configPath):
        Logging.Failure('expected a file "%s" to exist.' % _configPath)

    with open(str(nearby_fullpath), 'r') as configFile:
        data = configFile.read().replace('\n', '')

    return NearbyConfig(data) # initialize the global config path
    
        

    