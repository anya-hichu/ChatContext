using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace ChatContext;

public class NearbyPlayers : IDisposable
{
    private IClientState ClientState { get; init; }
    private IObjectTable ObjectTable { get; init; }
    private IFramework Framework { get; init; }
    private Configuration Configuration { get; init; }
    public ImmutableDictionary<string, string> TargetNameByName { get; private set; } = ImmutableDictionary.Create<string, string>();


    public NearbyPlayers(IClientState clientState, IObjectTable objectTable, IFramework framework, Configuration configuration)
    {
        ClientState = clientState;
        ObjectTable = objectTable;
        Framework = framework;
        Configuration = configuration;

        Framework.Update += OnFrameworkTick;
    }
    public void Dispose()
    {
        Framework.Update -= OnFrameworkTick;
    }

    public void OnFrameworkTick(IFramework framework)
    {
        if (Configuration.Enabled)
        {
            TargetNameByName = ObjectTable
                .Where(x => x is IPlayerCharacter)
                .Cast<IPlayerCharacter>()
                .Where(c => c.TargetObject is IPlayerCharacter)
                .ToImmutableDictionary(c => c.Name.TextValue, c => c.TargetObject!.Name.TextValue);
        }
    }

    public string? GetTargetName(string name)
    {
        if (TargetNameByName.TryGetValue(name, out var targetName))
        {
            return targetName;
        } 
        else
        {
            return null;
        }
    }
}
