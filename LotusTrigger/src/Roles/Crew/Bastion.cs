using System.Collections.Generic;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities.Collections;
using static LotusTrigger.Roles.Crew.Bastion.BastionTranslations.BastionOptionTranslations;

namespace LotusTrigger.Roles.Crew;

public class Bastion: Engineer
{
    private int bombsPerRounds;
    // Here we can use the vent button as cooldown
    [NewOnSetup] private HashSet<int> bombedVents;

    private int currentBombs;
    private Remote<CounterComponent>? counterRemote;


    protected override void PostSetup()
    {
        if (bombsPerRounds == -1) return;
        CounterHolder counterHolder = MyPlayer.NameModel().GetComponentHolder<CounterHolder>();
        LiveString ls = new(() => RoleUtils.Counter(currentBombs, bombsPerRounds, ModConstants.Palette.GeneralColor2));
        counterRemote = counterHolder.Add(new CounterComponent(ls,new[] { GameState.Roaming }, ViewMode.Additive, MyPlayer));
    }

    [RoleAction(LotusActionType.AnyEnterVent)]
    private void EnterVent(Vent vent, PlayerControl player, ActionHandle handle)
    {
        bool isBombed = bombedVents.Remove(vent.Id);
        VentLogger.Trace($"Bombed Vent Check: (player={player.name}, isBombed={isBombed})", "BastionAbility");
        if (isBombed) MyPlayer.InteractWith(player, CreateInteraction(player));
        else if (player.PlayerId == MyPlayer.PlayerId)
        {
            handle.Cancel();
            if (currentBombs == 0) return;
            currentBombs--;
            bombedVents.Add(vent.Id);
        }
    }

    [RoleAction(LotusActionType.RoundStart, triggerAfterDeath: true)]
    private void RefreshBastion()
    {
        currentBombs = bombsPerRounds;
        bombedVents.Clear();
    }

    [RoleAction(LotusActionType.MyDeath)]
    private void ClearCounter() => counterRemote?.Delete();

    private IndirectInteraction CreateInteraction(PlayerControl deadPlayer)
    {
        return new IndirectInteraction(new FatalIntent(true, () => new BombedEvent(deadPlayer, MyPlayer)), this);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Plant Bomb Cooldown", PlantBombCooldown)
                .BindFloat(v => VentCooldown = v)
                .Value(1f)
                .IOSettings(io => io.UnknownValueAction = ADEAnswer.Allow)
                .AddFloatRange(2.5f, 120, 2.5f, 8, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .KeyName("Bombs per Round", BombsPerRound)
                .Value(v => v.Text(ModConstants.Infinity).Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 20, 1, 0)
                .BindInt(i => bombsPerRounds = i)
                .Build());


    protected override AbstractBaseRole.RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor("#524f4d");

    [Localized(nameof(Bastion))]
    internal static class BastionTranslations
    {
        [Localized(ModConstants.Options)]
        public static class BastionOptionTranslations
        {
            [Localized(nameof(PlantBombCooldown))]
            public static string PlantBombCooldown = "Plant Bomb Cooldown";

            [Localized(nameof(BombsPerRound))]
            public static string BombsPerRound = "Bombs per Round";
        }
    }
}