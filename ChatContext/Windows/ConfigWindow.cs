using System;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ChatContext.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Chat Context Config##configWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 375),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }


    public override void Draw()
    {
        var enabled = Configuration.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            Configuration.Enabled = enabled;
            Configuration.Save();
        }

        if (ImGui.CollapsingHeader("Channels", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Types
            var types = Configuration.Types.ToHashSet();
            if (ImGui.BeginChild("channel", new Vector2(ImGui.GetWindowWidth(), 200)))
            {
                foreach (var enumName in Enum.GetNames(typeof(XivChatType)))
                {
                    if (enumName != "None")
                    {
                        var enumValue = (XivChatType)Enum.Parse(typeof(XivChatType), enumName);
                        var value = types.Contains(enumValue);
                        if (ImGui.Checkbox(enumName, ref value))
                        {
                            if (value)
                            {
                                types.Add(enumValue);
                            }
                            else
                            {
                                types.Remove(enumValue);
                            }
                            Configuration.Types = types;
                            Configuration.Save();
                        }
                    }
                }
                ImGui.EndChild();
            }
        }

        if (ImGui.CollapsingHeader("Chat info", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Format
            var format = Configuration.Format;
            if (ImGui.InputTextWithHint("Format", "Target name placeholder is {0}", ref format, 255))
            {
                Configuration.Format = format;
                Configuration.Save();
            }

            // Colors
            var colorName = Enum.GetName(typeof(UIColor), Configuration.Color)!;
            var colorNames = Enum.GetNames(typeof(UIColor)).ToList();

            var index = colorNames.IndexOf(colorName);
            if (ImGui.Combo("Color", ref index, colorNames.ToArray(), colorNames.Count))
            {
                var enumName = colorNames[index];
                Configuration.Color = (UIColor)Enum.Parse(typeof(UIColor), enumName);
                Configuration.Save();
            }
        }
    }
}
