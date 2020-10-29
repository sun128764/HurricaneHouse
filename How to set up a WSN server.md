# How to set up a WSN web server

[TOC]

This tutorial will go through all setting up methods based on Debian 9 server.

## Server Requirement

OS: Linux or Windows(optional)

CPU: 1 or more vCores

RAM: 512 MB

Storage: 20 GB SSD

IP : ipv4 or ipv6(optional)

## Update system software

```shell
sudo yum update
sudo yum upgrade
```

## Install Software

Install Nginx first.

```shell
sudo yum install nginx 
```

Install Influx DB and Grafana.

```shell
cat <<EOF | sudo tee /etc/yum.repos.d/influxdb.repo
[influxdb]
name=InfluxDB Repository - RHEL \$releasever
baseurl=https://repos.influxdata.com/rhel/\$releasever/\$basearch/stable
enabled=1
gpgcheck=1
gpgkey=https://repos.influxdata.com/influxdb.key
EOF

sudo yum install -y influxdb

sudo systemctl unmask influxdb.service
sudo systemctl start influxdb

sudo nano /etc/yum.repos.d/grafana.repo
[grafana]
name=grafana
baseurl=https://packages.grafana.com/oss/rpm
repo_gpgcheck=1
enabled=1
gpgcheck=1
gpgkey=https://packages.grafana.com/gpg.key
sslverify=1
sslcacert=/etc/pki/tls/certs/ca-bundle.crt

sudo yum install grafana
sudo service grafana-server start
```

For other operation systems, please check their office site for installation description.

https://www.nginx.com/resources/wiki/start/topics/tutorials/install/

https://docs.influxdata.com/influxdb/v1.8/introduction/install/

https://grafana.com/docs/grafana/latest/installation/

If you have a domain without a valid SSL certification, please use Let’s encrypt to set up a free SSL certification.

For web server, Please **ALWAYS** enable SSL since the password is transferred by Web API.

Install the Certbot

```shell
sudo apt-get install certbot
sudo service nginx stop
sudo certbot certonly --standalone -d example.com -d www.example.com
sudo service nginx start
```

Please replace the domain with your own domain. 

Since both nginx and certbot are using 443 port, the nginx service must be stopped during the cert process.

You can also check the installation method on their official site https://certbot.eff.org/

## Nginx setting

### Set Influx DB forward

The Influx DB will use the 8086 port for Web API without SLL. So we will use Nginx to forward port 8086 to our domain with SLL enabled.

Upload a .conf file with the content as follow to /etc/nginx/conf.d

```nginx
server {  
    listen  80;
    server_name <Your domain here>;
    rewrite ^(.*)$  https://$host$1 permanent;
}
server {
    listen       443 ssl;
    server_name <Your domain here>;
    ssl on;
    ssl_certificate /etc/letsencrypt/live/<Your domain here>/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/<Your domain here>/privkey.pem;
    ssl_prefer_server_ciphers on;
    location / {
        proxy_set_header X-Forwarded-For $remote_addr;
        proxy_set_header Host $http_host;
        proxy_pass http://127.0.0.1:8086;
    }
}
```

The first server is listening 80 port, which is the default port for http request, and re-direct the request to 443 port.

The second server is listening 443 port, which is the default port for https request. It will forward all request to 8086 port.

You can use SFTP to login and up this .conf file, or use file editor via SSH to make this file.

For example, you can use Nano to create this file

```shell
sudo nano /etc/nginx/conf.d/wsn.conf
```

Then paste the setting into the SSH window. Press <kbd>Ctrl</kbd> +<kbd>X</kbd> to save and exit.

Then restart Nginx service to apply the setting.

```shell
sudo service nginx restart
```

### Set Grafana forward

The Grafana dashboard will use 3000 port to provide Web UI service. To avoid port number shown in URL, you can use Nginx to forward the request.

Upload a .conf file with the content as follow to /etc/nginx/conf.d

```nginx
server {  
    listen  80;
    server_name <Your domain here>;
    rewrite ^(.*)$  https://$host$1 permanent;
      
}
server {
    listen       443 ssl;
    server_name <Your domain here>;
    ssl on;
    ssl_certificate /etc/letsencrypt/live/<Your domain here>/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/<Your domain here>/privkey.pem;
    ssl_prefer_server_ciphers on;
    location / {
        proxy_set_header X-Forwarded-For $remote_addr;
        proxy_set_header Host $http_host;
        proxy_pass http://127.0.0.1:3000;
    }
}
```

Then you can use same method as above to upload this .conf file and restart the Nginx.

Please note that **DO NOT** set same domain for both Influx DB and Grafana. 

## Set Influx DB

### Login Influx DB

#### Web UI

The Influx DB provide a Web UI at 8083. You can access the web UI in your browser at [<Your Server IP>:8083]()

By default, Influx DB authentication is disabled. You have fully access to the database once you open the Web UI page.

#### command line interface

The Influx DB also provide a CLI program to access the database. Please read the official site of the CLI https://docs.influxdata.com/influxdb/v1.8/tools/shell/

### Set user, password and privilege 

Both Web UI and CLI are using same query command.

First, you must set a password for admin user.

```sql
CREATE USER <username> WITH PASSWORD '<password>' WITH ALL PRIVILEGES
```

Then create a new database for WSN system and a new user to upload the data.

```sql
CREATE DATABASE "<db_name>"
CREATE USER <username> WITH PASSWORD '<password>'
GRANT ALL ON <db_name> TO <username>
SHOW GRANTS FOR <user_name>
```

If it shows that the user have the privilege to the database, you are good to go.

### Enable authentication 

Edit the config file of the Influx DB to enable the authentication.

```shell
sudo nano /etc/influxdb/influxdb.conf
```

Set the `auth-enabled` in `[http]` to `true`. Then press <kbd>Ctrl</kbd> +<kbd>X</kbd> to save and exit.

Restart the Influx DB.

```shell
sudo systemctl restart influxdb
```

## Set Grafana

Login the grafana dashboard by the domain you set in Nginx.

At the first login, it will ask you to set a password.

In Configuration->Data source, Add a data source. Select Influx DB.

Set the Data source as fellow, enter your user and password.

Then click `Save & Test`

![image-20201016220031631](C:\Users\sun12\source\repos\sun128764\HurricaneHouse\image-20201016220031631.png)

Then go to Dashboard-> Management->Import. In “Import via panel json”, paste the json as fellow

```json
{
  "annotations": {
    "list": [
      {
        "builtIn": 1,
        "datasource": "-- Grafana --",
        "enable": true,
        "hide": true,
        "iconColor": "rgba(0, 211, 255, 1)",
        "name": "Annotations & Alerts",
        "type": "dashboard"
      }
    ]
  },
  "editable": true,
  "gnetId": null,
  "graphTooltip": 0,
  "id": 1,
  "iteration": 1602900125044,
  "links": [],
  "panels": [
    {
      "aliasColors": {},
      "bars": false,
      "dashLength": 10,
      "dashes": false,
      "datasource": null,
      "fieldConfig": {
        "defaults": {
          "custom": {}
        },
        "overrides": []
      },
      "fill": 1,
      "fillGradient": 0,
      "gridPos": {
        "h": 14,
        "w": 12,
        "x": 0,
        "y": 0
      },
      "hiddenSeries": false,
      "id": 2,
      "interval": "",
      "legend": {
        "avg": false,
        "current": false,
        "max": false,
        "min": false,
        "show": true,
        "total": false,
        "values": false
      },
      "lines": true,
      "linewidth": 1,
      "nullPointMode": "null",
      "percentage": false,
      "pluginVersion": "7.1.3",
      "pointradius": 2,
      "points": false,
      "renderer": "flot",
      "seriesOverrides": [],
      "spaceLength": 10,
      "stack": false,
      "steppedLine": false,
      "targets": [
        {
          "alias": "Sensor $tag_SensorID",
          "groupBy": [
            {
              "params": [
                "$__interval"
              ],
              "type": "time"
            },
            {
              "params": [
                "SensorID"
              ],
              "type": "tag"
            }
          ],
          "measurement": "WSN",
          "orderByTime": "ASC",
          "policy": "default",
          "query": "SELECT mean(\"Value\") FROM \"Pressure\" WHERE (\"SensorID\" =~ /^$Sensors$/) AND $timeFilter GROUP BY time($__interval) fill(null)",
          "rawQuery": false,
          "refId": "A",
          "resultFormat": "time_series",
          "select": [
            [
              {
                "params": [
                  "Pressure"
                ],
                "type": "field"
              },
              {
                "params": [],
                "type": "mean"
              }
            ]
          ],
          "tags": [
            {
              "key": "SensorID",
              "operator": "=~",
              "value": "/^$Sensors$/"
            }
          ]
        }
      ],
      "thresholds": [],
      "timeFrom": null,
      "timeRegions": [],
      "timeShift": null,
      "title": "Sensor Pressure",
      "tooltip": {
        "shared": true,
        "sort": 0,
        "value_type": "individual"
      },
      "type": "graph",
      "xaxis": {
        "buckets": null,
        "mode": "time",
        "name": null,
        "show": true,
        "values": []
      },
      "yaxes": [
        {
          "decimals": null,
          "format": "none",
          "label": "Pressure(mBar)",
          "logBase": 1,
          "max": null,
          "min": "950",
          "show": true
        },
        {
          "format": "short",
          "label": null,
          "logBase": 1,
          "max": null,
          "min": null,
          "show": true
        }
      ],
      "yaxis": {
        "align": false,
        "alignLevel": null
      }
    },
    {
      "datasource": null,
      "description": "",
      "fieldConfig": {
        "defaults": {
          "custom": {
            "align": null
          },
          "mappings": [],
          "thresholds": {
            "mode": "absolute",
            "steps": [
              {
                "color": "green",
                "value": null
              }
            ]
          }
        },
        "overrides": []
      },
      "gridPos": {
        "h": 7,
        "w": 12,
        "x": 12,
        "y": 0
      },
      "id": 6,
      "options": {
        "frameIndex": 2,
        "showHeader": true,
        "sortBy": [
          {
            "desc": false,
            "displayName": "SensorID"
          }
        ]
      },
      "pluginVersion": "7.1.3",
      "targets": [
        {
          "groupBy": [
            {
              "params": [
                "SensorID"
              ],
              "type": "tag"
            }
          ],
          "measurement": "WSN",
          "orderByTime": "ASC",
          "policy": "default",
          "query": "SELECT last(\"Value\") FROM \"Battery\" WHERE $timeFilter GROUP BY \"SensorID\"",
          "rawQuery": false,
          "refId": "A",
          "resultFormat": "table",
          "select": [
            [
              {
                "params": [
                  "Battery"
                ],
                "type": "field"
              },
              {
                "params": [],
                "type": "last"
              }
            ]
          ],
          "tags": []
        }
      ],
      "timeFrom": null,
      "timeShift": null,
      "title": "Battery Level(V)",
      "type": "table"
    },
    {
      "datasource": null,
      "fieldConfig": {
        "defaults": {
          "custom": {
            "align": null
          },
          "decimals": 2,
          "mappings": [],
          "max": 4.3,
          "min": 3.2,
          "noValue": "0",
          "thresholds": {
            "mode": "absolute",
            "steps": [
              {
                "color": "dark-red",
                "value": null
              },
              {
                "color": "dark-yellow",
                "value": 3.6
              },
              {
                "color": "dark-green",
                "value": 3.8
              }
            ]
          },
          "unit": "volt"
        },
        "overrides": []
      },
      "gridPos": {
        "h": 7,
        "w": 12,
        "x": 12,
        "y": 7
      },
      "id": 4,
      "options": {
        "orientation": "auto",
        "reduceOptions": {
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        },
        "showThresholdLabels": false,
        "showThresholdMarkers": true
      },
      "pluginVersion": "7.1.3",
      "targets": [
        {
          "alias": "Sensor $tag_SensorID",
          "groupBy": [
            {
              "params": [
                "$__interval"
              ],
              "type": "time"
            },
            {
              "params": [
                "SensorID"
              ],
              "type": "tag"
            }
          ],
          "measurement": "WSN",
          "orderByTime": "ASC",
          "policy": "autogen",
          "query": "SELECT last(\"Value\") FROM \"autogen\".\"Battery\" WHERE (\"SensorID\" =~ /^$Sensors$/) AND $timeFilter GROUP BY time($__interval), \"SensorID\"",
          "rawQuery": false,
          "refId": "A",
          "resultFormat": "time_series",
          "select": [
            [
              {
                "params": [
                  "Battery"
                ],
                "type": "field"
              },
              {
                "params": [],
                "type": "last"
              }
            ]
          ],
          "tags": [
            {
              "key": "SensorID",
              "operator": "=~",
              "value": "/^$Sensors$/"
            }
          ]
        }
      ],
      "timeFrom": null,
      "timeShift": null,
      "title": "Battery",
      "type": "gauge"
    },
    {
      "aliasColors": {},
      "bars": false,
      "dashLength": 10,
      "dashes": false,
      "datasource": null,
      "fieldConfig": {
        "defaults": {
          "custom": {}
        },
        "overrides": []
      },
      "fill": 1,
      "fillGradient": 0,
      "gridPos": {
        "h": 8,
        "w": 12,
        "x": 0,
        "y": 14
      },
      "hiddenSeries": false,
      "id": 8,
      "legend": {
        "avg": false,
        "current": false,
        "max": false,
        "min": false,
        "show": true,
        "total": false,
        "values": false
      },
      "lines": true,
      "linewidth": 1,
      "nullPointMode": "null",
      "percentage": false,
      "pluginVersion": "7.1.3",
      "pointradius": 2,
      "points": false,
      "renderer": "flot",
      "seriesOverrides": [],
      "spaceLength": 10,
      "stack": false,
      "steppedLine": false,
      "targets": [
        {
          "groupBy": [
            {
              "params": [
                "$__interval"
              ],
              "type": "time"
            },
            {
              "params": [
                "null"
              ],
              "type": "fill"
            }
          ],
          "measurement": "WSN",
          "orderByTime": "ASC",
          "policy": "default",
          "refId": "A",
          "resultFormat": "time_series",
          "select": [
            [
              {
                "params": [
                  "WindSpeed"
                ],
                "type": "field"
              },
              {
                "params": [],
                "type": "mean"
              }
            ]
          ],
          "tags": [
            {
              "key": "SensorID",
              "operator": "=",
              "value": "101"
            }
          ]
        }
      ],
      "thresholds": [],
      "timeFrom": null,
      "timeRegions": [],
      "timeShift": null,
      "title": "Wind Speed",
      "tooltip": {
        "shared": true,
        "sort": 0,
        "value_type": "individual"
      },
      "type": "graph",
      "xaxis": {
        "buckets": null,
        "mode": "time",
        "name": null,
        "show": true,
        "values": []
      },
      "yaxes": [
        {
          "format": "short",
          "label": null,
          "logBase": 1,
          "max": null,
          "min": null,
          "show": true
        },
        {
          "format": "short",
          "label": null,
          "logBase": 1,
          "max": null,
          "min": null,
          "show": true
        }
      ],
      "yaxis": {
        "align": false,
        "alignLevel": null
      }
    },
    {
      "aliasColors": {},
      "bars": false,
      "dashLength": 10,
      "dashes": false,
      "datasource": null,
      "fieldConfig": {
        "defaults": {
          "custom": {}
        },
        "overrides": []
      },
      "fill": 1,
      "fillGradient": 0,
      "gridPos": {
        "h": 8,
        "w": 12,
        "x": 12,
        "y": 14
      },
      "hiddenSeries": false,
      "id": 10,
      "legend": {
        "avg": false,
        "current": false,
        "max": false,
        "min": false,
        "show": true,
        "total": false,
        "values": false
      },
      "lines": true,
      "linewidth": 1,
      "nullPointMode": "null",
      "percentage": false,
      "pluginVersion": "7.1.3",
      "pointradius": 2,
      "points": false,
      "renderer": "flot",
      "seriesOverrides": [],
      "spaceLength": 10,
      "stack": false,
      "steppedLine": false,
      "targets": [
        {
          "groupBy": [
            {
              "params": [
                "$__interval"
              ],
              "type": "time"
            },
            {
              "params": [
                "null"
              ],
              "type": "fill"
            }
          ],
          "measurement": "WSN",
          "orderByTime": "ASC",
          "policy": "default",
          "refId": "A",
          "resultFormat": "time_series",
          "select": [
            [
              {
                "params": [
                  "WindDirection"
                ],
                "type": "field"
              },
              {
                "params": [],
                "type": "mean"
              }
            ]
          ],
          "tags": [
            {
              "key": "SensorID",
              "operator": "=",
              "value": "101"
            }
          ]
        }
      ],
      "thresholds": [],
      "timeFrom": null,
      "timeRegions": [],
      "timeShift": null,
      "title": "Wind Direction",
      "tooltip": {
        "shared": true,
        "sort": 0,
        "value_type": "individual"
      },
      "type": "graph",
      "xaxis": {
        "buckets": null,
        "mode": "time",
        "name": null,
        "show": true,
        "values": []
      },
      "yaxes": [
        {
          "format": "short",
          "label": null,
          "logBase": 1,
          "max": null,
          "min": null,
          "show": true
        },
        {
          "format": "short",
          "label": null,
          "logBase": 1,
          "max": null,
          "min": null,
          "show": true
        }
      ],
      "yaxis": {
        "align": false,
        "alignLevel": null
      }
    }
  ],
  "refresh": "5s",
  "schemaVersion": 26,
  "style": "dark",
  "tags": [],
  "templating": {
    "list": [
      {
        "allValue": null,
        "current": {
          "selected": false,
          "text": "All",
          "value": "$__all"
        },
        "datasource": "InfluxDB",
        "definition": "SHOW TAG VALUES WITH KEY = \"SensorID\"",
        "hide": 0,
        "includeAll": true,
        "label": "Sensors List",
        "multi": true,
        "name": "Sensors",
        "options": [],
        "query": "SHOW TAG VALUES WITH KEY = \"SensorID\"",
        "refresh": 2,
        "regex": "",
        "skipUrlSync": false,
        "sort": 0,
        "tagValuesQuery": "",
        "tags": [],
        "tagsQuery": "",
        "type": "query",
        "useTags": false
      }
    ]
  },
  "time": {
    "from": "now-5m",
    "to": "now"
  },
  "timepicker": {
    "hidden": false,
    "refresh_intervals": [
      "5s",
      "10s",
      "30s",
      "1m",
      "5m"
    ]
  },
  "timezone": "",
  "title": "WSN",
  "uid": "ExxTMzIGz",
  "version": 6
}
```

Then the system is ready to go.

## Add user in Grafana

Login the Grafana dashboard as Admin user.

Go to Configuration->User->Invite, enter the user name or email, set the Role as Viewer, then press `Submit`.

For more information, please visit their official website https://grafana.com/