#!/usr/bin/env python
import os
import sys
import optparse
import subprocess
import glob
import csv
import pickle
import logging
import itertools

from bqapi import BQSession
from bqapi.util import fetch_image_planes, AttrDict
from lxml.builder import E


logging.basicConfig(level=logging.DEBUG)

EXEC = "mono RootNavLinux.exe"

def gettag (el, tagname):
    for kid in el:
        if kid.get ('name') == tagname:
            return kid, kid.get('value')
    return None,None

class RootNavLinux(object):

    def setup(self):
        #if not os.path.exists(self.images):
        #    os.makedirs(self.images)

        self.bq.update_mex('initializing')
        results = fetch_image_planes(self.bq, self.resource_url, '.')

        # extract gobject inputs
        #tips = self.bq.mex.find('inputs', 'tag').find('image_url', 'tag').find('tips', 'gobject')
        #with open('inputtips.csv', 'w') as TIPS:
        #    for point in tips.gobjects:
        #        print >>TIPS, "%(y)s, %(x)s" % dict(x=point.vertices[0].x,y=point.vertices[0].y)



    def start(self):
        self.bq.update_mex('executing')
        # Matlab requires trailing slash
        subprocess.call([EXEC])


   

    def run(self):
        #parser  = optparse.OptionParser()
        #parser.add_option('-d','--debug', action="store_true")
        #parser.add_option('-n','--dryrun', action="store_true")
        #parser.add_option('--credentials')
        #parser.add_option('--image_url')

        #(options, args) = parser.parse_args()
        #named = AttrDict (bisque_token=None, mex_url=None, staging_path=None)
        #for arg in list(args):
        #    tag, sep, val = arg.partition('=')
        #    if sep == '=':
        #        named[tag] = val
        #        args.remove(arg)
        #self.named_args = named
        #self.staging_path = named.get('staging_path')

        #if named.bisque_token:
        #    self.bq = BQSession().init_mex(named.mex_url, named.bisque_token)
        #    self.resource_url =  named.image_url
        #elif options.credentials:
        #    user,pwd = options.credentials.split(':')
        #    self.bq = BQSession().init_local(user,pwd)
        #    self.resource_url =  options.image_url
        #else:
        #    parser.error('need bisque_token or user credential')

        #if self.resource_url is None:
        #    parser.error('Need a resource_url')

        #if not args :
        #    commands = ['setup', 'start', 'teardown']
        #else:
        #    commands = [ args ]

        try:
        #    for command in commands:
        #        command = getattr(self, command)
        #        r = command()
        
        # use regular expressions in order to get the base name
        # of the file executing this cide and use it as the log file name
            self name = re.match(r'(.*)\.py$', sys.argv[0]).group(1)
        
            # start some logging (DEBUG is verbose, WARNING is not)
            log fn = self name + '.log'
            logging.basicConfig(filename=log fn , level=logging.WARNING)
            
            #logging . basicConfig ( filename=log fn , level=logging .DEBUG)
            logging.debug('Script invocation: ' + str(sys.argv))
            
            # read key=value pairs from the command line
            for arg in sys.argv[1:]:
                tag, sep, val = arg.partition('=')
                if sep != '=':
                    error msg = 'malformed argument ' + arg
                    logging.error(error msg)
                    raise Exception(error msg)
                named args[tag] = val
                logging.debug('parsed a named arg ' + str(tag) + '=' + str(val))

            # Three mandatory key=value pairs on command line
            murl = 'mex url'
            btoken = 'bisque_token'
            
            for required arg in [btoken, murl, img url tag]:
                if required arg not in named args:
                    error msg = 'missing mandatory argument ' + required_arg
                    logging.error(error msg)
                    raise Exception(error msg)
                    
            # Record staging path (maybe on cmd line)
            stage tag = 'staging_path'
            if stage tag in named args:
                staging path = named args[stage tag]
                del named args[stage tag] # it ’s saved, so delete it from named args
             else:
                staging path = os.getcwd() # get current working directory
                
            # establish the connection to the Bisque session
            logging.debug('init bqsession , mex url=' + str(named args[murl]) + ' and auth token=' + str(named args[btoken]))
            
            # Starting a Bisque session
            bqsession = bq.api.BQSession().init mex(named args[murl], named args[btoken])
            
            del named args[murl] # no longer needed
            del named args[btoken] # no longer needed
            
             self.bq.update_mex('executing')
             subprocess.call([EXEC])
            
            
        except Exception, e:
            logging.exception ("problem during %s" % command)
            self.bq.fail_mex(msg = "Exception during %s: %s" % (command,  e))
            sys.exit(1)

        sys.exit(0)




if __name__ == "__main__":
    RootNavLinux().run()

