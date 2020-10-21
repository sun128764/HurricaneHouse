// parameters
const unsigned int NetworkID = 5001; //
const unsigned int BoardID = 42; //
const unsigned int BoardType = 4; // 1.Coordinatorn (cellular/gps/main), 2. Anemometer, 3. Humidity., 4. regular

const unsigned int Fs = 50; // sample reading per second (per sensor)
const unsigned int nSensors = 5;
const unsigned int nPaylad = 40; // for each transaction (per sensor)


int loop_time = 1000 / Fs; // in milli-second

// define XBee
#include <Arduino.h>   // required before wiring_private.h
#include "wiring_private.h" // pinPeripheral() function
#include <XBee.h>
#include <ArduinoLowPower.h>
#include <SAMD_CRC32.h>
//#define MAX_FRAME_DATA_SIZE 104 // bytes
unsigned int XBeeRX = 3; // D3
unsigned int XBeeTX = 4; // D4
//    D6: Clear to send
//    D5: RX signal ignal strength of last transmission
//    D7: energy control
Uart Serial2 (&sercom2, XBeeRX, XBeeTX, SERCOM_RX_PAD_1, UART_TX_PAD_0);
void SERCOM2_Handler() {
  Serial2.IrqHandler();
}
XBee xbee = XBee();

// Create the CRC object
SAMD_CRC32 crc = SAMD_CRC32();
uint32_t crc_result = 0;
byte result[4];
uint32_t 32Mask = 255;

// define buffers
#include <stdio.h>
#include <string.h>
#include <inttypes.h>

unsigned int green = 25; unsigned int blue = 26; // Green and blue LEDs' pins
void led(unsigned int color, unsigned int n) {
  for (unsigned int i = 0; i < n; i++) {
    digitalWrite(color, LOW);
    delay(1);
    digitalWrite(color, HIGH);
  }
}

void TrySleep(int mil) {
  //Go sleep if Sleep command is found.
  if (!Serial2.available()) return;
  if (Serial2.find("Sleep")) {
    digitalWrite(7, HIGH); //Sleep Xbee.
    LowPower.deepSleep(mil); //Sleeo MCU.
    //Flush serial buffer.
    while (Serial2.available()) {
      Serial2.read();
    }
    digitalWrite(7, LOW); //Weakup Xbee
    delay(10000); //Wait 10s to recover Xbee network and read command.
  }
}

unsigned int Pressure[10];
int PressureIndex = 0;
int reading1;
int reading2;
int WindSpeed;
int WindDir;
int number;
bool lock;
unsigned long LastMillis;
unsigned long StartTime;
unsigned long CurrentMillis;
const unsigned long LMask = 255;
const unsigned int PMask = 255;
byte SerBuf[32];

void ForwardData() {
  while (Serial2.available()) {
    Serial.write( Serial2.read());
  }
}

void SendSample() {
  analogReadResolution(8);
  SerBuf[0] = BoardID;
  SerBuf[1] = BoardType;
  SerBuf[2] = analogRead(A0);//Temperature
  SerBuf[3] = analogRead(A1);//Battery
  //SerBuf[4] = analogRead(A3);//Extention A3
  SerBuf[4] = WindSpeed >> 8 & LMask;
  SerBuf[5] = WindSpeed & LMask;
  SerBuf[6] = WindDir >> 8 & LMask;
  SerBuf[7] = WindDir & LMask;
  //Mills() to Byte[]
  SerBuf[8] = (StartTime >> 24) & LMask;
  SerBuf[9] = (StartTime >> 16) & LMask;
  SerBuf[10] = (StartTime >> 8) & LMask;
  SerBuf[11] = (StartTime) & LMask;
  int i = 12;
  int totalP = 0;
  for (int j = 0; j < 10; j++) {
    SerBuf[i++] = (Pressure[j] >> 8) & PMask;
    SerBuf[i++] = (Pressure[j]) & PMask;
    totalP += Pressure[j];
  }
  Serial2.write(255);
  Serial2.write(SerBuf, 32);//Send data
  analogReadResolution(16);
  uint8_t status_code = crc.crc32(&SerBuf, sizeof(SerBuf), &crc_result); //Calculate CRC32
  for (int j = 0; j < 4; j++) { //Form CRC32 byte array
    result[j] = (crc_result >> j * 4) & 32Mask;
  }
  Serial2.write(result, 4); //Send CRC32
  Serial.printf("%d", (byte)crc_result);
  Serial.printf("SensorID: %d  ", BoardID);
  if (BoardType == 4) {
    Serial.printf("Pressure 16bits Reading: ");
    Serial.println(totalP / 10);
  }
  else {
    Serial.printf("WindSpeed 16bits Reading: %d ", WindSpeed);
    Serial.print("WindDirection 16bits Reading: ");
    Serial.println(WindDir);
  }
}

void setup() {
  delay(2000);
  Serial.begin(9600);
  Serial.print("start");
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
  pinMode(A5, INPUT);
  pinMode(green, OUTPUT);
  pinMode(blue, OUTPUT);
  pinMode(7, OUTPUT); //Xbee sleep control. Set to HIGH to enter sleep mode.
  digitalWrite(7, LOW);
  // Default ADC's resolution of the Atmel's ATSAM21 is 10bit (reading range from 0 to 1023).
  analogReadResolution(16);
  Serial2.begin(9600);
  pinPeripheral(XBeeRX, PIO_SERCOM_ALT);
  pinPeripheral(XBeeTX, PIO_SERCOM_ALT);
  xbee.setSerial(Serial2);
  delay(1100);
  Serial2.print("+++"); // start attention (AT) commands
  delay(1100);
  Serial2.printf("ATID %d\r", NetworkID ); // network PAN id
  delay(100);
  Serial2.printf("ATNI %d\r", BoardID ); // node id
  delay(100);
  if (BoardType == 1) {
    Serial2.print("ATCE 1\r"); // set cooridnator device
  } else {
    Serial2.print("ATCE 0\r"); // set router/endpoint device
  }
  delay(100);
  // DOTO: add encryption and check max payload available
  Serial2.print("ATEE 1\r" ); // enable encryption (secure communication)
  delay(100);
  Serial2.print("ATKY AAAAABBBBBCCCCCDDDDDEEEEEFFFFF12\r" ); // set encryption key 32 Hex digits
  delay(100);
  Serial2.print("ATNJ FF\r" ); // Set NJ to FF for unlimited join time.
  delay(100);
  Serial2.print("ATNW 1\r" ); // Set NW to 1 (1 minutes) to trigger a rejoin if the network is lost
  delay(100);
  Serial2.print("ATJV 1\r" ); // Set JV to 1 so that if you switch coordinators, your routers will search for the new one on startup.
  delay(100);
  Serial2.print("ATNP\r" ); // read the max payload of a packet. (determined from encryption and type of coommunication)
  delay(100);
  Serial2.print("ATWR\r" ); // Send a WR (Write) command to save the changes.
  delay(100);
  Serial2.print("ATSM 1\r" ); // Pin-controlled sleep mode = 1.
  delay(100);
  Serial2.print("ATCN\r" ); //Exit command mode.
  while (Serial2.available()) {
    Serial.write( Serial2.read() ); // display response
  }
  number = 0;
  LastMillis = millis();
  lock = true;
}

void loop() { // while true
  TrySleep(900000);//Sleep 15min if sleep command is received.
  CurrentMillis = millis();
  if ((CurrentMillis - LastMillis) >= 100 && !lock) {
    lock = true;
    StartTime = CurrentMillis;
  }
  switch (BoardType) {
    case 1:
      ForwardData();
      break;
    case 2:
      if ((CurrentMillis - LastMillis) <= 1000) return;
      while (number < 16384) {
        reading1 += analogRead(A4);
        reading2 += analogRead(A3);
        number++;
      }
      WindSpeed = reading1 >> 14;
      WindDir = reading2 >> 14;
      reading1 = 0;
      reading2 = 0;
      lock = false;
      number = 0;
      LastMillis = (CurrentMillis / 100) * 100; //Round
      SendSample();
      break;
    case 4:
      if (number < 4096 && lock) {
        reading1 += analogRead(A5);
        number++;
      }
      if (number >= 4096 && lock) {
        Pressure[PressureIndex++] = reading1 >> 12;
        reading1 = 0;
        lock = false;
        number = 0;
        LastMillis = (CurrentMillis / 100) * 100; //Round
      }
      if (PressureIndex > 9) {
        PressureIndex = 0;
        SendSample();
      }
    default:
      break;
  }
  // XCTU https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu#productsupport-utilities
}
