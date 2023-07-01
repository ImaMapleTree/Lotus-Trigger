using Lotus;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Options.LotusImpl.Roles;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Utilities;

namespace LotusTrigger.Options.Roles;

[Localized(ModConstants.Options)]
public class CrewmateOptions: LotusCrewmateOption
{
    public static CrewmateOptions Instance = null!;
    
    public CrewmateOptions()
    {
        Instance = this;
        OptionManager optionManager = OptionManager.GetManager(file: "options.txt");

        TriggerAddon.AddonInstance.Specials.CrewGuesser.GetGameOptionBuilder()
            .Tab(DefaultTabs.CrewmateTab)
            .KeyName("Crewmate Guessers", Color.white.Colorize(TranslationUtil.Colorize(Translations.CrewmateGuessers, ModConstants.Palette.CrewmateColor)))
            .BuildAndRegister(optionManager);
    }

    [Localized("RolesCrewmates")]
    private static class Translations
    {
        [Localized(nameof(CrewmateGuessers))]
        public static string CrewmateGuessers = "Crewmates::0 Guessers";
    }
}