using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCS.Logic;
using UCS.Core;
using UCS.Helpers;
using System.Configuration;
using System.Data.Entity;

namespace UCS.PacketProcessing
{
    //Packet 24104
    class OutOfSyncMessage : Message
    {

        private string m_vConnectionString;

        public OutOfSyncMessage()
        {
            m_vConnectionString = ConfigurationManager.AppSettings["databaseConnectionName"];
        }

        public OutOfSyncMessage(Client client) : base(client) 
        {
            SetMessageType(24104);
            Console.WriteLine("OutOfSync Save on " + DateTime.Now.ToString());
            client.GetLevel().SaveToJSON();
        }

        public override void Encode()
        {
            List<Byte> data = new List<Byte>();
            data.AddInt32(0);
            data.AddInt32(0);
            data.AddInt32(0);
            SetData(data.ToArray());
        }
    }
}
