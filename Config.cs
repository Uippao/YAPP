using System.Collections.Generic;
using MapGeneration;
using UnityEngine;

namespace YAPP
{
    public class Config
    {
        public bool Debug { get; set; } = false;

        public List<PillSpawnConfig> PillSpawns { get; set; } = new List<PillSpawnConfig>()
        {
            new PillSpawnConfig
            {
                Chance = 0.5f,
                Room = RoomName.HczArmory,
                Position = new Vector3(0f, 1f, 0f),
                PillName = "SCP-500-S"
            }
        };
        
        public Dictionary<string, string> CustomText { get; set; }
            = new Dictionary<string, string>();
    }

    public class PillSpawnConfig
    {
        public float Chance { get; set; } = 1f;
        public RoomName Room { get; set; }
        public Vector3 Position { get; set; }
        public string PillName { get; set; } = "random";
        public SpawnCondition Condition { get; set; } = null;
    }
}