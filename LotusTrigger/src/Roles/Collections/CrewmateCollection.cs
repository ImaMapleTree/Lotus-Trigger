using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Builtins.Vanilla;
using LotusTrigger.Options.Roles;
using LotusTrigger.Roles.Crew;
using LotusTrigger.Roles.Crew.Alchemist;
using LotusTrigger.Roles.Neutral;

namespace LotusTrigger.Roles.Collections;

public class CrewmateCollection: RoleCollection
{
    public CustomRole LOAD_CREW_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.CrewmateOptions = new CrewmateOptions());
    
    public Alchemist Alchemist = new Alchemist();
    public Bastion Bastion = new Bastion();
    public Bodyguard Bodyguard = new Bodyguard();
    public Chameleon Chameleon = new Chameleon();
    public Charmer Charmer = new Charmer();
    public Crewmate Crewmate = new Crewmate();
    public Crusader Crusader = new Crusader();
    public Demolitionist Demolitionist = new Demolitionist();
    public Dictator Dictator = new Dictator();
    public Doctor Doctor = new Doctor();

    public Escort Escort = new Escort();
    public ExConvict ExConvict = new ExConvict();
    public Herbalist Herbalist = new Herbalist();
    public Investigator Investigator = new Investigator();
    public Mayor Mayor = new Mayor();
    public Mechanic Mechanic = new Mechanic();
    public Medic Medic = new Medic();
    public Medium Medium = new Medium();
    public Mystic Mystic = new Mystic();
    public Observer Observer = new Observer();
    public Oracle Oracle = new Oracle();
    public Physicist Physicist = new Physicist();
    public Psychic Psychic = new Psychic();
    public Repairman Repairman = new Repairman();
    public Sheriff Sheriff = new Sheriff();
    public Snitch Snitch = new Snitch();
    public Speedrunner Speedrunner = new Speedrunner();
    public Swapper Swapper = new Swapper();
    public Tracker Tracker = new Tracker();
    public Transporter Transporter = new Transporter();
    public Trapster Trapster = new Trapster();
    public Veteran Veteran = new Veteran();
    public Vigilante Vigilante = new Vigilante();
}