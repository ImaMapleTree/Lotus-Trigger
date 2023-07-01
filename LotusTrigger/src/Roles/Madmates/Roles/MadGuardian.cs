using Lotus.API.Reactive.Actions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;

namespace LotusTrigger.Roles.Madmates.Roles;

public class MadGuardian : MadCrewmate
{
    [RoleAction(LotusActionType.Interaction)]
    private void MadGuardianAttacked(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (interaction.Intent is not (IFatalIntent or IHostileIntent)) return;
        handle.Cancel();
    }
}