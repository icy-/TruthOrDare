using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace TruthOrDare;

public class ChannelSetting
{
    public int ChatType { get; set; }
    public string Name { get; set; } = string.Empty;
    //---- Deprecated, setting will be managed per Reaction
    public bool Enabled { get; set; }
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;

    // Whether anyone typing !tod in saychat will make you start a new game
    public bool ReactToExclamTd { get; set; } = true;
    public bool ReactToExclamTod { get; set; } = true;
    public bool ReactToExclamTruth { get; set; } = true;
    public bool ReactToExclamDare { get; set; } = true;

    // Time for Rolls
    public int RollsTime { get; set; } = 45;
    public const int MinRollTime = 6;

    public bool DebugLogTypes { get; set; } = false;

    // List for now, but maybe Dictionary later
    public List<Roll> Rolls { get; set; } = []!;
    public Roll HighRoll { get; set; } = new Roll("", -1);
    public Roll LowRoll { get; set; } = new Roll("", 1000);

    public void ClearRolls()
    {
        Rolls.Clear();
        HighRoll = new Roll("", -1);
        LowRoll = new Roll("", 1000);
    }


    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
