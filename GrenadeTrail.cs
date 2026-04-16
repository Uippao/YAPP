using System;
using System.Linq;
using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace YAPP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GrenadeTrailCommand : ICommand
    {
        public string Command { get; set; } = "grenadetrail";
        public string[] Aliases { get; set; } = { "gdt" };
        public string Description => "Spawns a grenade trail where the specified player walks.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.HasPermissions("yapp.admin"))
            {
                response = "You don't have permission to use this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: grenadetrail <playerID> [count] [interval] [grenadeType]";
                return false;
            }

            string targetArg = arguments.At(0);

            int count = 6;
            float interval = 2f;
            ItemType grenadeType = ItemType.GrenadeHE;

            if (arguments.Count >= 2 && int.TryParse(arguments.At(1), out int parsedCount))
                count = Mathf.Max(1, parsedCount);

            if (arguments.Count >= 3 && float.TryParse(arguments.At(2), out float parsedInterval))
                interval = Mathf.Max(0.1f, parsedInterval);

            if (arguments.Count >= 4 && Enum.TryParse(arguments.At(3), true, out ItemType parsedType))
                grenadeType = parsedType;

            if (targetArg == "*")
            {
                foreach (var player in Player.List)
                {
                    Utils.GrenadeTrail(player, grenadeType, count, interval);
                }

                response = $"Spawned grenade trail on ALL players (count={count}, interval={interval}, type={grenadeType}).";
                return true;
            }

            if (!int.TryParse(targetArg, out int playerId))
            {
                response = $"\"{targetArg}\" is not a valid player ID.";
                return false;
            }

            Player target = Player.Get(playerId);

            if (target == null)
            {
                response = $"Player with ID {playerId} not found.";
                return false;
            }

            Utils.GrenadeTrail(target, grenadeType, count, interval);

            response = $"Spawned grenade trail on {target.Nickname} (count={count}, interval={interval}, type={grenadeType}).";
            return true;
        }
    }
}