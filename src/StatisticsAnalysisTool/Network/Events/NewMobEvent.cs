using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewMobEvent
    {
        public NewMobEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
            //Debug.Print($"[NewMobEvent] {JsonSerializer.Serialize(parameters)}");
            try
            {
                if (parameters.ContainsKey(0)) 
                    ObjectId = parameters[0].ObjectToLong();

                if (parameters.ContainsKey(1)) 
                    Type = parameters[1].ObjectToLong() ?? 0;

                if (parameters.ContainsKey(11))
                    MoveSpeed = parameters[11].ObjectToDouble();

                if (parameters.ContainsKey(13))
                    HitPoints = parameters[13].ObjectToInt();

                if (parameters.ContainsKey(14))
                    HitPointsMax = parameters[14].ObjectToInt();

                if (parameters.ContainsKey(17))
                    Energy = parameters[17].ObjectToInt();

                if (parameters.ContainsKey(18))
                    EnergyMax = parameters[18].ObjectToInt();

                if (parameters.ContainsKey(19))
                    EnergyRegeneration = parameters[19].ObjectToInt();

                if (parameters.ContainsKey(20))
                {
                    byte[] inputString = new byte[16];
                    inputString[0] = (byte)parameters[20].ToString()[0];
                    inputString[1] = (byte)parameters[20].ToString()[1];
                    inputString[2] = (byte)parameters[20].ToString()[2];
                    inputString[3] = (byte)parameters[20].ToString()[3];
                    inputString[4] = (byte)parameters[20].ToString()[4];
                    inputString[5] = (byte)parameters[20].ToString()[5];
                    inputString[6] = (byte)parameters[20].ToString()[6];
                    inputString[7] = (byte)parameters[20].ToString()[7];

                    inputString[8] = (byte)parameters[20].ToString()[0];
                    inputString[9] = (byte)parameters[20].ToString()[1];
                    inputString[10] = (byte)parameters[20].ToString()[2];
                    inputString[11] = (byte)parameters[20].ToString()[3];
                    inputString[12] = (byte)parameters[20].ToString()[4];
                    inputString[13] = (byte)parameters[20].ToString()[5];
                    inputString[14] = (byte)parameters[20].ToString()[6];
                    inputString[15] = (byte)parameters[20].ToString()[7];

                    Guid = inputString.ObjectToGuid();
                }
                    

                Debug.Print($"[NewMob] ObjectId: {ObjectId} Guid: {Guid} Type: {Type} MoveSpeed: {MoveSpeed} HitPoints: {HitPoints} HitPointsMax: {HitPointsMax} Energy: {Energy} EnergyMax: {EnergyMax} EnergyRegenration: {EnergyRegeneration}");

            }

            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public long? ObjectId { get; }
        public Guid? Guid { get; }
        public long Type { get; }
        public double MoveSpeed { get; }
        public int HitPoints { get; }
        public int HitPointsMax { get; }
        public int Energy { get; }
        public int EnergyMax { get; }
        public int EnergyRegeneration { get; }
        
    }
}