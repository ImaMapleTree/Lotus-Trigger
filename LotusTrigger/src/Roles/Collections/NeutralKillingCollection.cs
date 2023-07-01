using Lotus;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using LotusTrigger.Options.Roles;
using LotusTrigger.Roles.NeutralKilling;
using LotusTrigger.Roles.Undead.Roles;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Collections;

public class NeutralKillingCollection: RoleCollection
{
    public CustomRole LOAD_NEUTRAL_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.NeutralOptions = new NeutralOptions());

    public CustomRole NEUTRAL_KILLING_TITLE = new EnforceFunctionOrderingRole(() => new GameOptionTitleBuilder().Title("<size=2.3>★ Neutral Killing ★</size>").Color(ModConstants.Palette.KillingColor).Tab(DefaultTabs.NeutralTab).Build());
    
    public AgiTater AgiTater = new AgiTater();
    public Arsonist Arsonist = new Arsonist();
    public BloodKnight BloodKnight = new BloodKnight();
    public Demon Demon = new Demon();
    public Egoist Egoist = new Egoist();
    public Hitman Hitman = new Hitman();
    public Jackal Jackal = new Jackal();
    public Juggernaut Juggernaut = new Juggernaut();
    public Marksman Marksman = new Marksman();
    public Necromancer Necromancer = new Necromancer();
    public Occultist Occultist = new Occultist();
    public Pelican Pelican = new Pelican();
    public PlagueBearer PlagueBearer = new PlagueBearer();
    public Retributionist Retributionist = new Retributionist();
    public Glitch Glitch = new Glitch();
    public Werewolf Werewolf = new Werewolf();
}