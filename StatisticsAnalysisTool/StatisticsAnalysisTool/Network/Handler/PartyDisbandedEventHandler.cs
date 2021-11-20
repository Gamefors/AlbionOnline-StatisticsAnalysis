﻿using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyDisbandedEventHandler
    {
        private readonly TrackingController _trackingController;

        public PartyDisbandedEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(PartyDisbandedEvent value)
        {
            await _trackingController.EntityController.SetPartyAsync(value.PartyUsersGuid, true);
            await Task.CompletedTask;
        }
    }
}