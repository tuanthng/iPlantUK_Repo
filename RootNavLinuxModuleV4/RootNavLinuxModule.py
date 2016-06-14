#!/usr/bin/python
from __future__ import division #shoud place this at the beginning of the file

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
import requests

#mport gobject
#import six

#import BQSession
from os import sys, path

from lxml import etree

sys.path.append(path.dirname(path.dirname(path.abspath(__file__))))

sys.path.append('/home/tuan/bisque/bqapi/')

from bqapi.comm import BQSession
from bqapi import BQTag

from bqapi.util import fetch_image_planes, AttrDict, fetch_image_pixels, localpath2url,\
    save_blob
#from lxml.builder import E

#img_url_tag = 'image_url'

imageDownloaded = '';
BASE_URL = 'http://127.0.0.1:8080'

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
        
    def uploadFileToServer(self, fullFilename):
        #self.bq.push()
        uri_pattern = re.compile(ur'uri="(?P<uri>\S+)"') #pattern to extract uri later on from response header.
         
        files={'file':(fullFilename,open(fullFilename, "rb"))}
        response=requests.post('%s/import/transfer'%BASE_URL,files=files,auth=('admin' ,'admin'))
        file_uri=re.findall(uri_pattern, response.text)
        
        logging.debug('files: ' + str(files))
        logging.debug('response: ' + str(response))
        logging.debug('response.text: ' + response.text)
        logging.debug('file_uri: ' + str(file_uri))
       #a = 1
        #return;
                
    def setup(self):
        #if not os.path.exists(self.images):
        #    os.makedirs(self.images)

        self.bq.update_mex('initialising')
        #results = fetch_image_planes(self.bq, self.resource_url, '.')
	
        #self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
	
        #logging.debug('setup/ final options: ' + str(self.options))
        
        self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
        #get the image downloaded
        image = self.bq.load(self.options.image_url)
        inputDataFile = image.name + "_InputData.xml"
        
        inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
        
        logging.debug('Result file: ' + inputDataFileFullPath)
        
        #build the input data file
        inputDataNode = etree.Element("InputData")
        #pointsNode = etree.Element("Points")
        pointsNode = etree.SubElement(inputDataNode, "Points")
        
        adjustedPathsNode = etree.SubElement(inputDataNode, "AdjustedPaths")
        
        # extract gobject inputs
        tips = self.bq.mex.find('inputs', 'tag').find('image_url', 'tag').find('sources', 'gobject')
        #with open('inputtips.csv', 'w') as TIPS:
            #for point in tips.gobjects:
            #    print >>TIPS, "%(y)s, %(x)s" % dict(x=point.vertices[0].x,y=point.vertices[0].y)
                
            #for ob in tips.gobjects:
            #    print >> TIPS, "%(y)s, %(x)s" % dict(x=circle. .vertices[0].x,y=point.vertices[0].y)
            #    logging.debug('xmltag: ' + ob.xmltag +  ' ' + str(ob))
        
        for ob in tips.gobjects:
            logging.debug('xmltag: ' + ob.xmltag +  ' ' + str(ob))
            
            if ob.xmltag == 'point':
                pointNode = etree.SubElement(pointsNode, "Point", {'x' : ob.vertices[0].x, 'y' : ob.vertices[0].y, "type" : "Source", "Shape" : "Point"})
                
            elif ob.xmltag == 'circle':
                xL = float(ob.vertices[0].x)
                yT = float(ob.vertices[0].y)
                
                x =  xL + (float(ob.vertices[1].x) - xL)//2.0 #use // to get float number
                y = yT + (float(ob.vertices[1].y) - yT)//2.0
                
                pointNode = etree.SubElement(pointsNode, "Point", {'x' : str(x), 'y' : str(y), "type" : "Primary", "Shape" : "Circle", "xLeft" : str(xL), "yTop" : str(yT), "xRight" : str(ob.vertices[1].x), "yBottom" : str(ob.vertices[1].y)})
                
            elif ob.xmltag == 'square':
                xL = float(ob.vertices[0].x)
                yT = float(ob.vertices[0].y)
                
                x =  xL + (float(ob.vertices[1].x) - xL)//2.0
                y = yT + (float(ob.vertices[1].y) - yT)//2.0
                
                pointNode = etree.SubElement(pointsNode, "Point", {'x' : str(x), 'y' : str(y), "type" : "Lateral", "Shape" : "Square", "xLeft" : str(xL), "yTop" : str(yT), "xRight" : str(ob.vertices[1].x), "yBottom" : str(ob.vertices[1].y)})
                
            elif ob.xmltag == 'polyline':
                pathNode = etree.SubElement(adjustedPathsNode, "Path")
                
                numPoints = len(ob.vertices)
                
                for index in range(0, numPoints):
                    x = float(ob.vertices[index].x)
                    y = float(ob.vertices[index].y)
                    if index == 0:
                        pointNode = etree.SubElement(pathNode, "Point", {'x' : str(x), 'y' : str(y), "type" : "Start", "Shape" : "Polyline"})
                    else:
                        pointNode = etree.SubElement(pathNode, "Point", {'x' : str(x), 'y' : str(y), "type" : "Mid", "Shape" : "Polyline"})
                        
        #tree = etree.ElementTree(pointsNode)
        tree = etree.ElementTree(inputDataNode)
                
        with open(inputDataFileFullPath, "w") as id:
            tree.write(id, encoding="utf-8", xml_declaration=True)
        
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
        imageDownloaded = results[self.options.image_url];
        #imageDownloadedFullPath = os.path.join(self.options.stagingPath, imageDownloaded)
        
        #get input data xml file
        image = self.bq.load(self.options.image_url)
        inputDataFile = image.name + "_InputData.xml"
        
        inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
        
        #construct the parameters for the tool
        #' -ImageFile="' +  os.path.basename(imageDownloaded) + '"'
        parasRootNav = ' -ImageFile="' +  imageDownloaded + '"' + \
            ' -PresetName="' + self.options.PresetName + '"' + \
			' -InitialClassCount=' + self.options.InitialClassCount + \
			' -MaximumClassCount=' + self.options.MaximumClassCount + \
			' -ExpectedRootClassCount=' + self.options.ExpectedRootClassCount + \
			' -PatchSize=' + self.options.PatchSize + \
			' -BackgroundPercentage=' + self.options.BackgroundPercentage + \
			' -BackgroundExcessSigma=' + self.options.BackgroundExcessSigma + \
			' -Weights="' + self.options.Weights + '"' + \
            ' -InputPointsFile="' + inputDataFile + '"' + \
            ' -DoMeasurement="' + self.options.DoMeasurement + '"' + \
            ' -ImageResolutionValue="' + self.options.ImageRes + '"' + \
            ' -SplineSpacing="' + self.options.SplineSpacing + '"' + \
            ' -PlantName="' + self.options.PlantName + '"' + \
            ' -CurvatureProfile="' + self.options.CurvatureProfile + '"' + \
            ' -MapProfile="' + self.options.MapProfile + '"' + \
            ' -TravelMap="' + self.options.Travel + '"' + \
            ' -CompleteArch="' + self.options.CompleteArch + '"' + \
            ' -DoMeasurementTable="' + self.options.OutputMeasurementTable + '"' 
            
        #parasRootNav = str(parasRootNav)
         
        logging.debug('parasRootNav: ' + parasRootNav)
         
        fullPath = os.path.join(self.options.stagingPath, EXEC)
        logging.debug('fullPath: ' + fullPath)
         
        #fullExec = fullPath + ' ' + parasRootNav
        #logging.debug('Execute: ' + fullExec)
         
        #r = subprocess.call(['/home/tuan/bisque/modules/RootNavLinuxModuleV2/', EXEC])
        r = subprocess.call([fullPath, parasRootNav])
        #r = 0
        
        #self.bq.update_mex('Collecting result...')
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
        
        #load result data from the xml file. For testing, just use the fix xml i.e. will be changed later time
        #resultfile = os.path.join(self.options.stagingPath, '0002.jpg_result.xml')
        #reload parameters
        self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
        #get the image downloaded
        image = self.bq.load(self.options.image_url)
        imageDownloaded = image.name + ".tif"
        
        resultfile = os.path.join(self.options.stagingPath, imageDownloaded + '_result.xml')
        
        logging.debug('Result file: ' + resultfile)
    
        #load the result file and display info.
#       tree.parse('/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg_result.xml')
        tree = etree.ElementTree()
        tree.parse(resultfile)
        rootNode = tree.getroot()
#         
        logging.debug('Root node: ' + rootNode.tag)
#          
        # # look for the Tips Output tag
        tipDetectionNode = rootNode.findall("./Output/TipsDetected")
        
        outputTag = etree.Element('tag', name='outputs')
        outputSubTag = etree.SubElement(outputTag, 'tag', name='summary')
        
        if len(tipDetectionNode) > 0:
            
            totalAttrib = tipDetectionNode[0].get('total')
            
            logging.debug('tipDetectionNode : ' + totalAttrib)
            
            ##etree.SubElement(outputTag, 'tag', name='TipDetection', value=str(23))
            #etree.SubElement( outputSubTag, 'tag', name='Tip(s) detected', value=str(23))
            
            
            etree.SubElement( outputSubTag, 'tag', name='Tip(s) detected', value=totalAttrib)
            
           
            #using testing image: /home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png
            #filepath = '/home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png'
            filepath = '/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002_copy.jpg'
            #just for testing
           
            outputImgTag = etree.SubElement(outputTag, 'tag', name='OutputImage', value=self.options.image_url)
            #outputImgTag = etree.SubElement(outputTag, 'tag', name='OutputImage', value=localpath2url(filepath))
            #gObjectValue = ""
            #gObjectTag = etree.SubElement(outputImgTag, 'gobject', name='PointsDetected')
            logging.debug('appending children to the output image tag')
            gObjectTag = rootNode.findall("./Output/TipsDetected/gobject")[0]
            outputImgTag.append(gObjectTag)
            
            #test colour (this works for one point, change colour from red to yello)
            #etree.SubElement(gObjectTag[0], 'tag', name='color', value="#ffff00")
            
            #for tip in tipDetectionNode[0]:
            #    gPoint = etree.SubElement(gObjectTag, 'point', name=tip.attrib['id'])
            #    etree.SubElement(gPoint, 'vertex', x=tip.attrib['x'], y=tip.attrib['y'])
             
        #display root info
        #rootsTopMostNodes = rootNode.findall("./Output/RootTree/Root")
        
        rootsTopMostNodes = rootNode.xpath('./Output/RootTree/Root[@order="-1"]')
        
        for root in rootsTopMostNodes:
            etree.SubElement( outputSubTag, 'tag', name='Root length', value=root.get('length'))
            etree.SubElement( outputSubTag, 'tag', name='Root area', value=root.get('area'))
            etree.SubElement( outputSubTag, 'tag', name='Primary root', value=root.get('primaryRoots'))     
            
            
            #outputExtraImgTag = etree.SubElement(outputTag, 'tag', name='OutputExtraImage', value=self.options.image_url)
            
        resource = etree.Element ('image', name=os.path.basename(filepath), value=localpath2url(filepath))
        meta = etree.SubElement (resource, 'tag', name='Experimental')
        etree.SubElement (meta, 'tag', name='numberpoints', value="12")
        
        #resource = etree.Element ('image', name='new file %s'%(os.path.basename(filepath)))
                     
        logging.debug('resource: ' + str(resource))
        
        
        #self.uploadFileToServer(filepath)
        
        logging.debug('self.bq.service_map in teardown: ' + str(self.bq.service_map))
            
        #url = self.bq.service_url('data_service', 'image')
        #url = self.bq.service_url('blob_service')
        #url = self.bq.service_url('image_service', 'image') ##couldnt use for upload
        #url = self.bq.service_url('/import/transfer') #not a service
        #url = self.bq.service_url('import', 'transfer') #not a service
        #url = self.bq.service_url('import', path='transfer') #not a service
        #url = self.bq.service_url('http://127.0.0.1:8080/', '/import/transfer') #not a service
        #response = save_blob(self.bq, resource=resource)
        
        #logging.debug('url : ' + str(url))
        #response_xml = self.bq.postblob(localpath2url(filepath), xml=resource) #post image to bisque and get the response
        
        #logging.debug('response_xml: ' + str(response_xml))
        
        #r =  self.bq.postxml(url, resource, method='POST')
        
        
        #logging.debug('Response: ' + str(r))
        
        #response = self.bq.postblob(filepath, xml=resource)
        #response = self.bq.postblob(filepath, xml=resource)
        #blob = etree.XML(response).find('./')
        
        
        # if blob is None or blob.get('uri') is None:
        #    logging.debug('Could not insert the Histogram file into the system')
        #    self.bq.fail_mex('Could not insert the Histogram file into the system')
        # else:
            # outputExtraImgTag = etree.SubElement(outputTag, 'tag', name='/home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png')
        #    outputExtraImgTag = etree.SubElement(outputTag, 'tag', name='OutputExtraImage', value=blob.get('uri'), type='image')
            
       # if r is None or r.get('uri') is None:
        #    logging.debug('Could not insert the Histogram file into the system')
       #     self.bq.fail_mex('Could not insert the Histogram file into the system')
       # else:
            # outputExtraImgTag = etree.SubElement(outputTag, 'tag', name='/home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png')
        #    logging.debug('resource id: %s' %r.get('resource_uniq'))
        #    logging.debug('url: %s' %r.get('uri'))
        #    outputExtraImgTag = etree.SubElement(outputTag, 'tag', name='OutputExtraImage', value=r.get('uri'), type='image')
            # outputExtraImgTag = etree.SubElement(outputImgTag, 'tag', name='OutputExtraImage', value=r.get('value'), type='image')
        #etree.SubElement(outputTag, 'tag', name='OutputImage', value='/home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png')
        
        #etree.SubElement(outputTag, 'tag', name='OutputImage', value='file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png')
        
        #output shortest paths
        outputPathImgTag = etree.SubElement(outputTag, 'tag', name='OutputPathImage', value=self.options.image_url)
        
        #get primary paths
        primaryPathsNode = rootNode.findall("./Output/PrimaryPaths")
        if (primaryPathsNode is not None) and (len(primaryPathsNode) > 0):
            for path in primaryPathsNode[0]:
                outputPathImgTag.append(path)
        
        #get lateral paths
        lateralPathsNode = rootNode.findall("./Output/LateralPaths")
        if (lateralPathsNode is not None) and (len(lateralPathsNode) > 0):
            for path in lateralPathsNode[0]:
                outputPathImgTag.append(path)
        
        #node to display the root and convex hull
        outputRootImgTag = etree.SubElement(outputTag, 'tag', name='OutputRootsImage', value=self.options.image_url)
         
        gObjectRootNode = etree.SubElement(outputRootImgTag, 'gobject', name='Roots')
         
        splineInOtherRootsNodes = rootNode.xpath('./Output/RootTree/Root[@order!="-1"]/Spline')
        for spline in splineInOtherRootsNodes:
            #outputRootImgTag.append(spline[0])
            gObjectRootNode.append(spline[0])
         
        for root in rootsTopMostNodes:
            convexNodes = root.findall('ConvexHull')
            for cx in convexNodes:
                #outputRootImgTag.append(cx[0])                
                gObjectRootNode.append(cx[0])
        
        #get data for measurement table
        measurementTablesNode =  rootNode.xpath('./Output/Measurement/Tables')
                
        if (measurementTablesNode is not None and len (measurementTablesNode) > 0):
            #outputRootImgTag.append(measurementTablesNode[0])
            gObjectRootNode.append(measurementTablesNode[0])
        
        #get data for curvature profile
        curvNode = etree.SubElement(gObjectRootNode, 'tag', name='CurvatureProfile')
        curvatureProfileDataNode = rootNode.xpath('./Output/Measurement/CurvatureProfile')
        if (curvatureProfileDataNode is not None and len (curvatureProfileDataNode) > 0):
            for rowDataNode in curvatureProfileDataNode[0]:
                #gObjectRootNode.append()
                gObjectCurv = etree.SubElement(curvNode, 'gobject', type='row')
                etree.SubElement(gObjectCurv, 'tag', name='col0', value=rowDataNode.attrib['col0'])
                etree.SubElement(gObjectCurv, 'tag', name='col1', value=rowDataNode.attrib['col1'])
            
        #get data for travel map
        mapNode = etree.SubElement(gObjectRootNode, 'tag', name='MapProfile')
        mapProfileDataNode = rootNode.xpath('./Output/Measurement/MapProfile')
        if (mapProfileDataNode is not None and len (mapProfileDataNode) > 0):
            #gObjectRootNode.append(mapProfileDataNode[0])
            for rowDataNode in mapProfileDataNode[0]:
                gObjectMap = etree.SubElement(mapNode, 'gobject', type='row')
                etree.SubElement(gObjectMap, 'tag', name='col0', value=rowDataNode.attrib['col0'])
                etree.SubElement(gObjectMap, 'tag', name='col1', value=rowDataNode.attrib['col1'])
                etree.SubElement(gObjectMap, 'tag', name='col2', value=rowDataNode.attrib['col2'])
            
        #get data for RSML file for downloading 
        
        #or using # self.bq.addTag()
        #self.bq.finish_mex(tags = [outputTag], gobjects = [gObjectRootNode])
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
            self.bq = BQSession().init_mex(options.mexURL, options.token) # changed to below code for testing
    	  
            #self.bq =BQSession().init_local('admin', 'admin', bisque_root='http://127.0.0.1:8080') #initialize local session
            logging.debug('self.bq.service_map: ' + str(self.bq.service_map))
                
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

