using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;


namespace ChatContext.Windows;

public class MainWindow : Window
{
    private class TableComparer(ImGuiTableColumnSortSpecsPtr specs) : IComparer<KeyValuePair<string, string>>
    {
        private ImGuiTableColumnSortSpecsPtr Specs { get; init; } = specs;

        public int Compare(KeyValuePair<string, string> lhs, KeyValuePair<string, string> rhs)
        {
            var (left, right) = Specs.ColumnIndex == 0 ? (lhs.Key, rhs.Key) : (lhs.Value, rhs.Value);
            return Specs.SortDirection == ImGuiSortDirection.Ascending ? left.CompareTo(right) : right.CompareTo(left);
        }
    }

    private Plugin Plugin { get; init; }
    private NearbyPlayers NearbyPlayers { get; init; }

    public MainWindow(Plugin plugin, NearbyPlayers nearbyPlayers) : base("Chat Context - Nearby Players##mainWindow")
    {
        SizeConstraints = new()
        {
            MinimumSize = new(300, 180),
            MaximumSize = new(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
        NearbyPlayers = nearbyPlayers;
    }

    public override void Draw()
    {
        if (Plugin.Config.Enabled)
        {
            using (var table = ImRaii.Table("nearbyPlayersTable", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable, new(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetTextLineHeight() * 3)))
            {
                if (table)
                {
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortAscending);
                    ImGui.TableSetupColumn("Target name");
                    ImGui.TableHeadersRow();

                    var specs = ImGui.TableGetSortSpecs().Specs;
                    foreach (var entry in NearbyPlayers.TargetNameByName.ToList().Order(new TableComparer(specs)))
                    {
                        if (ImGui.TableNextColumn())
                        {
                            ImGui.Text(entry.Key);
                        }

                        if (ImGui.TableNextColumn())
                        {
                            ImGui.Text(entry.Value);
                        }
                    }
                }
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
