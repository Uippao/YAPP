using System.Collections.Generic;
using CustomItems.API;
using CustomPlayerEffects;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using MEC;

namespace YAPP.Pills
{
    public class SpeedPill : CustomItem
    {
        public override string Name => "SCP-500-S";

        public override string Description => Utils.GetPillText("SCP-500-S.description");

        public override ItemType Type => ItemType.SCP500;

        public override void OnRegistered() { }

        public override void OnUnregistered() { }

        public override void OnUsed(PlayerUsedItemEventArgs ev)
        {
            ev.Player.EnableEffect<MovementBoost>(200, 5f);
            ev.Player.EnableEffect<Invigorated>(1, 5f);
        }
    }
}