using System;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace YAPP.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PickupPosCommand : ICommand
    {
        public string Command { get; set; } = "pickuppos";
        public string[] Aliases { get; set; } = { "ppos" };
        public string Description => "Gets room-local coordinates of nearest pickup.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null)
            {
                response = "Player not found.";
                return false;
            }

            Room room = player.Room;
            if (room == null)
            {
                response = "You are not inside a room.";
                return false;
            }

            Pickup pickup = Pickup.List
                .OrderBy(p => Vector3.Distance(p.Position, player.Position))
                .FirstOrDefault();

            if (pickup == null)
            {
                response = "No pickups found.";
                return false;
            }

            Vector3 local = room.GameObject.transform.InverseTransformPoint(pickup.Position);

            response =
                $"Pickup Type: {pickup.Type}\n" +
                $"Room: {room.Name}\n" +
                $"World: {pickup.Position}\n" +
                $"Local: {local.x:F3}, {local.y:F3}, {local.z:F3}\n" +
                $"Config:\n" +
                $"  room: {room.Name}\n" +
                $"  position:\n" +
                $"    x: {local.x:F3}\n" +
                $"    y: {local.y:F3}\n" +
                $"    z: {local.z:F3}";

            return true;
        }
    }
}