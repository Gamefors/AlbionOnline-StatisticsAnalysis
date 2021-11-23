﻿using log4net;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StatisticsAnalysisTool.Common
{
    public class AlertController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly ObservableCollection<Alert> _alerts = new ();
        private readonly ICollectionView _itemsView;
        private readonly MainWindow _mainWindow;

        private readonly int _maxAlertsAtSameTime = 10;

        public AlertController(MainWindow mainWindow, ICollectionView itemsView)
        {
            _mainWindow = mainWindow;
            _itemsView = itemsView;

            SetActiveAlertsFromLocalFile();
        }

        private void Add(Item item, int alertModeMinSellPriceIsUndercutPrice)
        {
            if (IsAlertInCollection(item.UniqueName) || !IsSpaceInAlertsCollection()) return;

            _alerts.CollectionChanged += delegate(object _, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add) SaveActiveAlertsToLocalFile();
            };

            var alertController = this;
            var alert = new Alert(_mainWindow, alertController, item, alertModeMinSellPriceIsUndercutPrice);
            alert.StartEvent();
            _alerts.Add(alert);
        }

        private void Remove(string uniqueName)
        {
            _alerts.CollectionChanged += delegate(object _, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove) SaveActiveAlertsToLocalFile();
            };

            var alert = GetAlertByUniqueName(uniqueName);
            if (alert != null)
            {
                alert.StopEvent();
                _alerts.Remove(alert);
            }
        }

        public bool ToggleAlert(ref Item item)
        {
            try
            {
                if (!IsAlertInCollection(item.UniqueName) && !IsSpaceInAlertsCollection()) return false;

                if (IsAlertInCollection(item.UniqueName))
                {
                    DeactivateAlert(item.UniqueName);
                    return false;
                }

                ActivateAlert(item.UniqueName, item.AlertModeMinSellPriceIsUndercutPrice);
                return true;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return false;
            }
        }

        public void DeactivateAlert(string uniqueName)
        {
            try
            {
                var itemCollection = (ObservableCollection<Item>) _itemsView.SourceCollection;
                var item = itemCollection.FirstOrDefault(i => i.UniqueName == uniqueName);

                if (item == null) return;

                item.IsAlertActive = false;
                Remove(item.UniqueName);

                _mainWindow.Dispatcher?.Invoke(() => { _itemsView.Refresh(); });
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        private void ActivateAlert(string uniqueName, int minSellUndercutPrice)
        {
            try
            {
                var itemCollection = (ObservableCollection<Item>) _itemsView.SourceCollection;
                var item = itemCollection.FirstOrDefault(i => i.UniqueName == uniqueName);

                if (item == null) return;

                item.IsAlertActive = true;
                item.AlertModeMinSellPriceIsUndercutPrice = minSellUndercutPrice;
                Add(item, item.AlertModeMinSellPriceIsUndercutPrice);

                _mainWindow.Dispatcher?.Invoke(() => { _itemsView.Refresh(); });
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        private bool IsAlertInCollection(string uniqueName)
        {
            return _alerts.Any(alert => alert.Item.UniqueName == uniqueName);
        }

        private Alert GetAlertByUniqueName(string uniqueName)
        {
            return _alerts.FirstOrDefault(alert => alert.Item.UniqueName == uniqueName);
        }

        public bool IsSpaceInAlertsCollection()
        {
            return _alerts.Count < _maxAlertsAtSameTime;
        }

        #region Alert file controls

        private void SetActiveAlertsFromLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ActiveAlertsFileName}";

            if (File.Exists(localFilePath))
                try
                {
                    var localItemString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    var alertSaveObjectList = JsonSerializer.Deserialize<List<AlertSaveObject>>(localItemString);

                    if (alertSaveObjectList != null)
                    {
                        foreach (var alert in alertSaveObjectList)
                        {
                            ActivateAlert(alert.UniqueName, alert.MinSellUndercutPrice);
                        }
                    }
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
        }

        private void SaveActiveAlertsToLocalFile()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.ActiveAlertsFileName}";
            var activeItemAlerts = _alerts.Select(alert => new AlertSaveObject
                {UniqueName = alert.Item.UniqueName, MinSellUndercutPrice = alert.AlertModeMinSellPriceIsUndercutPrice}).ToList();
            var fileString = JsonSerializer.Serialize(activeItemAlerts);

            try
            {
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        private struct AlertSaveObject
        {
            public string UniqueName { get; set; }

            public int MinSellUndercutPrice { get; set; }
        }

        #endregion
    }
}