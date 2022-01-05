using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewMobEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewMobEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        enum MobTypes
        {
            Revenant = 693
        }

        public async Task OnActionAsync(NewMobEvent value)
        {
            string mobName = $"Unknown({value.Type})";
            if(Enum.IsDefined(typeof(MobTypes), (int)value.Type)){
                mobName = ((MobTypes)(int)value.Type).ToString();
            }

            if (value.Guid != null && value.ObjectId != null)
            {
                _trackingController.EntityController.AddEntity((long)value.ObjectId, (Guid)value.Guid, null, mobName, GameObjectType.Mob, GameObjectSubType.Mob);
            }
            await Task.CompletedTask;
        }
    }
}