import logging
import sys

def set_debug_mode():
    root = logging.getLogger()
    root.setLevel(logging.DEBUG)
    handler = logging.StreamHandler(sys.stdout)
    root.addHandler(handler)
