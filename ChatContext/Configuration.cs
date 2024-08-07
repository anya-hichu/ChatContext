using Dalamud.Configuration;
using Dalamud.Game.Text;
using System;
using System.Collections.Generic;

namespace ChatContext;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = true;

    public HashSet<XivChatType> Types { get; set; } = new() {
        XivChatType.Alliance, 
        XivChatType.Yell, 
        XivChatType.Party, 
        XivChatType.Say, 
        XivChatType.Shout 
    };

    public string Format { get; set; } = "[ {0}]";

    public UIColor Color { get; set; } = UIColor.Grey4;


    public bool FormatValid()
    {
        try 
        { 
            var _ = string.Format(Format, string.Empty);
            return true; 
        } 
        catch (FormatException)
        {
            return false;
        }
    }


    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
