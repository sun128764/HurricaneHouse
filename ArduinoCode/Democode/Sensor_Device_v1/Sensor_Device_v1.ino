

// parameters
const unsigned int NetworkID = 5001; //
const unsigned int BoardID = 2; //
const unsigned int BoardType = 4; // 1.Coordinatorn (cellular/gps/main), 2. Anemometer, 3. Humidity., 4. regular

const unsigned int Fs = 50; // sample reading per second (per sensor)
const unsigned int nSensors = 5;
const unsigned int nPaylad = 40; // for each transaction (per sensor)

int loop_time = 1000 / Fs; // in milli-second

//////////////////////////////////////////////////////////////////////

// define XBee
#include <Arduino.h>   // required before wiring_private.h
#include "wiring_private.h" // pinPeripheral() function
#include <XBee.h>
//#define MAX_FRAME_DATA_SIZE 104 // bytes
unsigned int XBeeRX = 3; // D3
unsigned int XBeeTX = 4; // D4
//    D6: Clear to send
//    D5: RX signal ignal strength of last transmission
//    D7: energy control
Uart Serial2 (&sercom2, XBeeRX, XBeeTX, SERCOM_RX_PAD_1, UART_TX_PAD_0);
void SERCOM2_Handler(){Serial2.IrqHandler();}
XBee xbee = XBee();

// define buffers
#include <stdio.h>
#include <string.h>
#include <inttypes.h>

unsigned int green = 25; unsigned int blue = 26; // Green and blue LEDs' pins
void led(unsigned int color, unsigned int n){ for(unsigned int i=0; i<n; i++){ digitalWrite(color, LOW); delay(1); digitalWrite(color, HIGH);}}

const unsigned int a0n = 2550; // internal buffer size
unsigned int a0i = 0; // current writing index
unsigned int a0j = 0; // current sending index
int adc = 1023 / 255 ; // convert 10bit resolution to 8bit.
uint8_t a0[nSensors][a0n]; // 2D circular buffer0
// OR
//unsigned int adc = 1 ; // keep the 10 bit resolution.
//uint16_t a0[nSensors][a0n]; // 2D buffer0 for reading sensors, 5 is the number of sensors


// Define multithreading (so the serial sending does not interrept the sampling)
#include "Thread.h"
Thread ReadThread = Thread();
Thread SendThread = Thread();

void ReadSample(){
// writer step
a0i++;
a0i = a0i % a0n;
// read common sensors
a0[0][a0i] = analogRead(A0) / adc;  // Temperature. output [2.7V - 3.3V]% +/- 1.5% (2.18PSI ~ 16.68PSI  or 15kPa ~ 115kPa)
a0[1][a0i] = analogRead(A1) / adc; // Battery. float dC = ( 85 + 40 ) / Clev; // (C) //  v = -40 + ( i + Vmax) * dC / dV;
a0[2][a0i] =  analogRead(A5) / adc; // Pressure. 0-100%

// read specalized sensors
switch (BoardType) {
  case 3: // Type 3 Humidity
  {
    a0[3][a0i]  = analogRead(A3) / adc;  // Humidity. output [00.0-100.0]% +/- 3% (RH)
    a0[4][a0i] = analogRead(A4); // nothing
  } break;
  case 2: // Type 2 Anemometer
  {
    a0[3][a0i] = analogRead(A3); // Anemometer1 Wind Speed
    a0[4][a0i] = analogRead(A4); // Anemometer2 Wind Direction
  } break;
}
}

void SendSample(){
switch (BoardType) {
  case 1: // Type 1 cellular (Coordinator)
  {
    // Forward received data to Serial (PC's USP)
    for (unsigned int j = 0; j < nSensors; j++){
    if(Serial2.available()){
    while(Serial2.available()){
      char r = Serial2.read();
      if (r == (char) 255) {
        Serial.println();
      } else {
      Serial.printf( "%03d" , r );
      }
    }
    }
    delay(10);
    }
    
    // Python: read the USB serial commuunication 
    // example: https://gist.github.com/projectweekend/1fae5a8cf2a5b9282f3d
    
  } break;
  default:{ // Sensors Not Type 1
    
//    Serial2.write("hello!123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789EndHello");
//    Serial.println( a0i );

    for(unsigned int j = 0; j < nSensors ; j++){
      Serial2.write( (char) 255 ); // Send the start byte
      for(unsigned int jj = 0; jj < nPaylad ; jj++){
        Serial2.write( a0[j][ (a0j+jj)%a0n] );

//        // Debug
//        Serial.printf( "%03d" , a0[j][(jj%a0n] );

//        // if using 16bit ADC resolution,
//        Serial2.write( ( a0[j][ (a0j+jj)%a0n ] >> 8   ) & 0xFF ); // Send upper char
//        Serial2.write( ( a0[j][ (a0j+jj)%a0n ] & 0xFF ) ); // Send lower char
      }
//  Serial.printf("\n");
    delay(10); //
    }
    
  }
}
}


void setup() {
  Serial.begin(9600);
  pinMode(A0, INPUT);
  pinMode(A1, INPUT);
  pinMode(A3, INPUT);
  pinMode(A4, INPUT);
  pinMode(A5, INPUT);
  pinMode(green, OUTPUT);
  pinMode(blue, OUTPUT);
  
  // Default ADC's resolution of the Atmel's ATSAM21 is 10bit (reading range from 0 to 1023).
  analogReadResolution(10);

  // multithreading
  ReadThread.setInterval(loop_time);
  ReadThread.onRun(ReadSample);
  SendThread.setInterval(loop_time);
  SendThread.onRun(SendSample);

  
   // XBee3:
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
   if (BoardType==1){
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
   Serial2.print("ATNP\r" ); // read the max payload of a packet. (determined from encryption and type of coommunication)
   delay(100);
   while(Serial2.available()){ Serial.write( Serial2.read() );} // display response

}


void loop() { // while true
  
if(ReadThread.shouldRun()){ // if the time interval has passed, measure a sample reading.
ReadThread.run();
}

unsigned int a = ( a0i - a0j + a0n ) % a0n; // if a payload block is ready, send it.
if( a >= nPaylad ){
SendThread.run();
a0j = a0j + nPaylad;
}

delay(1); // for stability



// debug
//for (int j=0;j<a0i+1;j++){ Serial.printf(  "%03d" , a0[j]  ); }
//Serial.println();

// XCTU https://www.digi.com/products/embedded-systems/digi-xbee/digi-xbee-tools/xctu#productsupport-utilities

}
