﻿using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using System;

namespace StatisticsAnalysisTool.Models
{
    public class ClusterInfo
    {
        public DateTime Entered { get; set; }
        public string Index { get; set; }
        public string MainClusterIndex { get; set; }
        public Guid? Guid { get; set; }
        public string UniqueName { get; set; }
        public string Type { get; set; }
        public string File { get; set; }
        public MapType MapType { get; set; } = MapType.Unknown;
        public ClusterType ClusterType => WorldData.GetClusterType(Type);
        public bool IsAvalonClusterTunnel => WorldData.IsAvalonClusterTunnel(Type);
        public AvalonTunnelType AvalonTunnelType => WorldData.GetTunnelType(Type);
        public Tier Tier => WorldData.GetTier(File);

        public string TierString 
        {
            get
            {
                return Tier switch
                {
                    Tier.T1 => "I",
                    Tier.T2 => "II",
                    Tier.T3 => "III",
                    Tier.T4 => "IV",
                    Tier.T5 => "V",
                    Tier.T6 => "VI",
                    Tier.T7 => "VII",
                    Tier.T8 => "VIII",
                    Tier.Unknown => "?",
                    _ => "?"
                };
            }
        }
    }
}