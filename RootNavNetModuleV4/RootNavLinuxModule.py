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

from bqapi.util import fetch_dataset, fetch_image_planes, AttrDict, fetch_image_pixels, localpath2url,\
    save_blob, normalize_unicode
#from lxml.builder import E

#img_url_tag = 'image_url'

imageDownloaded = '';
BASE_URL = 'http://127.0.0.1:8080'

#named_args = {}
#staging_path = None

logging.basicConfig(level=logging.DEBUG)

#EXEC = "mono RootNavLinux.exe"
EXEC = "./runRootNav.sh"

UPLOADED_FOLDER = 'RootNavNet'
IMAGE_MAP = "image_map.txt"
IMAGE_LINK = "image_link.txt"
IMAGE_NAME = "image_name.txt"


def gettag (el, tagname):
    for kid in el:
        if kid.get ('name') == tagname:
            return kid, kid.get('value')
    return None,None

#def etree_to_dict(t):
#    d = {t.tag : map(etree_to_dict, t.iterchildren())}
#    d.update(('@' + k, v) for k, v in t.attrib.iteritems())
#    d['text'] = t.text
#    return d

def etree_to_dict(element):
    node = dict()

    text = getattr(element, 'text', None)
    if text is not None:
        node['text'] = text

    node.update(element.items()) # element's attributes

    child_nodes = {}
    for child in element: # element's children
        child_nodes.setdefault(child, []).append( etree_to_dict(child) )

    # convert all single-element lists into non-lists
    for key, value in child_nodes.items():
        if len(value) == 1:
             child_nodes[key] = value[0]

    node.update(child_nodes.items())

    return node
    
class RootNavLinux(object):

    def mex_parameter_parser(self, options, mex_xml):
        """
        Parses input of the xml and add it to RootNav's options attribute
            
        @param: mex_xml
        """
        mex_inputs = mex_xml.xpath('tag[@name="inputs"]')
        
        if mex_inputs:
            for tag in mex_inputs[0]:
                if tag.tag == 'tag' and ('type' in tag.attrib) and tag.attrib['type'] != 'system-input':
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
    
    def downloadFileFromServer(selfs, fullfilename):

        logging.debug('files: ' + str(fullfilename))
    
    def postblobbytxn(self, session, filename, xml=None, path=None, method="POST", **params):
        """
            Create Multipart Post with blob to blob service

            @param filename: filename of the blob
            @param xml: xml to be posted along with the file
            @param params: params will be added to url query
            @return: a <resource type="uploaded" <image> uri="URI to BLOB" > </image>
        """
        #NOTE: because it couldn't get the url of the import service (unknown why). then 
        #use this function.
        
        #import_service_url = self.service_url('import', path='transfer')
        import_service_url = '%s/import/transfer'%session.bisque_root
        if import_service_url is None:
            raise BQApiError('Could not find import service to post blob.')

        url = session.c.prepare_url(import_service_url, **params)

        if xml!=None:
            if not isinstance(xml, basestring):
                xml = session.factory.to_string(xml)

        if filename is not None:
            filename = normalize_unicode(filename)
            with open(filename, 'rb') as f:
                fields = {'file': (filename, f)}
                if xml!=None:
                    fields['file_resource'] = (None, xml, "text/xml")
                return session.c.push(url, content=None, files=fields, headers={'Accept': 'text/xml'}, path=path, method=method)
        elif xml is not None:
            fields = {'file_resource': (None, xml, "text/xml")}
            return session.c.push(url, content=None, files=fields, headers={'Accept': 'text/xml'}, path=path, method=method)
        else:
            raise BQCommError("improper parameters for postblob")

    imgformatname = {
                    'png': 'png',
                    'tif': 'tiff',
                    'tiff': 'tiff',
                    'jpg': 'jpeg',
                    'jpeg': 'jpeg',
                    'jpe': 'jpeg',
                    'bmp': 'bmp'}

    def getimgformatname(self, ext):
        return self.imgformatname.get(ext, 'tiff')

    def fetch_image_pixelsbytxn(self, session, uri, dest, uselocalpath=False, imgformat='tiff', ext='tif'):
        """
        fetch original image locally as tif
        @param session: the bqsession
        @param uri: resource image uri
        @param dest: a destination directory
        @param uselocalpath: true when routine is run on same host as server
        """
        image = session.load(uri)
        name = image.name or next_name("image")
        ip = image.pixels().format(imgformat)
        if uselocalpath:
            ip = ip.localpath()
        pixels = ip.fetch()
        if os.path.isdir(dest):
            dest = os.path.join(dest, os.path.basename(name))
        else:
            dest = os.path.join('.', os.path.basename(name))
        if not dest.lower().endswith ('.'+ext):
            dest = "%s%s" % (dest, ext)
    
    
        if uselocalpath:
            # path = ET.XML(pixels).xpath('/resource/@src')[0]
            resource = session.factory.string2etree(pixels)
            path = resource.get ('value')
            # path = urllib.url2pathname(path[5:])
            if path.startswith('file:/'):
                path = path[5:]
                # Skip 'file:'
            if os.path.exists(path):
                safecopy(path, dest)
                return { uri : dest }
            else:
                log.error ("localpath did not return valid path: %s", path)
    
        f = open(dest, 'wb')
        f.write(pixels)
        f.close()
        return { uri : dest }


    def fetch_datasetbytxn(self, session, uri, dest, uselocalpath=False):
        """
        fetch elemens of dataset locally as tif

        @param session: the bqsession
        @param uri: resource image uri
        @param dest: a destination directory
        @param uselocalpath: true when routine is run on same host as server

        @return:
        """
        dataset = session.fetchxml(uri, view='deep')
        members = dataset.findall('.//value[@type="object"]')
    
        results = {}
        listfilename = []
        listlink = []
        
        for i, imgxml in enumerate(members):
            uri =  imgxml.text   #imgxml.get('uri')
            print "FETCHING", uri
            #fname = os.path.join (dest, "%.5d.tif" % i)
            
            image = self.bq.load(uri)
            fileid, ext = os.path.splitext(image.name);
            ext = ext.split('.')[-1] #remove . in the extension
            formatimg = self.getimgformatname(ext)
            
            x = self.fetch_image_pixelsbytxn(session, uri, dest, uselocalpath,formatimg,ext)
            results.update (x)
            
            listfilename.append(image.name)
            listlink.append(uri)
            
        return results, listfilename, listlink


                    
    def setup(self):
        #if not os.path.exists(self.images):
        #    os.makedirs(self.images)

        self.bq.update_mex('initialising')
        #results = fetch_image_planes(self.bq, self.resource_url, '.')
	
        #self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
	
        #logging.debug('setup/ final options: ' + str(self.options))
        
        self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
        #check dataset input?
        resource_xml = self.bq.fetchxml (self.options.image_url, view='short')
        self.options.is_dataset = resource_xml.tag == 'dataset'
        self.options.image_map_name = os.path.join(self.options.stagingPath, IMAGE_MAP)
        self.options.image_name = os.path.join(self.options.stagingPath, IMAGE_NAME)
        self.options.image_link = os.path.join(self.options.stagingPath, IMAGE_LINK)
                    
        if not self.options.is_dataset:
        
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
            
            statisticNode = etree.SubElement(inputDataNode, "StatisticNode")
    
            statisticDetectionNode  = etree.SubElement(inputDataNode, "StatisticDetectionNode")
            
            #for data detection from CNN
            datadetectedNode = etree.SubElement(inputDataNode, "DataDetected")
            etree.SubElement(datadetectedNode, "gobject", {'name' : 'detection'})
            ########
                    
            # extract gobject inputs
            tips = self.bq.mex.find('inputs', 'tag').find('image_url', 'tag').find('sources', 'gobject')
            #with open('inputtips.csv', 'w') as TIPS:
                #for point in tips.gobjects:
                #    print >>TIPS, "%(y)s, %(x)s" % dict(x=point.vertices[0].x,y=point.vertices[0].y)
                    
                #for ob in tips.gobjects:
                #    print >> TIPS, "%(y)s, %(x)s" % dict(x=circle. .vertices[0].x,y=point.vertices[0].y)
                #    logging.debug('xmltag: ' + ob.xmltag +  ' ' + str(ob))
            
            #fileid, ext = os.path.splitext(image.name);
            #ext = ext.split('.')[-1] #remove . in the extension
            
            numberpoints = 0
            numbercircles = 0
            numbersquares = 0
            numberpolylines = 0
            
            if tips is not None:
                for ob in tips.gobjects:
                    logging.debug('xmltag: ' + ob.xmltag +  ' ' + str(ob))
                    
                    if ob.xmltag == 'point':
                        pointNode = etree.SubElement(pointsNode, "Point", {'x' : ob.vertices[0].x, 'y' : ob.vertices[0].y, "type" : "Source", "Shape" : "Point"})
                        numberpoints = numberpoints + 1
                        
                    elif ob.xmltag == 'circle':
                        xL = float(ob.vertices[0].x)
                        yT = float(ob.vertices[0].y)
                        
                        x =  xL + (float(ob.vertices[1].x) - xL)//2.0 #use // to get float number
                        y = yT + (float(ob.vertices[1].y) - yT)//2.0
                        
                        pointNode = etree.SubElement(pointsNode, "Point", {'x' : str(x), 'y' : str(y), "type" : "Primary", "Shape" : "Circle", "xLeft" : str(xL), "yTop" : str(yT), "xRight" : str(ob.vertices[1].x), "yBottom" : str(ob.vertices[1].y)})
                        numbercircles = numbercircles + 1
                        
                    elif ob.xmltag == 'square':
                        xL = float(ob.vertices[0].x)
                        yT = float(ob.vertices[0].y)
                        
                        x =  xL + (float(ob.vertices[1].x) - xL)//2.0
                        y = yT + (float(ob.vertices[1].y) - yT)//2.0
                        
                        pointNode = etree.SubElement(pointsNode, "Point", {'x' : str(x), 'y' : str(y), "type" : "Lateral", "Shape" : "Square", "xLeft" : str(xL), "yTop" : str(yT), "xRight" : str(ob.vertices[1].x), "yBottom" : str(ob.vertices[1].y)})
                        numbersquares = numbersquares + 1
                        
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
                        
                        numberpolylines = numberpolylines + 1
            
            #numberpoints = 0
            #numbercircles = 0
            #numbersquares = 0
            #numberpolylines = 0
            snode = etree.SubElement(statisticNode, "NumberPoints", {'number' : str(numberpoints)})
            snode = etree.SubElement(statisticNode, "NumberCircles", {'number' : str(numbercircles)})
            snode = etree.SubElement(statisticNode, "NumberSquares", {'number' : str(numbersquares)})
            
                                    
            #if either sources or tips haven't not entered, then use CNN to detect source and tips
    #         if numberpoints == 0 or numbercircles == 0 or numbersquares == 0:
    #             tipsImg = os.path.join(self.options.stagingPath, fileid + '_tips.png')
    #         
    #             paras = imageDownloaded + ' ' + tipsImg
    #             
    #             logging.debug('execute python detecttipssingleimg.py ' + paras)
    #             
    #             os.system('python detecttipssingleimg.py ' + paras)
                    
            #tree = etree.ElementTree(pointsNode)
            tree = etree.ElementTree(inputDataNode)
                    
            with open(inputDataFileFullPath, "w") as id:
                tree.write(id, encoding="utf-8", xml_declaration=True)
        
        else:
            logging.debug('Input is the dataset')
            results, listfilename, listlink = self.fetch_datasetbytxn(self.bq, self.options.image_url, self.options.stagingPath)
            
            #save map
            with open(self.options.image_map_name, 'wb') as f:
                pickle.dump(results, f)
            
            #save image name
            with open(self.options.image_name, 'wb') as fn:
                #pickle.dump(results, f)
                for item in listfilename:
                    #save the filename
                    fn.write("%s\n" % item)
                    ############################################################
                    #create the input data for each image loaded
                    inputDataFile = item + "_InputData.xml"
                    
                    inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
                    
                    logging.debug('Result file: ' + inputDataFileFullPath)
                    
                    #build the input data file
                    inputDataNode = etree.Element("InputData")
                    #pointsNode = etree.Element("Points")
                    pointsNode = etree.SubElement(inputDataNode, "Points")
                    
                    adjustedPathsNode = etree.SubElement(inputDataNode, "AdjustedPaths")
                    
                    statisticNode = etree.SubElement(inputDataNode, "StatisticNode")
            
                    statisticDetectionNode  = etree.SubElement(inputDataNode, "StatisticDetectionNode")
                    
                    #for data detection from CNN
                    datadetectedNode = etree.SubElement(inputDataNode, "DataDetected")
                    etree.SubElement(datadetectedNode, "gobject", {'name' : 'detection'})
                    ########
                    
                    numberpoints = 0
                    numbercircles = 0
                    numbersquares = 0
                    numberpolylines = 0
                    
                    snode = etree.SubElement(statisticNode, "NumberPoints", {'number' : str(numberpoints)})
                    snode = etree.SubElement(statisticNode, "NumberCircles", {'number' : str(numbercircles)})
                    snode = etree.SubElement(statisticNode, "NumberSquares", {'number' : str(numbersquares)})
                            
                    #tree = etree.ElementTree(pointsNode)
                    tree = etree.ElementTree(inputDataNode)
                            
                    with open(inputDataFileFullPath, "w") as id:
                        tree.write(id, encoding="utf-8", xml_declaration=True)                   
                    ############################################################
                    
            #save image links                
            with open(self.options.image_link, 'wb') as fl:
                for item in listlink:
                    fl.write("%s\n" % item)
                    
                    
        return 0
	return;


    def start(self):
        self.bq.update_mex('executing')
        # Matlab requires trailing slash
        #build parameters for the tool
        # -ImageFile="0002.jpg" -PresetName="Custom" -InitialClassCount=3 -MaximumClassCount=4 -ExpectedRootClassCount=2 -PatchSize=150 -BackgroundPercentage=0.5 -BackgroundExcessSigma=1.5 -Weights="0.35,0.68,0.99"
        
        self.mex_parameter_parser(self.options, self.bq.mex.xmltree)
        #check dataset input?
        resource_xml = self.bq.fetchxml (self.options.image_url, view='short')
        self.options.is_dataset = resource_xml.tag == 'dataset'
        self.options.image_map_name = os.path.join(self.options.stagingPath, IMAGE_MAP)
        self.options.image_name = os.path.join(self.options.stagingPath, IMAGE_NAME)
        
        logging.debug('start/ final options: ' + str(self.options))
        
        if not self.options.is_dataset:
        
            #image_xml = self.bq.fetchxml(self.options.image_url)
            #logging.debug('image_xml: ' + str(image_xml))
            
            #image_url = self.bq.service_url('image_service',path=image_xml.attrib['resource_uniq'])
            #logging.debug('image_url: ' + str(image_url))
            
            #get input data xml file
            image = self.bq.load(self.options.image_url)
            inputDataFile = image.name + "_InputData.xml"
            
            inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
            
            fileid, ext = os.path.splitext(image.name);
            ext = ext.split('.')[-1] #remove . in the extension
            formatimg = self.getimgformatname(ext)
            
            #results = fetch_image_planes(self.bq, self.options.image_url, self.options.stagingPath)
            #results = fetch_image_pixels (self.bq, self.options.image_url, self.options.stagingPath)
            # trying to download images keeping their original types
            results = self.fetch_image_pixelsbytxn(self.bq, self.options.image_url, self.options.stagingPath,False,formatimg,ext)
              
            
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
            fileid, ext = os.path.splitext(imageDownloaded);
            
            #call FCN to do the segmentation
            #need to use png because the output image is in P mode, not RGB
            #else, need to convert the output to RGB before saving it
            #segmentedImg = os.path.join(self.options.stagingPath, fileid + '_seg' + ext)
            segmentedImg = os.path.join(self.options.stagingPath, fileid + '_seg.png')
            
            paras = imageDownloaded + ' ' + segmentedImg + ' ' + inputDataFileFullPath + ' ' + self.options.InputType
            
            logging.debug('execute python segmentsingleimg.py ' + paras)
            
            os.system('python segmentsingleimg.py ' + paras)
            
            #overlay the output on the original
            overlayedImg = os.path.join(self.options.stagingPath, fileid + '_ove' + ext)
            
            paras = segmentedImg + ' ' + imageDownloaded + ' ' + overlayedImg
            
            logging.debug('execute python overlayOutputToOriginalImg.py ' + paras)
            
            os.system('python overlayOutputToOriginalImg.py ' + paras)
            
            #######################
            #check if needs tip detected
            
            tipsImg = os.path.join(self.options.stagingPath, fileid + '_tips.png')
            scaledImg = os.path.join(self.options.stagingPath, fileid + '_scaled' + ext)
            
            paras = imageDownloaded + ' ' + tipsImg + ' ' + inputDataFileFullPath + ' ' + scaledImg + ' ' + self.options.InputType
                
            logging.debug('execute python detecttipssingleimg.py ' + paras)
                
            os.system('python detecttipssingleimg.py ' + paras)
                    
            ################
            
            #assign the output image to the imageDownloaded variable
            imageDownloaded = overlayedImg
            
    #         #get input data xml file
    #         image = self.bq.load(self.options.image_url)
    #         inputDataFile = image.name + "_InputData.xml"
    #         
    #         inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
            
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
        else:
            logging.debug('Dataset in Start')
            
            #########################################
            with open(self.options.image_name, 'r') as f:
                content = f.readlines()
            # you may also want to remove whitespace characters like `\n` at the end of each line
            content = [x.strip() for x in content] 
            
            for imagename in content:
                #get input data xml file
                
                inputDataFile = imagename + "_InputData.xml"
                
                inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
                
                fileid, ext = os.path.splitext(imagename);
                ext = ext.split('.')[-1] #remove . in the extension
                formatimg = self.getimgformatname(ext)
                
                # trying to download images keeping their original types
                
                imageDownloaded = os.path.join(self.options.stagingPath, imagename)
                
                
                #call FCN to do the segmentation
                #need to use png because the output image is in P mode, not RGB
                #else, need to convert the output to RGB before saving it
                #segmentedImg = os.path.join(self.options.stagingPath, fileid + '_seg' + ext)
                segmentedImg = os.path.join(self.options.stagingPath, fileid + '_seg.png')
                
                paras = imageDownloaded + ' ' + segmentedImg + ' ' + inputDataFileFullPath + ' ' + self.options.InputType
                
                logging.debug('execute python segmentsingleimg.py ' + paras)
                
                os.system('python segmentsingleimg.py ' + paras)
                
                #overlay the output on the original
                if ext[0] != '.':
                    overlayedImg = os.path.join(self.options.stagingPath, fileid + '_ove.' + ext)
                else:
                    overlayedImg = os.path.join(self.options.stagingPath, fileid + '_ove' + ext)
                
                paras = segmentedImg + ' ' + imageDownloaded + ' ' + overlayedImg
                
                logging.debug('execute python overlayOutputToOriginalImg.py ' + paras)
                
                os.system('python overlayOutputToOriginalImg.py ' + paras)
                
                #######################
                #check if needs tip detected
                
                tipsImg = os.path.join(self.options.stagingPath, fileid + '_tips.png')
                
                if ext[0] != '.':
                    scaledImg = os.path.join(self.options.stagingPath, fileid + '_scaled.' + ext)
                else:
                    scaledImg = os.path.join(self.options.stagingPath, fileid + '_scaled' + ext)
                
                paras = imageDownloaded + ' ' + tipsImg + ' ' + inputDataFileFullPath + ' ' + scaledImg + ' ' + self.options.InputType
                    
                logging.debug('execute python detecttipssingleimg.py ' + paras)
                    
                os.system('python detecttipssingleimg.py ' + paras)
                        
                ################
                
                #assign the output image to the imageDownloaded variable
                imageDownloaded = overlayedImg
                
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
                    ' -PlantName="' + fileid + '"' + \
                    ' -CurvatureProfile="' + self.options.CurvatureProfile + '"' + \
                    ' -MapProfile="' + self.options.MapProfile + '"' + \
                    ' -TravelMap="' + self.options.Travel + '"' + \
                    ' -CompleteArch="' + self.options.CompleteArch + '"' + \
                    ' -DoMeasurementTable="' + self.options.OutputMeasurementTable + '"' 
                    
                 
                logging.debug('parasRootNav: ' + parasRootNav)
                 
                fullPath = os.path.join(self.options.stagingPath, EXEC)
                logging.debug('fullPath: ' + fullPath)
                 
                #fullExec = fullPath + ' ' + parasRootNav
                #logging.debug('Execute: ' + fullExec)
                 
                #r = subprocess.call(['/home/tuan/bisque/modules/RootNavLinuxModuleV2/', EXEC])
                r = subprocess.call([fullPath, parasRootNav])
                #r = 0
                
                #self.bq.update_mex('Collecting result...')
            
            ##########################################
            
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
        
        #check dataset input?
        resource_xml = self.bq.fetchxml (self.options.image_url, view='short')
        self.options.is_dataset = resource_xml.tag == 'dataset'
        self.options.image_map_name = os.path.join(self.options.stagingPath, IMAGE_MAP)
        self.options.image_name = os.path.join(self.options.stagingPath, IMAGE_NAME)
        
        
        if not self.options.is_dataset:
        
            #get the image downloaded
            image = self.bq.load(self.options.image_url)
            #imageDownloaded = image.name + ".tif"
            imageDownloaded = image.name 
            
            fileid, ext = os.path.splitext(image.name);
            
            #resultfile = os.path.join(self.options.stagingPath, imageDownloaded + '_result.xml')
            resultfile = os.path.join(self.options.stagingPath, fileid +'_ove' + ext + '_result.xml')
            
            inputDataFile = imageDownloaded + "_InputData.xml"
            inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
            
            logging.debug('Result file: ' + resultfile)
            logging.debug('Input data file: ' + inputDataFileFullPath)
        
            #load the result file and display info.
    #       tree.parse('/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg_result.xml')
            tree = etree.ElementTree()
            tree.parse(resultfile)
            rootNode = tree.getroot()
            ########
            #tree = etree.ElementTree()
            tree.parse(inputDataFileFullPath)
            rootInputNode = tree.getroot()
            ##########
            logging.debug('Root node: ' + rootNode.tag)
    #          
            # # look for the Tips Output tag
            tipDetectionNode = rootNode.findall("./Output/TipsDetected")
            
            statisticDetectionNode = rootInputNode.findall("./StatisticDetectionNode")
    
            numberNode = statisticDetectionNode[0].findall("NumberSeeds")
            numberpoints = int(numberNode[0].attrib['number'])
            numberNode = statisticDetectionNode[0].findall("NumberPrimary")
            numbercircles = int(numberNode[0].attrib['number'])
            numberNode = statisticDetectionNode[0].findall("NumberLateral")
            numbersquares = int(numberNode[0].attrib['number'])
            
            
            outputTag = etree.Element('tag', name='outputs')
            outputSubTag = etree.SubElement(outputTag, 'tag', name='summary')
            
            #if len(tipDetectionNode) > 0:
            if numberpoints + numbercircles + numbersquares > 0:    
                #totalAttrib = tipDetectionNode[0].get('total')
                
                #logging.debug('tipDetectionNode : ' + totalAttrib)
                
                ##etree.SubElement(outputTag, 'tag', name='TipDetection', value=str(23))
                ##etree.SubElement( outputSubTag, 'tag', name='Tip(s) detected', value=str(23))
                
                
                #etree.SubElement( outputSubTag, 'tag', name='Tip(s) detected', value=totalAttrib)
                etree.SubElement( outputSubTag, 'tag', name='Seed(s) detected', value=str(numberpoints))
                etree.SubElement( outputSubTag, 'tag', name='Primary tip(s) detected', value=str(numbercircles))
                etree.SubElement( outputSubTag, 'tag', name='Lateral tip(s) detected', value=str(numbersquares))
                
                #using testing image: /home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png
                #filepath = '/home/tuan/bisque/modules/RootNavLinuxModuleV3/FeatureMapInMain.png'
                #filepath = '/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002_copy.jpg'
                #just for testing
               
                outputImgTag = etree.SubElement(outputTag, 'tag', name='OutputImage', value=self.options.image_url, type='image')
                #outputImgTag = etree.SubElement(outputTag, 'tag', name='OutputImage', value=localpath2url(filepath))
                #gObjectValue = ""
                #gObjectTag = etree.SubElement(outputImgTag, 'gobject', name='PointsDetected')
                logging.debug('appending children to the output image tag')
                #replacing: detection by features by detection by CNN
                #gObjectTag = rootNode.findall("./Output/TipsDetected/gobject")[0]
                #outputImgTag.append(gObjectTag)
                
                gObjectInputTag = rootInputNode.findall("./DataDetected/gobject")[0]
                outputImgTag.append(gObjectInputTag)
                
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
                
            #resource = etree.Element ('image', name=os.path.basename(filepath), value=localpath2url(filepath))
            #meta = etree.SubElement (resource, 'tag', name='Experimental')
            #etree.SubElement (meta, 'tag', name='numberpoints', value="12")
            
            #resource = etree.Element ('image', name='new file %s'%(os.path.basename(filepath)))
                         
            #logging.debug('resource: ' + str(resource))
            
            
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
            ##########################
            #upload the segmented image
            inputFileNode = rootNode.findall("./Input/File")
            
            imagefilepathNode = inputFileNode[0].find("ImageFile").text
            
            imagefile = os.path.basename(imagefilepathNode)
            #get mexid
            parts = self.options.stagingPath.split('/')
            mexid = parts[len(parts) - 1]
            
            resource = etree.Element ('image', name=os.path.join(UPLOADED_FOLDER, mexid, imagefile))
            
            response = self.postblobbytxn(self.bq, (imagefilepathNode), xml=resource)
            blob = etree.XML(response).find('./')
            
            if blob is None or blob.get('uri') is None:
                logging.debug('Could not upload the segmented image file into the system')
                self.bq.fail_mex('Could not upload the segmented image file into the system')
            else:
                ##create node for mex
                linksegmentedimgupload = blob.get('uri')
                
                segmentedImageTag = etree.SubElement(outputTag, 'tag', name='SegmentedImage', value=linksegmentedimgupload, type='image')
                
            
            ##########################
            #output shortest paths
            outputPathImgTag = etree.SubElement(outputTag, 'tag', name='OutputPathImage', value=self.options.image_url, type='image')
            
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
            outputRootImgTag = etree.SubElement(outputTag, 'tag', name='OutputRootsImage', value=self.options.image_url, type='image')
             
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
            #extract rsml file from xml data
            #inputFileNode = rootNode.findall("./Input/File")
            tempNode = inputFileNode[0].find("RSMLFile")
            
            if tempNode is not None:
                rsmlFileNode = inputFileNode[0].find("RSMLFile").text
                rsmlPathNode = inputFileNode[0].find("RSMLPath").text
                    
                #upload rsml file
                #parts = self.options.stagingPath.split('/')
                #mexid = parts[len(parts) - 1]
                resultrsmlfile = os.path.join(rsmlPathNode, rsmlFileNode)
                #resource = etree.Element ('image', name="\'" + os.path.join(mexid, rsmlFileNode) + "\'")
                resource = etree.Element ('resource', name=os.path.join(UPLOADED_FOLDER, mexid, rsmlFileNode))
                #resource = etree.Element ('resource', name='new file %s'%rsmlFileNode )
                #resource = etree.Element ('image', name='new file P.rsml')
                logging.debug('name resource: ' + os.path.join(mexid, rsmlFileNode))
                logging.debug('resultrsmlfile: ' + resultrsmlfile)
                logging.debug('localpath: ' + localpath2url(resultrsmlfile))
                logging.debug('resource: ' + str(resource))
            
                #self.uploadFileToServer(resultrsmlfile);
                
                #response = self.bq.postblob(localpath2url(resultrsmlfile), xml=resource)
                #response = self.bq.postblob((resultrsmlfile), xml=resource)
                response = self.postblobbytxn(self.bq, (resultrsmlfile), xml=resource)
                blob = etree.XML(response).find('./')
                if blob is None or blob.get('uri') is None:
                    logging.debug('Could not upload the rsml file into the system')
                    self.bq.fail_mex('Could not upload the rsml file into the system')
                else:
                    ##create node for mex
                    linkdataservice = blob.get('uri')
                    linkblobservice = linkdataservice.replace('data_service', 'blob_service');
                    outputRSMLFileTag = etree.SubElement(outputTag, 'tag', name='RSMLFile', value=linkblobservice, type='file')
                    outputRSMLNameTag = etree.SubElement(outputTag, 'tag', name='RSMLName', value=rsmlFileNode, type='name')
                    
            #response = save_blob(self.bq, localpath2url(resultrsmlfile), resource=resource)
    #         response = save_blob(self.bq, resultrsmlfile, resource=resource)
    #                        
    #         if response is None or response.get('uri') is None:
    #             logging.debug('Could not upload the rsml file into the system')
    #             self.bq.fail_mex('Could not upload the rsml file into the system')
    #         else:
    #             #create node for mex
    #             outputRSMLFileTag = etree.SubElement(outputTag, 'tag', name='RSMLFile', value=response.get('uri'), type='file')
                  
            
            #or using # self.bq.addTag()
            #self.bq.finish_mex(tags = [outputTag], gobjects = [gObjectRootNode])
            self.bq.finish_mex(tags = [outputTag])
            #self.bq.finish_mex('Finished')
            self.bq.close()
            return;
        
        else:
            
            logging.debug('Dataset in Tear down')
            
            with  open(self.options.image_map_name, 'rb') as f:
                self.options.url2file = pickle.load(f) #
                self.options.file2url =  dict((v,k) for k,v in self.options.url2file.iteritems())
            
            with open(self.options.image_name, 'r') as f:
                content = f.readlines()
            # you may also want to remove whitespace characters like `\n` at the end of each line
            content = [x.strip() for x in content]
            
            
            #newoutputtag = etree.Element('tag', name='rootresult')
            
            #mexTag = etree.Element('tag', name='rootresult')
            tags = [
                { 'name': 'execute_options',
                  'tag' : [ {'name': 'iterable', 'value' : 'image_url' } ]
                  },
                { 'name': 'outputs',
                  'tag' : [
                           {'name': 'mex_url', 'value': self.options.mexURL, 'type': 'mex'},
                           {'name': 'image_url', 'type':'dataset', 'value':self.options.image_url,}]
                  },
                    ]
            
            
            bigoutputtag = etree.Element('tag', name='outputs')
            bigoutputmextag = etree.SubElement(bigoutputtag, 'tag', name='mex_url', value=self.options.mexURL, type='mex')
            bigoutputurltag = etree.SubElement(bigoutputtag, 'tag', name='image_url', value=self.options.image_url, type='dataset')
            
            gobjects = []
            submexes = []
            
            for imagename in content:
            #####for each image processed
                #get the image downloaded
                #imageDownloaded = image.name + ".tif"
                imageDownloaded = imagename 
                
                fileid, ext = os.path.splitext(imagename);
                
                #resultfile = os.path.join(self.options.stagingPath, imageDownloaded + '_result.xml')
                resultfile = os.path.join(self.options.stagingPath, fileid +'_ove' + ext + '_result.xml')
                
                inputDataFile = imageDownloaded + "_InputData.xml"
                inputDataFileFullPath = os.path.join(self.options.stagingPath, inputDataFile)
                
                logging.debug('Result file: ' + resultfile)
                logging.debug('Input data file: ' + inputDataFileFullPath)
            
                #load the result file and display info.
        #       tree.parse('/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg_result.xml')
                tree = etree.ElementTree()
                tree.parse(resultfile)
                rootNode = tree.getroot()
                ########
                #tree = etree.ElementTree()
                tree.parse(inputDataFileFullPath)
                rootInputNode = tree.getroot()
                ##########
                logging.debug('Root node: ' + rootNode.tag)
        #          
                # # look for the Tips Output tag
                tipDetectionNode = rootNode.findall("./Output/TipsDetected")
                
                statisticDetectionNode = rootInputNode.findall("./StatisticDetectionNode")
        
                numberNode = statisticDetectionNode[0].findall("NumberSeeds")
                numberpoints = int(numberNode[0].attrib['number'])
                numberNode = statisticDetectionNode[0].findall("NumberPrimary")
                numbercircles = int(numberNode[0].attrib['number'])
                numberNode = statisticDetectionNode[0].findall("NumberLateral")
                numbersquares = int(numberNode[0].attrib['number'])
                
                
                #create mex node
                mexTag = etree.Element('mex', name=self.bq.mex.name, type=self.bq.mex.type, value='FINISHED')
                #mexTag = etree.SubElement(bigoutputtag, 'mex', name=self.bq.mex.name, type=self.bq.mex.type, value='FINISHED')
                
                mexinputTag = etree.SubElement(mexTag, 'tag', name='inputs')
                mexinputurlTag = etree.SubElement(mexinputTag, 'tag', name='image_url', value=self.options.file2url[os.path.join(self.options.stagingPath,imagename)])
                
                #submexes.append(etree_to_dict(mexTag))
                submexes.append(mexTag)
                
                
                #outputTag = etree.Element('tag', name='outputs')
                outputTag = etree.SubElement(mexTag, 'tag', name='outputs')
                outputSubTag = etree.SubElement(outputTag, 'tag', name='summary')
                
                #if len(tipDetectionNode) > 0:
                if numberpoints + numbercircles + numbersquares > 0:    
    
                    etree.SubElement( outputSubTag, 'tag', name='Seed(s) detected', value=str(numberpoints))
                    etree.SubElement( outputSubTag, 'tag', name='Primary tip(s) detected', value=str(numbercircles))
                    etree.SubElement( outputSubTag, 'tag', name='Lateral tip(s) detected', value=str(numbersquares))
                    
                   
                    outputImgTag = etree.SubElement(outputTag, 'tag', name='OutputImage', value=self.options.file2url[os.path.join(self.options.stagingPath,imagename)], type='image')
                    #outputImgTag = etree.SubElement(outputTag, 'tag', name='OutputImage', value=localpath2url(filepath))
                    #gObjectValue = ""
                    #gObjectTag = etree.SubElement(outputImgTag, 'gobject', name='PointsDetected')
                    logging.debug('appending children to the output image tag')
                    #replacing: detection by features by detection by CNN
                    #gObjectTag = rootNode.findall("./Output/TipsDetected/gobject")[0]
                    #outputImgTag.append(gObjectTag)
                    
                    gObjectInputTag = rootInputNode.findall("./DataDetected/gobject")[0]
                    outputImgTag.append(gObjectInputTag)
                    
    
                
                rootsTopMostNodes = rootNode.xpath('./Output/RootTree/Root[@order="-1"]')
                
                for root in rootsTopMostNodes:
                    etree.SubElement( outputSubTag, 'tag', name='Root length', value=root.get('length'))
                    etree.SubElement( outputSubTag, 'tag', name='Root area', value=root.get('area'))
                    etree.SubElement( outputSubTag, 'tag', name='Primary root', value=root.get('primaryRoots'))     
                    
                logging.debug('self.bq.service_map in teardown: ' + str(self.bq.service_map))
                
                ##########################
                #upload the segmented image
                inputFileNode = rootNode.findall("./Input/File")
                
                imagefilepathNode = inputFileNode[0].find("ImageFile").text
                
                imagefile = os.path.basename(imagefilepathNode)
                #get mexid
                parts = self.options.stagingPath.split('/')
                mexid = parts[len(parts) - 1]
                
                resource = etree.Element ('image', name=os.path.join(UPLOADED_FOLDER, mexid, imagefile))
                
                response = self.postblobbytxn(self.bq, (imagefilepathNode), xml=resource)
                blob = etree.XML(response).find('./')
                
                if blob is None or blob.get('uri') is None:
                    logging.debug('Could not upload the segmented image file into the system')
                    self.bq.fail_mex('Could not upload the segmented image file into the system')
                else:
                    ##create node for mex
                    linksegmentedimgupload = blob.get('uri')
                    
                    segmentedImageTag = etree.SubElement(outputTag, 'tag', name='SegmentedImage', value=linksegmentedimgupload, type='image')
                    
                
                ##########################
                #output shortest paths
                outputPathImgTag = etree.SubElement(outputTag, 'tag', name='OutputPathImage', value=self.options.file2url[os.path.join(self.options.stagingPath,imagename)], type='image')
                
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
                outputRootImgTag = etree.SubElement(outputTag, 'tag', name='OutputRootsImage', value=self.options.file2url[os.path.join(self.options.stagingPath,imagename)], type='image')
                 
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
                #extract rsml file from xml data
                #inputFileNode = rootNode.findall("./Input/File")
                tempNode = inputFileNode[0].find("RSMLFile")
                
                if tempNode is not None:
                    rsmlFileNode = inputFileNode[0].find("RSMLFile").text
                    rsmlPathNode = inputFileNode[0].find("RSMLPath").text
                        
                    #upload rsml file
    
                    resultrsmlfile = os.path.join(rsmlPathNode, rsmlFileNode)
    
                    resource = etree.Element ('resource', name=os.path.join(UPLOADED_FOLDER, mexid, rsmlFileNode))
    
                    logging.debug('name resource: ' + os.path.join(mexid, rsmlFileNode))
                    logging.debug('resultrsmlfile: ' + resultrsmlfile)
                    logging.debug('localpath: ' + localpath2url(resultrsmlfile))
                    logging.debug('resource: ' + str(resource))
                
    
                    response = self.postblobbytxn(self.bq, (resultrsmlfile), xml=resource)
                    blob = etree.XML(response).find('./')
                    if blob is None or blob.get('uri') is None:
                        logging.debug('Could not upload the rsml file into the system')
                        self.bq.fail_mex('Could not upload the rsml file into the system')
                    else:
                        ##create node for mex
                        linkdataservice = blob.get('uri')
                        linkblobservice = linkdataservice.replace('data_service', 'blob_service');
                        outputRSMLFileTag = etree.SubElement(outputTag, 'tag', name='RSMLFile', value=linkblobservice, type='file')
                        outputRSMLNameTag = etree.SubElement(outputTag, 'tag', name='RSMLName', value=rsmlFileNode, type='name')
                    

            #self.bq.finish_mex(tags = [outputTag], gobjects = [gObjectRootNode])
            #self.bq.finish_mex(tags = [outputTag])
            #self.bq.finish_mex(tags = tags, gobjects = gobjects, children= [('mex', submexes)])
            #self.bq.finish_mex(tags = tags, gobjects = gobjects, children= submexes)
            #self.bq.finish_mex(tags = [bigoutputtag], gobjects = gobjects, children= [('mex', submexes)])
            self.bq.finish_mex(tags = [bigoutputtag], gobjects = gobjects, children= [('mex', submexes)])
            #self.bq.finish_mex(tags = [bigoutputtag])
            
            
            #########
            
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
            
            #these for dataset resource 
            #resource_xml = self.bq.fetchxml (self.options.image_url, view='short')
            #self.options.is_dataset = resource_xml.tag == 'dataset' 
            #self.options.image_map_name = os.path.join(self.options.stagingPath, IMAGE_MAP)
            
                
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

