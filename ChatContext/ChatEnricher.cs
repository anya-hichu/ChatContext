using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using System;
using System.Linq;

namespace ChatContext;

public class ChatEnricher : IDisposable
{
    private IChatGui ChatGui { get; init; }
    private NearbyPlayers NearbyPlayers { get; init; }
    private Configuration Configuration { get; init; }
    private IPluginLog PluginLog { get; init; }

    public ChatEnricher(IChatGui chatGui, NearbyPlayers nearbyPlayers, Configuration configuration, IPluginLog pluginLog)
    {
        ChatGui = chatGui;
        NearbyPlayers = nearbyPlayers;
        Configuration = configuration;
        PluginLog = pluginLog;

        ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        ChatGui.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(XivChatType type, int a2, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Configuration.Enabled && Configuration.FormatValid() && Configuration.Types.Contains(type))
        {
            var senderName = GetSenderName(sender);
            PluginLog.Verbose($"Matched Type: {type}, Sender Name: {senderName}");
            var targetName = NearbyPlayers.GetTargetName(senderName);
            if (targetName != null)
            {
                PluginLog.Verbose($"Successful Target Lookup: {senderName} => {targetName}");
                var suffix = new SeStringBuilder()
                    .Append(" ")
                    .AddUiForeground((ushort)Configuration.Color)
                    .Append(string.Format(Configuration.Format, targetName))
                    .AddUiForegroundOff()
                    .Build();
                message.Append(suffix);
            }
            else
            {
                PluginLog.Verbose($"Failed Target Lookup: {senderName}");
            }
        }
    }

    private static string GetSenderName(SeString sender)
    {
        // Cross-world
        foreach (var payload in sender.Payloads)
        {
            if (payload is PlayerPayload playerPayload)
            {
                return playerPayload.PlayerName;
            }
        }

        // Reverse to ignore prefixes (party number, etc.)
        foreach (var payload in sender.Payloads.Reverse<Payload>())
        {
            if (payload is TextPayload rawPayload)
            {
                return rawPayload.Text!;
            }
        }

        return string.Empty;
    }
}
