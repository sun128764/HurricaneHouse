# How to set a WSN laptop

[TOC]

## Software requirement

Operation system: Windows 7 and newer

Software: Python 3.8+, Tapis-cli, XCTU, TeamViewer, Influx DB and Dropbox(or Box)

## Software installation

### Python and Tapis-cli

Download latest Python at https://www.python.org/downloads/

Apply “Add Python to PATH” and click “Install Now”

Press <kbd>Win</kbd> +<kbd>R</kbd> and run “CMD”. 

Run the following command to install Tapis-cli

```shell
pip install tapis-cli
```

Known issue: If you are using **Non-English operation system**, tapis may not installed properly. Go to Control panel->Region->Management-> Change system region setting and apply “"Beta: Use Unicode UTF-8 for worldwide language support”. After installation you can safely restore this change to avoid any potential risk since this is a Beta function.

Then set up the Tapis by your Design Safe account.

```
tapis auth init
Configure Tapis API access:
===========================
+---------------+--------------------------------------+----------------------------------------+
|      Name     |             Description              |                  URL                   |
+---------------+--------------------------------------+----------------------------------------+
|      3dem     |             3dem Tenant              |         https://api.3dem.org/          |
|   agave.prod  |         Agave Public Tenant          |      https://public.agaveapi.co/       |
|  araport.org  |               Araport                |        https://api.araport.org/        |
|     bridge    |                Bridge                |     https://api.bridge.tacc.cloud/     |
|   designsafe  |              DesignSafe              |    https://agave.designsafe-ci.org/    |
|  iplantc.org  |         CyVerse Science APIs         |       https://agave.iplantc.org/       |
|      irec     |              iReceptor               | https://irec.tenants.prod.tacc.cloud/  |
|    portals    |            Portals Tenant            |  https://portals-api.tacc.utexas.edu/  |
|      sd2e     |             SD2E Tenant              |         https://api.sd2e.org/          |
|      sgci     | Science Gateways Community Institute |        https://sgci.tacc.cloud/        |
|   tacc.prod   |                 TACC                 |      https://api.tacc.utexas.edu/      |
| vdjserver.org |              VDJ Server              | https://vdj-agave-api.tacc.utexas.edu/ |
+---------------+--------------------------------------+----------------------------------------+
Enter a tenant name [designsafe]:designsafe
designsafe username: <Your User namer>
designsafe password for <Your User namer>:<Your pass word>
Container registry access:
--------------------------
Registry Url [https://index.docker.io]:
```

Leave the Container registry access part blank and press <kbd>Enter</kbd>.

The tapis will create a Token to access the Design Safe data depot.

Please create a new Token each time using the WSN program by using the following command.

```
tapis auth tokens create
```

Enter your pass word and press <kbd>Enter</kbd>.

### Install Influx DB

Download latest Influx DB at https://portal.influxdata.com/downloads/

Download latest NSSM at https://nssm.cc/download

Unzip the Influx DB into C:/Program Files or any path you desired.

Unzip NSSM and move the nssm.exe to Influx DB folder.

Run “influxd.exe” and the Windows Defender pop up serval dialog boxes to request network connection for Influx DB. Click “Yes” for all dialog boxes.

When the command line window shows that the Influx DB is working properly, close the window.

Run “CMD” and run the following commands to install Influx DB as a service.

```shell
cd C:/Program Files/influxdb-1.8.1-1
or
cd <Your influxdb folder path>

nssm install InfluxDB
```

Then select the influxd.exe in the “Path” and press “Install service”.

Open Task manager->Service to make the “InfluxDB” service is running.

Run “influx.exe” in the Influx DB folder. Run following command to create a new data base named “WSN”.

```sql
CREATE DATABASE "WSN"
```

### Install other software

https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu

https://www.teamviewer.com/en/download/windows/

https://www.dropbox.com/downloading

Please follow the official manual to set up those software.

### Install WSN GUI program

Move the WSN GUI program into C:/Program Files or any path you desired.

Edit “database.json” in the program folder and replace the remote database setting by your setting.

Now you are ready to use WSN GUI program.