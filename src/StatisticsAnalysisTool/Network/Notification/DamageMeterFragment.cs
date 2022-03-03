﻿using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class DamageMeterFragment : INotifyPropertyChanged
    {
        private string _shopSubCategory;
        private Guid _causerGuid;
        private Item _causerMainHand;
        private long _damage;
        private double _damageInPercent;
        private double _damagePercentage;
        private double _dps;
        private string _dpsString;
        private string _name;
        private long _heal;
        private string _hpsString;
        private double _hps;
        private double _healInPercent;
        private double _healPercentage;
        private bool _isDamageMeterShowing = true;
        private string _damageShortString;
        private string _healShortString;

        public DamageMeterFragment(DamageMeterFragment damageMeterFragment)
        {
            CauserGuid = damageMeterFragment.CauserGuid;
            Damage = damageMeterFragment.Damage;
            Dps = damageMeterFragment.Dps;
            DamageInPercent = damageMeterFragment.DamageInPercent;
            DamagePercentage = damageMeterFragment.DamagePercentage;
            Heal = damageMeterFragment.Heal;
            Hps = damageMeterFragment.Hps;
            HealInPercent = damageMeterFragment.HealInPercent;
            HealPercentage = damageMeterFragment.HealPercentage;
            Name = damageMeterFragment.Name;
            CauserMainHand = damageMeterFragment.CauserMainHand;
        }

        public DamageMeterFragment()
        {
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public Guid CauserGuid
        {
            get => _causerGuid;
            set
            {
                _causerGuid = value;
                OnPropertyChanged();
            }
        }

        public bool IsDamageMeterShowing {
            get => _isDamageMeterShowing;
            set
            {
                _isDamageMeterShowing = value;
                OnPropertyChanged();
            }
        }

        #region Damage

        public long Damage {
            get => _damage;
            set {
                _damage = value;
                DamageShortString = _damage.ToShortNumberString();
                OnPropertyChanged();
            }
        }
        
        public string DamageShortString {
            get => _damageShortString;
            private set {
                _damageShortString = value;
                OnPropertyChanged();
            }
        }
        
        public string DpsString {
            get => _dpsString;
            private set {
                _dpsString = value;
                OnPropertyChanged();
            }
        }

        public double Dps {
            get => _dps;
            set {
                _dps = value;
                DpsString = _dps.ToShortNumberString();
                OnPropertyChanged();
            }
        }

        public double DamageInPercent {
            get => _damageInPercent;
            set {
                _damageInPercent = value;
                OnPropertyChanged();
            }
        }

        public double DamagePercentage {
            get => _damagePercentage;
            set {
                _damagePercentage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Heal

        public long Heal {
            get => _heal;
            set {
                _heal = value;
                HealShortString = _heal.ToShortNumberString();
                OnPropertyChanged();
            }
        }
        
        public string HealShortString {
            get => _healShortString;
            private set {
                _healShortString = value;
                OnPropertyChanged();
            }
        }
        
        public string HpsString {
            get => _hpsString;
            private set {
                _hpsString = value;
                OnPropertyChanged();
            }
        }

        public double Hps {
            get => _hps;
            set {
                _hps = value;
                HpsString = _hps.ToShortNumberString();
                OnPropertyChanged();
            }
        }

        public double HealInPercent {
            get => _healInPercent;
            set {
                _healInPercent = value;
                OnPropertyChanged();
            }
        }

        public double HealPercentage {
            get => _healPercentage;
            set {
                _healPercentage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public Item CauserMainHand
        {
            get => _causerMainHand;
            set
            {
                _causerMainHand = value;
                ShopSubCategory = CategoryController.ShopSubCategoryToShopSubCategoryString(_causerMainHand?.ShopShopSubCategory1 ?? Common.ShopSubCategory.Unknown);
                OnPropertyChanged();
            }
        }

        public string ShopSubCategory
        {
            get => _shopSubCategory;
            set
            {
                _shopSubCategory = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            return obj is DamageMeterFragment damageMeterFragment && Name == damageMeterFragment.Name
                                                                  && Damage == damageMeterFragment.Damage
                                                                  && CauserGuid == damageMeterFragment.CauserGuid
                                                                  && Heal == damageMeterFragment.Heal;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, CauserGuid, Damage, Heal);
        }
    }
}