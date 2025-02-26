using log4net;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager
{
    //make 2 enums 1 for player 1 for mob because some abilites have the same name
    enum CausingSpellType : int
    {
        AutoAttack = -1,
        Bleeding_Dot = 84,
        Bane = 89,
        Chain_Lightning_From_Thetford_Cape = 192,
        Deadly_Shot = 1656,
        Explosive_Arrows = 1661,
        Raging_Storm = 1689,
        Vial_CurseCharge = 1733,
        Cursed_Sickle = 1735,
        Vial_Curse_Dot = 1736,
        Armor_Piercer = 1738,
        Cursed_Beam = 1740,
        GrudgeDot = 1744,
        Dark_Matter = 1746,
        Death_Curse_Four_Stacks = 1749,
        Death_Curse_Three_Stacks = 1750,
        Death_Curse_Two_Stacks = 1751,
        Death_Curse_One_Stacks = 1752,
        Death_Curse_No_Stacks = 1753,
        HauntingScreams = 1765,
        Cataclysm = 1766,
        Sunder_Armor = 1776,
        Deadly_Swipe = 1783,
        Throwing_Blades = 1788,
        Forbidden_Stab = 1791,
        Lunging_Stabs = 1812,
        Frost_Bolt = 1884,
        Hoarfrost = 1885,
        Ice_Shard = 1887,
        Frost_Bomb = 1888,
        Frost_Nova = 1890,
        Frost_Beam = 1892,
        Shatter_First_Ring = 1895,
        Shatter_Second_Ring = 1896,
        Shatter_Third_Ring = 1897,
        Freezing_Wind = 1899,
        Ice_Crystal = 1911,
        Smite = 1960,
        Sacred_Pulse = 1964,
        Inferno_Shield = 2333,
        Force_Field = 2396,
        Mage_Cowl_Poison = 2402,
        Minor_Poison_Potion = 2842,
        Unholy_Frenzy = 3805,
      

        Freeze = 3904,
        Frozen = 3838,

        Mob_Brittle_Skeletons_Bone = 4828,
        Mob_Strong_Impact = 3985,
        Mob_Cursed_Arrow = 3923,
        Mob_Rain_of_Arrows = 3911,
        Mob_Howl = 3889,
        Mob_Undead_Ghost_Frost_Bolt = 4892,
        Mob_Undead_Swordsman_Unholy_Frenzy = 3950,
        Mob_Blizzard = 4895,
        Mob_Suffocating_Cloud = 3891,
        Mob_Undead_Deathmonger_Frozen = 3906,
        Mob_Implode = 3086,
        Mob_Linear_Strike = 4650,
        Mob_Assault = 5356,
        Mob_Molten_Trail = 4625,
        Mob_Sustained_Fire = 3495,
        Mob_Heavy_Blunt = 3517,
        Mob_Aimed_Shot = 3513,
        Mob_Scorching_Coal = 3511,
        Mob_Hamstring = 3542,
        Mob_Cursed_Arrow_2 = 3815,
        Mob_Crush = 3543,
        Mob_Dispersed_Searing = 4553,
        Mob_Energizing_Mushroom = 4552,
        Static_Lightning = 5278,
        Mob_Penetrating_Roots = 4385,
        Mob_Sudden_Discharge = 4359,
        Mob_Static_Discharge = 4360,
        Mob_Groundwrecker  = 4352,
        Mob_Charge = 4544,
        Mob_Heavy_Smash = 4456,
        Mob_Rockslide = 5351,
        Morgana_Knight_Mob_Crush = 3769,
        Mob_Eearthkeeper_Static_Discharge = 4365,
        Mob_Rotten_Ground = 4367,
        Mob_Smash = 4335,
        Mob_Geyser = 4328,
        Mob_Punch = 4423,
        Mob_War_Stomp = 4419,
        Mob_Boulder = 4440,
        Mob_Ground_Breaker = 4584,
        Mob_Giant_Rocks = 4441,
        Mob_Strong_Kick = 4438,
        Mob_Impact = 4439,
        Mob_Demonic_Field = 3759,
        Morgana_Mob_Punch = 4626,
        Mob_Suppression_Fire = 3739,
        Mob_Disengage = 3744,








    }
    
 
    class ReceivedDamageObject
    {
        public LocalUserData PlayerData { get; set; }
        public List<DamageObject> PlayerDamage { get; set; }
        public List<DamageObject> PartyDamage { get; set; }
    }

    public class CombatController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        public bool IsDamageMeterActive { get; set; } = false;

        string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}{"receivedDamage.json"}";

        public CombatController(TrackingController trackingController, MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;

            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;

            OnChangeCombatMode += AddCombatTime;

#if DEBUG
            RunDamageMeterDebugAsync(0, 0);
#endif
        }

        #region Damage Meter methods
        
        public async Task AddDamageAsync(long objectId, long causerId, double healthChange, double newHealthValue, int causingSpellType, EffectOrigin effectOrigin, EffectType effectTpye)
        {
            if (!IsDamageMeterActive || objectId == causerId) return;
            if (_trackingController.EntityController.LocalUserData == null) return;
            
            var attackerEntity = _trackingController?.EntityController?.GetEntity(causerId);
            if (attackerEntity?.Value == null)
            {
                ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"[CombatController] Id: {causerId} | Entity not found.", "#FF0000");
                Debug.Print($"[CombatController] Id: {causerId} | Entity not found.");
                return;
            }

            var victimEntity = _trackingController?.EntityController?.GetEntity(objectId);

            double receivedDamage = Math.Abs(healthChange);

            var empty = new ReceivedDamageObject();
            empty.PlayerData = _trackingController.EntityController.LocalUserData;
            empty.PartyDamage = new List<DamageObject>();
            empty.PlayerDamage = new List<DamageObject>();
            var emptyFileString = JsonConvert.SerializeObject(empty);

            if (!File.Exists(filePath)) File.WriteAllText(filePath, emptyFileString, Encoding.UTF8);

            var jsonData = File.ReadAllText(filePath);

            var receivedDamageObject = JsonConvert.DeserializeObject<ReceivedDamageObject>(jsonData) ?? new ReceivedDamageObject();
            receivedDamageObject.PlayerData = _trackingController.EntityController.LocalUserData;

            string causingSpellName = $"Unknown({causingSpellType})";
            if (Enum.IsDefined(typeof(CausingSpellType), causingSpellType))
            {
                causingSpellName = ((CausingSpellType)causingSpellType).ToString();
            }

            if (_trackingController.EntityController.LocalUserData.UserObjectId == objectId && healthChange < 0)
            {
                Spell spell = new Spell()
                {
                    Name = causingSpellName,
                    Type = effectTpye.ToString(),
                    Origin = effectOrigin.ToString()
                };
                DamageObject damageObject = new DamageObject()
                {
                    Victim = victimEntity.Value.Value.Name,
                    Attacker = attackerEntity.Value.Value.Name,
                    Damage = (int)receivedDamage,
                    Spell = spell
                };


                receivedDamageObject.PlayerDamage.Add(damageObject);
                Debug.Print($"[CombatController] Attacker: {damageObject.Attacker} Spell: {damageObject.Spell.Name}({damageObject.Spell.Origin}) DMG:{damageObject.Damage} Victim: {damageObject.Victim}");
                ConsoleManager.WriteLineForCombatController(damageObject);
            }

            if (_trackingController.EntityController.LocalUserData.UserObjectId != objectId && _trackingController.EntityController.IsUserInParty(objectId) && healthChange < 0)
            {
                Spell spell = new Spell()
                {
                    Name = causingSpellName,
                    Type = effectTpye.ToString(),
                    Origin = effectOrigin.ToString()
                };
                DamageObject damageObject = new DamageObject()
                {
                    Victim = victimEntity.Value.Value.Name,
                    Attacker = attackerEntity.Value.Value.Name,
                    Damage = (int)receivedDamage,
                    Spell = spell
                };
                receivedDamageObject.PartyDamage.Add(damageObject);
                Debug.Print($"[CombatController] Attacker: {damageObject.Attacker} Spell: {damageObject.Spell.Name}({damageObject.Spell.Origin}) DMG:{damageObject.Damage} Victim: {damageObject.Victim}");
                ConsoleManager.WriteLineForCombatController(damageObject);
            }

            jsonData = JsonConvert.SerializeObject(receivedDamageObject);
            System.IO.File.WriteAllText(filePath, jsonData);

            //check if entity that caused the damage exists
            if (attackerEntity?.Value == null
            || attackerEntity.Value.Value?.ObjectType != GameObjectType.Player
            || !_trackingController.EntityController.IsUserInParty(attackerEntity.Value.Value.Name)
            )
            {
                return;
            }




            if (GetHealthChangeType(healthChange) == HealthChangeType.Damage)
            {
                var damageChangeValue = (int)Math.Round(healthChange.ToPositiveFromNegativeOrZero(), MidpointRounding.AwayFromZero);
                if (damageChangeValue <= 0)
                {
                    return;
                }

                attackerEntity.Value.Value.Damage += damageChangeValue;
            }

            if (GetHealthChangeType(healthChange) == HealthChangeType.Heal)
            {
                var healChangeValue = healthChange;
                if (healChangeValue <= 0)
                {
                    return;
                }

                if (IsMaxHealthReached(objectId, newHealthValue))
                {
                    return;
                }

                attackerEntity.Value.Value.Heal += (int)Math.Round(healChangeValue, MidpointRounding.AwayFromZero);
            }

            if (attackerEntity.Value.Value?.CombatStart == null)
            {
                attackerEntity.Value.Value.CombatStart = DateTime.UtcNow;
            }

            if (IsUiUpdateAllowed())
            {
                await UpdateDamageMeterUiAsync(_mainWindowViewModel.DamageMeter, _trackingController.EntityController.GetAllEntities());
            }
        }

        private static bool _isUiUpdateActive;

        public async Task UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
        {
            _isUiUpdateActive = true;

            var highestDamage = entities.GetHighestDamage();
            var highestHeal = entities.GetHighestHeal();
            _trackingController.EntityController.DetectUsedWeapon();

            foreach (var healthChangeObject in entities)
            {
                if (_mainWindow?.Dispatcher == null || healthChangeObject.Value?.UserGuid == null)
                {
                    continue;
                }

                var fragment = damageMeter.ToList().FirstOrDefault(x => x.CauserGuid == healthChangeObject.Value.UserGuid);
                if (fragment != null)
                {
                    UpdateDamageMeterFragment(fragment, healthChangeObject, entities, highestDamage, highestHeal);
                }
                else
                {
                    await AddDamageMeterFragmentAsync(damageMeter, healthChangeObject, entities, highestDamage, highestHeal).ConfigureAwait(true);
                }

                Application.Current.Dispatcher.Invoke(() => _mainWindowViewModel.SetDamageMeterSort());
            }

            if (HasDamageMeterDupes(_mainWindowViewModel.DamageMeter))
            {
                await RemoveDuplicatesAsync(_mainWindowViewModel.DamageMeter);
            }

            _isUiUpdateActive = false;
        }

        private static void UpdateDamageMeterFragment(DamageMeterFragment fragment, KeyValuePair<Guid, PlayerGameObject> healthChangeObject, List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            if (healthChangeObject.Value?.CharacterEquipment?.MainHand != null)
            {
                var item = ItemController.GetItemByIndex((int)healthChangeObject.Value?.CharacterEquipment?.MainHand);
                if (item != null)
                {
                    fragment.CauserMainHand = item;
                }
            }

            // Damage
            if (healthChangeObject.Value?.Damage > 0)
            {
                fragment.DamageInPercent = (double)healthChangeObject.Value.Damage / highestDamage * 100;
                fragment.Damage = (long)healthChangeObject.Value?.Damage;
            }

            if (healthChangeObject.Value?.Dps != null)
            {
                fragment.Dps = healthChangeObject.Value.Dps;
            }

            // Heal
            if (healthChangeObject.Value?.Heal > 0)
            {
                fragment.HealInPercent = (double)healthChangeObject.Value.Heal / highestHeal * 100;
                fragment.Heal = (long)healthChangeObject.Value?.Heal;
            }

            if (healthChangeObject.Value?.Hps != null)
            {
                fragment.Hps = healthChangeObject.Value.Hps;
            }

            // Generally
            if (healthChangeObject.Value != null)
            {
                fragment.DamagePercentage = entities.GetDamagePercentage(healthChangeObject.Value.Damage);
                fragment.HealPercentage = entities.GetHealPercentage(healthChangeObject.Value.Heal);
            }
        }

        private static async Task AddDamageMeterFragmentAsync(ObservableCollection<DamageMeterFragment> damageMeter, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
            List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            if (healthChangeObject.Value == null
                || (double.IsNaN(healthChangeObject.Value.Damage) && double.IsNaN(healthChangeObject.Value.Heal))
                || (healthChangeObject.Value.Damage <= 0 && healthChangeObject.Value.Heal <= 0))
            {
                return;
            }

            var item = ItemController.GetItemByIndex(healthChangeObject.Value?.CharacterEquipment?.MainHand ?? 0);

            var damageMeterFragment = new DamageMeterFragment
            {
                CauserGuid = healthChangeObject.Value.UserGuid,
                Damage = healthChangeObject.Value.Damage,
                Dps = healthChangeObject.Value.Dps,
                DamageInPercent = (double)healthChangeObject.Value.Damage / highestDamage * 100,
                DamagePercentage = entities.GetDamagePercentage(healthChangeObject.Value.Damage),

                Heal = healthChangeObject.Value.Heal,
                Hps = healthChangeObject.Value.Hps,
                HealInPercent = (double)healthChangeObject.Value.Heal / highestHeal * 100,
                HealPercentage = entities.GetHealPercentage(healthChangeObject.Value.Heal),

                Name = healthChangeObject.Value.Name,
                CauserMainHand = item
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                damageMeter.Add(damageMeterFragment);
            });
        }

        private static bool HasDamageMeterDupes(IEnumerable<DamageMeterFragment> damageMeter)
        {
            return damageMeter.ToList().GroupBy(x => x.Name).Any(g => g.Count() > 1);
        }

        private static async Task RemoveDuplicatesAsync(ObservableCollection<DamageMeterFragment> damageMeter)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var damageMeterWithoutDupes = (from dmf in damageMeter.ToList()
                                               group dmf by dmf.Name into x
                                               select new DamageMeterFragment(x.FirstOrDefault())).ToList();

                if (damageMeterWithoutDupes.Count <= 0)
                {
                    return;
                }

                foreach (var damageMeterFragment in damageMeter.ToList())
                {
                    if (damageMeterWithoutDupes.Any(x => x.Equals(damageMeterFragment)))
                    {
                        damageMeter.Remove(damageMeterFragment);
                    }
                }
            });
        }

        public void ResetDamageMeter()
        {
            _trackingController.EntityController.ResetEntitiesDamageTimes();
            _trackingController.EntityController.ResetEntitiesDamage();
            _trackingController.EntityController.ResetEntitiesHeal();
            _trackingController.EntityController.ResetEntitiesDamageStartTime();

            Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                _mainWindowViewModel?.DamageMeter?.Clear();
            });
        }

        public ConcurrentDictionary<long, double> LastPlayersHealth = new();

        public bool IsMaxHealthReached(long objectId, double newHealthValue)
        {
            var playerHealth = LastPlayersHealth?.ToArray().FirstOrDefault(x => x.Key == objectId);
            if (playerHealth?.Value.CompareTo(newHealthValue) == 0)
            {
                return true;
            }

            SetLastPlayersHealth(objectId, newHealthValue);
            return false;
        }

        private void SetLastPlayersHealth(long key, double value)
        {
            if (LastPlayersHealth.ContainsKey(key))
            {
                LastPlayersHealth[key] = value;
            }
            else
            {
                try
                {
                    LastPlayersHealth.TryAdd(key, value);
                }
                catch (Exception e)
                {
                    Log.Warn(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }
        }

        private static HealthChangeType GetHealthChangeType(double healthChange) => healthChange <= 0 ? HealthChangeType.Damage : HealthChangeType.Heal;

        private DateTime _lastDamageUiUpdate;

        private bool IsUiUpdateAllowed(int waitTimeInSeconds = 1)
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
            if (difference.Seconds >= waitTimeInSeconds && !_isUiUpdateActive)
            {
                _lastDamageUiUpdate = currentDateTime;
                return true;
            }

            return false;
        }

        #endregion

        #region Combat Mode / Combat Timer

        public event Action<long, bool, bool> OnChangeCombatMode;

        public void UpdateCombatMode(long objectId, bool inActiveCombat, bool inPassiveCombat)
        {
            OnChangeCombatMode?.Invoke(objectId, inActiveCombat, inPassiveCombat);
        }

        private void AddCombatTime(long objectId, bool inActiveCombat, bool inPassiveCombat)
        {
            if (!_trackingController.EntityController.IsUserInParty(objectId))
            {
                return;
            }

            var playerObject = _trackingController.EntityController.GetEntity(objectId);

            if (playerObject?.Value == null)
            {
                return;
            }

            if ((inActiveCombat || inPassiveCombat) && playerObject.Value.Value.CombatTimes.Any(x => x?.EndTime == null))
            {
                return;
            }

            if (inActiveCombat || inPassiveCombat) playerObject.Value.Value.AddCombatTime(new TimeCollectObject(DateTime.UtcNow));

            if (!inActiveCombat && !inPassiveCombat)
            {
                var combatTime = playerObject.Value.Value.CombatTimes.FirstOrDefault(x => x.EndTime == null);
                if (combatTime != null)
                {
                    combatTime.EndTime = DateTime.UtcNow;
                }
            }
        }

        #endregion

        #region Debug methods

        private static readonly Random Random = new(DateTime.Now.Millisecond);

        private async void RunDamageMeterDebugAsync(int player = 20, int damageRuns = 100)
        {
            var entities = SetRandomDamageValues(player);
            var tasks = new List<Task>();

            foreach (var entity in entities)
            {
                tasks.Add(AddDamageAsync(entity.Value, damageRuns));
            }

            await Task.WhenAll(tasks);
        }

        private async Task AddDamageAsync(PlayerGameObject entity, int runs)
        {
            for (var i = 0; i < runs; i++)
            {
               // var damage = _random.Next(-100, 100);
                // await AddDamageAsync(9999, entity.ObjectId ?? -1, damage, _random.Next(2000, 3000));
                //Debug.Print($"--- AddDamage - {entity.Name}: {damage}");

                await Task.Delay(Random.Next(1, 1000));
            }
        }

        private List<KeyValuePair<Guid, PlayerGameObject>> SetRandomDamageValues(int playerAmount)
        {
            for (var i = 0; i < playerAmount; i++)
            {
                var guid = new Guid($"{Random.Next(1000, 9999)}0000-0000-0000-0000-000000000000");
                var interactGuid = Guid.NewGuid();
                var name = TestMethods.GenerateName(Random.Next(3, 10));

                _trackingController?.EntityController?.AddEntity(i, guid, interactGuid, name, GameObjectType.Player, GameObjectSubType.Mob);

                // Only if SetCharacterMainHand is public
                //_trackingController?.EntityController?.SetCharacterMainHand(i, TestMethods.GetRandomWeaponIndex());
                _trackingController?.EntityController?.AddToPartyAsync(guid, name);
            }

            return _trackingController?.EntityController?.GetAllEntities();
        }

        #endregion
    }
}