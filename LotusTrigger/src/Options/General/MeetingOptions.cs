using System.Collections.Generic;
using Lotus;
using Lotus.Options;
using Lotus.Options.LotusImpl;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace LotusTrigger.Options.General;

[Localized(ModConstants.Options)]
public class MeetingOptions: LotusMeetingOptions
{
    public static MeetingOptions Instance = null!;    
    private static Color _optionColor = new(0.27f, 0.75f, 1f);
    private static List<GameOption> additionalOptions = new();

    public MeetingOptions()
    {
        Instance = this;
        AllOptions.Add(new GameOptionTitleBuilder()
            .Title(LotusMeetingOptions.MeetingOptionTranslations.SectionTitle)
            .Color(_optionColor)
            .Tab(DefaultTabs.GeneralTab)
            .Build());

        AllOptions.Add(Builder("Single Meeting Pool")
            .IsHeader(true)
            .Name(LotusMeetingOptions.MeetingOptionTranslations.SingleMeetingPool)
            .BindInt(i => MeetingButtonPool= i)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(-1).Build())
            .AddIntRange(1, 30)
            .BuildAndRegister());

        AllOptions.Add(Builder("Resolve Tie Mode")
            .Name(LotusMeetingOptions.MeetingOptionTranslations.ResolveTieMode)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(0).Build())
            .Value(v => v.Text(LotusMeetingOptions.MeetingOptionTranslations.RandomPlayer).Color(ModConstants.Palette.InfinityColor).Value(1).Build())
            .Value(v => v.Text(LotusMeetingOptions.MeetingOptionTranslations.KillAll).Color(ModConstants.Palette.GeneralColor4).Value(2).Build())
            .BindInt(i => ResolveTieMode = (ResolveTieMode)i)
            .BuildAndRegister());

        AllOptions.Add(Builder("No Vote Mode")
            .Name(LotusMeetingOptions.MeetingOptionTranslations.SkipVoteMode)
            .Value(v => v.Text(GeneralOptionTranslations.OffText).Color(Color.red).Value(0).Build())
            .Value(v => v.Text(LotusMeetingOptions.MeetingOptionTranslations.RandomVote).Color(ModConstants.Palette.InfinityColor).Value(1).Build())
            .Value(v => v.Text(LotusMeetingOptions.MeetingOptionTranslations.ReverseVote).Color(new Color(0.55f, 0.73f, 1f)).Value(2).Build())
            .Value(v => v.Text(LotusMeetingOptions.MeetingOptionTranslations.ExplodeOnSkip).Color(new Color(1f, 0.4f, 0.2f)).Value(3).Build())
            .BindInt(i => NoVoteMode = (SkipVoteMode)i)
            .BuildAndRegister());

        additionalOptions.ForEach(o =>
        {
            o.Register();
            AllOptions.Add(o);
        });
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.GeneralTab).Color(_optionColor);
}

