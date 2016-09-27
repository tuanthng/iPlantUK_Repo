#!/usr/bin/env python
import numpy as np
import sys
from PIL import Image
#import matplotlib.pyplot as plt
import os
#from scipy import misc
#from sklearn.feature_extraction import image
from __builtin__ import str
#import math
#import pip

#cwd = os.getcwd();

#def installPackage(package):
#	pip.main(['install', package])


#pip.main(['install -U', 'scikit-learn'])
outimagepath = sys.argv[1];
originimagepath = sys.argv[2];
overlayedimgpath = sys.argv[3]; #sys.argv[3]

		
outimg = Image.open(outimagepath);
orginimg = Image.open(originimagepath);		

npoutimg = np.array(outimg, dtype=np.uint8);
nporginimg = np.array(orginimg, dtype=np.uint8);

npfilteredimg = nporginimg

npfilteredimg[npoutimg == 0] = 0

npfilteredimg[npoutimg > 0] = 1

newimg = Image.fromarray(npfilteredimg)

newimg.save(overlayedimgpath)
	
