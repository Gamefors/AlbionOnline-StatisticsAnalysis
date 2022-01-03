using System;
using System.Linq;
using System.Reflection;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Views;
using Discord.Webhook;
using Discord;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for DamageMeterControl.xaml
    /// </summary>
    public partial class DamageMeterControl
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private DiscordWebhookClient DiscordWebhookClient = null;
        public DamageMeterControl()
        {
            InitializeComponent();
        }
        
        public void ResetDamageMeter()
        {
            var dialog = new DialogWindow(LanguageController.Translation("RESET_DAMAGE_METER"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_DAMAGE_METER"));
            var dialogResult = dialog.ShowDialog();

            if (dialogResult is true)
            {
                var vm = (MainWindowViewModel)DataContext;
                vm?.TrackingController?.CombatController?.ResetDamageMeter();
            }
        }

        public void OpenDamageMeterWindow()
        {
            try
            {
                if (Utilities.IsWindowOpen<DamageMeterWindow>())
                {
                    var existItemWindow = Application.Current.Windows.OfType<DamageMeterWindow>().FirstOrDefault();
                    existItemWindow?.Activate();
                }
                else
                {
                    var vm = (MainWindowViewModel)DataContext;
                    var itemWindow = new DamageMeterWindow(vm?.DamageMeter);
                    itemWindow.Show();
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public void CopyDamageMeterToClipboard()
        {
            var output = string.Empty;
            var counter = 1;

            var vm = (MainWindowViewModel)DataContext;

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Damage)
            {
                Clipboard.SetDataObject(vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage}({entity.DamagePercentage:N2}%)|{entity.Dps:N2} DPS\n"));
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Dps)
            {
                Clipboard.SetDataObject(vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Dps:N2} DPS\n"));
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Heal)
            {
                Clipboard.SetDataObject(vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Heal} Heal\n"));
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Hps)
            {
                Clipboard.SetDataObject(vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Hps:N2} HPS\n"));
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Name)
            {
                Clipboard.SetDataObject(vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage}({entity.DamagePercentage:N2}%)|{entity.Dps:N2} DPS\n"));
            }
        }

        public void PostDamageToDiscord()
        {
            var vm = (MainWindowViewModel)DataContext;
            var output = string.Empty;
            var counter = 1;
            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Damage)
            {
                output = vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage} ({entity.DamagePercentage:N2}%) | {entity.Dps:N2} DPS\n");
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Dps)
            {
                output = vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Dps:N2} DPS\n");
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Heal)
            {
                output = vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Heal} Heal\n");
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Hps)
            {
                output = vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Hps:N2} HPS\n");
            }

            if (vm?.DamageMeterSortSelection.DamageMeterSortType == DamageMeterSortType.Name)
            {
                output = vm.DamageMeter.Aggregate(output, (current, entity) => current + $"{counter++}. {entity.Name}: {entity.Damage} ({entity.DamagePercentage:N2}%) | {entity.Dps:N2} DPS\n");
            }


            if (vm?.DiscordWebhookUrl.Length != 0 && output.Length != 0)
            {
                DiscordWebhookClient = new DiscordWebhookClient(vm?.DiscordWebhookUrl);
                var embed = new EmbedBuilder
                {
                    Title = $"AlbionOnline-StatisticsAnalysis | Sort by {vm?.DamageMeterSortSelection.DamageMeterSortType.ToString()} | Posted by {vm?.TrackingUsername}",
                    Description = output,
                    Color = Color.Red
                };

                DiscordWebhookClient.SendMessageAsync(embeds: new[] { embed.Build() });

            }
            
        }

        public void DamageMeterActivationToggle()
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.IsDamageMeterTrackingActive = !vm.IsDamageMeterTrackingActive;
            }
        }

        #region Ui events

        private void BtnDamageMeterReset_Click(object sender, RoutedEventArgs e)
        {
            ResetDamageMeter();
        }

        private void OpenDamageMeterInfoPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsDamageMeterPopupVisible = Visibility.Visible;
        }

        private void CloseDamageMeterInfoPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsDamageMeterPopupVisible = Visibility.Hidden;
        }

        private void OpenDamageMeterWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenDamageMeterWindow();
        }

        private void CopyDamageMeterToClipboard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            CopyDamageMeterToClipboard();
        }

        private void PostDamageMeterToDiscord_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PostDamageToDiscord();
        }

        private void DamageMeterModeActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DamageMeterActivationToggle();
        }

        #endregion
    }
}
