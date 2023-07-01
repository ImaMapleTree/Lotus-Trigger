using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.API.Vanilla.Meetings;
using Lotus.Extensions;
using Lotus.Options.LotusImpl;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Enums;
using LotusTrigger.Options;
using VentLib.Logging;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard;

public partial class StandardGamemode
{
    [LotusAction(LotusActionType.MeetingCalled)]
    public void SyncMeetingButtons(ActionHandle handle)
    {
        if (!GeneralOptions.MeetingOptions.SyncMeetingButtons) return;
        VentLogger.Trace($"Checking Synced Meeting Buttons. (ButtonsUsed={Game.MatchData.EmergencyButtonsUsed}, TotalButtons={GeneralOptions.MeetingOptions.MeetingButtonPool})");
        if (Game.MatchData.EmergencyButtonsUsed++ <GeneralOptions.MeetingOptions.MeetingButtonPool) return;
        VentLogger.Trace("Cancelling meeting, too many buttons used.");
        handle.Cancel();
    }

    [LotusAction(LotusActionType.RoundStart)]
    public void RoundStartKillExiled()
    {
        if (GeneralOptions.MeetingOptions.ResolveTieMode is not ResolveTieMode.KillAll ||MeetingDelegate.Instance.TiedPlayers.Count < 2) return;
        VentLogger.Trace("Round Start - Killing Tied Players");
        MeetingDelegate.Instance.TiedPlayers.Filter(Players.PlayerById).ForEach(p => p.RpcExileV2(true));
    }
}