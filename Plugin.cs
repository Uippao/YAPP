using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LabApi;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MEC;
using Logger = LabApi.Features.Console.Logger;
using Random = System.Random;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;
using CustomItems.API;
using LabApi.Events.Arguments.ServerEvents;
using YAPP.Pills;

namespace YAPP
{
    public class YAPP: Plugin<Config>
    {
        public override string Name { get; } = "YAPP";
        public override string Description { get; } = "Yet Another Pill Plugin. Adds new SCP-500 instances with unique effects.";
        public override string Author { get; } = "Uippao";
        public override Version Version { get; } = new Version(1, 0, 0, 0);
        public override Version RequiredApiVersion { get; } = new Version(LabApiProperties.CompiledVersion);

        public static YAPP Instance { get; private set; }
        internal static readonly Random Random = new Random();
        public static Tag CustomItemsTag => new Tag("customitems_hint");
    
        public override void Enable()
        {  
            Instance = this;
            LoadConfigs();
            CustomItems.API.CustomItems.RegisterAll();
            
            ServerEvents.RoundStarting += OnRoundStarting;
            
            PopulateMissingTextKeys();
        }  

        public override void Disable()
        {
            ServerEvents.RoundStarting -= OnRoundStarting;
            
            Instance = null;
        }

        private void OnRoundStarting(RoundStartingEventArgs ev)
        {
            SpawnConfiguredPills();
        }
        
        public void SpawnConfiguredPills()
        {
            foreach (var spawn in Config.PillSpawns)
            {
                Utils.DebugLog($"Evaluating spawn: {spawn.PillName}");

                if (spawn.Conditions != null && spawn.Conditions.Count > 0 &&
                    !spawn.Conditions.All(Utils.EvaluateCondition))
                {
                    Utils.DebugLog($"Rejected by conditions: {spawn.PillName}");
                    continue;
                }

                if (Random.NextDouble() > spawn.Chance)
                {
                    Utils.DebugLog($"Rejected by chance ({spawn.Chance}): {spawn.PillName}");
                    continue;
                }

                string pillName = spawn.PillName;

                if (pillName.Equals("random", StringComparison.OrdinalIgnoreCase))
                {
                    pillName = Utils.GetRandomPillName();
                    Utils.DebugLog($"Random pill resolved to: {pillName}");

                    if (pillName == null)
                    {
                        Utils.DebugLog("Random pill resolution failed (no pills available)");
                        continue;
                    }
                }

                if (spawn.Locations == null || spawn.Locations.Count == 0)
                {
                    Utils.DebugLog($"No locations defined for: {pillName}");
                    continue;
                }

                var location = spawn.Locations[YAPP.Random.Next(spawn.Locations.Count)];

                Utils.DebugLog(
                    $"Spawning {pillName} in {location.Room} at {location.Position}"
                );

                Utils.SpawnPillInRoom(
                    pillName,
                    location.Room,
                    location.Position
                );
            }
        }
        
        private void PopulateMissingTextKeys()
        {
            var cfg = Config.CustomText;

            foreach (var kv in Utils.PillTextDefaults.Values)
            {
                if (!cfg.ContainsKey(kv.Key))
                    cfg[kv.Key] = kv.Value;
            }

            SaveConfig();
        }
    }
}
