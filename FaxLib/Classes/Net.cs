﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

// Edit made by Faxity as a part of FaxLib as of version 1.2
namespace FaxLib.Net {
    /// <summary>
    /// Class for all Net/Socket functions inside of FaxLib
    /// </summary>
    [DebuggerStepThrough]
    public class Net {
        #region Pinger
        private static int _instances = 0;
        private static object _lock = new object();
        private static List<string> _foundIP = new List<string>();

        public static List<string> FindIPs(string baseIP, int timeOut, int ttl) {
            _foundIP.Clear();
            var op = new PingOptions(ttl, true);

            var data = new byte[10];
            new Random().NextBytes(data);
            var pingers = Pingers(256);

            for (int i = 0; i < pingers.Count; i++) {
                lock (_lock)
                    _instances++;
                pingers[i].SendAsync(baseIP + i, timeOut, data, op);
            }

            while (_instances > 0) { }

            pingers.Clear();
            return _foundIP;
        }
        private static List<Ping> Pingers(int count) {
            var list = new List<Ping>();
            for (int i = 2; i < count; i++) {
                var p = new Ping();
                p.PingCompleted += delegate (object o, PingCompletedEventArgs e) {
                    lock (_lock)
                        _instances--;
                    if (e.Reply.Status == IPStatus.Success)
                        _foundIP.Add(e.Reply.Address.ToString());
                };
                list.Add(p);
            }
            return list;
        }
        #endregion

        /// <summary>
        /// Pings a host (server or client).
        /// </summary>
        /// <param name="host">Host ip</param>
        public static PingReply GetPing(string host) {
            return new Ping().Send(host, 3000, new byte[32]);
        }
        /// <summary>
        /// Pings a host (server or client).
        /// </summary>
        /// <param name="host">Host ip</param>
        /// <param name="timeout">Timespan of timeout</param>
        public static PingReply GetPing(string host, TimeSpan timeout) {
            return new Ping().Send(host, timeout.Milliseconds, new byte[32]);
        }
        /// <summary>
        /// Pings a host (server or client).
        /// </summary>
        /// <param name="host">Host ip</param>
        /// <param name="timeout">Timespan of timeout</param>
        /// <param name="packetSize">The packet size</param>
        public static PingReply GetPing(string host, TimeSpan timeout, int packetSize) {
            return new Ping().Send(host, timeout.Milliseconds, new byte[packetSize]);
        }

        public static List<string> PortScan() {
            var client = new TcpClient("", 0);
            return null;
        }

        public static string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return null;
        }
    }

    /// <summary>
    /// Event trigger for messages
    /// </summary>
    public class NetMessageEvent : EventArgs {
        public string Message { get; private set; }
        public NetMessageEvent(string message) {
            Message = message;
        }
    }

    /// <summary>
    /// Class for storing client info for <see cref="NetHost"/>
    /// </summary>
    [DebuggerStepThrough]
    public class NetClient {
        #region Properties
        public string IP { get; private set; }
        public string Name { get; private set; }
        public int Port { get; private set; }
        public TcpClient Client { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Event that fires everytime a message is received
        /// </summary>
        public event EventHandler<NetMessageEvent> MessageReceived;
        protected virtual void OnMessageReceived(NetMessageEvent e) {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        /// <summary>
        /// Event that fires when this client disconnects
        /// </summary>
        internal event EventHandler<EventArgs> Disconnecting;
        protected virtual void OnDisconnecting() {
            if (Disconnecting != null)
                Disconnecting(this, new EventArgs());
        }
        #endregion

        internal NetClient(TcpClient client) : this(client, null) { }
        internal NetClient(TcpClient client, string name) {
            // Set properties & start the listener
            Name = name;
            Client = client;

            var endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            IP = endPoint.Address.ToString();
            Port = endPoint.Port;

            Task.Factory.StartNew(Listen);
        }

        internal void SendMessage(string message) {
            if (Client.Connected) {
                var stream = Client.GetStream();
                var buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
            }
        }
        void Listen() {
            // Start listener
            byte[] buffer;
            var stream = Client.GetStream();

            while (Client.Connected) {
                // Prevent the listener from crashing
                try {
                    buffer = new byte[Client.ReceiveBufferSize];

                    // Message has successfully been received
                    if (stream.Read(buffer, 0, Client.ReceiveBufferSize) > 0)
                        OnMessageReceived(new NetMessageEvent(Encoding.UTF8.GetString(buffer, 0, buffer.Length)));
                }
                catch { break; }
            }

            OnDisconnecting();
            Client.Close();
        }
    }

    /// <summary>
    /// Class for hosting a <see cref="NetNode"/> socket session
    /// </summary>
    [DebuggerStepThrough]
    public class NetHost {
        #region Properties & Fields
        /// <summary>
        /// Lists all the <see cref="NetClient"/>'s connected to this host
        /// </summary>
        public List<NetClient> Clients { get; set; }
        /// <summary>
        /// Is the host listening for any clients?
        /// </summary>
        public bool Listening { get; set; }

        TcpListener _tcpListener;
        #endregion

        #region Events
        /// <summary>
        /// Event that fires everytime a message is received
        /// </summary>
        public event EventHandler<NetMessageEvent> MessageReceived;
        protected virtual void OnMessageReceived(NetMessageEvent e) {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }
        /// <summary>
        /// Event that fires everytime a new client connects
        /// </summary>
        public event EventHandler<EventArgs> ClientConnecting;
        protected virtual void OnClientConnecting(object sender) {
            if (ClientConnecting != null)
                ClientConnecting(sender, EventArgs.Empty);
        }
        /// <summary>
        /// Event that fires everytime a client disconnects
        /// </summary>
        public event EventHandler<EventArgs> ClientDisconnecting;
        protected virtual void OnClientDisconnecting(object sender) {
            if (ClientDisconnecting != null)
                ClientDisconnecting(sender, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// Starts a Tcp host for <see cref="NetNode"/>'s
        /// </summary>
        /// <param name="port">TCP port to bind to</param>
        public NetHost(int port) {
            Init(IPAddress.Any, port);
        }
        /// <summary>
        /// Starts a Tcp host for <see cref="NetNode"/>'s
        /// </summary>
        /// <param name="port">TCP port to bind to</param>
        /// <param name="ipAddr">IP address to bind server to</param>
        public NetHost(int port, IPAddress ipAddr) {
            Init(ipAddr, port);
        }

        /// <summary>
        /// Sends a message to all the connected clients
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(string message) {
            foreach (var client in Clients)
                client.SendMessage(message);
        }
        /// <summary>
        /// Sends a message to one of the connected clients
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="message"></param>
        public bool SendMessage(string clientName, string message) {
            foreach (var client in Clients) {
                if (client.Name == clientName) {
                    client.SendMessage(message);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Removes a client from the server
        /// </summary>
        /// <param name="clientID"></param>
        public bool RemoveClient(string clientName) {
            try {
                var client = Clients.Find(x => x.Name == clientName);
                client.Client.Close();
                Clients.Remove(client);
                return true;
            }
            catch {
                return false;
            }
        }

        void Init(IPAddress ipAddr, int port) {
            Clients = new List<NetClient>();
            _tcpListener = new TcpListener(ipAddr, port);
            _tcpListener.Start();
            // Start thread listener
            Task.Factory.StartNew(Listen);
        }

        void Listen() {
            Listening = true;
            while (Listening) {
                if (_tcpListener.Pending()) {
                    var client = new NetClient(_tcpListener.AcceptTcpClient());
                    OnClientConnecting(client);

                    // Attach events and add to Clients list
                    client.MessageReceived += MessageReceived;
                    client.Disconnecting += Disconnecting;
                    Clients.Add(client);
                }
            }
        }
        void Disconnecting(object sender, EventArgs e) {
            // Remove the disconnected client
            OnClientDisconnecting(sender);
            Clients.Remove((NetClient)sender);
        }
    }

    /// <summary>
    /// Class to connect to a <see cref="NetHost"/>
    /// </summary>
    [DebuggerStepThrough]
    public class NetNode {
        #region Properties
        /// <summary>
        /// The amount of attempts to connect to the host
        /// </summary>
        public int Attempts { get; set; }
        /// <summary>
        /// Gets if the client is connected to a host
        /// </summary>
        public bool Connected {
            get {
                return Client != null ? Client.Connected : false;
            }
        }
        /// <summary>
        /// TcpClient associated to this NetNode
        /// </summary>
        public TcpClient Client { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Event that fires everytime a message is received
        /// </summary>
        public event EventHandler<NetMessageEvent> MessageReceived;
        protected virtual void OnMessageReceived(NetMessageEvent e) {
            if (MessageReceived != null)
                MessageReceived(this, e);
        }

        /// <summary>
        /// Event that fires when this client disconnects
        /// </summary>
        public event EventHandler<EventArgs> Disconnecting;
        protected virtual void OnDisconnecting() {
            if (Disconnecting != null)
                Disconnecting(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// Makes a new node to connect to a host
        /// </summary>
        public NetNode(int attempts = 3) {
            Attempts = attempts;
        }

        /// <summary>
        /// Attempt to connect to a host
        /// </summary>
        /// <param name="hostname">Hostname or IPAddress to connect to</param>
        /// <param name="port">Host port to connect to</param>
        public bool Connect(string hostname, int port) {
            for (var i = 0; i < Attempts; i++) {
                Client = new TcpClient();
                Client.Connect(hostname, port);
                Task.Factory.StartNew(Listen);
                return Client.Connected;
            }
            return false;
        }
        /// <summary>
        /// Attempt to disconnect from the connected host
        /// </summary>
        public bool Disconnect() {
            if (Client.Connected)
                Client.Close();
            return !Client.Connected;
        }
        /// <summary>
        /// Sends a message to the connected host
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(string message) {
            if (Client.Connected) {
                var stream = Client.GetStream();
                var buffer = Encoding.UTF8.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        void Listen() {
            // Start listener
            byte[] buffer;
            var stream = Client.GetStream();

            while (Client.Connected) {
                // Prevent the listener from crashing
                try {
                    buffer = new byte[Client.ReceiveBufferSize];

                    // Message has successfully been received
                    if (stream.Read(buffer, 0, Client.ReceiveBufferSize) > 0)
                        OnMessageReceived(new NetMessageEvent(Encoding.UTF8.GetString(buffer, 0, buffer.Length)));
                }
                catch { break; }
            }

            OnDisconnecting();
            Client.Close();
        }
    }

    public class ThrottleProgressEventArgs : EventArgs {
        /// <summary>
        /// Amount of bytes received
        /// </summary>
        public long BytesReceived { get; private set; }
        /// <summary>
        /// Amount of total bytes to receive
        /// </summary>
        public long BytesTotal { get; private set; }
        /// <summary>
        /// Download progress in percent
        /// </summary>
        public float Progress { get; private set; }
        public ThrottleProgressEventArgs() { }
        public ThrottleProgressEventArgs(int byteRec, long byteTot, float progress) {
            BytesReceived = byteRec;
            BytesTotal = byteTot;
            Progress = progress;
        }
    }

    //[DebuggerStepThrough]
    /*public class Throttle
    {
        #region Events
        public event EventHandler DownloadCompleted;
        void OnDownloadCompleted()
        {
            if (DownloadCompleted != null)
                DownloadCompleted(null, new EventArgs());
        }
        public event EventHandler<ThrottleProgressEventArgs> ProgressUpdated;
        void OnProgressUpdated(int cur, long rec, long tot)
        {
            if (ProgressUpdated != null)
                ProgressUpdated(null, new ThrottleProgressEventArgs(cur, tot, ((float)rec / tot) * 100));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Amount of data to read every second in kilobytes
        /// </summary>
        public int Speed { get; set; }
        public string Url { get; set; }
        public string Path { get; private set; }
        MemoryStream ms;
        FileStream fs;
        #endregion
        
        Throttle(string url, int speed = 100, string path = null)
        {
            Url = url;
            Speed = speed;
            Path = path;
            if (path != null)
                fs = File.Create(path + ".tmp");
            else
                ms = new MemoryStream();   
        }
        public static Throttle DownloadToFile(string url, string path, int speed = 100)
        {
            // Create a new Throttler
            var throttle = new Throttle(url, speed, path);
            // Create the request and run it async
            Task.Run(() => throttle.Request(CreateRequest(url)));
            // Return this object
            return throttle;
        }
        public static Throttle DownloadToStream(string url, int speed = 100)
        {
            // Create a new Throttler
            var throttle = new Throttle(url, speed);
            // Create the request and run it async
            Task.Run(() => throttle.Request(CreateRequest(url)));
            // Return this object
            return throttle;
        }
        static HttpWebRequest CreateRequest(string url, string method = "GET")
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.AllowReadStreamBuffering = false;
            request.Method = method.ToUpper();
            return request;
        }
        async void Request(HttpWebRequest request)
        {
            Stream stream;
            if (fs != null) stream = fs;
            else stream = ms;

            var response = (HttpWebResponse)request.GetResponse();
            long total = response.ContentLength;
            // Read stream into memory
            using (var rs = response.GetResponseStream())
            {
                byte[] buffer = new byte[(Speed / 10) * 1024];
                int read;
                while ((read = rs.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, read);
                    OnProgressUpdated(read * 10, stream.Length, total);
                    Console.WriteLine(read + " of " + ((Speed / 10) * 1024));
                    //buffer = new byte[Speed * 1024];
                    // Interval each second
                    await Task.Delay(1000 / 10);
                }
                // Completed remove .tmp extension if OK
                if (fs != null)
                    File.Move(Path, Path.Substring(0, Path.Length - 4));
                // Trigger completed event
                OnDownloadCompleted();
            }
        }
    }*/

    public class Throttle {
        #region Fields
        MemoryStream _ms;
        FileStream _fs;
        Task _task;
        string _file;
        string _url;
        long _maxBps;
        long _byteCount = 0;
        long _start = Environment.TickCount;
        bool _active = false;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the maximum bytes per second that can be transferred through the base stream.
        /// </summary>
        /// <value>The maximum bytes per second.</value>
        public long MaxBps {
            get { return _maxBps; }
            set {
                if (_maxBps != value) {
                    _maxBps = value;
                    Reset();
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new Throttled Downloader
        /// </summary>
        /// <param name="max">The maximum bytes per second</param>
        /// <param name="path">File to write to. If null then it will be saved to memory instead</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="baseStream"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="maximumBytesPerSecond"/> is a negative value.</exception>
        public Throttle(string url, long max = 0, string filePath = null) {
            if (filePath != null) {
                _file = filePath + ".tmp";
                _fs = File.Create(_file);
            }
            else
                _ms = new MemoryStream();
            _url = url;
            _maxBps = max < 0 ? 0 : max;
            _task = Task.Factory.StartNew(Init);
        }
        #endregion

        #region Public Methods
        public void Start() {

        }

        public void Pause() {

        }

        public void Dispose() {
            if (_fs != null) {
                _fs.Dispose();
                if (File.Exists(_file))
                    File.Delete(_file);
            }
            else if (_ms != null)
                _ms.Dispose();
        }
        #endregion

        #region Protected Methods
        // Initializes the throttler
        protected void Init() {
            var req = WebRequest.CreateHttp(_url);
            var res = req.GetResponse();
            var resStream = res.GetResponseStream();
            while (true) {
                //Throttler();
            }
        }

        /// <summary>
        /// Throttles for the specified buffer size in bytes.
        /// </summary>
        /// <param name="bufferSizeInBytes">The buffer size in bytes.</param>
        protected async void Throttler(int bufferSize) {
            // Make sure the buffer isn't empty.
            if (_maxBps <= 0 || bufferSize <= 0)
                return;

            _byteCount += bufferSize;
            long elapsed = Environment.TickCount - _start;

            if (elapsed > 0) {
                long bps = _byteCount * 1000L / elapsed;

                // If the bps are more then the maximum bps, try to throttle.
                if (bps > _maxBps) {
                    // Calculate the time to sleep.
                    long wakeElapsed = _byteCount * 1000L / _maxBps;
                    int toSleep = (int)(wakeElapsed - elapsed);

                    if (toSleep > 1) {
                        await Task.Delay(toSleep);
                        // Reset after sleep
                        Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Will reset the bytecount to 0 and reset the start time to the current time.
        /// </summary>
        protected void Reset() {
            // Only reset counters when a known history is available of more then 1 second.
            if (Environment.TickCount - _start > 1000) {
                _byteCount = 0;
                _start = Environment.TickCount;
            }
        }
        #endregion
    }
}
