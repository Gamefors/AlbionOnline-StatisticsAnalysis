﻿using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class EntityController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly ConcurrentDictionary<Guid, PlayerGameObject> _knownEntities = new();
        private readonly ConcurrentDictionary<Guid, string> _knownPartyEntities = new();
        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ObservableCollection<EquipmentItemInternal> _newEquipmentItems = new();
        private readonly ObservableCollection<SpellEffect> _spellEffects = new();
        private readonly ConcurrentDictionary<long, CharacterEquipmentData> _tempCharacterEquipmentData = new();
        private double _lastLocalEntityGuildTaxInPercent;
        private double _lastLocalEntityClusterTaxInPercent;

        public LocalUserData LocalUserData { get; set; }

        public EntityController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindowViewModel = mainWindowViewModel;
        }
        
        #region Entities

        public event Action<GameObject> OnAddEntity;

        public void AddEntity(long objectId, Guid userGuid, Guid? interactGuid, string name, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            if (objectSubType == GameObjectSubType.LocalPlayer)
            {
                _trackingController.SetMainCharacterNameForTracking(name);
            }

            PlayerGameObject gameObject;

            if (_knownEntities.TryRemove(userGuid, out var oldEntity))
            {
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    InteractGuid = interactGuid,
                    ObjectSubType = objectSubType,
                    CharacterEquipment = oldEntity.CharacterEquipment,
                    CombatStart = oldEntity.CombatStart,
                    CombatTime = oldEntity.CombatTime,
                    Damage = oldEntity.Damage,
                    Heal = oldEntity.Heal
                };
            }
            else
            {
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    ObjectSubType = objectSubType
                };
            }

            if (_tempCharacterEquipmentData.TryGetValue(objectId, out var characterEquipmentData))
            {
                ResetTempCharacterEquipment();
                gameObject.CharacterEquipment = characterEquipmentData.CharacterEquipment;
                _tempCharacterEquipmentData.TryRemove(objectId, out _);
            }

            _knownEntities.TryAdd(gameObject.UserGuid, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveAllEntities()
        {
            foreach (var entity in _knownEntities.Where(x =>
                x.Value.ObjectSubType != GameObjectSubType.LocalPlayer && !_knownPartyEntities.ContainsKey(x.Key)))
                _knownEntities.TryRemove(entity.Key, out _);

            foreach (var entity in _knownEntities.Where(x =>
                x.Value.ObjectSubType == GameObjectSubType.LocalPlayer || _knownPartyEntities.ContainsKey(x.Key)))
                entity.Value.ObjectId = null;
        }
        
        public KeyValuePair<Guid, PlayerGameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        }

        public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntities(bool onlyInParty = false)
        {
            return new List<KeyValuePair<Guid, PlayerGameObject>>(onlyInParty ? _knownEntities.ToArray().Where(x => IsUserInParty(x.Value.Name)) : _knownEntities.ToArray());
        }

        public bool IsEntityInParty(long objectId) => GetAllEntities(true).Any(x => x.Value.ObjectId == objectId);

        public bool IsEntityInParty(string name) => GetAllEntities(true).Any(x => x.Value.Name == name);

        #endregion

        #region Party

        public async Task AddToPartyAsync(Guid guid, string username)
        {
            if (_knownPartyEntities.All(x => x.Key != guid))
            {
                _knownPartyEntities.TryAdd(guid, username);
            }

            await SetPartyMemberUiAsync();
        }

        public async Task RemoveFromParty(string username)
        {
            var partyMember = _knownPartyEntities.FirstOrDefault(x => x.Value == username);

            if (partyMember.Value != null) _knownPartyEntities.TryRemove(partyMember.Key, out _);

            await SetPartyMemberUiAsync();
        }

        public async Task ResetPartyMemberAsync()
        {
            _knownPartyEntities.Clear();

            foreach (var member in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer))
            {
                _knownPartyEntities.TryAdd(member.Key, member.Value.Name);
            }

            await SetPartyMemberUiAsync();
        }

        public async Task SetPartyAsync(Dictionary<Guid, string> party, bool resetPartyBefore = false)
        {
            if (resetPartyBefore)
            {
                await ResetPartyMemberAsync();
            }

            foreach (var member in party)
            {
                await AddToPartyAsync(member.Key, member.Value);
            }

            await SetPartyMemberUiAsync();
        }

        private async Task SetPartyMemberUiAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _mainWindowViewModel.PartyMemberCircles.Clear();
                foreach (var member in _knownPartyEntities) _mainWindowViewModel.PartyMemberCircles.Add(new PartyMemberCircle
                {
                    Name = member.Value,
                    UserGuid = member.Key
                });
                _mainWindowViewModel.PartyMemberNumber = _knownPartyEntities.Count;
            });
        }
        
        public bool IsUserInParty(string name)
        {
            return _knownPartyEntities.Any(x => x.Value == name);
        }

        public bool IsUserInParty(long objectId)
        {
            var entity = _knownEntities.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity.Value == null)
            {
                return false;
            }

            return _knownPartyEntities.Any(x => x.Value == entity.Value.Name);
        }

        #endregion

        #region Equipment

        public void SetCharacterEquipment(long objectId, CharacterEquipment equipment)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity?.Value != null)
            {
                entity.Value.Value.CharacterEquipment = equipment;
            }
            else
            {
                _tempCharacterEquipmentData.TryAdd(objectId, new CharacterEquipmentData
                {
                    CharacterEquipment = equipment,
                    TimeStamp = DateTime.UtcNow
                });
            }
        }

        public void ResetTempCharacterEquipment()
        {
            foreach (var characterEquipment in _tempCharacterEquipmentData.ToArray())
            {
                if (Utilities.IsBlockingTimeExpired(characterEquipment.Value.TimeStamp, 30))
                {
                    _tempCharacterEquipmentData.TryRemove(characterEquipment.Key, out _);
                }
            }
        }

        public void AddEquipmentItem(EquipmentItemInternal itemInternal)
        {
            lock (_newEquipmentItems)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_newEquipmentItems.ToList().Any(x => x == null || x.ItemIndex.Equals(itemInternal?.ItemIndex) && x.SpellDictionary?.Values == itemInternal.SpellDictionary?.Values))
                    {
                        return;
                    }

                    _newEquipmentItems.Add(itemInternal);
                });
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void AddSpellEffect(SpellEffect spell)
        {
            if (!IsUserInParty(spell.CauserId))
            {
                return;
            }

            lock(_spellEffects)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_spellEffects.Any(x => x == null || x.CauserId.Equals(spell.CauserId) && x.SpellIndex.Equals(spell.SpellIndex)))
                    {
                        return;
                    }

                    _spellEffects.Add(spell);
                });
            }

            RemoveSpellAndEquipmentObjects();
        }

        public void DetectUsedWeapon()
        {
            var playerItemList = new Dictionary<long, int>();

            try
            {
                lock (_newEquipmentItems)
                {
                    foreach (var item in _newEquipmentItems.ToList())
                    {
                        foreach (var spell in 
                                 (from itemSpell in item.SpellDictionary.ToArray() 
                                     from spell in _spellEffects.ToArray() where spell != null && spell.SpellIndex.Equals(itemSpell.Value) select spell).ToArray())
                        {
                            if (playerItemList.Any(x => x.Key.Equals(spell.CauserId)))
                            {
                                continue;
                            }

                            playerItemList.Add(spell.CauserId, item.ItemIndex);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }

            foreach (var (key, value) in playerItemList.ToList())
            {
                SetCharacterMainHand(key, value);
            }
        }

        private void SetCharacterMainHand(long objectId, int itemIndex)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);

            if (entity?.Value == null)
            {
                return;
            }

            if (entity.Value.Value?.CharacterEquipment == null)
            {
                entity.Value.Value.CharacterEquipment = new CharacterEquipment
                {
                    MainHand = itemIndex
                };
            }

            //if (entity.Value.Value != null)
            //{
            //    entity.Value.Value.CharacterEquipment.MainHand = itemIndex;
            //}
        }

        private void RemoveSpellAndEquipmentObjects()
        {
            lock (_newEquipmentItems)
            {
                foreach (var item in _newEquipmentItems.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _newEquipmentItems.Remove(item);
                    });
                }
            }

            lock (_spellEffects)
            {
                foreach (var spell in _spellEffects.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _spellEffects.Remove(spell);
                    });
                }
            }
        }

        public class CharacterEquipmentData
        {
            public DateTime TimeStamp { get; set; }
            public CharacterEquipment CharacterEquipment { get; set; }
        }

        #endregion

        #region Damage

        public void ResetEntitiesDamageStartTime()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.CombatStart = null;
            }
        }

        public void ResetEntitiesDamageTimes()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.ResetCombatTimes();
            }
        }

        public void ResetEntitiesDamage()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.Damage = 0;
            }
        }

        public void ResetEntitiesHeal()
        {
            foreach (var entity in _knownEntities)
            {
                entity.Value.Heal = 0;
            }
        }

        #endregion

        #region Health

        public void HealthUpdate(
            long objectId,
            GameTimeStamp timeStamp,
            double healthChange,
            double newHealthValue,
            EffectType effectType,
            EffectOrigin effectOrigin,
            long causerId,
            int causingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                timeStamp,
                healthChange,
                newHealthValue,
                effectType,
                effectOrigin,
                causerId,
                causingSpellType
            );
        }

        public event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;

        #endregion

        #region Local Entity

        public FixPoint GetLastLocalEntityClusterTax(FixPoint yieldPreClusterTax) => FixPoint.FromFloatingPointValue(yieldPreClusterTax.DoubleValue / 100 * _lastLocalEntityClusterTaxInPercent);

        public void SetLastLocalEntityClusterTax(FixPoint yieldPreTax, FixPoint clusterTax)
        {
            _lastLocalEntityClusterTaxInPercent = (100 / yieldPreTax.DoubleValue) * clusterTax.DoubleValue;
        }

        public void SetLastLocalEntityGuildTax(FixPoint yieldPreTax, FixPoint guildTax)
        {
            _lastLocalEntityGuildTaxInPercent = (100 / yieldPreTax.DoubleValue) * guildTax.DoubleValue;
        }

        public FixPoint GetLastLocalEntityGuildTax(FixPoint yieldPreTax) => FixPoint.FromFloatingPointValue(yieldPreTax.DoubleValue / 100 * _lastLocalEntityGuildTaxInPercent);

        public bool ExistLocalEntity()
        {
            return _knownEntities?.Any(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer) ?? false;
        }

        public KeyValuePair<Guid, PlayerGameObject>? GetLocalEntity() => _knownEntities?.ToArray().FirstOrDefault(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer);

        #endregion
    }
}