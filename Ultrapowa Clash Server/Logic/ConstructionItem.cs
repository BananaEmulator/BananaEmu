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
using UCS.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UCS.Logic
{
    class ConstructionItem : GameObject
    {
        protected Timer m_vTimer;
        protected Level m_vLevel;
        public int UpgradeLevel { get; set; }
        public bool IsBoosted { get; set; }
        protected DateTime m_vBoostEndTime;
        protected bool m_vIsConstructing;

        protected bool Locked;

        public ConstructionItem(Data data, Level level) : base(data, level)
        {
            m_vLevel = level;
            this.IsBoosted = false;
            m_vBoostEndTime = level.GetTime();
            m_vIsConstructing = false;
            this.UpgradeLevel = -1;
        }

        public bool CanUpgrade()
        {
            bool result = false;
            if (!IsConstructing())
            {
                if (this.UpgradeLevel < GetConstructionItemData().GetUpgradeLevelCount() - 1)
                {
                    result = true;
                    if (ClassId == 0 || ClassId == 4)
                    {
                        int currentTownHallLevel = GetLevel().GetPlayerAvatar().GetTownHallLevel();
                        int requiredTownHallLevel = GetRequiredTownHallLevelForUpgrade();
                        if (currentTownHallLevel < requiredTownHallLevel)
                        {
                            result = false;
                        }
                    }
                }
            }
            return result;
        }

        public void CancelConstruction()
        {
            if (IsConstructing())
            {
                bool wasUpgrading = IsUpgrading();
                m_vIsConstructing = false;
                if (wasUpgrading)
                {
                    SetUpgradeLevel(UpgradeLevel);
                }
                var bd = GetConstructionItemData();
                var rd = bd.GetBuildResource(UpgradeLevel + 1);
                int cost = bd.GetBuildCost(UpgradeLevel + 1);
                int multiplier = ObjectManager.DataTables.GetGlobals().GetGlobalData("BUILD_CANCEL_MULTIPLIER").NumberValue;
                int resourceCount = (int)((((long)(cost * multiplier)) * (long)1374389535) >> 32);
                resourceCount = Math.Max((resourceCount >> 5) + (resourceCount >> 31), 0);
                GetLevel().GetPlayerAvatar().CommodityCountChangeHelper(0, rd, resourceCount);
                m_vLevel.WorkerManager.DeallocateWorker(this);
                if (UpgradeLevel == -1)
                    m_vLevel.GameObjectManager.RemoveGameObject(this);
            }
        }

        public DateTime GetBoostEndTime()
        {
            return this.m_vBoostEndTime;
        }

        public int GetBoostDuration()
        {
            if (GetResourceProductionComponent() != null)
            {
                return ObjectManager.DataTables.GetGlobals().GetGlobalData("RESOURCE_PRODUCTION_BOOST_MINS").NumberValue;
            }
            if (GetUnitProductionComponent() != null)
            {
                if (GetUnitProductionComponent().IsSpellForge())
                {
                    return ObjectManager.DataTables.GetGlobals().GetGlobalData("SPELL_FACTORY_BOOST_MINS").NumberValue;
                }
                else
                {
                    return ObjectManager.DataTables.GetGlobals().GetGlobalData("BARRACKS_BOOST_MINS").NumberValue;
                }
            }
            if (GetHeroBaseComponent() != null)
            {
                return ObjectManager.DataTables.GetGlobals().GetGlobalData("HERO_REST_BOOST_MINS").NumberValue;
            }

            return 0;
        }

        public float GetBoostMultipier()
        {
            if (GetResourceProductionComponent() != null)
            {
                return ObjectManager.DataTables.GetGlobals().GetGlobalData("RESOURCE_PRODUCTION_BOOST_MULTIPLIER").NumberValue;
            }
            if (GetUnitProductionComponent() != null)
            {
                if (GetUnitProductionComponent().IsSpellForge())
                {
                    return ObjectManager.DataTables.GetGlobals().GetGlobalData("SPELL_FACTORY_BOOST_MULTIPLIER").NumberValue;
                }
                else
                {
                    return ObjectManager.DataTables.GetGlobals().GetGlobalData("BARRACKS_BOOST_MULTIPLIER").NumberValue;
                }
            }
            if (GetHeroBaseComponent() != null)
            {
                return ObjectManager.DataTables.GetGlobals().GetGlobalData("HERO_REST_BOOST_MULTIPLIER").NumberValue;
            }

            return 0;
        }

        public ConstructionItemData GetConstructionItemData()
        {
            return (ConstructionItemData)GetData();
        }

        public HeroBaseComponent GetHeroBaseComponent(bool enabled = false)
        {
            Component comp = this.GetComponent(10, enabled);
            if (comp != null && comp.Type != -1)
            {
                return (HeroBaseComponent)comp;
            }
            return null;
        }

        public UnitProductionComponent GetUnitProductionComponent(bool enabled = false)
        {
            Component comp = this.GetComponent(3, enabled);
            if (comp != null && comp.Type != -1)
            {
                return (UnitProductionComponent)comp;
            }
            return null;
        }

        public UnitStorageComponent GetUnitStorageComponent(bool enabled = false)
        {
            Component comp = this.GetComponent(0, enabled);
            if (comp != null && comp.Type != -1)
            {
                return (UnitStorageComponent)comp;
            }
            return null;
        }

        public UnitUpgradeComponent GetUnitUpgradeComponent(bool enabled = false)
        {
            Component comp = this.GetComponent(9, enabled);
            if (comp != null && comp.Type != -1)
            {
                return (UnitUpgradeComponent)comp;
            }
            return null;
        }

        public ResourceStorageComponent GetResourceStorageComponent(bool enabled = false)
        {
            Component comp = this.GetComponent(6, enabled);
            if (comp != null && comp.Type != -1)
            {
                return (ResourceStorageComponent)comp;
            }
            return null;
        }

        public ResourceProductionComponent GetResourceProductionComponent(bool enabled = false)
        {
            Component comp = this.GetComponent(5, enabled);
            if (comp != null && comp.Type != -1)
            {
                return (ResourceProductionComponent)comp;
            }
            return null;
        }

        public int GetRemainingConstructionTime()
        {
            return m_vTimer.GetRemainingSeconds(m_vLevel.GetTime());
        }

        public int GetRequiredTownHallLevelForUpgrade()
        {
            int upgradeLevel = Math.Min(UpgradeLevel + 1, GetConstructionItemData().GetUpgradeLevelCount() - 1);
            return GetConstructionItemData().GetRequiredTownHallLevel(upgradeLevel);
        }

        public int GetUpgradeLevel()
        {
            return this.UpgradeLevel;
        }

        public void BoostBuilding()
        {
            IsBoosted = true;
            this.m_vBoostEndTime = GetLevel().GetTime().AddMinutes(GetBoostDuration());
        }

        public void StartConstructing(int newX, int newY)
        {
            this.X = newX;
            this.Y = newY;
            int constructionTime = GetConstructionItemData().GetConstructionTime(UpgradeLevel + 1);
            if (constructionTime < 1)
            {
                FinishConstruction();
            }
            else
            {
                m_vTimer = new Timer();
                m_vTimer.StartTimer(constructionTime, m_vLevel.GetTime());
                m_vLevel.WorkerManager.AllocateWorker(this);
                m_vIsConstructing = true;
            }
        }

        public void StartUpgrading()
        {
            int constructionTime = GetConstructionItemData().GetConstructionTime(UpgradeLevel + 1);
            if (constructionTime < 1)
            {
                FinishConstruction();
            }
            else
            {
                //todo : collecter les ressources avant upgrade si un component de ressources est défini
                m_vIsConstructing = true;
                m_vTimer = new Timer();
                m_vTimer.StartTimer(constructionTime, m_vLevel.GetTime());
                m_vLevel.WorkerManager.AllocateWorker(this);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (IsConstructing())
            {
                if (m_vTimer.GetRemainingSeconds(m_vLevel.GetTime()) <= 0)
                    FinishConstruction();
            }
        }

        public void FinishConstruction()
        {
            m_vIsConstructing = false;
            m_vLevel.WorkerManager.DeallocateWorker(this);
            SetUpgradeLevel(GetUpgradeLevel() + 1);
            if (GetResourceProductionComponent() != null)
            {
                GetResourceProductionComponent().Reset();
            }

            //Add exp to client avatar
            int constructionTime = GetConstructionItemData().GetConstructionTime(GetUpgradeLevel());
            int exp = (int)Math.Pow(constructionTime, 0.5f);
            this.GetLevel().GetPlayerAvatar().AddExperience(exp);


            //LogicAchievementManager::refreshStatus(v48)
            //LogicLevel::refreshNewShopUnlocksTH(v50)
            //v28 = v27(v7, 10, 1);//enable
            if (GetHeroBaseComponent(true) != null)//(GetBuildingData().IsHeroBarrack)
            {
                var data = (BuildingData)GetData();
                HeroData hd = ObjectManager.DataTables.GetHeroByName(data.HeroType);
                GetLevel().GetPlayerAvatar().SetUnitUpgradeLevel(hd, 0);
                GetLevel().GetPlayerAvatar().SetHeroHealth(hd, 0);
                GetLevel().GetPlayerAvatar().SetHeroState(hd, 3);
            }
        }

        public void SetUpgradeLevel(int level)
        {
            this.UpgradeLevel = level;
            if (GetConstructionItemData().IsTownHall())
            {
                GetLevel().GetPlayerAvatar().SetTownHallLevel(level);
            }
            if (this.UpgradeLevel > -1 || IsUpgrading() || !IsConstructing())
            {
                if (GetUnitStorageComponent(true) != null)
                {
                    var data = (BuildingData)GetData();
                    if (data.GetUnitStorageCapacity(level) > 0)
                    {
                        if (!data.Bunker)
                        {
                            GetUnitStorageComponent().SetMaxCapacity(data.GetUnitStorageCapacity(level));
                            GetUnitStorageComponent().SetEnabled(!IsConstructing());
                        }
                    }
                }
                var resourceStorageComponent = GetResourceStorageComponent(true);
                if (resourceStorageComponent != null)
                {
                    List<int> maxStoredResourcesList = ((BuildingData)GetData()).GetMaxStoredResourceCounts(this.UpgradeLevel);
                    resourceStorageComponent.SetMaxArray(maxStoredResourcesList);
                }
            }
        }

        public void SpeedUpConstruction()
        {
            if (IsConstructing())
            {
                var ca = GetLevel().GetPlayerAvatar();
                int remainingSeconds = m_vTimer.GetRemainingSeconds(m_vLevel.GetTime());
                int cost = GamePlayUtil.GetSpeedUpCost(remainingSeconds);
                if (ca.HasEnoughDiamonds(cost))
                {
                    ca.UseDiamonds(cost);
                    FinishConstruction();
                }
            }
        }

        public bool IsConstructing()
        {
            return m_vIsConstructing;
        }

        public bool IsMaxUpgradeLevel()
        {
            return (this.UpgradeLevel >= (GetConstructionItemData().GetUpgradeLevelCount() - 1));
        }

        public bool IsUpgrading()
        {
            return m_vIsConstructing && this.UpgradeLevel >= 0;
        }

        public new JObject Save(JObject jsonObject)
        {
            jsonObject.Add("lvl", this.UpgradeLevel);
            if (IsConstructing())
                jsonObject.Add("const_t", m_vTimer.GetRemainingSeconds(m_vLevel.GetTime()));
            if (Locked)
                jsonObject.Add("locked", true);
            if (IsBoosted)
            {

                if ((int)(this.m_vBoostEndTime - GetLevel().GetTime()).TotalSeconds >= 0)
                {
                    jsonObject.Add("boost_t", (int)(this.m_vBoostEndTime - GetLevel().GetTime()).TotalSeconds);
                }
                jsonObject.Add("boost_endTime", this.m_vBoostEndTime);
            }
            base.Save(jsonObject);
            return jsonObject;
        }

        public new void Load(JObject jsonObject)
        {
            this.UpgradeLevel = jsonObject["lvl"].ToObject<int>();
            this.m_vLevel.WorkerManager.DeallocateWorker(this);
            var constTimeToken = jsonObject["const_t"];
            if (constTimeToken != null)
            {
                m_vTimer = new Timer();
                this.m_vIsConstructing = true;
                int remainingConstructionTime = constTimeToken.ToObject<int>();
                m_vTimer.StartTimer(remainingConstructionTime, m_vLevel.GetTime());//a changer ici, utilise seconds_from_last_respawn?
                this.m_vLevel.WorkerManager.AllocateWorker(this);
            }
            this.Locked = false;
            var lockedToken = jsonObject["locked"];
            if (lockedToken != null)
            {
                this.Locked = lockedToken.ToObject<bool>();
            }

            var boostToken = jsonObject["boost_endTime"];
            if (boostToken != null)
            {
                this.m_vBoostEndTime = jsonObject["boost_endTime"].ToObject<DateTime>();
                this.IsBoosted = true;
            }

            this.SetUpgradeLevel(this.UpgradeLevel);
            base.Load(jsonObject);
        }
    }

}