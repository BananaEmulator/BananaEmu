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
    class SetMaxVillage : GameOpCommand
    {

        private static string m_vHomeTowerMax;

        //private int m_vCurrentGems;
        private string[] m_vArgs;

        public SetMaxVillage(string[] args)
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
                    using (StreamReader sr = new StreamReader(@"gamefiles/default/homemax.json"))
                    {
                        m_vHomeTowerMax = sr.ReadToEnd();
                    }
                    level.SetHome(m_vHomeTowerMax);
                    //level.LoadFromJSON(m_vHomeTowerMax);
                    if (ResourcesManager.IsPlayerOnline(level))
                    {
                        var p = new OutOfSyncMessage(level.GetClient());
                        PacketManager.ProcessOutgoingPacket(p);
                    }
                }
            }
            else
            {
                SendCommandFailedMessage(level.GetClient());
            }
        }
    }
}