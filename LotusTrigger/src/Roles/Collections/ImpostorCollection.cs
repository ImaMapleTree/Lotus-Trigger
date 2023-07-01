using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Builtins.Vanilla;
using LotusTrigger.Options.Roles;
using LotusTrigger.Roles.Impostors;
using LotusTrigger.Roles.Madmates;
using LotusTrigger.Roles.Madmates.Roles;

namespace LotusTrigger.Roles.Collections;

public class ImpostorCollection: RoleCollection
{
    public CustomRole LOAD_IMPOSTOR_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.ImpostorOptions = new ImpostorOptions());
    
    public Assassin Assassin = new Assassin();
    public Blackmailer Blackmailer = new Blackmailer();
    public BountyHunter BountyHunter = new BountyHunter();
    public Camouflager Camouflager = new Camouflager();
    public Consort Consort = new Consort();
    public Creeper Creeper = new Creeper();
    public Disperser Disperser = new Disperser();
    public Escapist Escapist = new Escapist();
    /*public FireWorks FireWorks = new FireWorks();*/
    public Freezer Freezer = new Freezer();
    public Grenadier Grenadier = new Grenadier();
    public IdentityThief IdentityThief = new IdentityThief();
    public Impostor Impostor = new Impostor();
    public Janitor Janitor = new Janitor();
    public Mafioso Mafioso = new Mafioso();
    public Mare Mare = new Mare();
    public Mastermind Mastermind = new Mastermind();
    public Miner Miner = new Miner();
    public Morphling Morphling = new Morphling();
    public Ninja Ninja = new Ninja();
    public PickPocket PickPocket = new PickPocket();
    public Puppeteer Puppeteer = new Puppeteer();
    public SerialKiller SerialKiller = new SerialKiller();
    public Sniper Sniper = new Sniper();
    public Swooper Swooper = new Swooper();
    public TimeThief TimeThief = new TimeThief();
    public Vampire Vampire = new Vampire();
    public Warlock Warlock = new Warlock();
    public Witch Witch = new Witch();
    public YinYanger YinYanger = new YinYanger();
    
    public CustomRole MADMATE_TITLE = new EnforceFunctionOrderingRole(() => RoleOptions.MadmateOptions = new MadmateOptions());
    
    public CrewPostor CrewPostor = new CrewPostor();
    public Madmate Madmate = new Madmate();
    public MadGuardian MadGuardian = new MadGuardian();
    public MadSnitch MadSnitch = new MadSnitch();
    public Parasite Parasite = new Parasite();
}