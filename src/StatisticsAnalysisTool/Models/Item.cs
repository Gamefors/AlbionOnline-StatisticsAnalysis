﻿using FontAwesome5;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShopCategory = StatisticsAnalysisTool.Common.ShopCategory;

namespace StatisticsAnalysisTool.Models
{
    public class Item
    {
        private BitmapImage _icon;
        public string LocalizationNameVariable { get; set; }
        public string LocalizationDescriptionVariable { get; set; }
        public LocalizedNames LocalizedNames { get; set; }
        public int Index { get; set; }
        public string UniqueName { get; set; }

        public string LocalizedNameAndEnglish => LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper() == "EN-US"
            ? $"{ItemController.LocalizedName(LocalizedNames, null, UniqueName)}{GetUniqueNameIfDebug()}"
            : $"{ItemController.LocalizedName(LocalizedNames, null, UniqueName)}\n{ItemController.LocalizedName(LocalizedNames, "EN-US", string.Empty)}{GetUniqueNameIfDebug()}";

        public string LocalizedName => ItemController.LocalizedName(LocalizedNames, null, UniqueName);

        public int Level => ItemController.GetItemLevel(UniqueName);
        public int Tier => ItemController.GetItemTier(this);

        public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetItemImage(UniqueName));

        public ItemJsonObject FullItemInformation { get; set; }
        public ShopCategory ShopCategory { get; set; }
        public ShopSubCategory ShopShopSubCategory1 { get; set; }
        public int AlertModeMinSellPriceIsUndercutPrice { get; set; }
        public bool IsAlertActive { get; set; }
        public bool IsFavorite { get; set; }
        public EFontAwesomeIcon AlertToggle => IsAlertActive ? EFontAwesomeIcon.Solid_ToggleOn : EFontAwesomeIcon.Solid_ToggleOff;
        public Brush AlertToggleColor => IsAlertActive ? ItemController.ToggleOnColor : ItemController.ToggleOffColor;
        private string GetUniqueNameIfDebug()
        {
#if DEBUG
            return $"\n{UniqueName}";
#else
            return string.Empty;
#endif
        }
    }
}