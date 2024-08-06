using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatContext;
public class ChatEnricher : IDisposable
{
    private IChatGui ChatGui;
    private NearbyPlayers NearbyPlayers;
    private Configuration Configuration;

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
        
        if (Configuration.Types.Contains(type))
        {
            string name;
            
            if (sender.Payloads.Count > 0 && sender.Payloads[0] is PlayerPayload) 
            {
                // cross world player format
                name = ((PlayerPayload)sender.Payloads[0]).PlayerName;
            } else
            {
                name = sender.TextValue;
            }

            var targetName = NearbyPlayers.GetTargetName(name);
            if (targetName != null)
            {
                var suffix = new SeStringBuilder()
                    .AddUiForeground((ushort)Configuration.Color)
                    .Append(string.Format(Configuration.Format, targetName))
                    .AddUiForegroundOff()
                    .Build();

                message.Append(suffix);
            }
        }
    }
}
