using Lotus;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using LotusTrigger.Roles.Neutral;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Collections;

public class NeutralCollection: RoleCollection
{
    public CustomRole NEUTRAL_PASSIVE_TITLE = new EnforceFunctionOrderingRole(() => new GameOptionTitleBuilder().Title("<size=2.3>❀ Neutral Passive ❀</size>").Color(ModConstants.Palette.PassiveColor).Tab(DefaultTabs.NeutralTab).Build());
    
    public Amnesiac Amnesiac = new Amnesiac();
    /*public Archangel Archangel = new Archangel();*/
    public Copycat Copycat = new Copycat();
    public Executioner Executioner = new Executioner();
    public Hacker Hacker = new Hacker();
    public Jester Jester = new Jester();
    public Opportunist Opportunist = new Opportunist();
    public Phantom Phantom = new Phantom();
    public Pirate Pirate = new Pirate();
    public Postman Postman = new Postman();
    public SchrodingersCat SchrodingersCat = new SchrodingersCat();
    public Survivor Survivor = new Survivor();
    public Terrorist Terrorist = new Terrorist();
    public Vulture Vulture = new Vulture();
}