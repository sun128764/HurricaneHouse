using System.IO.Ports;
using System.Text;
using System.Windows;

namespace GUI
{
    /// <summary>
    /// Serial communitcation warp.Data received event is attached when call InitCOM.
    /// </summary>
    class SerialCOM
    {
        public SerialPort serialPort;//Serial object
        public bool IsOpen
        {
            get
            {
                return serialPort != null && serialPort.IsOpen;
            }
        }
        public bool InitCOM(string PortName, SerialDataReceivedEventHandler SerialPort_DataReceived)
        {
            serialPort = new SerialPort(PortName, 9600, Parity.None, 8, StopBits.One);
            serialPort.DataReceived += SerialPort_DataReceived;//DataReceived event delegate
            serialPort.ReceivedBytesThreshold = 31;
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
            catch { }
            if (serialPort.IsOpen)
            {
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
