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

### Features

- Read sensor data from serial port.
- Read, save and create setting files.
- Display real time sensor status and plot.
- Save data to local .csv files.
- Upload data files to Design Safe data depot automatically.
- Upload data to local or remote Influx DB server.
- Automatically add undefined sensor to sensor list.
- CRC32 check for each data package.
- Automatic serial port recover.

### Data File

The local .csv files contains all data received form serial port. The saving interval is defined by user. Each file starting with a header row. The definition of each column is as follow.

- Base computer time stamp(UTC): The time when the computer received this data package in ISO 8601 format.
-  Network ID: The network ID of the Xbee network. Also known as Pan ID in Xbee module.
-  Board ID: The board ID of the sensor board. Also known as Node ID in Xbee module.
-  Sensor local time stamp: The time span form the board started in mill second. This value is given by Mills() function and determined when the sensor board start collect the Pressure 1.
-  Temperature: 16-bit temperature reading. The original resolution is 8-bit.
-  Battery: 16-bit battery level reading. The original resolution is 8-bit.
-  Wind Speed: 16-bit wind speed reading. The original resolution is 16-bit.
-  Wind Direction:  16-bit wind direction reading. The original resolution is 16-bit.
-  Humidity: 16-bit humidity reading. The original resolution is 16-bit.
- Pressure 1-10: 16-bit pressure reading. The original resolution is 16-bit. All pressure reading are sampled at even interval(0.1s). The pressure 1 sampling start at the time of sensor local time stamp. For example, if the local time stamp is 6100, the pressure 1 sampling started at 6100, the pressure 2 sampling sampling started at 6200 and so on. Each pressure reading is a average of 4096 points of 12->16 oversampling ADC output.

### Setting Files

#### Program Setting

Program setting file contains project information, sensor list, local and remote data file path and serial port settings. This setting file is read or created by setting up Wizard.

#### Sensor List

Sensor list file contains sensor information such as sensor name, network ID, sensor type and meta data. 

## Known issue

* pip install tapis-cli -> non-Unicode application, enable UTF-8

## Sponsor

SciChart : https://www.scichart.com/