#!/usr/bin/python

# Edit this to match the port in Arduino's (Tools > Port) tab.
port = '/dev/cu.usbmodem14401'
port = 'COM5'
import sys
if len(sys.argv) > 1:
	port = sys.argv[1] 

# if you don't have the library, "pip install pyserial==2.7" or "pip install serial"
import serial
from datetime import datetime
import sys, signal
usb = serial.Serial( port , 9600 )
file = open("data.txt","a", 8)
def signal_handler(signal, frame):
    file.close()
    print("\n*Recording interrupted by user.*\n")
    sys.exit(0)
signal.signal(signal.SIGINT, signal_handler) # press ctrl+c to exit
def main():
	while 1:
		now = datetime.now()  # current date and time
		line = now.strftime("%m/%d/%Y, %H:%M:%S:%f, ") + usb.readline().decode('utf-8')
		file.write( line )
		print( line )

if __name__ == "__main__":
	main()