using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Configuration;
using UCS.PacketProcessing;
using UCS.Core;
using UCS.GameFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UCS.Logic
{
    class ResourceProductionComponent : Component
    {

        public override int Type
        {
            get { return 5; }
        }

        private DateTime m_vTimeSinceLastClick;
        private ResourceData m_vProductionResourceData;
        private List<int> m_vResourcesPerHour;
        private List<int> m_vMaxResources;

        public ResourceProductionComponent(ConstructionItem ci, Level level) : base(ci)
        {
            this.m_vTimeSinceLastClick = level.GetTime();
            this.m_vProductionResourceData = ObjectManager.DataTables.GetResourceByName(((BuildingData)ci.GetData()).ProducesResource);
            this.m_vResourcesPerHour = ((BuildingData)ci.GetData()).ResourcePerHour;
            this.m_vMaxResources = ((BuildingData)ci.GetData()).ResourceMax;
        }

        public void CollectResources()
        {
            ConstructionItem ci = (ConstructionItem)GetParent();
            TimeSpan span = ci.GetLevel().GetTime() - this.m_vTimeSinceLastClick;
            float currentResources = 0;
            if (!ci.IsBoosted)
            {
                currentResources = ((this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f)) * (float)(span.TotalSeconds));
            }
            else
            {
                if (ci.GetBoostEndTime() >= ci.GetLevel().GetTime())
                {
                    currentResources = ((this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f)) * (float)(span.TotalSeconds));
                    currentResources *= ci.GetBoostMultipier();
                }
                else
                {
                    float boostedTime = (float)span.TotalSeconds - (float)(ci.GetLevel().GetTime() - ci.GetBoostEndTime()).TotalSeconds;
                    float notBoostedTime = (float)span.TotalSeconds - boostedTime;

                    currentResources = ((this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f)) * notBoostedTime);
                    currentResources += ((this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f)) * boostedTime) * ci.GetBoostMultipier();
                    ci.IsBoosted = false;
                }
            }

            currentResources = Math.Min(Math.Max(currentResources, 0), this.m_vMaxResources[ci.UpgradeLevel]);

            if (currentResources >= 1)
            {
                ClientAvatar ca = ci.GetLevel().GetPlayerAvatar();
                if (ca.GetResourceCap(this.m_vProductionResourceData) >= ca.GetResourceCount(this.m_vProductionResourceData))
                {
                    if (ca.GetResourceCap(this.m_vProductionResourceData) - ca.GetResourceCount(this.m_vProductionResourceData) < currentResources)
                    {
                        int newCurrentResources = ca.GetResourceCap(this.m_vProductionResourceData) - ca.GetResourceCount(this.m_vProductionResourceData);
                        this.m_vTimeSinceLastClick = ci.GetLevel().GetTime().AddSeconds(-((currentResources - newCurrentResources) / (this.m_vResourcesPerHour[ci.UpgradeLevel] / (60f * 60f))));
                        currentResources = newCurrentResources;
                    }
                    else
                    {
                        this.m_vTimeSinceLastClick = ci.GetLevel().GetTime();
                    }
                    ca.CommodityCountChangeHelper(0, this.m_vProductionResourceData, (int)currentResources);
                }
            }
        }

        public void Reset()
        {
            this.m_vTimeSinceLastClick = GetParent().GetLevel().GetTime();
        }

        public override void Load(JObject jsonObject)
        {
            JObject productionObject = (JObject)jsonObject["production"];
            if (productionObject != null)
            {
                this.m_vTimeSinceLastClick = productionObject["t_lastClick"].ToObject<DateTime>();
            }
        }

        public override JObject Save(JObject jsonObject)
        {
            if (((ConstructionItem)GetParent()).GetUpgradeLevel() != -1)
            {
                JObject productionObject = new JObject();
                productionObject.Add("t_lastClick", this.m_vTimeSinceLastClick);
                jsonObject.Add("production", productionObject);
                ConstructionItem ci = (ConstructionItem)GetParent();
                float seconds = (float)(GetParent().GetLevel().GetTime() - this.m_vTimeSinceLastClick).TotalSeconds;
                if (ci.IsBoosted)
                {
                    if (ci.GetBoostEndTime() >= ci.GetLevel().GetTime())
                    {

                        seconds *= ci.GetBoostMultipier();
                    }
                    else
                    {
                        float boostedTime = seconds - (float)(ci.GetLevel().GetTime() - ci.GetBoostEndTime()).TotalSeconds;
                        float notBoostedTime = seconds - boostedTime;
                        seconds = boostedTime * ci.GetBoostMultipier() + notBoostedTime;
                    }
                }
                jsonObject.Add("res_time", (int)(((float)this.m_vMaxResources[ci.GetUpgradeLevel()] / (float)this.m_vResourcesPerHour[ci.GetUpgradeLevel()] * 3600f) - seconds));
            }

            return jsonObject;
        }
    }
}