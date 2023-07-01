using Lotus;
using Lotus.API.Odyssey;
using Lotus.Roles.Events;
using LotusTrigger.Roles.Undead.Roles;
using VentLib.Utilities;

namespace LotusTrigger.Roles.Undead.Events;

public class ConvertEvent : TargetedAbilityEvent
{
    public ConvertEvent(PlayerControl source, PlayerControl target, bool successful = true) : base(source, target, successful)
    {
    }

    public override string Message() => $"{UndeadRole.UndeadColor.Colorize(Game.GetName(Player()))} turned {ModConstants.HColor2.Colorize(Game.GetName(Target()))} to the Undead.";
}