﻿using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models
{
    public class SettingsWindowTranslation
    {
        public string Settings => LanguageController.Translation("SETTINGS");
        public string Language => LanguageController.Translation("LANGUAGE");
        public string RefreshRate => LanguageController.Translation("REFRESH_RATE");
        public string UpdateItemListByDays => LanguageController.Translation("UPDATE_ITEM_LIST_BY_DAYS");
        public string ItemListSourceUrl => LanguageController.Translation("ITEM_LIST_SOURCE_URL");
        public string OpenItemWindowInNewWindow => LanguageController.Translation("OPEN_ITEM_WINDOW_IN_NEW_WINDOW");
        public string ShowInfoWindowOnStart => LanguageController.Translation("SHOW_INFO_WINDOW_ON_START");
        public string Save => LanguageController.Translation("SAVE");
        public string FullItemInformationUpdateCycleDays => LanguageController.Translation("FULL_ITEM_INFO_UPDATE_CYCLE_DAYS");
        public string AlarmSoundUsed => LanguageController.Translation("ALARM_SOUND_USED");
        public string ToolDirectory => LanguageController.Translation("TOOL_DIRECTORY");
        public string OpenToolDirectory => LanguageController.Translation("OPEN_TOOL_DIRECTORY");
        public string OpenCloseDebugConsole => LanguageController.Translation("OPEN_OR_CLOSE_DEBUG_CONSOLE");
        public string CreateDesktopShortcut => LanguageController.Translation("CREATE_DESKTOP_SHORTCUT");
        public string CityPricesApiUrl => LanguageController.Translation("CITY_PRICES_API_URL");
        public string CityPricesHistoryApiUrl => LanguageController.Translation("CITY_PRICES_HISTORY_API_URL");
        public string GoldStatsApiUrl => LanguageController.Translation("GOLD_STATS_API_URL");
        public string AutomaticLootLoggerSave => LanguageController.Translation("AUTOMATIC_LOOT_LOGGER_SAVE");
        public string SavePathForAutomaticLootLoggerSave => LanguageController.Translation("SAVE_PATH_FOR_AUTOMATIC_LOOT_LOGGER_SAVE");
    }
}