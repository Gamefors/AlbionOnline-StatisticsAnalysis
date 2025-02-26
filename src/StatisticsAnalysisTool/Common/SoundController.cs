﻿using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;

namespace StatisticsAnalysisTool.Common
{
    public class SoundController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public static List<FileInformation> AlertSounds { get; set; } = new();

        public static void InitializeSoundFilesFromDirectory()
        {
            if (AlertSounds?.Count > 0)
            {
                return;
            }

            var soundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SoundDirectoryName);

            if (!Directory.Exists(soundFilePath))
            {
                return;
            }

            var files = DirectoryController.GetFiles(soundFilePath, "*.wav");

            if (files == null)
            {
                return;
            }

            AlertSounds ??= new List<FileInformation>();

            foreach (var file in files)
            {
                var fileInformation = new FileInformation(Path.GetFileNameWithoutExtension(file), file);
                AlertSounds.Add(fileInformation);
            }
        }

        public static void PlayAlertSound()
        {
            try
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var player = new SoundPlayer(GetCurrentSound());
                player.Load();
                player.Play();
                player.Dispose();
#pragma warning restore CA1416 // Validate platform compatibility
            }
            catch (Exception e) when (e is InvalidOperationException || e is UriFormatException || e is FileNotFoundException ||
                                      e is ArgumentException)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        private static string GetCurrentSound()
        {
            try
            {
                var currentSound = AlertSounds.FirstOrDefault(s => s.FileName == SettingsController.CurrentSettings.SelectedAlertSound);
                return currentSound?.FilePath ?? string.Empty;
            }
            catch (Exception e) when (e is ArgumentException)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return string.Empty;
            }
        }
    }
}