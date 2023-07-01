using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using LotusTrigger.Options.Roles;
using Bait = LotusTrigger.Roles.Subroles.Bait;
using Bewilder = LotusTrigger.Roles.Subroles.Bewilder;
using Bloodlust = LotusTrigger.Roles.Subroles.Bloodlust;
using Deadly = LotusTrigger.Roles.Subroles.Deadly;
using Diseased = LotusTrigger.Roles.Subroles.Diseased;
using Flash = LotusTrigger.Roles.Subroles.Flash;
using Honed = LotusTrigger.Roles.Subroles.Honed;
using Nimble = LotusTrigger.Roles.Subroles.Nimble;
using Oblivious = LotusTrigger.Roles.Subroles.Oblivious;
using Romantic = LotusTrigger.Roles.Subroles.Romantics.Romantic;
using Sleuth = LotusTrigger.Roles.Subroles.Sleuth;
using TieBreaker = LotusTrigger.Roles.Subroles.TieBreaker;
using Torch = LotusTrigger.Roles.Subroles.Torch;
using Unstoppable = LotusTrigger.Roles.Subroles.Unstoppable;
using Watcher = LotusTrigger.Roles.Subroles.Watcher;
using Workhorse = LotusTrigger.Roles.Subroles.Workhorse;

namespace LotusTrigger.Roles.Collections;

public class ModifierCollection: RoleCollection
{
    public CustomRole LOAD_MODIFIER_OPTIONS = new EnforceFunctionOrderingRole(() => RoleOptions.SubroleOptions = new SubroleOptions());
    
    public Bait Bait = new Bait();
    public Bewilder Bewilder = new Bewilder();
    public Bloodlust Bloodlust = new Bloodlust();
    public Deadly Deadly = new Deadly();
    public Diseased Diseased = new Diseased();
    public Flash Flash = new Flash();
    public Honed Honed = new Honed();
    public Nimble Nimble = new Nimble();
    public Oblivious Oblivious = new Oblivious();
    public Romantic Romantic = new Romantic();
    public Sleuth Sleuth = new Sleuth();
    public TieBreaker TieBreaker = new TieBreaker();
    public Torch Torch = new Torch();
    public Unstoppable Unstoppable = new Unstoppable();
    public Watcher Watcher = new Watcher();
    public Workhorse Workhorse = new Workhorse();
}