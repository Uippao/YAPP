using System;
using System.Collections.Generic;
using System.Globalization;
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
    
        public override void Enable()
        {  
            Instance = this;
            LoadConfigs();
            CustomItems.API.CustomItems.Register(new Pills.SpeedPill());
            
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
                if (!Utils.EvaluateCondition(spawn.Condition))
                    continue;
                
                if (Random.NextDouble() > spawn.Chance)
                    continue;

                string pillName = spawn.PillName;

                if (pillName.Equals("random", StringComparison.OrdinalIgnoreCase))
                {
                    pillName = Utils.GetRandomPillName();
                    if (pillName == null)
                        continue;
                }

                Utils.SpawnPillInRoom(pillName, spawn.Room, spawn.Position);
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
