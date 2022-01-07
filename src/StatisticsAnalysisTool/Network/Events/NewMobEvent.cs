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
                    string uniqueString = parameters[20].ToString();
                    byte[] inputString = new byte[16];
                    for (int i = 0; i < 16; i++)
                    {
                        inputString[i] = (byte)uniqueString[i % uniqueString.Length];

                    }
                    Guid = inputString.ObjectToGuid();
                }
                //Debug.Print($"[NewMob] ObjectId: {ObjectId} Guid: {Guid} Type: {Type}");
            }
            catch (Exception e)
            {
                if (parameters.ContainsKey(0))
                {
                    Debug.Print($"[NewMobEvent] Id: {parameters[0].ObjectToLong()} | Mob could not be added.");
                }
                else
                {
                    Debug.Print($"[NewMobEvent] Mob id could not be added.");
                }
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