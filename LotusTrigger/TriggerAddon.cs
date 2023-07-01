using HarmonyLib;
using Lotus.Addons;
using Lotus.Options.LotusImpl;
using Lotus.Roles.Internals.Enums;
using LotusTrigger.Gamemodes.Standard;
using LotusTrigger.Options;
using LotusTrigger.Options.General;
using LotusTrigger.Roles.Collections;
using VentLib.Version;
using VentLib.Version.Git;

namespace LotusTrigger;

public class TriggerAddon: LotusAddon
{
    public static TriggerAddon AddonInstance;
    public override string Name => "Lotus-Trigger";
    public override Version Version { get; } = new GitVersion();
    
    public ImpostorCollection Impostors = null!;
    public CrewmateCollection Crewmates = null!;
    public NeutralKillingCollection NeutralKillings = null!;
    public NeutralCollection NeutralPassives = null!;
    public ModifierCollection Modifiers = null!;
    public SpecialCollection Specials = null!;
    
    private Harmony harmony;
    
    public override void Initialize()
    {
        AddonInstance = this;
        
        harmony = new Harmony("com.tealeaf.LotusTrigger");
        harmony.PatchAll();
        
        Impostors = new ImpostorCollection();
        Crewmates = new CrewmateCollection();
        NeutralKillings = new NeutralKillingCollection();
        NeutralPassives = new NeutralCollection();
        Modifiers = new ModifierCollection();
        Specials = new SpecialCollection();

        GeneralOptions.GameplayOptions = new GameplayOptions();
        GeneralOptions.MayhemOptions = new MayhemOptions();
        GeneralOptions.MeetingOptions = new MeetingOptions();
        GeneralOptions.MiscellaneousOptions = new MiscellaneousOptions();
        GeneralOptions.SabotageOptions = new SabotageOptions();
        GeneralOptions.DebugOptions = new DebugOptions();
        
        ExportRoles();
        ExportGamemodes(new StandardGamemode());
    }

    private void ExportRoles()
    {
        ExportRoles(Impostors.GetRoles(), LotusRoleType.Impostors);
        ExportRoles(Crewmates.GetRoles(), LotusRoleType.Crewmates);
        ExportRoles(NeutralKillings.GetRoles(), LotusRoleType.NeutralKillers);
        ExportRoles(NeutralPassives.GetRoles(), LotusRoleType.NeutralPassives);
        ExportRoles(Modifiers.GetRoles(), LotusRoleType.Modifiers);
        ExportRoles(Specials.GetRoles(), SpecialCollection.Internal);
    }
}