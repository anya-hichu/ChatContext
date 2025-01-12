using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ChatContext.Windows;
using Dalamud.Interface;

namespace ChatContext;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    private const string CommandName = "/chatcontext";
    private const string CommandHelpMessage = $"Available subcommands for {CommandName} are main, config, disable and enable";

    public readonly WindowSystem WindowSystem = new("ChatContext");

    public Config Config { get; init; }
    private NearbyPlayers NearbyPlayers { get; init; }
    private ChatEnricher ChatEnricher { get; init; }
    private MainWindow MainWindow { get; init; }
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Config = PluginInterface.GetPluginConfig() as Config ?? new() { 
            Types = new(Config.DEFAULT_TYPES)
        };

        NearbyPlayers = new NearbyPlayers(ClientState, ObjectTable, Framework, Config);
        ChatEnricher = new ChatEnricher(ChatGui, NearbyPlayers, Config, PluginLog);

        MainWindow = new MainWindow(this, NearbyPlayers)
        {
            TitleBarButtons = [new()
            {
                Icon = FontAwesomeIcon.Cog,
                Click = (_) => ToggleConfigUI()
            }]
        };
        ConfigWindow = new ConfigWindow(this)
        {
            TitleBarButtons = [new()
            {
                Icon = FontAwesomeIcon.ListAlt,
                Click = (_) => ToggleMainUI()
            }]
        };

        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = CommandHelpMessage
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
        else if (subcommand == "enable")
        {
            Config.Enabled = true;
            Config.Save();
        }
        else if (subcommand == "disable")
        {
            Config.Enabled = false;
            Config.Save();
        }
        else
        {
            ChatGui.Print(CommandHelpMessage);
        }
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
