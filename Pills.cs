using System.Collections.Generic;
using System.Linq;
using CustomItems.API;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;
using PlayerRoles;
using PlayerRoles.Visibility;
using RueI.API;
using RueI.API.Elements;
using UnityEngine;

namespace YAPP.Pills
{
    public class SpeedPill : CustomItem
    {
        public override string Name => "SCP-500-S";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.EnableEffect<MovementBoost>(200, 5f);
            ev.Player.EnableEffect<Invigorated>(1, 5f);
        }
    }
    
    public class CoinPill : CustomItem
    {
        public override string Name => "SCP-500-C";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            Vector3 playerPos = ev.Player.Position;
      
            for (int i = 0; i < 8; i++)
            {
                float angle = (i * 45f) * Mathf.Deg2Rad;
                
                Vector3 coinPos = new Vector3(
                    playerPos.x + Mathf.Cos(angle) * 0.5f,
                    playerPos.y,
                    playerPos.z + Mathf.Sin(angle) * 0.5f
                );
                
                Pickup coin = Pickup.Create(ItemType.Coin, coinPos);
                if (coin != null)
                {
                    coin.Spawn();
                }
            }
        }
    }
    
    public class RampagePill : CustomItem
    {
        public override string Name => "SCP-500-A";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.EnableEffect<MovementBoost>(50, 15f);
            ev.Player.EnableEffect<Invigorated>(1, 15f);
            ev.Player.EnableEffect<SilentWalk>(10, 15f);
            ev.Player.EnableEffect<DamageReduction>(100, 15f);
            ev.Player.StaminaRemaining = 1f;

            Timing.CallDelayed(15f, () =>
            {
                ev.Player.EnableEffect<Exhausted>(30, 30f);
                ev.Player.EnableEffect<Disabled>(1, 30f);
            });
        }
    }
    
    public class BoomPill : CustomItem
    {
        public override string Name => "SCP-500-B";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            Utils.LaunchGrenadeCircle(ev.Player, ItemType.GrenadeHE, 4f);
        }
    }
    
    public class BoomTrailPill : CustomItem
    {
        public override string Name => "SCP-500-E";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            Utils.GrenadeTrail(ev.Player, ItemType.GrenadeHE);
        }
    }
    
    public class GhostPill : CustomItem
    {
        public override string Name => "SCP-500-G";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.EnableEffect<Fade>(80, 10);
            ev.Player.EnableEffect<Ghostly>(1, 10);
            ev.Player.EnableEffect<SilentWalk>(10, 10);
        }
    }
    
    public class HealthPill : CustomItem
    {
        public override string Name => "SCP-500-H";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.ArtificialHealth = ev.Player.ArtificialHealth += 75f;
        }
    }
    
    public class InvisPill : CustomItem
    {
        public override string Name => "SCP-500-I";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.EnableEffect<Invisible>(1, 7f);
        }
    }
    
    public class AllyPill : CustomItem
    {
        public override string Name => "SCP-500-F";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsing(PlayerUsingItemEventArgs ev)
        {
            List<Player> spectators = Player.List.Where(p => p.Role == RoleTypeId.Spectator).ToList();
            if (spectators.Count == 0)
            {
                RueDisplay display = RueDisplay.Get(ev.Player);
                display.Remove(new Tag("funnycoins_cooldown"));
                display.Remove(new Tag("funnycoins_effect"));
                display.Show(
                    YAPP.CustomItemsTag,
                    new BasicElement(
                        250,
                        $"<align=left>{Utils.GetPillText($"{Name}.noSpectators")}</align>"
                        ),
                    2f
                    );
                ev.IsAllowed = false;
            }
        }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            List<Player> spectators = Player.List.Where(p => p.Role == RoleTypeId.Spectator).ToList();
            
            Player randomSpectator = spectators[YAPP.Random.Next(0, spectators.Count)];
            
            RoleTypeId role;
            switch (ev.Player.Team)
            {
                case Team.FoundationForces:
                    role = RoleTypeId.NtfPrivate;
                    break;

                case Team.ChaosInsurgency:
                    role = RoleTypeId.ChaosRifleman;
                    break;
                
                case Team.ClassD:
                    role = RoleTypeId.ClassD;
                    break;
                
                case Team.Scientists:
                    role = RoleTypeId.Scientist;
                    break;

                case Team.SCPs:
                    role = RoleTypeId.Tutorial;
                    break;

                default:
                    role = RoleTypeId.ClassD;
                    break;
            }
            
            randomSpectator.SetRole(role, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
            randomSpectator.Position = ev.Player.Position;
        }
    }
    
    public class TeleportPill : CustomItem
    {
        public override string Name => "SCP-500-T";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            if (Warhead.IsDetonated)
            {
                Utils.TeleportToSurfacePosition(ev.Player);
                return;
            }

            List<Room> safeRooms = Utils.GetSafeRooms();

            if (safeRooms.Count == 0)
                return;

            Room randomRoom = safeRooms[UnityEngine.Random.Range(0, safeRooms.Count)];
            ev.Player.Position = randomRoom.Position + new Vector3(0f, 1f, 0f);
        }
    }
    
    public class WallhackPill : CustomItem
    {
        public override string Name => "SCP-500-W";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.EnableEffect<Scp1344>(1, 15);
            ev.Player.EnableEffect<NightVision>(20, 15);
            ev.Player.EnableEffect<FogControl>(1, 15);
        }
    }
    
    public class RandomPill : CustomItem
    {
        public override string Name => "SCP-500-?";

        public override string Description => Utils.GetPillText($"{Name}.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            var allPills = CustomItems.API.CustomItems.AllItems
                .Where(ci => ci.Name.StartsWith("SCP-500") && ci.Name != Name)
                .ToList();

            if (allPills.Count == 0)
                return;

            var randomPill = allPills[YAPP.Random.Next(allPills.Count)];

            ushort itemId = CustomItems.API.CustomItems.GetIdByName(randomPill.Name);

            if (itemId == 0)
                return;

            Vector3 spawnPosition = ev.Player.Position;

            CustomItems.API.CustomItems.TrySpawn(itemId, spawnPosition, out Pickup _);
        }
    }
}