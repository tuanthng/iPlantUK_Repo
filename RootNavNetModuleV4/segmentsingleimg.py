#!/usr/bin/env python
from __future__ import division
import sys , os

from lxml import etree
from scipy import misc, ndimage
from skimage import measure
from skimage.feature import blob_doh
#from math import sqrt
from skimage.color import rgb2gray
from scipy.ndimage.measurements import center_of_mass
import math

sys.path.append('/home/tuan/caffe/caffe/python')
#sys.path.append('/usr/local/lib/python2.7/dist-packages')
sys.path.append('/home/tuan/caffe/caffe/lib')
"""
adding lib path to the system (this works for older version of caffe. With the new version, no need to do this
Tested with the version available on caffe github

sudo gedit /etc/ld.so.conf.d/caffe.conf

then, enter the path 
/home/tuan/caffe/caffe/lib

save, and quit

run: sudo ldconfig
"""
os.environ['LD_LIBRARY_PATH'] = '/home/tuan/caffe/caffe/lib'
from ctypes import *
cdll.LoadLibrary('/home/tuan/caffe/caffe/lib/libcaffe.so')

import caffe

import numpy as np
from PIL import Image
#from PIL import ImageDraw
#import scipy.io
#import string

# few settings
##for threeclasses
fWeights = '/home/tuan/MyProject/rootnet/threeclasses/weights-30-08-2016/trainscore59pool43_iter_140000.caffemodel'
fModel = '/home/tuan/MyProject/rootnet/threeclasses/deploy/deploythreeclasses.prototxt'

#fWeights = sys.argv[1]
#fModel = sys.argv[2]

##for five classess
#fWeights = '/home/tuan/MyProject/rootnet/fiveclasses/weights-07-07-2016-t2/trainscore59pool43_iter_140000.caffemodel'
#fModel = '/home/tuan/MyProject/rootnet/fiveclasses/deploy/deployfiveclasses.prototxt'

#useGPU = int(sys.argv[3])
#GPUid = int(sys.argv[4])
useGPU = 0
GPUid = 0
useDivide = 1
#imgpath = sys.argv[5]
#resultimgpath = sys.argv[6]
imgpath = sys.argv[1]
resultimgpath = sys.argv[2]
inputdatafile = sys.argv[3]
inputdatatype = sys.argv[4]

x_cells = float(3);
y_cells = float(3);

#make sure the class ID matching with Ids in the test data
BackgroundId = 0
RootID = 1
SeedID = 2
#PrimaryId = 3
#LateralId = 4

useSmoothed = 0

thresholdareaseed = 3

palette = np.zeros((255,3))
palette[0,:] = [   0,   0,   0] # Background
palette[1,:] = [ 200, 147, 255] # root
#palette[1,:] = [ 255, 184, 153] # root
palette[2,:] = [ 112,  65,  57] # seed
#palette[3,:] = [  51, 153, 255] # primary
palette[3,:] = [  51, 153, 255] # primary
palette[4,:] = [ 67,   0,   0] # lateral


#palette[4,:] = [ 219, 144, 101] # Nose
palette[5,:] = [ 135,   4,   0] # Upper lip
palette[6,:] = [  67,   0,   0] # Mouth
palette[7,:] = [ 135,   4,   0] # Lower lip
palette = palette.astype('uint8').tostring()

#######################
def calculateTestedClass(testedimg, classId):
    #img = Image.open(imgfile);
    img = testedimg
    npimg = np.array(img, dtype=np.uint8);
    
    filtereddataclasses = npimg
    
    #make sure to keep only needed class
    filtereddataclasses[npimg != classId] = 0
    
    #find all connected components    
    #smooth the image to remove small objects?
    blurradius = 0.5
    #threshold = 0
    
    if useSmoothed == 1:
        smoothedimg = ndimage.gaussian_filter(filtereddataclasses, blurradius)
    else:
        smoothedimg = filtereddataclasses
    
    
    labeled, nr_objects = ndimage.label(smoothedimg > 0)
    
    print 'Number of items (classid: ' + str(classId) + ') detected: ' + str(nr_objects)
    
    centreobjects = center_of_mass(labeled, labels=labeled, index=range(1, nr_objects+1))
    
    return centreobjects  # for each item, the format is y, x

def calculateTestedClassV2(testedimg, classId, thresholdarea):
    #this version 2 is trying to remove small tips classified as noise
    #img = Image.open(imgfile);
    img = testedimg
    npimg = np.array(img, dtype=np.uint8);
    #npimg = testedimg.copy()
    
    filtereddataclasses = npimg
    
    #make sure to keep only needed class
    filtereddataclasses[npimg != classId] = 0
    
    #find all connected components    
    #smooth the image to remove small objects?
    blurradius = 0.5
    #threshold = 0
    
    if useSmoothed == 1:
        smoothedimg = ndimage.gaussian_filter(filtereddataclasses, blurradius)
    else:
        smoothedimg = filtereddataclasses
    
    labeled, nr_objects = ndimage.label(smoothedimg > 0)
    
    regionproperties = measure.regionprops(labeled, None, False)
    
    allarea = [r.area for r in regionproperties]
    
    removedindex = [index for index, area in enumerate(allarea) if area <= thresholdarea]
    
    filteredlabeled = np.array(labeled, dtype=np.uint8);
    
    for index in range(0, len(removedindex)):
        filteredlabeled[labeled == removedindex[index] + 1] = 0 # +1 because the background labeled 0
    
    smoothedimg[filteredlabeled == 0] = 0
    
    #label again
    labeled, nr_objects = ndimage.label(smoothedimg > 0)
    
    print 'Number of items (classid: ' + str(classId) + ') detected: ' + str(nr_objects)
    
    centreobjects = center_of_mass(labeled, labels=labeled, index=range(1, nr_objects+1))
    
    return centreobjects  # for each item, the format is y, x


#####################################
#add tips to input data xml file
tree = etree.ElementTree()
tree.parse(inputdatafile)
rootNode = tree.getroot()
#         
#logging.debug('Root node: ' + rootNode.tag)
#          
# # look for the Tips Output tag
pointsNode = rootNode.findall("./Points")
statisticNode = rootNode.findall("./StatisticNode")
gobjectdatadetectionNode = rootNode.findall("./DataDetected/gobject")
statisticDetectionNode = rootNode.findall("./StatisticDetectionNode")


numberNode = statisticNode[0].findall("NumberPoints")
numberpoints = int(numberNode[0].attrib['number'])
numberNode = statisticNode[0].findall("NumberCircles")
numbercircles = int(numberNode[0].attrib['number'])
numberNode = statisticNode[0].findall("NumberSquares")
numbersquares = int(numberNode[0].attrib['number'])

# set up caffe
if useGPU == 0:
    caffe.set_mode_cpu()
else:
    caffe.set_mode_gpu()
    caffe.set_device(GPUid)

# load the network into memory
net = caffe.Net(fModel,
                fWeights,
                caffe.TEST)

print 'Test img: ' + imgpath

fileid, ext = os.path.splitext(imgpath);
savedimgdir = os.path.dirname(imgpath);
    
# prepare the image
imgsrc = Image.open(imgpath)
if imgsrc.mode != 'RGB':
    imgsrc = imgsrc.convert('RGB')
    
im = np.array(imgsrc, dtype=np.float32)

#divide the image into multiple partsl
#for each part, do the segmentation and then combine it
widthImg = im.shape[1]
heighImg = im.shape[0]


if useDivide == 1:

    w_section = int(math.ceil(widthImg/x_cells))
    h_section = int(math.ceil(heighImg/y_cells))
    
    count = 0
    
    newImg = Image.new('P', (widthImg, heighImg))
    x_offset = 0
    y_offset = 0
    
    
    for y in range(0, int(y_cells)):
        starty = y * h_section
        x_offset = 0
        
        for x in range(0, int(x_cells)):
            startx = x * w_section
            
            sectionImg = im[starty : min(starty + h_section, heighImg), startx : min(startx + w_section, widthImg)]
            
            if len(sectionImg.shape) == 2:
                sectionImg = np.reshape(sectionImg, sectionImg.shape + (1,))
                sectionImg = np.concatenate((sectionImg, sectionImg, sectionImg), axis=2)
            sectionImg = sectionImg[:, :, ::-1]  # RGB to BGR
            
            sectionImg -= np.array((87.86, 101.92, 133.01))  # -means (BGR not RGB!!!)
            sectionImg = sectionImg.transpose((2, 0, 1))  # WxHxD to DxWxH
            
            #do the segmentation
            data = sectionImg
    
            # forward pass the image
            net.blobs['data'].reshape(1, *data.shape)  # resize input layer
            net.blobs['data'].data[...] = data
            net.forward()
    
            # get the (hopefully...) segmented output
            out = net.blobs['score'].data[0].argmax(axis=0)
            print out.shape;
    
            #sectionout = out.astype(np.uint8)
            sectionout = Image.fromarray(out.astype(np.uint8))
            sectionout.putpalette(palette)
            
             #combine each section
            newImg.paste(sectionout, (x_offset, y_offset))
            x_offset = x_offset + sectionout.size[0]
            
            if (x == int(x_cells) - 1):
                y_offset = y_offset + sectionout.size[1]
            
            if (ext == '.jpg') | (ext == '.jpeg'):
                sectionout = sectionout.convert('RGB')
            
            #save each section
            sectionImgName = fileid + '_' + str(x) + '_' + str(y) + ext;
            sectionImgPath =  os.path.join(savedimgdir, sectionImgName)
            print 'Sectioned img path: ' + sectionImgPath
            
            sectionout.save(sectionImgPath)
            
            
    
    #out = Image.fromarray(out.astype(np.uint8))
    #out.putpalette(palette)
    #out.save(resultimgpath)
    
    if newImg.mode != 'P':
        newImg.putpalette(palette)
        
    #if (ext == '.jpg') | (ext == '.jpeg'):
    #    newImg = newImg.convert('RGB')
    newImg.save(resultimgpath)
    
    out = newImg
else:
    if len(im.shape) == 2:
        im = np.reshape(im, im.shape + (1,))
        im = np.concatenate((im, im, im), axis=2)
        
    im = im[:, :, ::-1]  # RGB to BGR
    
    im -= np.array((87.86, 101.92, 133.01))  # -means (BGR not RGB!!!)
    im = im.transpose((2, 0, 1))  # WxHxD to DxWxH
    
    data = im
    
    # forward pass the image
    net.blobs['data'].reshape(1, *data.shape)  # resize input layer
    net.blobs['data'].data[...] = data
    net.forward()
    
    # get the (hopefully...) segmented output
    out = net.blobs['score'].data[0].argmax(axis=0)
    print out.shape;
    
    out = Image.fromarray(out.astype(np.uint8))
    out.putpalette(palette)
    
    out.save(resultimgpath)

print 'Output img: ' + resultimgpath

#extract source if necessary
#find primary tips, lateral tips
testedseeds = calculateTestedClassV2(out, SeedID, thresholdareaseed)
numberdetectedpoints = len(testedseeds)
#numbercircles = 0
#numbersquares = 0
#radius = 5

snode = etree.SubElement(statisticDetectionNode[0], "NumberSeeds", {'number' : str(numberdetectedpoints)})

for idx, tip in enumerate(testedseeds):
    #xL = float(tip[1] - radius)
    #yT = float(tip[0] - radius)
    
    #xR =  tip[1] + radius #use // to get float number
    #yB = tip[0] + radius
    gobjectNode = etree.SubElement(gobjectdatadetectionNode[0], "gobject", {"type" : "point", "name" : str(idx)})
    tagnode = etree.SubElement(gobjectNode, "tag", {'name' : 'color', "value" : "#FF0000"})
    vertexnode = etree.SubElement(gobjectNode, "vertex", {'x' : str(tip[1]), 'y' : str(tip[0]), "index" : "0"})
    tagnode = etree.SubElement(gobjectNode, "tag", {'name' : 'shape', "value" : "0"})
    tagnode = etree.SubElement(gobjectNode, "tag", {'name' : 'line_width', "value" : "1"})
    
    #pointnode = etree.SubElement(gobjectdatadetectionNode[0], "point", {"type" : "Source", "name" : str(idx)})
    #vertexnode = etree.SubElement(pointnode, "vertex", {'x' : str(tip[1]), 'y' : str(tip[0]), "index" : "0"})
    #tagnode = etree.SubElement(pointnode, "tag", {'name' : 'color', "value" : "#FF0000"})
    
    if inputdatatype == 'Auto': #note: the Source always at the top of the list
        if numberpoints == 0:
            #pointNode = etree.SubElement(pointsNode[0], "Point", {'x' : str(tip[1]), 'y' : str(tip[0]), "type" : "Source", "Shape" : "Point"})
            pointNode = etree.Element("Point", {'x' : str(tip[1]), 'y' : str(tip[0]), "type" : "Source", "Shape" : "Point"})
            pointsNode[0].insert(0, pointNode)
            
    elif inputdatatype == 'Semi-Auto':
        print 'In semi-auto part of seg'
        #pointNode = etree.SubElement(pointsNode[0], "Point", {'x' : str(tip[1]), 'y' : str(tip[0]), "type" : "Source", "Shape" : "Point"})
        pointNode = etree.Element("Point", {'x' : str(tip[1]), 'y' : str(tip[0]), "type" : "Source", "Shape" : "Point"})
        pointsNode[0].insert(0, pointNode)
    
#tree.write(inputdatafile)
with open(inputdatafile, "w") as id:
    tree.write(id, encoding="utf-8", xml_declaration=True)
