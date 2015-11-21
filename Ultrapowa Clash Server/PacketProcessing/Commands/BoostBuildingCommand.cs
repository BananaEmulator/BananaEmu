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

namespace UCS.PacketProcessing
{
    //Commande 0x20E
    class BoostBuildingCommand : Command
    {
        public BoostBuildingCommand(BinaryReader br)
        {
            BuildingIds = new List<int>();
            BoostedBuildingsCount = br.ReadInt32WithEndian();
            for (int i = 0; i < BoostedBuildingsCount; i++)
            {
                BuildingIds.Add(br.ReadInt32WithEndian()); //buildingId - 0x1DCD6500;
            }
        }

        public override void Execute(Level level)
        {
            ClientAvatar ca = level.GetPlayerAvatar();
            foreach (int buildingId in BuildingIds)
            {
                GameObject go = level.GameObjectManager.GetGameObjectByID(buildingId);

                ConstructionItem b = (ConstructionItem)go;
                int costs = ((BuildingData)b.GetConstructionItemData()).BoostCost[b.UpgradeLevel];
                if (ca.HasEnoughDiamonds(costs))
                {
                    b.BoostBuilding();
                    ca.SetDiamonds(ca.GetDiamonds() - costs);
                }
            }
        }
        //00 00 02 0E 1D CD 65 05 00 00 8C 52

        public int BoostedBuildingsCount { get; set; }
        public List<int> BuildingIds { get; set; }
    }
}
