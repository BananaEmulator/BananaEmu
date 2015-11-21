using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using UCS.PacketProcessing;
using UCS.Logic;
using UCS.Helpers;
using UCS.GameFiles;
using UCS.Network;

namespace UCS.Core
{
    class Gestiune
    {
        public Gestiune()
        {
            while (true)
            {
                Console.WriteLine("");
                string line = Console.ReadLine();
                if (line == "/shutdown")
                {
                    foreach (var onlinePlayer in ResourcesManager.GetOnlinePlayers())
                    {
                        var p = new ShutdownStartedMessage(onlinePlayer.GetClient());
                        p.SetCode(5);
                        PacketManager.ProcessOutgoingPacket(p);
                    }
                    Console.WriteLine("Message has been send to the user");
                }
                else if (line == "/sysinfo")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Server Status is now sent to all online players");
                    AllianceMailStreamEntry mail = new AllianceMailStreamEntry();
                    mail.SetId((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                    mail.SetSenderId(0);
                    mail.SetSenderAvatarId(0);
                    mail.SetSenderName("System Manager");
                    mail.SetIsNew(0);
                    mail.SetAllianceId(0);
                    mail.SetAllianceBadgeData(0);
                    mail.SetAllianceName("Legendary Administrator");
                    mail.SetMessage("Latest Server Status:\nConnected Players:" + ResourcesManager.GetConnectedClients().Count + "\nIn Memory Alliances:" + ObjectManager.GetInMemoryAlliances().Count + "\nIn Memory Levels:" + ResourcesManager.GetInMemoryLevels().Count);
                    mail.SetSenderLeagueId(22);
                    mail.SetSenderLevel(500);

                    foreach (var onlinePlayer in ResourcesManager.GetOnlinePlayers())
                    {
                        var p = new AvatarStreamEntryMessage(onlinePlayer.GetClient());
                        var pm = new GlobalChatLineMessage(onlinePlayer.GetClient());
                        pm.SetChatMessage("Our current Server Status is now sent at your mailbox!");
                        pm.SetPlayerId(0);
                        pm.SetLeagueId(22);
                        pm.SetPlayerName("System Manager");
                        p.SetAvatarStreamEntry(mail);
                        PacketManager.ProcessOutgoingPacket(p);
                        PacketManager.ProcessOutgoingPacket(pm);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
    }
}