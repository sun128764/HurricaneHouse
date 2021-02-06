HOW TO USE THE PROGRAM

1. Download.
a. For Microship, Arduino https://www.arduino.cc/en/main/software
b. For XBee, XCTU https://www.digi.com/resources/documentation/digidocs/90001526/tasks/t_download_and_install_xctu.htm
c. For Python, Anaconda https://www.anaconda.com/distribution/
 

2. Open the ino file and configure Arduino
a. Tab Arduino > Preferences... > Additional Board Manager URLs, and add https://adafruit.github.io/arduino-board-index/package_adafruit_index.json, then OK.
b. Tab Tools > Board > Boards Manager, and install: "Ardunio SAMD Boards" and "Adafruit SAMD Boards," then tab Tools > Boards > Adafruit feather M0.
c. Tab Sketch > Include Library > Mange Libraries, and install "XBee-Arduniio library", "XBee Serial Array", "Arduino Low Power" and 'ArduinoThread"

3. Connect the board to a usb, and tab Tools > Port > [Your_USB_Here]

4. In the ino file, edit these parameters, then click upload.
NetworkID = 5001 // must be the same for all connected boards in a network.
BoardID = 1 // counter to identify the boards
BoardType = 1.Coordinator (cellular/gps/main), 2. Anemometer, 3. Humidity., 4. regular

5. While a Coordinator is connected to the usb, open a terminal window, browse to the current directory, and enter:
python3 Record.py /dev/cu.usbmodem14401

Where /dev/cu.usbmodem14401 is from the Port tab.
Each line is a sensor, and each three digit is a reading. The line is prefixed with the recording date/time of the lines' last samples. Click <Control + C> to safely exit the python's loop.

6. to build a new corrdinator. Incert New dongles on USB then open xctu and turn the dongle on. In. ino file find the "encryption key" in the code at lin 168 looks like "Serial2.print("ATKY AAAAABBBBBCCCCCDDDDDEEEEEFFFFF12\r" );" Copy "AAA........FF12" to KYT parameter.
The sensor source data would be exported to a file called "data.txt" in the same directory.





//////

This was developed for Dr. Subramanian.
ahasanain2014@my.fit.edu



