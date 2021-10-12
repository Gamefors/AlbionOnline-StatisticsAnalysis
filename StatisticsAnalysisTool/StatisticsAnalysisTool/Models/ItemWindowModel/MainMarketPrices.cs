using StatisticsAnalysisTool.Common;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Models.ItemWindowModel
{
    public class MainMarketPrices : INotifyPropertyChanged
    {
        private static readonly DateTime zeroDate = new (1,1,1,0,0,0);

        private string _itemTypeId;
        private Location _location = Location.Unknown;
        private string _locationName;
        private int _qualityLevel;
        private ulong _sellPriceMin;
        private DateTime _sellPriceMinDate;
        private ulong _sellPriceMax;
        private DateTime _sellPriceMaxDate;
        private ulong _buyPriceMin;
        private DateTime _buyPriceMinDate;
        private ulong _buyPriceMax;
        private DateTime _buyPriceMaxDate;
        private string _sellPriceMinDateLastUpdateTime;
        private string _sellPriceMaxDateLastUpdateTime;
        private string _buyPriceMinDateLastUpdateTime;
        private string _buyPriceMaxDateLastUpdateTime;
        private string _sellPriceMinDateString;
        private string _sellPriceMaxDateString;
        private string _buyPriceMinDateString;
        private string _buyPriceMaxDateString;
        private bool _bestSellMinPrice;
        private bool _bestSellMaxPrice;
        private bool _bestBuyMinPrice;
        private bool _bestBuyMaxPrice;
        private Style _sellPriceMinStyle = Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
        private Style _buyPriceMaxStyle = Application.Current.FindResource("ListView.Grid.StackPanel.Label.Price") as Style;
        private Style _sellPriceMinDateStyle;

        public MainMarketPrices(MarketResponse marketResponse)
        {
            SellPriceMinDate = zeroDate;
            SellPriceMaxDate = zeroDate;
            BuyPriceMinDate = zeroDate;
            BuyPriceMaxDate = zeroDate;
            SetValues(marketResponse);
        }

        public void SetValues(MarketResponse marketResponse)
        {
            ItemTypeId = marketResponse.ItemTypeId;

            if (Location == Location.Unknown)
            {
                Location = Locations.GetName(marketResponse.City);
            }

            QualityLevel = marketResponse.QualityLevel;

            if (marketResponse.SellPriceMinDate > SellPriceMinDate)
            {
                SellPriceMin = marketResponse.SellPriceMin;
                SellPriceMinDate = marketResponse.SellPriceMinDate;
            }

            if (marketResponse.SellPriceMaxDate > SellPriceMaxDate)
            {
                SellPriceMax = marketResponse.SellPriceMax;
                SellPriceMaxDate = marketResponse.SellPriceMaxDate;
            }

            if (marketResponse.BuyPriceMinDate > BuyPriceMinDate)
            {
                BuyPriceMin = marketResponse.BuyPriceMin;
                BuyPriceMinDate = marketResponse.BuyPriceMinDate;
            }

            if (marketResponse.BuyPriceMaxDate > BuyPriceMaxDate)
            {
                BuyPriceMax = marketResponse.BuyPriceMax;
                BuyPriceMaxDate = marketResponse.BuyPriceMaxDate;
            }
        }

        public string ItemTypeId
        {
            get => _itemTypeId;
            set
            {
                _itemTypeId = value;
                OnPropertyChanged();
            }
        }

        public Location Location
        {
            get => _location;
            set
            {
                _location = value;
                LocationName = Locations.GetName(Location);
                OnPropertyChanged();
            }
        }

        public string LocationName
        {
            get => _locationName;
            set
            {
                _locationName = value;
                OnPropertyChanged();
            }
        }

        public int QualityLevel
        {
            get => _qualityLevel;
            set
            {
                _qualityLevel = value;
                OnPropertyChanged();
            }
        }

        public ulong SellPriceMin
        {
            get => _sellPriceMin;
            set
            {
                _sellPriceMin = value;
                OnPropertyChanged();
            }
        }

        public DateTime SellPriceMinDate
        {
            get => _sellPriceMinDate;
            set
            {
                _sellPriceMinDate = value;
                SellPriceMinDateLastUpdateTime = Formatting.DateTimeToLastUpdateTime(value);
                SellPriceMinDateString = Formatting.CurrentDateTimeFormat(value);
                SellPriceMinDateStyle = ItemController.GetStyleByTimestamp(value);
                OnPropertyChanged();
            }
        }

        public string SellPriceMinDateString
        {
            get => _sellPriceMinDateString;
            set
            {
                _sellPriceMinDateString = value;
                OnPropertyChanged();
            }
        }

        public string SellPriceMinDateLastUpdateTime
        {
            get => _sellPriceMinDateLastUpdateTime;
            set
            {
                _sellPriceMinDateLastUpdateTime = value;
                OnPropertyChanged();
            }
        }

        public ulong SellPriceMax
        {
            get => _sellPriceMax;
            set
            {
                _sellPriceMax = value;
                OnPropertyChanged();
            }
        }

        public DateTime SellPriceMaxDate
        {
            get => _sellPriceMaxDate;
            set
            {
                _sellPriceMaxDate = value;
                SellPriceMaxDateLastUpdateTime = Formatting.DateTimeToLastUpdateTime(value);
                SellPriceMaxDateString = Formatting.CurrentDateTimeFormat(value);
                OnPropertyChanged();
            }
        }

        public string SellPriceMaxDateString
        {
            get => _sellPriceMaxDateString;
            set
            {
                _sellPriceMaxDateString = value;
                OnPropertyChanged();
            }
        }

        public string SellPriceMaxDateLastUpdateTime
        {
            get => _sellPriceMaxDateLastUpdateTime;
            set
            {
                _sellPriceMaxDateLastUpdateTime = value;
                OnPropertyChanged();
            }
        }

        public ulong BuyPriceMin
        {
            get => _buyPriceMin;
            set
            {
                _buyPriceMin = value;
                OnPropertyChanged();
            }
        }

        public DateTime BuyPriceMinDate
        {
            get => _buyPriceMinDate;
            set
            {
                _buyPriceMinDate = value;
                BuyPriceMinDateLastUpdateTime = Formatting.DateTimeToLastUpdateTime(value);
                BuyPriceMaxDateString = Formatting.CurrentDateTimeFormat(value);
                OnPropertyChanged();
            }
        }

        public string BuyPriceMinDateString
        {
            get => _buyPriceMinDateString;
            set
            {
                _buyPriceMinDateString = value;
                OnPropertyChanged();
            }
        }

        public string BuyPriceMinDateLastUpdateTime
        {
            get => _buyPriceMinDateLastUpdateTime;
            set
            {
                _buyPriceMinDateLastUpdateTime = value;
                OnPropertyChanged();
            }
        }

        public ulong BuyPriceMax
        {
            get => _buyPriceMax;
            set
            {
                _buyPriceMax = value;
                OnPropertyChanged();
            }
        }

        public DateTime BuyPriceMaxDate
        {
            get => _buyPriceMaxDate;
            set
            {
                _buyPriceMaxDate = value;
                BuyPriceMaxDateLastUpdateTime = Formatting.DateTimeToLastUpdateTime(value);
                BuyPriceMaxDateString = Formatting.CurrentDateTimeFormat(value);
                OnPropertyChanged();
            }
        }

        public string BuyPriceMaxDateString
        {
            get => _buyPriceMaxDateString;
            set
            {
                _buyPriceMaxDateString = value;
                OnPropertyChanged();
            }
        }

        public string BuyPriceMaxDateLastUpdateTime
        {
            get => _buyPriceMaxDateLastUpdateTime;
            set
            {
                _buyPriceMaxDateLastUpdateTime = value;
                OnPropertyChanged();
            }
        }

        public bool BestSellMinPrice
        {
            get => _bestSellMinPrice;
            set
            {
                _bestSellMinPrice = value;
                SellPriceMinStyle = ItemController.PriceStyle(value);
                OnPropertyChanged();
            }
        }

        public bool BestSellMaxPrice
        {
            get => _bestSellMaxPrice;
            set
            {
                _bestSellMaxPrice = value;
                OnPropertyChanged();
            }
        }

        public bool BestBuyMinPrice
        {
            get => _bestBuyMinPrice;
            set
            {
                _bestBuyMinPrice = value;
                OnPropertyChanged();
            }
        }

        public bool BestBuyMaxPrice
        {
            get => _bestBuyMaxPrice;
            set
            {
                _bestBuyMaxPrice = value;
                BuyPriceMaxStyle = ItemController.PriceStyle(value);
                OnPropertyChanged();
            }
        }

        public Style SellPriceMinStyle
        {
            get => _sellPriceMinStyle;
            set
            {
                _sellPriceMinStyle = value;
                OnPropertyChanged();
            }
        }

        public Style BuyPriceMaxStyle
        {
            get => _buyPriceMaxStyle;
            set
            {
                _buyPriceMaxStyle = value;
                OnPropertyChanged();
            }
        }

        public Style SellPriceMinDateStyle
        {
            get => _sellPriceMinDateStyle;
            set
            {
                _sellPriceMinDateStyle = value;
                OnPropertyChanged();
            }
        }

        public Style LocationStyle => ItemController.LocationStyle(Location);

        public Style SellPriceMaxDateStyle => ItemController.GetStyleByTimestamp(SellPriceMaxDate);

        public Style BuyPriceMinDateStyle => ItemController.GetStyleByTimestamp(BuyPriceMinDate);

        public Style BuyPriceMaxDateStyle => ItemController.GetStyleByTimestamp(BuyPriceMaxDate);


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}