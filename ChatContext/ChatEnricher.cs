using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using System;

namespace ChatContext;
public class ChatEnricher : IDisposable
{
    private IChatGui ChatGui { get; init; }
    private NearbyPlayers NearbyPlayers { get; init; }
    private Configuration Configuration { get; init; }

    public ChatEnricher(IChatGui chatGui, NearbyPlayers nearbyPlayers, Configuration configuration)
    {
        ChatGui = chatGui;
        NearbyPlayers = nearbyPlayers;
        Configuration = configuration;

        ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        ChatGui.ChatMessage -= OnChatMessage;
    }

    void OnChatMessage(XivChatType type, int a2, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Configuration.Enabled && Configuration.FormatValid() && Configuration.Types.Contains(type))
        {
            string name;
            if (sender.Payloads.Count > 0 && sender.Payloads[0] is PlayerPayload)
            {
                // cross world player format
                name = ((PlayerPayload)sender.Payloads[0]).PlayerName;
            }
            else
            {
                name = sender.TextValue;
            }

            var targetName = NearbyPlayers.GetTargetName(name);
            if (targetName != null)
            {
                var suffix = new SeStringBuilder()
                    .Append(" ")
                    .AddUiForeground((ushort)Configuration.Color)
                    .Append(string.Format(Configuration.Format, targetName))
                    .AddUiForegroundOff()
                    .Build();

                message.Append(suffix);
            }
        }
    }
}
