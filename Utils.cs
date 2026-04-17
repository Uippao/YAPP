using System;
using System.Collections.Generic;
using System.Linq;
using CustomItems.API;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace YAPP
{
    public static class Utils
    {
        public static void DebugLog(string message)
        {
            if (YAPP.Instance?.Config?.Debug != true)
                return;

            Logger.Debug(message);
        }
        
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
        
        public static Pickup SpawnPillInRoom(string customItemName, RoomName roomName, Vector3 offset)
        {
            Room room = Room.Get(roomName).FirstOrDefault();

            if (room == null)
            {
                DebugLog($"Room not found: {roomName}");
                return null;
            }

            ushort itemId = CustomItems.API.CustomItems.GetIdByName(customItemName);

            Vector3 position = room.GameObject.transform.TransformPoint(offset);

            if (!CustomItems.API.CustomItems.TrySpawn(itemId, position, out Pickup pickup) || pickup == null)
            {
                DebugLog($"TrySpawn failed: {customItemName} at {position}");
                return null;
            }

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

                case Types.SpawnConditionType.Weekday:
                    return MatchWeekday(c.StringValue, true);

                case Types.SpawnConditionType.NotWeekday:
                    return MatchWeekday(c.StringValue, false);

                case Types.SpawnConditionType.Month:
                    return MatchMonth(c.StringValue, true);

                case Types.SpawnConditionType.NotMonth:
                    return MatchMonth(c.StringValue, false);
                
                case Types.SpawnConditionType.TimeRange:
                    return MatchTimeRange(c.StringValue, true);

                case Types.SpawnConditionType.NotTimeRange:
                    return MatchTimeRange(c.StringValue, false);

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
                { "SCP-500-S.description", "Makes you extremely fast for 5 seconds" },

                { "SCP-500-C.description", "Spawns a circle of coins around you" },

                { "SCP-500-A.description", "Makes you faster, tougher and stealthier for 15 seconds, then tires you" },

                { "SCP-500-B.description", "Launches a ring of grenades around you" },
    
                { "SCP-500-E.description", "Lays a trail of grenades that explode in sequence" },

                { "SCP-500-G.description", "Become a ghost for 10 seconds (walk through doors)" },

                { "SCP-500-H.description", "Gives you 75 AHP" },

                { "SCP-500-I.description", "Turns you invisible for a few seconds" },

                { "SCP-500-F.description", "Resurrects a spectator as a teammate" },
                { "SCP-500-F.noSpectators", "There are no spectators for you to summon" },

                { "SCP-500-T.description", "Teleports you to a random room location" },

                { "SCP-500-W.description", "Enhances your vision temporarily" },

                { "SCP-500-?.description", "Spawns a random pill at your feet" }
            };
        }
        
        private static bool MatchWeekday(string value, bool shouldMatch)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            if (!Enum.TryParse<DayOfWeek>(value, true, out var target))
                return true;

            bool result = DateTime.Now.DayOfWeek == target;
            return shouldMatch ? result : !result;
        }

        private static bool MatchMonth(string value, bool shouldMatch)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            if (int.TryParse(value, out int monthNumber))
            {
                bool result = DateTime.Now.Month == monthNumber;
                return shouldMatch ? result : !result;
            }

            if (Enum.TryParse<Types.MonthEnum>(value, true, out var monthEnum))
            {
                bool result = DateTime.Now.Month == (int)monthEnum;
                return shouldMatch ? result : !result;
            }

            return true;
        }
        
        private static bool MatchTimeRange(string value, bool shouldMatch)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var parts = value.Split('-');
            if (parts.Length != 2)
                return true;

            if (!TimeSpan.TryParse(parts[0], out var start))
                return true;

            if (!TimeSpan.TryParse(parts[1], out var end))
                return true;

            var now = DateTime.Now.TimeOfDay;

            bool result;

            if (start <= end)
                result = now >= start && now <= end;
            else
                result = now >= start || now <= end;

            return shouldMatch ? result : !result;
        }
        
        public static List<TimedGrenadeProjectile> LaunchGrenadeCircle(Player player, ItemType grenadeType, float velocity = 3f)
        {
            List<TimedGrenadeProjectile> grenades = new List<TimedGrenadeProjectile>();
            Vector3 playerPos = player.Position;

            float radius = 0.4f;

            for (int i = 0; i < 5; i++)
            {
                float angle = (i * 72f) * Mathf.Deg2Rad;

                Vector3 spawnPos = new Vector3(
                    playerPos.x + Mathf.Cos(angle) * radius,
                    playerPos.y + 0.2f,
                    playerPos.z + Mathf.Sin(angle) * radius
                );

                TimedGrenadeProjectile grenade = TimedGrenadeProjectile.SpawnActive(spawnPos, grenadeType, player);
                if (grenade != null && grenade.Rigidbody != null)
                {
                    Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                    grenade.Rigidbody.linearVelocity = direction * velocity;
                    grenade.Rigidbody.angularVelocity = Vector3.zero;

                    grenades.Add(grenade);
                }
            }

            return grenades;
        }
        
        public static void GrenadeTrail(Player player, ItemType grenadeType = ItemType.GrenadeHE, int count = 6, float interval = 2f)
        {
            float fuse = count * interval;

            for (int i = 0; i < count; i++)
            {
                int index = i;

                Timing.CallDelayed(index * interval, () =>
                {
                    Vector3 pos = player.Position;

                    TimedGrenadeProjectile.SpawnActive(
                        pos,
                        grenadeType,
                        player,
                        timeOverride: fuse
                    );
                });
            }
        }
        
        public static ExplosiveGrenadeProjectile SpawnInstantExplosion(Player owner)
        {
            TimedGrenadeProjectile grenade = TimedGrenadeProjectile.SpawnActive(
                owner.Position,
                ItemType.GrenadeHE,
                owner,
                timeOverride: 0.0
            );

            if (grenade is ExplosiveGrenadeProjectile explosiveGrenade)
            {
                explosiveGrenade.ScpDamageMultiplier = 3f;
            }

            return null;
        }
        
        public static List<Room> GetSafeRooms()
        {
            HashSet<RoomName> dangerousRooms = new HashSet<RoomName>
            {
                RoomName.HczAcroamaticAbatement,
                RoomName.Lcz173,
                RoomName.HczWaysideIncinerator,
                RoomName.EzCollapsedTunnel,
                RoomName.EzEvacShelter,
                RoomName.EzIntercom,
                RoomName.Hcz079,
                RoomName.Hcz096,
                RoomName.Hcz049,
                RoomName.HczTesla,
                RoomName.HczTestroom,
                RoomName.HczMicroHID,
                RoomName.Unnamed,
                RoomName.Outside,
                RoomName.Pocket,
                RoomName.Hcz106,
                RoomName.EzGateA,
                RoomName.EzGateB
            };

            bool lczDecontaminated = Decontamination.IsDecontaminating;

            List<Room> safeRooms = new List<Room>();

            foreach (Room room in Room.List)
            {
                if (dangerousRooms.Contains(room.Name))
                    continue;

                if (lczDecontaminated && room.Zone == FacilityZone.LightContainment)
                    continue;

                safeRooms.Add(room);
            }

            return safeRooms;
        }

        public static Room TeleportToSurfacePosition(Player player)
        {
            Vector3[] surfacePositions = new Vector3[]
            {
                new Vector3(29.408f, 291.878f, -26.503f),
                new Vector3(-40.5f, 291.881f, -36.430f),
                new Vector3(38.584f, 300.958f, -50.593f),
                new Vector3(138.307f, 295.461f, -64.975f),
                new Vector3(123.469f, 290.696f, 7.048f)
            };

            Vector3 randomPosition = surfacePositions[UnityEngine.Random.Range(0, surfacePositions.Length)];
            player.Position = randomPosition;

            return null;
        }
    }
}