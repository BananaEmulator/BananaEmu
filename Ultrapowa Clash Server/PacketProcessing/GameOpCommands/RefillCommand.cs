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
    class RefillCommand : GameOpCommand
    {

        private string[] m_vArgs;

        public RefillCommand(string[] args)
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
                    
                    //SetResourceCount(ObjectManager.DataTables.GetResourceByName("Gold"), Convert.ToInt32(ConfigurationManager.AppSettings["startingGold"]));
                   // ResourcesManager.GetPlayer().GetPlayerAvatar().SetResourceCount(ObjectManager.DataTables.GetResourceByName("Gold"), Convert.ToInt32("32"));
                    level.GetPlayerAvatar().SetResourceCount(ObjectManager.DataTables.GetResourceByName("Gold"), Convert.ToInt32("999999999"));
                    level.GetPlayerAvatar().SetResourceCount(ObjectManager.DataTables.GetResourceByName("DarkElixir"), Convert.ToInt32("999999999"));
                    level.GetPlayerAvatar().SetResourceCount(ObjectManager.DataTables.GetResourceByName("Elixir"), Convert.ToInt32("999999999"));
                    level.GetPlayerAvatar().SetDiamonds(99999999);
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