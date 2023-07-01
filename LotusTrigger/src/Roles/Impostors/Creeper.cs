using Lotus;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static LotusTrigger.Roles.Impostors.Creeper.CreeperTranslations.CreeperOptionTranslations;

namespace LotusTrigger.Roles.Impostors;

public class Creeper : Shapeshifter
{
    private bool canKillNormally;
    private bool creeperProtectedByShields;
    private float explosionRadius;
    private Cooldown gracePeriod;

    [UIComponent(UI.Text)]
    public string GracePeriodText() => gracePeriod.IsReady() ? "" : Color.red.Colorize(CreeperTranslations.ExplosionGracePeriod).Formatted(gracePeriod + "s");

    [RoleAction(LotusActionType.RoundStart)]
    private void BeginGracePeriod()
    {
        gracePeriod.Start();
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => canKillNormally && base.TryKill(target);

    [RoleAction(LotusActionType.OnPet)]
    [RoleAction(LotusActionType.Shapeshift)]
    private void CreeperExplode()
    {
        if (gracePeriod.NotReady()) return;
        RoleUtils.GetPlayersWithinDistance(MyPlayer, explosionRadius).ForEach(p =>
        {
            FatalIntent intent = new(true, () => new BombedEvent(p, MyPlayer));
            MyPlayer.InteractWith(p, new LotusInteraction(intent, this));
        });

        FatalIntent suicideIntent = new(false, () => new BombedEvent(MyPlayer, MyPlayer));
        MyPlayer.InteractWith(MyPlayer, new LotusInteraction(suicideIntent, this) { IsPromised = creeperProtectedByShields });
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Can Kill Normally", CanKillNormal)
                .AddOnOffValues()
                .BindBool(b => canKillNormally = b)
                .Build())
            .SubOption(sub => sub.KeyName("Creeper Protected By Shielding", CreeperProtection)
                .AddOnOffValues()
                .BindBool(b => creeperProtectedByShields = b)
                .Build())
            .SubOption(sub => sub.KeyName("Explosion Radius", ExplosionRadius)
                .Value(v => v.Value(2f).Text(SmallDistance).Build())
                .Value(v => v.Value(3f).Text(MediumDistance).Build())
                .Value(v => v.Value(4f).Text(LargeDistance).Build())
                .BindFloat(f => explosionRadius = f)
                .Build())
            .SubOption(sub => sub.KeyName("Creeper Grace Period", CreeperGracePeriod)
                .AddFloatRange(0, 60, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(gracePeriod.SetDuration)
                .Build());

    [Localized(nameof(Creeper))]
    internal static class CreeperTranslations
    {
        [Localized(nameof(ExplosionGracePeriod))]
        public static string ExplosionGracePeriod = "Explosion Grace Period: {0}";

        [Localized(ModConstants.Options)]
        internal static class CreeperOptionTranslations
        {
            public static string SmallDistance = "Small";

            public static string MediumDistance = "Medium";

            public static string LargeDistance = "Large";

            [Localized(nameof(CanKillNormal))]
            public static string CanKillNormal = "Can Kill Normally";

            [Localized(nameof(CreeperGracePeriod))]
            public static string CreeperGracePeriod = "Grace Period";

            [Localized(nameof(CreeperProtection))]
            public static string CreeperProtection = "Protected by Shielding";

            [Localized(nameof(ExplosionRadius))]
            public static string ExplosionRadius = "Explosion Radius";

        }
    }
}