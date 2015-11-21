using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using UCS.Network;
using UCS.PacketProcessing;
using UCS.Core;
using System.Timers;
using System.Diagnostics;

namespace UCS
{
    class Program
    {
        public static System.Timers.Timer aTimer;

        static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine(" [i] EuroClash.Net - C# Emulator Improved by AlexBerescu");
            Console.WriteLine(" [i] Thanks to UltraPowa for Main Source & Packet Managenment");
            Console.WriteLine(" [i] Version 0.9A");
            Console.WriteLine("");
            Gateway g = new Gateway();
            PacketManager ph = new PacketManager();
            MessageManager dp = new MessageManager();
            ResourcesManager rm = new ResourcesManager();
            ObjectManager pm = new ObjectManager();
            dp.Start();
            ph.Start();
            g.Start();
            //ApiManager api = new ApiManager();
            ApiManager2 api2 = new ApiManager2();
            Core.Debugger.SetLogLevel(Int32.Parse(ConfigurationManager.AppSettings["loggingLevel"]));
            Logger.SetLogLevel(Int32.Parse(ConfigurationManager.AppSettings["loggingLevel"]));
            Console.WriteLine("");
            Console.WriteLine(" [i] Server successfully Loaded, you can login now!");
            Console.WriteLine(" [i] Automatic Server Status will be loaded shortly..");
            Console.WriteLine("");
            aTimer = new System.Timers.Timer(4000);
            aTimer.Elapsed += new ElapsedEventHandler(RunThis);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            Gestiune abe = new Gestiune();
            Console.ReadLine();
        }

        private static void RunThis(object source, ElapsedEventArgs e)
        {
            String url = "https://euroclash.net/visitors.php";

            Process proc = Process.GetCurrentProcess();
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine(" [i] Total Completed Connections : " + ResourcesManager.GetConnectedClients().Count);
            Console.WriteLine(" [i] Players Online : " + ResourcesManager.GetOnlinePlayers().Count);
            Console.WriteLine(" [i] In Memory Levels : " + ResourcesManager.GetInMemoryLevels().Count);
            Console.WriteLine(" [i] In Memory Clans : " + ObjectManager.GetInMemoryAlliances().Count);
            Console.WriteLine(" [i] Used Ram : " + proc.PrivateMemorySize64 + " bytes");
            Console.WriteLine(" [i] Last Update: " + DateTime.Now.ToString("h:mm:ss tt"));
            //Console.WriteLine(" [i] Servers Online Using BananaEmulator: " + (new System.Net.WebClient().DownloadString(url)) );
            Console.WriteLine("");
            Console.WriteLine(" --------------------------------[L]-[O]-[G]-[S]--------------------------------");
            Console.WriteLine("");
        }
    }
}
