# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.

import json
from os import path

## A NearbyConfig is just a json config that is serialized in to a python object. The 'Nearby' comes from the assumption that the configuration file
## is sitting exactly next to this script! 
##
## Keys in the json are created as public fields of the class.
## for example, the following json:
## { "alpha" : "omega" } 

## import nearby_config
## nearby_config.named('foo.json')

## print nearby_config.alpha 
## should print 'omega'

class NearbyConfig(object):
    def __init__(self, jsonText):
        self.__dict__ = json.loads(jsonText)

# global variables that may be useful?
folder      = path.dirname(path.realpath(__file__))
fullpath    = None # this gets filled in by the function 'named'

# factory method
# The weird name is to hit the 'scenario' 

# I wanted to be able to instantiate so that it reads like:
# analysis_worker_config = nearby_config.named('analysis_worker_config.json')
def named(configfilename):
    fullpath = path.join(folder, configfilename)

    if not path.exists(fullpath):
        print('expecting config at %s, but did not find it.' % str(fullpath))

    with open(str(fullpath), 'r') as configFile:
        data = configFile.read().replace('\n', '')

    return NearbyConfig(data) # initialize the global config path
    
        

    