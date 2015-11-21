using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UCS.Logic;
using UCS.Helpers;
using UCS.GameFiles;
using UCS.Core;
using UCS.Network;


namespace UCS.PacketProcessing
{
    class ServerStatusGameOpCommand : GameOpCommand
    {
        private string[] m_vArgs;

        public ServerStatusGameOpCommand(string[] args)
        {
            m_vArgs = args;
            SetRequiredAccountPrivileges(0);
        }

        public override void Execute(Level level)
        {
            if (level.GetAccountPrivileges() >= GetRequiredAccountPrivileges())
            {
                if (m_vArgs.Length >= 1)
                {
                    string message = string.Join("\n", m_vArgs.Skip(1));
                    AllianceMailStreamEntry mail = new AllianceMailStreamEntry();
                    mail.SetId((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                    mail.SetSenderId(0);
                    mail.SetSenderAvatarId(0);
                    mail.SetSenderName("EuroClash.Net");
                    mail.SetIsNew(0);
                    mail.SetAllianceId(0);
                    mail.SetAllianceBadgeData(0);
                    mail.SetAllianceName("Automatic Information");
                    mail.SetMessage("Total Completed Connections: " + ResourcesManager.GetConnectedClients().Count + "\nPlayers Online: " + ResourcesManager.GetOnlinePlayers().Count + "\nIn Memory Alliances: " + ObjectManager.GetInMemoryAlliances().Count + "\nIn Memory Levels: " + ResourcesManager.GetInMemoryLevels().Count);
                    mail.SetSenderLeagueId(22);
                    var p = new AvatarStreamEntryMessage(level.GetClient());
                    p.SetAvatarStreamEntry(mail);
                    PacketManager.ProcessOutgoingPacket(p);
                }
            }
            else
            {
                SendCommandFailedMessage(level.GetClient());
            }
        }
    }
}