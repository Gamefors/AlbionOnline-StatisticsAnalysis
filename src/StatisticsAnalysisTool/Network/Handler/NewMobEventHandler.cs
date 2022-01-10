using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;
using System;

namespace StatisticsAnalysisTool.Network.Handler
{
    enum MobTypes : int
    {
        Revenant = 693,
        Frost_Weaver = 707,
        Brittle_Revenant = 687,
        Ghoul = 691,
        Feeble_Frostweaver = 689,
        Bone_Archer = 689,
        Bone_Archer_2 = 699,
        Unyielding_Revenant = 695,
        Brittle_Skeleton = 2057,
        Brittle_Skeleton_2 = 2052,
        Brittle_Skeleton_Group_Dungeon = 2182,
        Summoned_Spectral  = 2027,
        Undead_Swordsman = 2077,
        Undead_Swordsman_Group_Dungeon = 2207,
        Undead_Scoprion = 2087,
        Undead_Archer = 2092,
        Undead_Spectral_Bat = 2072,
        Undead_Ghost = 2127,
        Rotten_Rat = 2047,
        Undead_Deathmonger = 2102,
        Morgana_Squire = 2629,
        Morgana_Cultist = 2674,
        Summoned_Imp = 2724,
        Morgana_Conjurer = 2699,
        Morgana_Footman = 2654,
        Morgana_Infestor = 2704,
        Bound_Molten_Demon_Solo_Dungeon = 2649,
        Bound_Molten_Demon_Group_Dungeon = 2774,
        Heretic_Scavenger = 1752,
        Heretic_Miner = 1759,
        Foul_Rat = 1745,
        Heretic_Shooter = 1864,
        Heretic_Bouncer = 1773,
        Heretic_Poacher = 1794,
        Heretic_Firestarter = 1815,
        Heretic_Cannoneer = 1836,
        Heretic_Thief = 1780,
        Heretic_Hermit = 1808,
        Boss_Heretic_Thug = 1787,
        Earthkeeper_Wildling = 2347,
        Earthkeeper_Cultivator = 2370,
        Earthkeeper_Knifeling = 2337,
        Keeper_Living_Root = 2302,
        Keeper_Bolt_Arrester = 1707,
        Earthkeeper_Druid = 2400,
        Will_o_Wisp = 2327,
        Raging_Wisp = 2332,
        Earthkeeper_Axe_Thrower = 2390,
        Keeper_Bear = 2360,
        Boss_Earthkeeper_Patriarch = 2380,
        Rock_Elemental = 2322,
        Morgana_Crossbowman_Group_Dungeon = 2789,
        Forgotten_General_Boss_Group_Dungeon = 2282,
        Morgana_Marksman_Boss_Group_Dungeon = 2794,
        Morgana_Knight_Solo_Dungeon = 2709,
        Earthkeeper_Wildling_Group_Dungeon = 2485,
        Earthkeeper_Knifeling_Group_Dungeon = 2475,
        Earthkeeper_Axe_Thrower_Group_Dungeon = 2528,
        Earthkeeper_Druid_Group_Dungeon = 2538,
        Keeper_Bear_Group_Dungeon = 2498,
        Earthkeeper_Earthchild = 2503,
        Earthkeeper_Beserker = 2518,
        Will_o_Wisp_Group_Dungeon = 2465,
        Rock_Elemental_Group_Dungeon = 2460,
        Keeper_Bolt_Arreseter_Group_Dungeon = 1712,
        Earthkeeper_Sharpeye_Group_Dungeon = 2475,
        Earthkeeper_Greybeard_Boss_Group_Dungeon = 2558,
        Earthkeeper_Axe_Maiden_Boss_Group_Dungeon = 2533,
        Raging_Wisp_Group_Dungeon = 2470,
        Earthkeeper_Sage_Group_Dungeon = 2543,
        Earthkeeper_Patriach_Group_Dungeon = 2523,
        Earthkeeper_Giant_Group_Dungeon = 2553,
        Athos_Boss_Group_Dungeon = 2573,
        Keeper_Living_Roots_Group_Dungeon = 2312,

    }
    public class NewMobEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewMobEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewMobEvent value)
        {
            string mobName = $"Unknown({value.Type})";
            if(Enum.IsDefined(typeof(MobTypes), (int)value.Type)){
                mobName = ((MobTypes)(int)value.Type).ToString();
            }

            if (value.Guid != null && value.ObjectId != null)
            {
                // Debug.Print($"[NewMob] ObjectId: {value.ObjectId} Type: {mobName}");
                _trackingController.EntityController.AddEntity((long)value.ObjectId, (Guid)value.Guid, null, mobName, GameObjectType.Mob, GameObjectSubType.Mob);
            }
            await Task.CompletedTask;
        }
    }
}