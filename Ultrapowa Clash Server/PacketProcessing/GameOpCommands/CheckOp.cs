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
    class CheckOp : GameOpCommand
    {

        //private int m_vCurrentGems;
        private string[] m_vArgs;

        public CheckOp(string[] args)
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
                    if(level.GetAccountPrivileges() == 5)
                    {

                    } else
                    {
                        if (ResourcesManager.IsPlayerOnline(level))
                        {
                            var p = new OutOfSyncMessage(level.GetClient());
                            PacketManager.ProcessOutgoingPacket(p);
                        }
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