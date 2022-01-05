﻿using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class HealthUpdateEventHandler
    {
        private readonly TrackingController _trackingController;

        public HealthUpdateEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(HealthUpdateEvent value)
        {
            await _trackingController.CombatController.AddDamageAsync(value.ObjectId, value.CauserId, value.HealthChange, value.NewHealthValue, value.CausingSpellType, value.EffectOrigin, value.EffectType);
        }
    }
}