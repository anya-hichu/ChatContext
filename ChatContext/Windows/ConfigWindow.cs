using System;
using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ChatContext.Windows;

public class ConfigWindow : Window
{
    private Config Config { get; init; }

    public ConfigWindow(Plugin plugin) : base("Chat Context - Config##configWindow")
    {
        SizeConstraints = new()
        {
            MinimumSize = new(300, 380),
            MaximumSize = new(float.MaxValue, float.MaxValue)
        };

        Config = plugin.Config;
    }

    public override void Draw()
    {
        var enabled = Config.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            Config.Enabled = enabled;
            Config.Save();
        }

        if (ImGui.CollapsingHeader("Channels", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Types
            var types = Config.Types.ToHashSet();
            using (ImRaii.Child("channel", new(ImGui.GetWindowWidth(), 180)))
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
                            Config.Types = types;
                            Config.Save();
                        }
                    }
                }
            }
        }

        if (ImGui.CollapsingHeader("Chat info", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Format
            var format = Config.Format;
            if (ImGui.InputTextWithHint("Format", "Target name placeholder is {0}", ref format, 255))
            {
                Config.Format = format;
                Config.Save();
            }

            ImGui.Text("Preview: ");
            ImGui.SameLine();
            ImGui.Text(Config.FormatValid()? string.Format(format, "Random Name") : "Invalid format");

            // Colors
            var colorName = Enum.GetName(typeof(UIColor), Config.Color)!;
            var colorNames = Enum.GetNames(typeof(UIColor)).ToList();

            var index = colorNames.IndexOf(colorName);
            if (ImGui.Combo("Color", ref index, colorNames.ToArray(), colorNames.Count))
            {
                var enumName = colorNames[index];
                Config.Color = (UIColor)Enum.Parse(typeof(UIColor), enumName);
                Config.Save();
            }
        }
    }
}
