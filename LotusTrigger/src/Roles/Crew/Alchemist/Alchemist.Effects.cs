using Lotus.API.Reactive.Actions;
using Lotus.API.Vanilla.Meetings;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Optionals;

namespace LotusTrigger.Roles.Crew.Alchemist;

public partial class Alchemist
{
    [RoleAction(LotusActionType.RoundStart)]
    private void ResetCertainEffects()
    {
        ExtraVotes = 0;
        QuickFixSabotage = false;
    }

    [RoleAction(LotusActionType.Interaction)]
    private void ProtectionEffect(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (!IsProtected) return;
        if (interaction.Intent is INeutralIntent or IHelpfulIntent) return;
        IsProtected = false;
        handle.Cancel();
    }

    [RoleAction(LotusActionType.MyVote)]
    private void IncreasedVoting(Optional<PlayerControl> votedFor, MeetingDelegate meetingDelegate)
    {
        if (!votedFor.Exists()) return;
        for (int i = 0; i < ExtraVotes; i++) meetingDelegate.CastVote(MyPlayer, votedFor);
    }

    [RoleAction(LotusActionType.SabotagePartialFix)]
    private void SabotageQuickFix(PlayerControl fixer, ISabotage sabotage)
    {
        if (fixer.PlayerId != MyPlayer.PlayerId) return;
        sabotage.Fix(MyPlayer);
        QuickFixSabotage = false;
    }
}