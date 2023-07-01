using Lotus;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Options.LotusImpl.Roles;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Options.IO;
using VentLib.Utilities;
using static Lotus.ModConstants.Palette;
using static LotusTrigger.TriggerAddon;

namespace LotusTrigger.Options.Roles;

[Localized(ModConstants.Options)]
public class NeutralOptions: LotusNeutralOptions
{
    public static NeutralOptions Instance = null!;
    public int MinimumNeutralPassiveRoles;
    public int MaximumNeutralPassiveRoles;

    public int MinimumNeutralKillingRoles;
    public int MaximumNeutralKillingRoles;
    
    private bool openGuessers;

    public NeutralOptions()
    {
        Instance = this;
        string GColor(string input) => TranslationUtil.Colorize(input, NeutralColor, PassiveColor, KillingColor);

        Builder("Neutral Teaming Mode")
            .IsHeader(true)
            .Name(GColor(NeutralOptionTranslations.NeutralTeamMode))
            .BindInt(i => NeutralTeamingMode = (NeutralTeaming)i)
            .Value(v => v.Text(GeneralOptionTranslations.DisabledText).Value(0).Color(Color.red).Build())
            .Value(v => v.Text(NeutralOptionTranslations.SameRoleText).Value(1).Color(GeneralColor2).Build())
            .Value(v => v.Text(GColor(NeutralOptionTranslations.KillerNeutral)).Value(2).Build())
            .Value(v => v.Text(GeneralOptionTranslations.AllText).Value(3).Color(GeneralColor4).Build())
            .IOSettings(io => io.UnknownValueAction = ADEAnswer.UseDefault)
            .ShowSubOptionPredicate(v => (int)v >= 1)
            .SubOption(sub => sub.KeyName("Team Knows Each Other's Roles", NeutralOptionTranslations.AlliedKnowRoles)
                .AddOnOffValues()
                .BindBool(b => KnowAlliedRolesProtected = b)
                .Build())
            .BuildAndRegister();

        Builder("Minimum Neutral Passive Roles")
            .IsHeader(true)
            .Name(GColor(NeutralOptionTranslations.MinimumNeutralPassiveRoles))
            .BindInt(i => MinimumNeutralPassiveRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();

        Builder("Maximum Neutral Passive Roles")
            .Name(GColor(NeutralOptionTranslations.MaximumNeutralPassiveRoles))
            .BindInt(i => MaximumNeutralPassiveRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();

        Builder("Minimum Neutral Killing Roles")
            .Name(GColor(NeutralOptionTranslations.MinimumNeutralKillingRoles))
            .BindInt(i => MinimumNeutralKillingRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();

        Builder("Maximum Neutral Killing Roles")
            .Name(GColor(NeutralOptionTranslations.MaximumNeutralKillingRoles))
            .BindInt(i => MaximumNeutralKillingRoles = i)
            .AddIntRange(0, 15)
            .BuildAndRegister();

        Builder("Neutral Guessers")
            .IsHeader(true)
            .Name(TranslationUtil.Colorize(NeutralOptionTranslations.NeutralGuessers, NeutralColor))
            .BindBool(b => openGuessers = b)
            .AddOnOffValues(false)
            .ShowSubOptionPredicate(b => (bool)b)
            .SubOption(_ => AddonInstance.Specials.NeutralKillerGuesser.GetGameOptionBuilder()
                .IsHeader(false)
                .KeyName("Neutral Killing Guessers", Color.white.Colorize(GColor(NeutralOptionTranslations.NeutralKillerGuesser)))
                .Build())
            .SubOption(_ => AddonInstance.Specials.NeutralGuesser.GetGameOptionBuilder()
                .IsHeader(false)
                .KeyName("Neutral Guessers", Color.white.Colorize(GColor(NeutralOptionTranslations.NeutralPassiveGuesser)))
                .Build())
            .BuildAndRegister();
    }

    private GameOptionBuilder Builder(string key) => new GameOptionBuilder().Key(key).Tab(DefaultTabs.NeutralTab);

    [Localized("RolesNeutral")]
    private static class NeutralOptionTranslations
    {
        [Localized(nameof(MinimumNeutralPassiveRoles))]
        public static string MinimumNeutralPassiveRoles = "Minimum Neutral::0 Passive::1 Roles";

        [Localized(nameof(MaximumNeutralPassiveRoles))]
        public static string MaximumNeutralPassiveRoles = "Maximum Neutral::0 Passive::1 Roles";

        [Localized(nameof(MinimumNeutralKillingRoles))]
        public static string MinimumNeutralKillingRoles = "Minimum Neutral::0 Killing::2 Roles";

        [Localized(nameof(MaximumNeutralKillingRoles))]
        public static string MaximumNeutralKillingRoles = "Maximum Neutral::0 Killing::2 Roles";

        [Localized(nameof(NeutralTeamMode))]
        public static string NeutralTeamMode = "Neutral::0 Teaming Mode";

        [Localized(nameof(SameRoleText))]
        public static string SameRoleText = "Same Role";

        [Localized("KillingAndPassiveText")]
        public static string KillerNeutral = "Killing::2 And Passive::1";

        [Localized(nameof(AlliedKnowRoles))]
        public static string AlliedKnowRoles = "Team Knows Everyone's Role";

        [Localized(nameof(NeutralGuessers))]
        public static string NeutralGuessers = "Neutral::0 Guessers";

        [Localized(nameof(NeutralKillerGuesser))]
        public static string NeutralKillerGuesser = "Neutral::0 Killing::2 Guessers";

        [Localized(nameof(NeutralPassiveGuesser))]
        public static string NeutralPassiveGuesser = "Neutral::0 Passive::1 Guessers";

    }
}

