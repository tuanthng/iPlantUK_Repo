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

from lxml import etree

sys.path.append(path.dirname(path.dirname(path.abspath(__file__))))

sys.path.append('/home/tuan/bisque/bqapi/')

from bqapi.comm import BQSession

from bqapi.util import fetch_image_planes, AttrDict, fetch_image_pixels
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

    def mex_parameter_parser(self, options, mex_xml):
        """
        Parses input of the xml and add it to RootNav's options attribute
            
        @param: mex_xml
        """
        mex_inputs = mex_xml.xpath('tag[@name="inputs"]')
        
        if mex_inputs:
            for tag in mex_inputs[0]:
                if tag.tag == 'tag' and tag.attrib['type'] != 'system-input':
                    logging.debug('Set options with %s as %s'%(tag.attrib['name'],tag.attrib['value']))
                    setattr(options,tag.attrib['name'],tag.attrib['value'])
        else:
            logging.debug('No Inputs Found on MEX!')
        
        logging.debug('mex_parameter_parser/ options: ' + str(options))
        
        
    def setup(self):
        #if not os.path.exists(self.images):
        #    os.makedirs(self.images)

        self.bq.update_mex('initializing')
        #results = fetch_image_planes(self.bq, self.resource_url, '.')
	
        #self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
	
        #logging.debug('setup/ final options: ' + str(self.options))
        
        # extract gobject inputs
        #tips = self.bq.mex.find('inputs', 'tag').find('image_url', 'tag').find('tips', 'gobject')
        #with open('inputtips.csv', 'w') as TIPS:
        #    for point in tips.gobjects:
        #        print >>TIPS, "%(y)s, %(x)s" % dict(x=point.vertices[0].x,y=point.vertices[0].y)
	return;


    def start(self):
        self.bq.update_mex('executing')
        # Matlab requires trailing slash
        #build parameters for the tool
        # -ImageFile="0002.jpg" -PresetName="Custom" -InitialClassCount=3 -MaximumClassCount=4 -ExpectedRootClassCount=2 -PatchSize=150 -BackgroundPercentage=0.5 -BackgroundExcessSigma=1.5 -Weights="0.35,0.68,0.99"
        
        self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
        
        logging.debug('start/ final options: ' + str(self.options))
        
        
        #image_xml = self.bq.fetchxml(self.options.image_url)
        #logging.debug('image_xml: ' + str(image_xml))
        
        #image_url = self.bq.service_url('image_service',path=image_xml.attrib['resource_uniq'])
        #logging.debug('image_url: ' + str(image_url))
        
        #results = fetch_image_planes(self.bq, self.options.image_url, self.options.stagingPath)
        results = fetch_image_pixels (self.bq, self.options.image_url, self.options.stagingPath)
#         image = self.bq.load(self.options.image_url)
#         pixels = image.pixels() #.fetch()
#         dest = os.path.join(self.options.stagingPath, image.name)
#         f = open(dest, 'wb')
#         f.write(pixels)
#         f.close()
        logging.debug('results fetching image: ' + str(results))
        #logging.debug('results fetching image: ' + str(image))
        #logging.debug('image name: ' + str(image.name))
        
        parasRootNav = ' -ImageFile="' +  results[self.options.image_url] + '"' + \
            ' -PresetName="' + self.options.PresetName + '"' + \
			' -InitialClassCount=' + self.options.InitialClassCount + \
			' -MaximumClassCount=' + self.options.MaximumClassCount + \
			' -ExpectedRootClassCount=' + self.options.ExpectedRootClassCount + \
			' -PatchSize=' + self.options.PatchSize + \
			' -BackgroundPercentage=' + self.options.BackgroundPercentage + \
			' -BackgroundExcessSigma=' + self.options.BackgroundExcessSigma + \
			' -Weights="' + self.options.Weights + '"'
         
        #parasRootNav = str(parasRootNav)
         
        logging.debug('parasRootNav: ' + parasRootNav)
         
        fullPath = os.path.join(self.options.stagingPath, EXEC)
        logging.debug('fullPath: ' + fullPath)
         
        #fullExec = fullPath + ' ' + parasRootNav
        #logging.debug('Execute: ' + fullExec)
         
        #r = subprocess.call(['/home/tuan/bisque/modules/RootNavLinuxModuleV2/', EXEC])
        r = subprocess.call([fullPath, parasRootNav])
         
        return r;

    def teardown(self):
        self.bq.update_mex('Collecting result...')
        # Post all submex for files and return xml list of results
        #gobjects = self._read_results()
        #tags = [{ 'name': 'outputs',
        #          'tag' : [{'name': 'rootimage', 'type':'image', 'value':self.resource_url,
        #                    'gobject' : [{ 'name': 'root_tips', 'type': 'root_tips', 'gobject' : gobjects }] }]
        #          }]
        #self.bq.finish_mex(tags = tags)
        
#         mex_outputs = self.bq.mex.xmltree.xpath('tag[@name="outputs"]')
#         logging.debug('Outputs mex:' + str(mex_outputs))
#         if mex_outputs:
#             logging.debug('Outputs mex:' + str(mex_outputs))
#             
#             for tag in mex_outputs[0]:
#                 if tag.tag == 'tag' and tag.attrib['name'] == 'TipDetection':
#                     tag.attrib['value'] = "390"
# #                 if tag.tag == 'tag' and tag.attrib['type'] != 'system-input':
# #                     logging.debug('Set options with %s as %s'%(tag.attrib['name'],tag.attrib['value']))
#                     setattr(options,tag.attrib['name'],tag.attrib['value'])
#         else:
#             logging.debug('No Outputs Found on MEX!')
        
        outputTag = etree.Element('tag', name='outputs')
        outputSubTag = etree.SubElement(outputTag, 'tag', name='summary')
        
        #etree.SubElement(outputTag, 'tag', name='TipDetection', value=str(23))
        etree.SubElement( outputSubTag, 'tag', name='Tip(s) detected', value=str(23))
        
        self.bq.finish_mex(tags = [outputTag])
        #self.bq.finish_mex('Finished')
        self.bq.close()
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
	  #parser.add_option('-d','--debug', action="store_true")
	  #parser.add_option('-n','--dryrun', action="store_true")
	  #parser.add_option('--credentials')
	  #parser.add_option('--image_url')
	  
	  parser.add_option('--image_url', dest="image_url")
	  parser.add_option('--mex_url', dest="mexURL")
	  parser.add_option('--module_dir', dest="modulePath")
	  parser.add_option('--staging_path', dest="stagingPath")
	  parser.add_option('--auth_token', dest="token")
	  
	  (options, args) = parser.parse_args()

	  logging.debug('optparse, options: ' + str(options))
	  
	  if options.image_url is None:
	    logging.debug('image_url option needed.')
	  else:
	    logging.debug('image_url option: ' + options.image_url)
	  
	  self.options = options;
	  
	  logging.debug('optparse, args: ' + str(args))

	  named = AttrDict (auth_token=None, mex_url=None, staging_path=None, modulePath=None)
	  
	  for arg in list(args):
	      tag, sep, val = arg.partition('=')
	      logging.debug('args , tag=' + str(tag) + ' and sep ' + str(sep) + ' and value: ' + str(val))
	      
	      if sep == '=':
		  named[tag] = val
		  args.remove(arg)
	  
	  logging.debug('optparse, named: ' + str(named))
	  
	  logging.debug('optparse, final args: ' + str(args))
	    
	  #self.bq = BQSession().init_mex(args[0], args[1])  #mex_url, bisque_token
	  self.bq = BQSession().init_mex(options.mexURL, options.token)
	  	  
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
      
	  if len(args) == 1:
	    commands = [ args.pop(0)]
	  else:
            commands =['setup','start', 'teardown']
      
	  #if not args :
	  #  commands = ['setup', 'start', 'teardown']
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

