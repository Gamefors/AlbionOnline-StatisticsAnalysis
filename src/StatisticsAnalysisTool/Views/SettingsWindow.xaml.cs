﻿using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Shortcut;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views
{

    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private readonly SettingsWindowViewModel _settingsWindowViewModel;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public SettingsWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();
            _settingsWindowViewModel = new SettingsWindowViewModel(this, mainWindowViewModel);
            DataContext = _settingsWindowViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _settingsWindowViewModel.SaveSettings();
        }

        private void OpenToolDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = Process.Start(new ProcessStartInfo { FileName = _settingsWindowViewModel.ToolDirectory, UseShellExecute = true });
            }
            catch (Exception exception)
            {
                _ = MessageBox.Show(exception.Message, LanguageController.Translation("ERROR"));
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
            }
        }

        private void CreateDesktopShortcut_Click(object sender, RoutedEventArgs e)
        {
            ShortcutController.CreateShortcut();
        }

        private void OpenDebugConsole_Click(object sender, RoutedEventArgs e)
        {
            ConsoleManager.Toggle();
        }
    }
}
