using System.Collections.Generic;
using System.Linq;
using CustomItems.API;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace YAPP
{
    public static class Utils
    {
        public static string GetRandomPillName()
        {
            var pills = CustomItems.API.CustomItems.AllItems
                .Where(ci => ci.Name.StartsWith("SCP-500"))
                .Select(ci => ci.Name)
                .ToList();

            if (pills.Count == 0)
                return null;

            return pills[YAPP.Random.Next(pills.Count)];
        }
        
        public static Pickup SpawnPillInRoom(string customItemName, RoomName roomName, Vector3 subcoordinate)
        {
            Room room = Room.Get(roomName).FirstOrDefault();
            if (room == null)
                return null;

            ushort itemId = CustomItems.API.CustomItems.GetIdByName(customItemName);
            if (itemId == 0)
                return null;

            Vector3 finalPosition = room.Position + subcoordinate;

            bool success = CustomItems.API.CustomItems.TrySpawn(itemId, finalPosition, out Pickup pickup);
            if (!success)
                return null;

            return pickup;
        }
        
        public static bool EvaluateCondition(SpawnCondition condition)
        {
            if (condition == null)
                return true;

            var c = condition.Resolve();

            if (c.Type == Types.SpawnConditionType.None)
                return true;

            int playerCount = Player.List.Count;

            switch (c.Type)
            {
                case Types.SpawnConditionType.PlayerCountAtLeast:
                    return playerCount >= c.IntValue;

                case Types.SpawnConditionType.PlayerCountAtMost:
                    return playerCount <= c.IntValue;

                default:
                    return true;
            }
        }
        
        public static string GetPillText(string key)
        {
            var cfg = YAPP.Instance.Config;

            if (cfg.CustomText != null &&
                cfg.CustomText.TryGetValue(key, out var value))
                return value;

            if (PillTextDefaults.Values.TryGetValue(key, out var fallback))
                return fallback;

            return key;
        }
        
        public static class PillTextDefaults
        {
            public static readonly Dictionary<string, string> Values = new Dictionary<string, string>
            {
                { "SCP-500-S.description", "SPEEEED!!!" }
            };
        }
    }
}