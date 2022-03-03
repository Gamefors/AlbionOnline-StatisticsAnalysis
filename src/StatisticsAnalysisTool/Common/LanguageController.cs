﻿using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Xml;

namespace StatisticsAnalysisTool.Common
{
    public static class LanguageController
    {
        private static readonly Dictionary<string, string> _translations = new();
        private static CultureInfo _currentCultureInfo;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public static List<FileInformation> LanguageFiles { get; set; }

        public static CultureInfo CurrentCultureInfo
        {
            get => _currentCultureInfo;
            set
            {
                _currentCultureInfo = value;
                SettingsController.CurrentSettings.CurrentLanguageCultureName = value.TextInfo.CultureName;
                try
                {
                    Thread.CurrentThread.CurrentUICulture = value;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }
        }

        public static bool InitializeLanguage()
        {
            try
            {
                if (CurrentCultureInfo == null)
                {
                    if (!string.IsNullOrEmpty(SettingsController.CurrentSettings.CurrentLanguageCultureName))
                    {
                        CurrentCultureInfo = new CultureInfo(SettingsController.CurrentSettings.CurrentLanguageCultureName);
                    }
                    else if (!string.IsNullOrEmpty(Settings.Default.DefaultLanguageCultureName))
                    {
                        CurrentCultureInfo = new CultureInfo(Settings.Default.DefaultLanguageCultureName);
                    }
                    else
                    {
                        throw new CultureNotFoundException();
                    }
                }

                if (SetLanguage())
                {
                    return true;
                }

                CurrentCultureInfo = new CultureInfo(Settings.Default.DefaultLanguageCultureName);
                if (SetLanguage())
                {
                    return true;
                }

                throw new CultureNotFoundException();
            }
            catch (CultureNotFoundException)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, new CultureNotFoundException());
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, new CultureNotFoundException());
                MessageBox.Show("No culture info found!", Translation("ERROR"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public static string Translation(string key)
        {
            try
            {
                if (_translations.TryGetValue(key, out var value))
                {
                    return !string.IsNullOrEmpty(value) ? value : key;
                }
            }
            catch (ArgumentNullException)
            {
                return "TRANSLATION-ERROR";
            }

            return key;
        }

        public static bool SetLanguage()
        {
            InitializeLanguageFilesFromDirectory();

            try
            {
                if (LanguageFiles == null)
                {
                    throw new FileNotFoundException();
                }

                var fileInfos = (from file in LanguageFiles
                    where file.FileName.ToUpper() == CurrentCultureInfo?.TextInfo.CultureName.ToUpper()
                    select new FileInformation(file.FileName, file.FilePath)).FirstOrDefault();

                if (fileInfos == null)
                {
                    return false;
                }

                if (!ReadAndAddLanguageFile(fileInfos.FilePath))
                {
                    return false;
                }

                return true;
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show(e.Message, Translation("ERROR"));
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return false;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Language file not found. ", Translation("ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                return false;
            }
        }

        private static bool ReadAndAddLanguageFile(string filePath)
        {
            try
            {
                _translations.Clear();
                var xmlReader = XmlReader.Create(filePath);
                while (xmlReader.Read())
                    if (xmlReader.Name == "translation" && xmlReader.HasAttributes)
                        AddTranslationsToDictionary(xmlReader);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Translation("ERROR"));
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return false;
            }

            return true;
        }

        private static void AddTranslationsToDictionary(XmlReader xmlReader)
        {
            while (xmlReader.MoveToNextAttribute())
            {
                if (_translations.ContainsKey(xmlReader.Value))
                {
                    Log.Warn($"{nameof(AddTranslationsToDictionary)}: {Translation("DOUBLE_VALUE_EXISTS_IN_THE_LANGUAGE_FILE")}: {xmlReader.Value}");
                }
                else if (xmlReader.Name == "name")
                {
                    _translations.Add(xmlReader.Value, xmlReader.ReadString());
                }
            }
        }

        private static void InitializeLanguageFilesFromDirectory()
        {
            if (LanguageFiles != null)
            {
                return;
            }

            var languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName);
            if (!Directory.Exists(languageFilePath))
            {
                return;
            }

            var files = DirectoryController.GetFiles(languageFilePath, "*.xml");
            if (files == null)
            {
                return;
            }

            LanguageFiles ??= new List<FileInformation>();

            foreach (var file in files)
            {
                var fileInfo = new FileInformation(Path.GetFileNameWithoutExtension(file), file);
                LanguageFiles.Add(fileInfo);
            }
        }
    }
}