using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ChatContext.Windows;

public class TableComparer : IComparer<KeyValuePair<string, string>>
{
    private ImGuiTableColumnSortSpecsPtr Specs;
    public TableComparer(ImGuiTableColumnSortSpecsPtr specs)
    {
        Specs = specs;
    }

    public int Compare(KeyValuePair<string, string> lhs, KeyValuePair<string, string> rhs)
    {
        var left = Specs.ColumnIndex == 0 ? lhs.Key : lhs.Value;
        var right = Specs.ColumnIndex == 0 ? rhs.Key : rhs.Value;

        return Specs.SortDirection == ImGuiSortDirection.Ascending ? left.CompareTo(right) : right.CompareTo(left);
    }
}

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private NearbyPlayers NearbyPlayers;

    public MainWindow(Plugin plugin, NearbyPlayers nearbyPlayers) : base("Chat Context##mainWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 375),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        NearbyPlayers = nearbyPlayers;
    }

    public void Dispose() { }


    public override void Draw()
    {
        if (Plugin.Configuration.Enabled)
        {
            ImGui.Text("Nearby player targets:");
            if (ImGui.BeginTable("nearbyPlayersTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable, new Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetTextLineHeight() * 4)))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortAscending);
                ImGui.TableSetupColumn("Target name");
                ImGui.TableHeadersRow();

                var specs = ImGui.TableGetSortSpecs().Specs;
                foreach (var entry in NearbyPlayers.GetTargetNameByName().ToList().Order(new TableComparer(specs))) {
                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text(entry.Key);
                    }

                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text(entry.Value);
                    }    
                }
                ImGui.EndTable();
            }
        }
        else
        {
            ImGui.Text("Plugin is disabled");
            if(ImGui.Button("Config"))
            {
                Plugin.ToggleConfigUI();
            }
        }  
    }
}
