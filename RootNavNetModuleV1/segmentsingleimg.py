#!/usr/bin/env python
from __future__ import division
import sys , os

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
#imgpath = sys.argv[5]
#resultimgpath = sys.argv[6]
imgpath = sys.argv[1]
resultimgpath = sys.argv[2]

#make sure the class ID matching with Ids in the test data
BackgroundId = 0
RootID = 1
SeedID = 2
PrimaryId = 3
LateralId = 4

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
    
# prepare the image
imgsrc = Image.open(imgpath)
if imgsrc.mode != 'RGB':
    imgsrc = imgsrc.convert('RGB')
    
im = np.array(imgsrc, dtype=np.float32)
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
