using Lotus;
using Lotus.API.Odyssey;
using Lotus.Roles.Events;
using LotusTrigger.Roles.Undead.Roles;
using VentLib.Utilities;

namespace LotusTrigger.Roles.Undead.Events;

public class InitiateEvent : TargetedAbilityEvent
{
    public InitiateEvent(PlayerControl source, PlayerControl target, bool successful = true) : base(source, target, successful)
    {
    }

    public override string Message() => $"{UndeadRole.UndeadColor.Colorize(Game.GetName(Player()))} initiated {ModConstants.HColor2.Colorize(Game.GetName(Target()))} within the undead.";
}