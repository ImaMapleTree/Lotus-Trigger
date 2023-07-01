using System;
using Lotus;
using Lotus.Chat;
using Lotus.Chat.Commands;
using Lotus.Chat.Patches;
using Lotus.Managers;
using LotusTrigger.Options;
using LotusTrigger.Options.General;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Commands;

public class CosmeticCommands: CommandTranslations
{
    [Localized("Color.NotInRange")] public static string ColorNotInRangeMessage = "{0} is not in range of valid colors.";
    
    [Command(CommandFlag.LobbyOnly, "name")]
    public static void Name(PlayerControl source, string name)
    {
        if (name.IsNullOrWhiteSpace()) return;
        int allowedUsers = MiscellaneousOptions.Instance.ChangeNameUsers;
        bool permitted = allowedUsers switch
        {
            0 => source.IsHost(),
            1 => source.IsHost() || PluginDataManager.FriendManager.IsFriend(source),
            2 => true,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!permitted)
        {
            ChatHandlers.NotPermitted().Send(source);
            return;
        }

        if (name.Length > 25)
        {
            ChatHandler.Of($"Name too long ({name.Length} > 25).", CommandError).LeftAlign().Send(source);
            return;
        }

        if (source.IsHost()) OnChatPatch.EatMessage = true;

        source.RpcSetName(name);
    }
    
    [Command(CommandFlag.LobbyOnly, "color", "colour")]
    public static void SetColor(PlayerControl source, int color)
    {
        int allowedUsers = MiscellaneousOptions.Instance.ChangeColorAndLevelUsers;
        bool permitted = allowedUsers switch
        {
            0 => source.IsHost(),
            1 => source.IsHost() || PluginDataManager.FriendManager.IsFriend(source),
            2 => true,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!permitted)
        {
            ChatHandlers.NotPermitted().Send(source);
            return;
        }

        if (color > Palette.PlayerColors.Length - 1)
        {
            ChatHandler.Of($"{ColorNotInRangeMessage.Formatted(color)} (0-{Palette.PlayerColors.Length - 1})", ModConstants.Palette.InvalidUsage.Colorize(InvalidUsage)).LeftAlign().Send(source);
            return;
        }

        source.RpcSetColor((byte)color);
    }
}