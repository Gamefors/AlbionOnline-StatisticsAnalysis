﻿using AutoUpdaterDotNET;
using StatisticsAnalysisTool.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace StatisticsAnalysisTool.Common
{
    public static class Utilities
    {
        public static void AutoUpdate()
        {
            AutoUpdater.Start(Settings.Default.AutoUpdateConfigUrl);
            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ApplicationExitEvent += AutoUpdaterApplicationExitAsync;
        }

        private static async void AutoUpdaterApplicationExitAsync()
        {
            await Task.Delay(5000);
            Application.Current.Shutdown();
        }

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static string LongNumberToString(long value)
        {
            return value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));
        }

        public static string UlongMarketPriceToString(ulong value)
        {
            return value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));
        }

        public static string MarketPriceDateToString(DateTime value)
        {
            return Formatting.CurrentDateTimeFormat(value);
        }

        public static string GetValuePerHourInShort(double value, TimeSpan span)
        {
            return Formatting.ToStringShort(GetValuePerHourToDouble(value, span.TotalSeconds));
        }
        
        public static double GetValuePerHourToDouble(double value, double seconds)
        {
            try
            {
                var hours = seconds / 60d / 60d;
                return value / hours;
            }
            catch (OverflowException)
            {
                return double.MaxValue;
            }
        }

        public static double GetValuePerSecondToDouble(double value, DateTime? combatStart, TimeSpan time, double maxValue = -1)
        {
            if (double.IsInfinity(value)) return maxValue > 0 ? maxValue : double.MaxValue;

            if (time.Ticks <= 1 && combatStart != null)
            {
                var startTimeSpan = DateTime.UtcNow - (DateTime) combatStart;
                var calculation = value / startTimeSpan.TotalSeconds;
                return calculation > maxValue ? maxValue : calculation;
            }

            var valuePerSeconds = value / time.TotalSeconds;
            if (maxValue > 0 && valuePerSeconds > maxValue) return maxValue;

            return valuePerSeconds;
        }

        public static bool IsBlockingTimeExpired(DateTime dateTime, int waitingSeconds)
        {
            var currentDateTime = DateTime.UtcNow;
            var difference = currentDateTime.Subtract(dateTime);
            return difference.Seconds >= waitingSeconds;
        }

        public static double AddValue(double value, double? lastValue, out double? newLastValue)
        {
            if (lastValue == null)
            {
                newLastValue = value;
                return 0;
            }

            var newValue = (double)(value - lastValue);
            if (newValue == 0)
            {
                newLastValue = value;
                return 0;
            }

            newLastValue = value;
            return newValue;
        }

        #region Window Flash

        private const uint FlashwStop = 0; //Stop flashing. The system restores the window to its original state.
        private const uint FlashwCaption = 1; //Flash the window caption.        
        private const uint FlashwTray = 2; //Flash the taskbar button.        
        private const uint FlashwAll = 3; //Flash both the window caption and taskbar button.        
        private const uint FlashwTimer = 4; //Flash continuously, until the FLASHW_STOP flag is set.        
        private const uint FlashwTimernofg = 12; //Flash continuously until the window comes to the foreground.  

        [StructLayout(LayoutKind.Sequential)]
        private struct FlashInfo
        {
            public uint cbSize; //The size of the structure in bytes.            
            public IntPtr hwnd; //A Handle to the Window to be Flashed. The window can be either opened or minimized.


            public uint dwFlags; //The Flash Status.            
            public uint uCount; // number of times to flash the window            

            public uint
                dwTimeout; //The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.        
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FlashInfo pwfi);

        public static void FlashWindow(this Window win, uint count = uint.MaxValue)
        {
            win.Dispatcher.Invoke(() =>
            {
                if (win.IsActive) return;

                var h = new WindowInteropHelper(win);

                var info = new FlashInfo
                {
                    hwnd = h.Handle,
                    dwFlags = FlashwAll | FlashwTimer,
                    uCount = count,
                    dwTimeout = 0
                };

                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
                FlashWindowEx(ref info);
            });
        }

        public static void StopFlashingWindow(this Window win)
        {
            win.Dispatcher.Invoke(() =>
            {
                var h = new WindowInteropHelper(win);
                var info = new FlashInfo {hwnd = h.Handle};
                info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
                info.dwFlags = FlashwStop;
                info.uCount = uint.MaxValue;
                info.dwTimeout = 0;
                FlashWindowEx(ref info);
            });
        }

        #endregion
    }
}