﻿using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class InventoryPutItemEventHandler
    {
        private readonly TrackingController _trackingController;

        public InventoryPutItemEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(InventoryPutItemEvent value)
        {
            //_trackingController.LootController.AddPutLootAsync(value.ObjectId, value.InteractGuid);
            await Task.CompletedTask;
        }
    }
}