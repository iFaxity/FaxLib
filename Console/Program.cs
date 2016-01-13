using System;
using FaxLib.Net;
using System.Threading;
using System.IO;

namespace Consoler {
    static class ConsoleLog {
        static StreamWriter logOutput;
        static FileStream fileOutput;

        static bool enabled = false;
        public static bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
            }
        }

        public static void Enable(string fileName) {
            fileOutput = new FileStream(fileName, FileMode.Create);
            logOutput = new StreamWriter(fileOutput, Console.OutputEncoding);
            Console.SetOut(logOutput);
            enabled = true;
        }
        public static void Disable() {
            Console.OpenStandardOutput();
            enabled = false;
        }
    }

    class Program {     
        // NetHost & Node Testing
        static void Main(string[] args) {
            var str = Console.ReadLine().ToLower();

            if (str == "server") {
                var server = new NetHost(8888);
                server.ClientConnecting += OnCon;
                server.MessageReceived += OnMsg;
                server.ClientDisconnecting += OnDis;

                Console.WriteLine("Server Started\n");
                while(true) {
                    Console.Write("Server > ");
                    var msg = Console.ReadLine();
                    if (msg.ToLower() == "close")
                        break;
                    else
                        server.Broadcast(msg);
                }
            }
            else if (str == "client") {
                var client = new NetNode();
                client.Disconnecting += (sender, e) => Console.WriteLine("Disconnected from server");
                client.MessageReceived += (sender, e) => Console.WriteLine("Server says: " + e.Message);
                Console.WriteLine("Client Started!\n");
                if (client.Connect("127.0.0.1", 8888)) {
                    while (true) {
                        Console.Write("Client > ");
                        var msg = Console.ReadLine();
                        if (msg.ToLower() == "loop") {
                            while (true) {
                                Console.WriteLine("Sending heartbeat.\n");
                                client.SendMessage("Heartbeat");
                                Thread.Sleep(2000);
                            }
                        }
                        else if (msg.ToLower() == "close")
                            break;
                        else
                            client.SendMessage(msg);
                    }
                }
                else
                    Console.WriteLine("Can't connect to server!");
            }
            else if (str == "ping") {
                var ips = Net.FindIPs("192.168.1.", 100, 40);
                foreach (var ip in ips)
                    Console.WriteLine(ip + " Found");
                Console.WriteLine("End");
                Console.ReadLine();
            }
            else
                Console.WriteLine("\nNOT SET!");
            Console.WriteLine("\nShutting Down...");
            Console.ReadLine();
        }

        static void OnDis(object sender, EventArgs e) {
            var client = (NetClient)sender;
            Console.WriteLine("Client " + client.Name + " Disconnected.");
        }
        static void OnMsg(object sender, NetMessageEvent e) {
            var client = (NetClient)sender;
            Console.WriteLine("\"" + e.Message + "\" count " + e.Message.Length + " from " + client.Name + ".");
        }
        static void OnCon(object sender, EventArgs e) {
            var client = (NetClient)sender;
            Console.WriteLine("Client with the ID " + client.Name + " Connected.");
        }

        /* Throttle
        static void Main() {
            var time = DateTime.Now;
            var throttle = Throttle.DownloadToFile("http://localhost/file.mp4", "lel2.mp4", 100);
            throttle.ProgressUpdated += (sender, e) => {
                Console.WriteLine(string.Format("{0:N2} % @ {1:N2} Kbps", e.Progress, e.BytesReceived / 1024d));
            };
            throttle.DownloadCompleted += (sender, e) => {
                Console.WriteLine(string.Format("Execution Time '{0:mm:ss}'", DateTime.Now.Subtract(time).TotalMinutes));
                Console.WriteLine("Completed");
                using (var fileStream = File.Create("lel.mp4"))
                {
                    throttle.Stream.Seek(0, SeekOrigin.Begin);
                    throttle.Stream.CopyTo(fileStream);
                }
                Console.WriteLine("Saved file to: lel.mp4");
            };
            Thread.Sleep(2000);
            Console.WriteLine("ACCELERATING!");
            throttle.Speed = 300;
            Console.ReadLine();
        }*/
    }
}