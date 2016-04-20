#!/usr/bin/python
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

#img_url_tag = 'image_url'

#named_args = {}
#staging_path = None

logging.basicConfig(level=logging.DEBUG)

#EXEC = "mono RootNavLinux.exe"
EXEC = "./runRootNav.sh"

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
        #results = fetch_image_planes(self.bq, self.resource_url, '.')

        # extract gobject inputs
        #tips = self.bq.mex.find('inputs', 'tag').find('image_url', 'tag').find('tips', 'gobject')
        #with open('inputtips.csv', 'w') as TIPS:
        #    for point in tips.gobjects:
        #        print >>TIPS, "%(y)s, %(x)s" % dict(x=point.vertices[0].x,y=point.vertices[0].y)
	return;


    def start(self):
        self.bq.update_mex('executing')
        # Matlab requires trailing slash
        logging.debug(['/home/tuan/bisque/modules/RootNavLinuxModuleV2/', EXEC])
        #r = subprocess.call(['/home/tuan/bisque/modules/RootNavLinuxModuleV2/', EXEC])
        r = subprocess.call(['/home/tuan/bisque/modules/RootNavLinuxModuleV2/./runRootNav.sh'])
        
        return r;

    def teardown(self):
        # Post all submex for files and return xml list of results
        #gobjects = self._read_results()
        #tags = [{ 'name': 'outputs',
        #          'tag' : [{'name': 'rootimage', 'type':'image', 'value':self.resource_url,
        #                    'gobject' : [{ 'name': 'root_tips', 'type': 'root_tips', 'gobject' : gobjects }] }]
        #          }]
        #self.bq.finish_mex(tags = tags)
        self.bq.finish_mex('Finished')
        return;
   
    def run(self):
        try:
	  
	  # use regular expressions in order to get the base name
	  # of the file executing this cide and use it as the log file name
	  self_name = re.match(r'(.*)\.py$', sys.argv[0]).group(1)

	  # start some logging (DEBUG is verbose, WARNING is not)
	  log_fn = self_name + '.log'
	  logging.basicConfig(filename=log_fn , level=logging.WARNING)

	  #logging . basicConfig ( filename=log fn , level=logging .DEBUG)
	  logging.debug('Script invocation: ' + str(sys.argv))
	  
	  parser  = optparse.OptionParser()
	  parser.add_option('-d','--debug', action="store_true")
	  parser.add_option('-n','--dryrun', action="store_true")
	  parser.add_option('--credentials')
	  #parser.add_option('--image_url')

	  (options, args) = parser.parse_args()
	  named = AttrDict (bisque_token=None, mex_url=None, staging_path=None)
	  for arg in list(args):
	      tag, sep, val = arg.partition('=')
	      logging.debug('args , tag=' + str(tag) + ' and sep ' + str(sep) + ' and value: ' + str(val))
	      
	      if sep == '=':
		  named[tag] = val
		  args.remove(arg)
	  
	  self.bq = BQSession().init_mex(args[0], args[1])
	  #if named.bisque_token:
	  #  self.bq = BQSession().init_mex(named.mex_url, named.bisque_token)
	  #  self.resource_url =  named.image_url
	  #elif options.credentials:
	  #  user,pwd = options.credentials.split(':')
	  #  self.bq = BQSession().init_local(user,pwd)
	  #  self.resource_url =  options.image_url
	  #else:
	  #  parser.error('need bisque_token or user credential')

	  #if self.resource_url is None:
	  #  parser.error('Need a resource_url')
      
	  #if not args :
	  commands = ['setup', 'start', 'teardown']
	  #else:
	  #  commands = [ args ]
            
	  for command in commands:
	    command = getattr(self, str(command))
            r = command()
            
        except Exception, e:
            #logging.exception ("problem during %s" % command)
            logging.exception ("problem during %s" % e)
            #self.bq.fail_mex(msg = "Exception during %s: %s" % (command,  e))
            #bqsession.fail_mex(msg = "Exception during %s: %s" % (command,  e))
            #bqsession.fail_mex(msg = "Exception during %s: " % ( e))
            self.bq.fail_mex(msg = "Exception during %s: " % ( e))
            sys.exit(1)

        sys.exit(r)


if __name__ == "__main__":
    RootNavLinux().run()

