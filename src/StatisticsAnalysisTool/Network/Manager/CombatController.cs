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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager
{
    class DamageObject
    {
        public string Victim { get; set; }

        public string Attacker { get; set; }

        public int Damage { get; set; }

        public string Spell { get; set; }

        public string SpellOrigin { get; set; }

        public string SpellType { get; set; }
    }

        public class CombatController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        public bool IsDamageMeterActive { get; set; } = false;

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

        enum causingSpellType : int
        {
            AutoAttack = -1,
            VialCurseCharge = 1733,
            VialCurseDot = 1736,
            HauntingScreams = 1765,
            GrudgeDot = 1744,
            Bane = 89,
            Unholy_Frenzy = 3805

        }

        public async Task AddDamageAsync(long objectId, long causerId, double healthChange, double newHealthValue, int causingSpellType, EffectOrigin effectOrigin, EffectType effectTpye)
        {
            if (!IsDamageMeterActive || objectId == causerId) return;

            if (_trackingController.EntityController.LocalUserData == null) return;

            var causerEntity = _trackingController?.EntityController?.GetEntity(causerId);
            var receiverEntity = _trackingController?.EntityController?.GetEntity(objectId);
            double receivedDamage = Math.Abs(healthChange);
            if (causerEntity?.Value == null) return;


            var filePath = @"C:\Users\gamef\Downloads\receivedDamage.json";
            // Read existing json data
            var jsonData = System.IO.File.ReadAllText(filePath);
            // De-serialize to object or create new list
            var damageList = JsonConvert.DeserializeObject<List<DamageObject>>(jsonData)
                                  ?? new List<DamageObject>();


            string causingSpellName = $"Unknown({causingSpellType})";
            if (Enum.IsDefined(typeof(causingSpellType), causingSpellType))
            {
                causingSpellName = ((causingSpellType)causingSpellType).ToString();
            }

            if (_trackingController.EntityController.LocalUserData.UserObjectId == objectId)
            {
                Debug.Print($"[CombatController] You took {receivedDamage} Damage from {causingSpellName} which was a {effectOrigin} caused by {causerEntity.Value.Value.Name}.");
                
                damageList.Add(new DamageObject()
                {
                    Victim = receiverEntity.Value.Value.Name,
                    Attacker = causerEntity.Value.Value.Name,
                    Damage = (int)receivedDamage,
                    Spell = causingSpellName,
                    SpellOrigin = effectOrigin.ToString(),
                    SpellType = effectTpye.ToString(),
                });
                jsonData = JsonConvert.SerializeObject(damageList);
                System.IO.File.WriteAllText(filePath, jsonData);

            }

            if (_trackingController.EntityController.LocalUserData.UserObjectId != objectId && _trackingController.EntityController.IsUserInParty(objectId))
            {
               
                Debug.Print($"[CombatController] {receiverEntity.Value.Value.Name} took {receivedDamage} Damage from {causingSpellName} which was a {effectOrigin} caused by {causerEntity.Value.Value.Name}.");
            }

            //check if entity that caused the damage exists
                if (causerEntity?.Value == null
                || causerEntity.Value.Value?.ObjectType != GameObjectType.Player
                || !_trackingController.EntityController.IsUserInParty(causerEntity.Value.Value.Name)
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

                causerEntity.Value.Value.Damage += damageChangeValue;
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

                causerEntity.Value.Value.Heal += (int)Math.Round(healChangeValue, MidpointRounding.AwayFromZero);
            }

            if (causerEntity.Value.Value?.CombatStart == null)
            {
                causerEntity.Value.Value.CombatStart = DateTime.UtcNow;
            }

            if (IsUiUpdateAllowed())
            {
                await UpdateDamageMeterUiAsync(_mainWindowViewModel.DamageMeter, _trackingController.EntityController.GetAllEntities());
            }
        }

        private static bool IsUiUpdateActive;

        public async Task UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter, List<KeyValuePair<Guid, PlayerGameObject>> entities)
        {
            IsUiUpdateActive = true;

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
                    await UpdateDamageMeterFragmentAsync(fragment, healthChangeObject, entities, highestDamage, highestHeal).ConfigureAwait(true);
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

            IsUiUpdateActive = false;
        }

        private async Task UpdateDamageMeterFragmentAsync(DamageMeterFragment fragment, KeyValuePair<Guid, PlayerGameObject> healthChangeObject, List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            var itemInfo = await SetItemInfoIfSlotTypeMainHandAsync(fragment.CauserMainHand, healthChangeObject.Value?.CharacterEquipment?.MainHand).ConfigureAwait(false);
            fragment.CauserMainHand = itemInfo;

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

                _trackingController.EntityController.SetPartyCircleColor(healthChangeObject.Value.UserGuid, itemInfo?.FullItemInformation?.CategoryId);
            }
        }

        private async Task AddDamageMeterFragmentAsync(ObservableCollection<DamageMeterFragment> damageMeter, KeyValuePair<Guid, PlayerGameObject> healthChangeObject,
            List<KeyValuePair<Guid, PlayerGameObject>> entities, long highestDamage, long highestHeal)
        {
            if (healthChangeObject.Value == null
                || (double.IsNaN(healthChangeObject.Value.Damage) && double.IsNaN(healthChangeObject.Value.Heal))
                || (healthChangeObject.Value.Damage <= 0 && healthChangeObject.Value.Heal <= 0))
            {
                return;
            }

            var mainHandItem = ItemController.GetItemByIndex(healthChangeObject.Value?.CharacterEquipment?.MainHand ?? 0);
            var itemInfo = await SetItemInfoIfSlotTypeMainHandAsync(mainHandItem, healthChangeObject.Value?.CharacterEquipment?.MainHand);

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
                CauserMainHand = itemInfo
            };
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                damageMeter.Add(damageMeterFragment);
            });

            _trackingController.EntityController.SetPartyCircleColor(healthChangeObject.Value.UserGuid, itemInfo?.FullItemInformation?.CategoryId);
        }

        private bool HasDamageMeterDupes(IEnumerable<DamageMeterFragment> damageMeter)
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

        private HealthChangeType GetHealthChangeType(double healthChange) => healthChange <= 0 ? HealthChangeType.Damage : HealthChangeType.Heal;

        private async Task<Item> SetItemInfoIfSlotTypeMainHandAsync(Item currentItem, int? newIndex)
        {
            if (newIndex is null or <= 0)
            {
                return currentItem;
            }

            var item = ItemController.GetItemByIndex((int)newIndex);
            if (item == null) return currentItem;

            var fullItemInfo = await ItemController.GetFullItemInformationAsync(item);
            if (ItemController.IsItemSlotType(fullItemInfo, "mainhand"))
            {
                item.FullItemInformation = fullItemInfo;
                return item;
            }

            return currentItem;
        }

        private DateTime _lastDamageUiUpdate;

        private bool IsUiUpdateAllowed(int waitTimeInSeconds = 1)
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(_lastDamageUiUpdate);
            if (difference.Seconds >= waitTimeInSeconds && !IsUiUpdateActive)
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

        private static readonly Random _random = new(DateTime.Now.Millisecond);

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
                var damage = _random.Next(-100, 100);
               // await AddDamageAsync(9999, entity.ObjectId ?? -1, damage, _random.Next(2000, 3000));
                //Debug.Print($"--- AddDamage - {entity.Name}: {damage}");

                await Task.Delay(_random.Next(1, 1000));
            }
        }

        private List<KeyValuePair<Guid, PlayerGameObject>> SetRandomDamageValues(int playerAmount)
        {
            for (var i = 0; i < playerAmount; i++)
            {
                var guid = new Guid($"{_random.Next(1000, 9999)}0000-0000-0000-0000-000000000000");
                var interactGuid = Guid.NewGuid();
                var name = TestMethods.GenerateName(_random.Next(3, 10));

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