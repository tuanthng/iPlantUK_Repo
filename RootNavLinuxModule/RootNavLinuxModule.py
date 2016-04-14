#!/usr/bin/env python
import os
import sys
import optparse
import subprocess
import glob
import csv
import pickle
import logging
import re
import itertools
#mport gobject

#import BQSession
from os import sys, path

sys.path.append(path.dirname(path.dirname(path.abspath(__file__))))

sys.path.append('/home/tuan/bisque/bqapi/')

from bqapi.comm import BQSession

from bqapi.util import fetch_image_planes, AttrDict
#from lxml.builder import E

img_url_tag = 'image_url'

named_args = {}
staging_path = None

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

    #def teardown(self):
        # Post all submex for files and return xml list of results
        #gobjects = self._read_results()
        #tags = [{ 'name': 'outputs',
        #          'tag' : [{'name': 'rootimage', 'type':'image', 'value':self.resource_url,
        #                    'gobject' : [{ 'name': 'root_tips', 'type': 'root_tips', 'gobject' : gobjects }] }]
        #          }]
        #self.bq.finish_mex(tags = tags)
   
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
            self_name = re.match(r'(.*)\.py$', sys.argv[0]).group(1)

            # start some logging (DEBUG is verbose, WARNING is not)
            log_fn = self_name + '.log'
            logging.basicConfig(filename=log_fn , level=logging.WARNING)

            #logging . basicConfig ( filename=log fn , level=logging .DEBUG)
            logging.debug('Script invocation: ' + str(sys.argv))

            # read key=value pairs from the command line
            for arg in sys.argv[1:]:
                tag, sep, val = arg.partition('=')

                if sep != '=':
                    error_msg = 'malformed argument ' + arg
                    logging.error(error_msg)
                    #raise Exception(error_msg)
                else:
                    tag = tag.replace('--', ''); #remove the '--', this string by convention is the way to pass values via argument in linux/unix
                    named_args[tag] = val
                    logging.debug('parsed a named arg ' + str(tag) + '=' + str(val))

            # Three mandatory key=value pairs on command line
            murl = 'mex_url'
            btoken = 'bisque_token'
            auth_token = 'auth_token'
            
            #for required_arg in [btoken, murl, img_url_tag]:
            #    if required_arg not in named_args:
            #        error_msg = 'missing mandatory argument ' + required_arg
            #        logging.error(error msg)
            #        raise Exception(error msg)

            # Record staging path (maybe on cmd line)
            stage_tag = 'staging_path'

            if stage_tag in named_args:
                staging_path = named_args[stage_tag]
                del named_args[stage_tag] # it's saved, so delete it from named args.
            else:
                staging_path = os.getcwd() # get current working directory

            # establish the connection to the Bisque session
            #logging.debug('init bqsession , mex_url=' + str(named_args[murl]) + ' and auth token=' + str(named_args[btoken]))
            logging.debug('init bqsession , mex_url=' + str(named_args[murl]) + ' and auth token=' + str(named_args[auth_token]))

            # Starting a Bisque session
            #bqsession = bq.api.BQSession().init mex(named_args[murl], named_argsbtoken])
            #bqsession = bq.api.BQSession().init_mex(named_args[murl], named_args[btoken])
            self.bq = BQSession().init_mex(named_args[murl], named_args[auth_token])
            #bqsession = BQSession().init_mex(named_args[murl], named_args[auth_token])
            
            del named_args[murl] # no longer needed
            #del named_args[btoken] # no longer needed
            del named_args[auth_token] # no longer needed

            self.bq.update_mex('executing')
            #bqsession.update_mex('executing')
            subprocess.call([EXEC])

        except Exception, e:
            #logging.exception ("problem during %s" % command)
            logging.exception ("problem during %s" % e)
            #self.bq.fail_mex(msg = "Exception during %s: %s" % (command,  e))
            #bqsession.fail_mex(msg = "Exception during %s: %s" % (command,  e))
            #bqsession.fail_mex(msg = "Exception during %s: " % ( e))
            self.bq.fail_mex(msg = "Exception during %s: " % ( e))
            sys.exit(1)

        sys.exit(0)



if __name__ == "__main__":
    RootNavLinux().run()

