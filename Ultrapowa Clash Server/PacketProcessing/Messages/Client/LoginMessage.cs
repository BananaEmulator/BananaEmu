using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Security.Cryptography;
using UCS.Helpers;
using UCS.Core;
using UCS.Network;
using UCS.Logic;

namespace UCS.PacketProcessing
{
    //Packet 10101
    class LoginMessage : Message
    {
        private long m_vAccountId;
        private string m_vPassToken;
        private int m_vClientMajorVersion;
        private int m_vClientBuild;
        private int m_vClientContentVersion;
        private string m_vResourceSha;
        private string m_vUDID;
        private string m_vOpenUDID;
        private string m_vMacAddress;
        private string m_vDevice;
        private string m_vPreferredDeviceLanguage;
        //unchecked
        private string m_vPhoneId;
        private string m_vGameVersion;
        private string m_vSignature2;
        private string m_vSignature3;
        private string m_vSignature4;
        private uint m_vClientSeed; 

        public LoginMessage(Client client, BinaryReader br) : base (client, br)
        {
        }

        public override void Decode()
        {
            using (var br = new BinaryReader(new MemoryStream(GetData())))
            {
                m_vAccountId = br.ReadInt64WithEndian();
                m_vPassToken = br.ReadScString();
                m_vClientMajorVersion = br.ReadInt32WithEndian();
                m_vClientContentVersion = br.ReadInt32WithEndian();
                m_vClientBuild = br.ReadInt32WithEndian();
                m_vResourceSha = br.ReadScString();
                m_vUDID = br.ReadScString();
                m_vOpenUDID = br.ReadScString();
                m_vMacAddress = br.ReadScString();
                m_vDevice = br.ReadScString();
                br.ReadInt32WithEndian();//00 1E 84 81, readDataReference for m_vPreferredLanguage
                m_vPreferredDeviceLanguage = br.ReadScString();
                //unchecked
                m_vPhoneId = br.ReadScString();
                m_vGameVersion = br.ReadScString();
                br.ReadByte();//01
                br.ReadInt32WithEndian();//00 00 00 00
                m_vSignature2 = br.ReadScString();
                m_vSignature3 = br.ReadScString();
                br.ReadByte();//01
                m_vSignature4 = br.ReadScString();
                m_vClientSeed = br.ReadUInt32WithEndian();
                if(GetMessageVersion() >=7 )//7.156
                {
                    br.ReadByte();
                    br.ReadUInt32WithEndian();
                    br.ReadUInt32WithEndian();
                }
            }
        }

        public override void Process(Level level)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["maintenanceMode"]))
            {
                var p = new LoginFailedMessage(this.Client);
                p.SetErrorCode(10);
                PacketManager.ProcessOutgoingPacket(p);
                return;
            }

            string[] versionData = ConfigurationManager.AppSettings["clientVersion"].Split('.');
            if(versionData.Length >= 2)
            {
                if(m_vClientMajorVersion != Convert.ToInt32(versionData[0]) || m_vClientBuild != Convert.ToInt32(versionData[1]))
                {
                    var p = new LoginFailedMessage(this.Client);
                    p.SetErrorCode(8);
                    p.SetUpdateURL("market://details?id=com.supercell.clashofclans");
                    PacketManager.ProcessOutgoingPacket(p);
                    return;
                }
            }
            else
            {
                Debugger.WriteLine("Connection failed. UCS config key clientVersion is not properly set.");
            }

            level = ResourcesManager.GetPlayer(m_vAccountId);
            if(level != null)
            {
                if(level.GetAccountStatus() == 99)
                {
                    var p = new LoginFailedMessage(this.Client);
                    p.SetErrorCode(11);
                    PacketManager.ProcessOutgoingPacket(p);
                    return;
                }
            }

            if(Convert.ToBoolean(ConfigurationManager.AppSettings["useCustomPatch"]))
            {
                if (m_vResourceSha != ObjectManager.FingerPrint.sha)
                {
                    var p = new LoginFailedMessage(this.Client);
                    p.SetErrorCode(7);
                    p.SetResourceFingerprintData(ObjectManager.FingerPrint.SaveToJson());
                    p.SetContentURL(ConfigurationManager.AppSettings["patchingServer"]);
                    p.SetUpdateURL("market://details?id=com.supercell.clashofclans");
                    PacketManager.ProcessOutgoingPacket(p);
                    return;
                }
            }

            this.Client.ClientSeed = m_vClientSeed;
            PacketManager.ProcessOutgoingPacket(new SessionKeyMessage(this.Client));
            //Console.WriteLine("Debug: Retrieve Player Data for player " + auth.PlayerId.ToString());
            //New player
            if (level == null)
            {
                level = ObjectManager.CreateAvatar(m_vAccountId);
                byte[] tokenSeed = new byte[20];
                new Random().NextBytes(tokenSeed);
                SHA1 sha = new SHA1CryptoServiceProvider();
                m_vPassToken = BitConverter.ToString(sha.ComputeHash(tokenSeed)).Replace("-","");
            }
            //if (level.GetAccountPrivileges() > 0)
            //    level.GetPlayerAvatar().SetLeagueId(21);
            //if (level.GetAccountPrivileges() > 4)
            //    level.GetPlayerAvatar().SetLeagueId(22);
            if (level.GetPlayerAvatar().GetScore() >= 400 && level.GetPlayerAvatar().GetScore() <= 800)
            {
                level.GetPlayerAvatar().SetLeagueId(3);
            }
            else if (level.GetPlayerAvatar().GetScore() >= 801 && level.GetPlayerAvatar().GetScore() <= 1400)
            {
                level.GetPlayerAvatar().SetLeagueId(6);
            }
            else if (level.GetPlayerAvatar().GetScore() >= 1401 && level.GetPlayerAvatar().GetScore() <= 2000)
            {
                level.GetPlayerAvatar().SetLeagueId(9);
            }
            else if (level.GetPlayerAvatar().GetScore() >= 2001 && level.GetPlayerAvatar().GetScore() <= 2600)
            {
                level.GetPlayerAvatar().SetLeagueId(12);
            }
            else if (level.GetPlayerAvatar().GetScore() >= 2601 && level.GetPlayerAvatar().GetScore() <= 3200)
            {
                level.GetPlayerAvatar().SetLeagueId(15);
            }
            else if (level.GetPlayerAvatar().GetScore() >= 3201 && level.GetPlayerAvatar().GetScore() <= 4099)
            {
                level.GetPlayerAvatar().SetLeagueId(18);
            }
            else if (level.GetPlayerAvatar().GetScore() >= 4100)
            {
                level.GetPlayerAvatar().SetLeagueId(21);
            }

            ResourcesManager.LogPlayerIn(level, this.Client);
            level.Tick();

            var loginOk = new LoginOkMessage(this.Client);
            var avatar = level.GetPlayerAvatar();
            loginOk.SetAccountId(avatar.GetId());
            loginOk.SetPassToken(m_vPassToken);
            loginOk.SetServerMajorVersion(m_vClientMajorVersion);
            loginOk.SetServerBuild(m_vClientBuild);
            loginOk.SetContentVersion(m_vClientContentVersion);
            loginOk.SetServerEnvironment("prod");
            loginOk.SetDaysSinceStartedPlaying(10);
            loginOk.SetServerTime(Math.Round((level.GetTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds * 1000).ToString());
            loginOk.SetAccountCreatedDate("1414003838000");
            loginOk.SetStartupCooldownSeconds(0);
            loginOk.SetCountryCode("FR");
            PacketManager.ProcessOutgoingPacket(loginOk);

            if (ResourcesManager.IsPlayerOnline(level))
            {
                AllianceMailStreamEntry mail = new AllianceMailStreamEntry();
                mail.SetId((int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
                mail.SetSenderId(0);
                mail.SetSenderAvatarId(0);
                mail.SetSenderName("EuroClash.Net");
                mail.SetIsNew(0);
                mail.SetAllianceId(0);
                mail.SetAllianceBadgeData(0);
                mail.SetAllianceName("Automatic Information");
                mail.SetMessage("Welcome to EuroClash ! \nTo see available commands type /help \n\nSaveGame is fully automatic, you don't have to type anything, commands like /savegame or ./savegame doesn't exist, all commands are available on /help\n\nTrophies are randomly given on first login, this will be fixed soon.\n\nNow Clans calculate trophies of players, 50% of their trophies will be added to clan trophies, still kinda bugged, we're testing with simple math. Clan Owner's trophies are not added (bug).\n\nServer is currently in Alpha, to see bugfixes please visit http://euroclash.net ChangeLog Section!");
                mail.SetSenderLeagueId(22);
                var abe = new AvatarStreamEntryMessage(level.GetClient());
                abe.SetAvatarStreamEntry(mail);
                PacketManager.ProcessOutgoingPacket(abe);
            }
            //level.GetPlayerAvatar().SetResourceCount(ObjectManager.DataTables.GetResourceByName("Diamonds"), Convert.ToInt32(1000000));

            Alliance alliance = ObjectManager.GetAlliance(level.GetPlayerAvatar().GetAllianceId());
            if (alliance == null)
                level.GetPlayerAvatar().SetAllianceId(0);
            PacketManager.ProcessOutgoingPacket(new OwnHomeDataMessage(this.Client, level));
            if (alliance != null)
                PacketManager.ProcessOutgoingPacket(new AllianceStreamMessage(this.Client, alliance));
        }
    }
}
