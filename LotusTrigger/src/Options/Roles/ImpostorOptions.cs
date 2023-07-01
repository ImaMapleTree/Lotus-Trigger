using Lotus.Extensions;
using Lotus.Options;
using Lotus.Options.LotusImpl.Roles;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options;
using VentLib.Utilities;

namespace LotusTrigger.Options.Roles;

public class ImpostorOptions: LotusImpostorOptions
{
    public static ImpostorOptions Instance = null!;
    
    public ImpostorOptions()
    {
        Instance = this;
        OptionManager optionManager = OptionManager.GetManager(file: "options.txt");

        TriggerAddon.AddonInstance.Specials.ImpostorGuesser.GetGameOptionBuilder()
            .Tab(DefaultTabs.ImpostorsTab)
            .KeyName("Impostor Guessers", Color.white.Colorize(TranslationUtil.Colorize(Translations.ImpostorGuessers, Color.red)))
            .BuildAndRegister(optionManager);
    }

    [Localized("RolesImpostors")]
    private static class Translations
    {
        [Localized(nameof(ImpostorGuessers))]
        public static string ImpostorGuessers = "Impostor::0 Guessers";
    }

}