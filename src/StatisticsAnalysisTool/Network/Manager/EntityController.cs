﻿using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Divis.AsyncObservableCollection;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class EntityController
    {
        private readonly ConcurrentDictionary<Guid, PlayerGameObject> _knownEntities = new();
        private readonly ConcurrentDictionary<Guid, string> _knownPartyEntities = new();
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly AsyncObservableCollection<EquipmentItem> _newEquipmentItems = new();
        private readonly AsyncObservableCollection<SpellEffect> _spellEffects = new();
        private readonly ConcurrentDictionary<long, CharacterEquipmentData> _tempCharacterEquipmentData = new();
        private double _lastLocalEntityGuildTaxInPercent;
        private double _lastLocalEntityClusterTaxInPercent;

        public LocalUserData LocalUserData { get; set; }

        public EntityController(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        #region Entities

        public event Action<GameObject> OnAddEntity;

        public void AddEntity(long objectId, Guid userGuid, Guid? interactGuid, string name, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            PlayerGameObject gameObject;

            if (_knownEntities.TryRemove(userGuid, out var oldEntity))
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
            else
                gameObject = new PlayerGameObject(objectId)
                {
                    Name = name,
                    ObjectType = objectType,
                    UserGuid = userGuid,
                    ObjectSubType = objectSubType
                };

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

        public bool ExistLocalEntity()
        {
            return _knownEntities?.Any(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer) ?? false;
        }

        public KeyValuePair<Guid, PlayerGameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        }

        public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntities(bool onlyInParty = false)
        {
            return onlyInParty ? _knownEntities.Where(x => IsUserInParty(x.Value.Name)).ToList() : _knownEntities.ToList();
        }

        public bool IsEntityInParty(long objectId) => GetAllEntities(true).Any(x => x.Value.ObjectId == objectId);

        public bool IsEntityInParty(string name) => GetAllEntities(true).Any(x => x.Value.Name == name);

        public KeyValuePair<Guid, PlayerGameObject>? GetLocalEntity() => _knownEntities?.FirstOrDefault(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer);

        #endregion

        #region Party

        public async Task AddToPartyAsync(Guid guid, string username)
        {
            if (_knownPartyEntities.All(x => x.Key != guid)) _knownPartyEntities.TryAdd(guid, username);

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

        public void SetPartyCircleColor(Guid userGuid, string weaponCategoryId)
        {
            var memberObject = _mainWindowViewModel?.PartyMemberCircles?.FirstOrDefault(x => x.UserGuid == userGuid);
            if (memberObject != null && memberObject.WeaponCategoryId != weaponCategoryId)
            {
                memberObject.WeaponCategoryId = weaponCategoryId;
            }

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
                entity.Value.Value.CharacterEquipment = equipment;
            else
                _tempCharacterEquipmentData.TryAdd(objectId, new CharacterEquipmentData
                {
                    CharacterEquipment = equipment,
                    TimeStamp = DateTime.UtcNow
                });
        }

        public void ResetTempCharacterEquipment()
        {
            foreach (var characterEquipment in _tempCharacterEquipmentData)
                if (Utilities.IsBlockingTimeExpired(characterEquipment.Value.TimeStamp, 30))
                    _tempCharacterEquipmentData.TryRemove(characterEquipment.Key, out _);
        }

        public void AddEquipmentItem(EquipmentItem item)
        {
            if (_newEquipmentItems.ToList().Any(x => x == null || x.ItemIndex.Equals(item?.ItemIndex) && x.SpellDictionary?.Values == item?.SpellDictionary?.Values)
                || item == null)
            {
                return;
            }

            _newEquipmentItems.Init(Application.Current.Dispatcher.Invoke);
            _newEquipmentItems.Add(item);

            RemoveSpellAndEquipmentObjects();
        }

        public void AddSpellEffect(SpellEffect spell)
        {
            if (!IsUserInParty(spell.CauserId))
            {
                return;
            }

            if (_spellEffects.Any(x => x == null || x.CauserId.Equals(spell.CauserId) && x.SpellIndex.Equals(spell.SpellIndex)))
            {
                return;
            }

            _spellEffects.Init(Application.Current.Dispatcher.Invoke);
            _spellEffects.Add(spell);

            RemoveSpellAndEquipmentObjects();
        }

        public void DetectUsedWeapon()
        {
            var playerItemList = new Dictionary<long, int>();

            foreach (var item in _newEquipmentItems.ToList())
            {
                foreach (var spell in (from itemSpell in item.SpellDictionary from spell in _spellEffects where spell.SpellIndex.Equals(itemSpell.Value) select spell).ToList())
                {
                    if (playerItemList.Any(x => x.Key.Equals(spell.CauserId)))
                    {
                        continue;
                    }

                    playerItemList.Add(spell.CauserId, item.ItemIndex);
                }
            }

            foreach (var (key, value) in playerItemList.ToList())
            {
                SetCharacterMainHand(key, value);
            }
        }

        private void SetCharacterMainHand(long objectId, int itemIndex)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);

            if (entity?.Value == null) return;

            if (entity.Value.Value?.CharacterEquipment == null)
                entity.Value.Value.CharacterEquipment = new CharacterEquipment
                {
                    MainHand = itemIndex
                };

            //if (entity.Value.Value != null)
            //{
            //    entity.Value.Value.CharacterEquipment.MainHand = itemIndex;
            //}
        }

        private void RemoveSpellAndEquipmentObjects()
        {
            foreach (var item in _newEquipmentItems.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)))
                lock (item)
                {
                    _newEquipmentItems.Init(Application.Current.Dispatcher.Invoke);
                    _newEquipmentItems.Remove(item);
                }

            foreach (var spell in _spellEffects.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-15)))
                lock (spell)
                {
                    _spellEffects.Init(Application.Current.Dispatcher.Invoke);
                    _spellEffects.Remove(spell);
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

        #endregion

        #region Health

        public void HealthUpdate(
            long objectId,
            GameTimeStamp TimeStamp,
            double HealthChange,
            double NewHealthValue,
            EffectType EffectType,
            EffectOrigin EffectOrigin,
            long CauserId,
            int CausingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                TimeStamp,
                HealthChange,
                NewHealthValue,
                EffectType,
                EffectOrigin,
                CauserId,
                CausingSpellType
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

        #endregion
    }
}