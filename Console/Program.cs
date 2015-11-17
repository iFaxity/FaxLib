using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Text;
using System.Configuration;
using System.Xml;

using FaxLib.Web;
using FaxLib.Net;
using FaxLib;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;

namespace Consoler {
    class Program {
        /* Send PHP Get
        static void Main(string[] args)
        {
            Console.BufferWidth = 500;

            //Login checker
            Console.Write("Write your username or email: ");
            var user = Console.ReadLine();
            Console.Write("Write your password: ");
            var pass = Console.ReadLine();

            var dict = new Dictionary<string, string>();
            dict.Add("login", user);
            dict.Add("password", pass);

            var result = Web.SendRequest(@"http://faxity.us.to/api/login.php", FaxLib.Web.HttpRequestMethod.POST, dict);

            Console.Write(result);
            Console.ReadKey();
        }*/

        //Throttle
        static void Main() {
            /*var time = DateTime.Now;
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
            Console.ReadLine();*/
        }

        /*NetHost & Node Testing
        static void Main(string[] args)
        {
            var str = Console.ReadLine();

            if (str.ToLower() == "server") {
                NetHost server = new NetHost(8888);
                server.ClientConnecting += OnCon;
                server.MessageReceived += OnMsg;
                server.ClientDisconnecting += OnDis;

                Console.WriteLine("Server Started\n");
                Console.ReadLine();
            }
            else if (str.Contains(" ") && str.Split(' ')[0].ToLower() == "client") {
                NetNode client = new NetNode();

                if (client.Connect("127.0.0.1", 8888, str.Split(' ')[1]))
                {
                    Console.WriteLine("Client Started!\n");
                    while (true)
                    {
                        Console.WriteLine("Sending Message.\n");
                        client.SendMessage("Heartbeat");
                        System.Threading.Thread.Sleep(2000);
                    }
                }
                else Console.WriteLine("Can't connect to server!");
            }
            else if(str == "ping") {
                var ips = Net.FindIPs("192.168.1.", 100, 40);
                foreach(string ip in ips)
                {
                    Console.WriteLine(ip + " Found");
                }
                Console.ReadLine();
            }
            else Console.WriteLine("\nNOT SET!");
            Console.WriteLine("\nShutting Down...");
            Console.ReadLine();
        }

        static void OnDis(object sender, EventArgs e) {
            var client = sender as NetClient;

            Console.WriteLine("Client " + client.ID + " Disconnected.");
        }
        static void OnMsg(object sender, NetMessageEvent e) {
            var client = sender as NetClient;
            Console.WriteLine("\"" + e.Message + "\" count " + e.Message.Length + " from " + client.ID + ".");
        }
        static void OnCon(object sender, EventArgs e) {
            var client = sender as NetClient;
            Console.WriteLine("Client with the ID " + client.ID + " Connected.");
        }*/

        /*Network Scanner
        static void Main(string[] args) {
            Console.WriteLine("Searching for available clients in the network\n");
            var ips = FaxLib.Net.Net.FindIPs("192.168.224.", 1000, 128);
            Console.WriteLine("IP Adresses found in network:\n");
            foreach (var ip in ips)
                Console.WriteLine(ip);
            Console.ReadLine();
        }*/

        /*Clipboard
        [STAThread]
        static void Main(string[] args) {
            var arr = System.Windows.Forms.Clipboard.GetText().Split('\n');
            //Formats the Enum
            for (int i = 0; i < arr.Length; i++)
                arr[i] = (arr[i].Substring(0, arr[i].IndexOf('=') - 1) + ",").Replace(" ", "");
            System.Windows.Forms.Clipboard.SetText(string.Join("\n", arr));
            Console.WriteLine("Done");
            Console.ReadLine();
        }*/
    }
}