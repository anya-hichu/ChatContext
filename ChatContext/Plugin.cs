using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ChatContext.Windows;

namespace ChatContext;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; set; } = null!;
    [PluginService] internal static IClientState ClientState { get; set; } = null!;

    private const string CommandName = "/chatcontext";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("ChatContext");

    private NearbyPlayers NearbyPlayers;
    private ChatEnricher ChatEnricher;
    

    private MainWindow MainWindow { get; init; }
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        NearbyPlayers = new NearbyPlayers(ClientState, ObjectTable, Framework);
        ChatEnricher = new ChatEnricher(ChatGui, NearbyPlayers, Configuration);

        MainWindow = new MainWindow(this, NearbyPlayers);
        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open chat context window (help, config and main subcommands are available)"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    public void Dispose()
    {
        NearbyPlayers.Dispose();
        ChatEnricher.Dispose();

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        var subcommand = args.Split(" ", 2)[0];

        if (subcommand == "main")
        {
            ToggleMainUI();
        } 
        else if (subcommand == "config")
        {
            ToggleConfigUI();
        }
        else
        {
            ChatGui.Print($"Available subcommands for {CommandName} are help, config and main");
        }
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
