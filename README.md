# Hurricane House

This is the code repository of the FIT Hurricane House Project. The goal of this project is to study the wind profile of the house during the hurricane period to improve the building design guideline. This project is founded by NIST. All data will be upload to DesignSafe-CI so that public can access to these data. The post process will be a sperate program on DesignSafe-CI using Jupiter Notebook with Python.

## File Folder

This repository contains four folders.

- Arduino Code – this folder contain all code for sensor board.
- GUI – this folder contain all code and file for real time monitor running on Windows.

## Sensor board

The sensor board is designed by Jaycon Systems Inc. The MCU of the board is ATSAM21. This board is based on Adafruit Feather M0, which support Arduino IDE. We choose Arduino IDE to develop this project since it is easy to use and there are lots of useful libraries. 

The communication module of the board is Digi Xbee3 so that sensors can communicate by ZigBee network. 

This board has a Li-ion battery pack to support it for at least 3 days without external power supply. This board also support solar panel to charge it so that it doesn’t need charge during the sunny days. Those sensors could be installed on the roof serval days before the storm coming without any worry of battery dead.

This board will be stalled in a water proof cap-shape plastic surface. There will be a hold at the center of the surface and connected to the pressure on the board to measure the ambient static pressure. The accuracy of the pressure measurement is about $\pm$10Pa for each sample and the resolution is 2 Pa/div.  The native accuracy of the pressure sensor is $\pm$2% (15 – 115 kPa) and the native bit depth of the MCU build in ADC is 12 bits. We use over sampling and average value to improve the accuracy of the pressure measurement.

Since we don’t need high accuracy temperature data, the temperature sensor is place on the corner of the board to avoid the inflection from chips.

The sending and sampling timing is determined by mills() function to save processer resource and increase timing stability. The board will collect 1024 point of pressure measurement and calculate the average value then send to Xbee module. The data will be read by router node and send to computer by serial port.

## GUI – Windows real time monitor

The objective of this program is to monitor the sensors activity remotely during the data collecting period. Sensor data will be transmitted to this program so that the monitor can show those data in real time plot. This program is able to show up to 3 plots at the same time to compare the behavior of different sensors. 

### Technology Stack

IDE : Visual Studio 2019

Depended libraries : .Net Core , WPF , SciChart

### Class definition

#### Sensor data:

Contain all data and information from sensors.

- Private Variables: Starting by _ and lower case letter. Those variables is the original integer value of sensors measurement. The maximum value depends on the bit depth of the MCU ADC. Those variables can only be accessed by public get and set methods.
- Public Variables: 
  - Sensor info with set and get methods.
  - List of measurement data in Time Series format.
  - SciChart plot series
  - Sensor data string with ``NotifyPropertyChanged()``.
  - Set and get methods of sensor data.
- Public Methods:
  - Convert to String: Convert different integer data to string. The unit is determined by bool variable``isSI``.
  - Get Sensor Data: Read input string from serial communication and add those data to it’s class members.
- Private Methods:
  - Add Data: Add time series data point to List.

## Known issue

* pip install tapis-cli -> non-Unicode application, enable UTF-8

## Sponsor

SciChart : https://www.scichart.com/