using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using UCS.Helpers;
using UCS.Logic;
using UCS.Network;
using UCS.Core;
using System.Net;

namespace UCS.PacketProcessing
{

    //14715
    class SendGlobalChatLineMessage : Message
    {
        public SendGlobalChatLineMessage(Client client, BinaryReader br) : base(client, br)
        {
        }

        public override void Decode()
        {
            using (var br = new BinaryReader(new MemoryStream(GetData())))
            {
                Message = br.ReadScString();
            }
        }

        public String Message { get; set; }

       public override void Process(Level level)
        {
            if(Message.Length > 0)
            {
                if(Message[0] == '/')
                {
                    object obj = GameOpCommandFactory.Parse(Message);
                    if (obj != null)
                    {
                        string player = "";
                        if (level != null)
                            player += " (" + level.GetPlayerAvatar().GetId() + ", " + level.GetPlayerAvatar().GetAvatarName() + ")";
                        Debugger.WriteLine("\t" + obj.GetType().Name + player);
                        ((GameOpCommand)obj).Execute(level);
                    }
                }
                else
                {
                    var alliances = ObjectManager.GetAlliance(level.GetPlayerAvatar().GetAllianceId());
                    long senderId = level.GetPlayerAvatar().GetId();
                    string senderName = level.GetPlayerAvatar().GetAvatarName();
                    foreach(var onlinePlayer in ResourcesManager.GetOnlinePlayers())
                    {
                        var p = new GlobalChatLineMessage(onlinePlayer.GetClient());

                        if (onlinePlayer.GetAccountPrivileges() > 0)
                        {
                            if (level.GetPlayerAvatar().GetAllianceId() >= 1)
                            {
                                p.SetPlayerName(senderName + " #" + senderId + "\n🏰	 " + alliances.GetAllianceName());
                            }
                            else
                            {
                                p.SetPlayerName(senderName + " #" + senderId);
                            }
                        }
                        else
                        {
                            if (level.GetPlayerAvatar().GetAllianceId() >= 1)
                            {
                                p.SetPlayerName(senderName + "\n🏰 " + alliances.GetAllianceName());
                            }
                            else
                            {
                                p.SetPlayerName(senderName);
                            }
                        }
                        
                        if(level.GetPlayerAvatar().GetAllianceId() >= 1)
                        {
                            p.SetChatMessage("\n" + this.Message);
                        }
                        else
                        {
                            p.SetChatMessage(this.Message);
                        }

                        p.SetPlayerId(senderId);



                        // 400 BRONZO || 800 ARGENTO || ORO 1400 | CRISTALLO 2000 | MASTER 2600 | CHAMP 3200 | TITAN 4100
                        if (level.GetPlayerAvatar().GetScore() >= 400 && level.GetPlayerAvatar().GetScore() <= 800 )
                        {
                            p.SetLeagueId(3); // bronzo
                        }
                        else if (level.GetPlayerAvatar().GetScore() >= 801 && level.GetPlayerAvatar().GetScore() <= 1400)
                        {
                            p.SetLeagueId(6);
                        }
                        else if (level.GetPlayerAvatar().GetScore() >= 1401 && level.GetPlayerAvatar().GetScore() <= 2000)
                        {
                            p.SetLeagueId(9);
                        }
                        else if (level.GetPlayerAvatar().GetScore() >= 2001 && level.GetPlayerAvatar().GetScore() <= 2600)
                        {
                            p.SetLeagueId(12);
                        }
                        else if (level.GetPlayerAvatar().GetScore() >= 2601 && level.GetPlayerAvatar().GetScore() <= 3200)
                        {
                            p.SetLeagueId(15);
                        }
                        else if (level.GetPlayerAvatar().GetScore() >= 3201 && level.GetPlayerAvatar().GetScore() <= 4099)
                        {
                            p.SetLeagueId(18);
                        }
                        else if (level.GetPlayerAvatar().GetScore() >= 4100)
                        {
                            p.SetLeagueId(21);
                        }

                        if(level.GetAccountPrivileges() == 5)
                        {
                            p.SetLeagueId(22);
                        }

                        //p.SetLeagueId(4);
                        //level.GetPlayerAvatar().GetScore();
                        PacketManager.ProcessOutgoingPacket(p);
                    }
                }
            }    
        }
    }
}
