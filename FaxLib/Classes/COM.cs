using System;
using System.Collections.Generic;
using System.Linq;

using System.IO.Ports;

namespace FaxLib.COM {
    /// <summary>
    /// Custom data event 
    /// </summary>
    public class COMEvent : EventArgs {
        /// <summary>
        /// Data that was received
        /// </summary>
        public string Data { get; private set; }

        /// <summary>
        /// Creates a new empty instance of DataEvent class
        /// </summary>
        public COMEvent() { }
        /// <summary>
        /// Creates a new instance of DataEvent class
        /// </summary>
        /// <param name="data">The data that was received</param>
        public COMEvent(string data) {
            Data = data;
        }
    }

    /// <summary>
    /// A class that simplyfies writing and receiving data from a serial port
    /// </summary>
    public class COMPort {
        #region Properties & Fields
        /// <summary>
        /// SerialPort variable
        /// </summary>
        SerialPort _port;

        /// <summary>
        /// Checks if the serial COM port is open
        /// </summary>
        public bool IsOpen {
            get {
                return _port.IsOpen;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Event fires when data was received
        /// </summary>
        public static event EventHandler<COMEvent> DataReceived;
        /// <summary>
        /// Event triggerer for data being recived by the COM port
        /// </summary>
        /// <param name="port">COM Port class</param>
        /// <param name="data">Data that was received</param>
        static void OnDataReceived(COMPort port, string data) {
            if(DataReceived != null)
                DataReceived(port, new COMEvent(data));
        }
        #endregion

        /// <summary>
        /// Gets all opened Serial COM Ports
        /// </summary>
        public static List<string> GetAllPorts() {
            return SerialPort.GetPortNames().ToList();
        }
        /// <summary>
        /// Opens a Serial COM Port
        /// </summary>
        /// <param name="port">Port name. Use <see cref="GetAllPorts()"/> to get all ports</param>
        public static COMPort Open(string port) {
            var com = new COMPort();

            com._port = new SerialPort(port);
            if(com._port.IsOpen == false) //if not open, open the port
                com._port.Open();

            com._port.DataReceived += (sender, e) => OnDataReceived(com, com._port.ReadExisting());

            return com;
        }

        /// <summary>
        /// Closes the serial COM port connection
        /// </summary>
        public void Close() {
            _port.Close();
        }
        /// <summary>
        /// Sends data to the serial COM port
        /// </summary>
        /// <param name="data">Data to send as string</param>
        public void Send(string data) {
            _port.Write(data);
        }
        /// <summary>
        /// Sends data to the serial COM port
        /// </summary>
        /// <param name="data">Data to send as byte array</param>
        public void Send(byte[] data, int offset = 0) {
            _port.Write(data, offset, data.Length - offset);
        }
    }
}
