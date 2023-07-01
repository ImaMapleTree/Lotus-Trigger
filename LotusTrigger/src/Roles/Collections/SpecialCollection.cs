using Lotus.Roles.Internals.Enums;
using LotusTrigger.Roles.Subroles.Guessers;

namespace LotusTrigger.Roles.Collections;

public class SpecialCollection: RoleCollection
{
    public static LotusRoleType Internal = new("Internals", 100);

    public ImpGuesser ImpostorGuesser = new ImpGuesser();
    public CrewGuesser CrewGuesser = new CrewGuesser();
    public NeutralKillerGuesser NeutralKillerGuesser = new NeutralKillerGuesser();
    public NeutralGuesser NeutralGuesser = new NeutralGuesser();
}