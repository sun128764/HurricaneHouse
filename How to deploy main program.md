# How to compile main program and sensors

## Install SciChart

Before you start compiling the GUI program, you need install SciChart and active it with valid license. SciChart provide free education license for university users. Please contact them for the education license at https://www.scichart.com/contact-us/

Download SciChart https://www.scichart.com/login/?redirect_to=https://www.scichart.com/downloads/

Use license wizard to active SciChart on your computer. Copy the License Key.

## Compile main program

Clone the whole repo to your local dirve.

Open HurricanHouse_DataCollect.sln in Visual Studio.

Edit App.xmal.cs, replace the License Key.

Now you can compile the main program in visual studio.

## Burn Arduino code

Please read the ReadMe.txt at \ArduinoCode\Democode\Sensor_Device_v1\ to set up Arduino IDE and libraries.

Then download SAMD CRC32 library at https://github.com/knicholson32/SAMD_CRC32

In Arduino IDE, Sketch-> Include Library-> Add .zip library, select the SAMD CRC32 library zip file you just downloaded.



