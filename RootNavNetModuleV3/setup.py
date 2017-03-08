#!/usr/bin/env python
# Install script for RootTip Mult
import sys
from bqapi.comm import BQSession
#from bq.setup.module_setup import matlab_setup, python_setup, read_config, docker_setup
from bq.setup.module_setup import python_setup, require, read_config
from bq.setup.bisque_setup import getanswer

def setup(params, *args, **kw):
    python_setup("RootNavLinuxModule.py", params)
#    matlab_setup('matlab/maizeG.m', bisque_deps=False, params=params)
#    docker_setup('roottipmulti', 'RootTipMulti', 'matlab_runtime', params=params)

if __name__ =="__main__":
    params = read_config('runtime-bisque.cfg')
    if len(sys.argv)>1:
        params = eval (sys.argv[1])
    sys.exit(setup(params))
