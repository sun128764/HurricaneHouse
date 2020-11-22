using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Timers;
using System;

namespace MainProgram
{
    /// <summary>
    /// Serial communitcation warp.Data received event is attached when call InitCOM.
    /// </summary>
    internal class SerialCOM
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public SerialPort serialPort;//Serial object
        private static Timer aTimer;

        private void SetTimer()
        {
            // Create a timer with a 1 second interval.
            aTimer = new Timer(1000);
            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += CheckPort;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        private void CheckPort(Object source, ElapsedEventArgs e)
        {
            if (!IsOpen)
            {
                try
                {
                    serialPort.Open();
                }
                catch(Exception ex)
                {
                    Logger.Error(ex, "Cannot open serial port.");
                }
            }
        }

        public bool IsOpen
        {
            get
            {
                return serialPort != null && serialPort.IsOpen;
            }
        }

        public bool InitCOM(string PortName,int BuadRate, SerialDataReceivedEventHandler SerialPort_DataReceived)
        {
            serialPort = new SerialPort(PortName, BuadRate, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += SerialPort_DataReceived;//DataReceived event delegate
            serialPort.ReceivedBytesThreshold = 32;
            serialPort.RtsEnable = true;
            return OpenPort();
        }

        /// <summary>
        /// Open serial port
        /// </summary>
        /// <returns>If port open success.</returns>
        public bool OpenPort()
        {
            try//Avoid program crash when port open failed.
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Cannot open serial port.");
            }
            if (serialPort.IsOpen)
            {
                SetTimer();
                return true;
            }
            else
            {
                MessageBox.Show("Fial to open serial port!");
                return false;
            }
        }

        /// <summary>
        /// Send command via serial port
        /// </summary>
        /// <param name="CommandString">Command string</param>
        public void SendCommand(string CommandString)
        {
            byte[] WriteBuffer = Encoding.ASCII.GetBytes(CommandString);
            serialPort.Write(WriteBuffer, 0, WriteBuffer.Length);
        }

        public void Close()
        {
            serialPort.Close();
        }
    }
}