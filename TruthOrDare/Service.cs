using Dalamud.Game.Text;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace TruthOrDare;

internal class Service
{

    [PluginService]
    public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    public static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    public static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    public static ISigScanner SigScanner { get; private set; } = null!;

    [PluginService]
    public static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    public static IFramework Framework { get; private set; } = null!;

    [PluginService] public static IPluginLog Logger { get; set; } = null!;

    public static ChatServer ChatServer { get; set; } = null!;
    public static ChatSender ChatSender { get; set; } = null!;

    public static Plugin? plugin;
    public static Configuration configuration { get; set; } = null!;

    private const uint CHANNEL_COUNT = 23;

    // So apparently Puppet just makes this class completely static class methods
    public static void SomeMethod()
    {

    }

    public static void InitializeConfig()
    {
        // This line makes it persist, but for testing maybe I stick with new Configuration()
        // PERSISTING CONFIGURATION?
        //configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // TEST CONFIGURATION?
        configuration = new Configuration();  


        configuration.Initialize(PluginInterface);

        // Dummy data for my table!!
        //configuration.Rolls.Add(new Roll("Michael Jordan", 333));
        //configuration.Rolls.Add(new Roll("Scarlett Johansson", 987));
        //configuration.Rolls.Add(new Roll("Jane Doe", 545));


        //if (configuration.Version < ConfigVersion.CURRENT)
        //{
        //    migrateConfiguration(ref configuration);
        //}

        if (configuration.EnabledChannels.Count != CHANNEL_COUNT)
        {
            configuration.EnabledChannels =
            [
                new() {ChatType = (int)XivChatType.CrossLinkShell1, Name = "CWLS1"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell2, Name = "CWLS2"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell3, Name = "CWLS3"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell4, Name = "CWLS4"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell5, Name = "CWLS5"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell6, Name = "CWLS6"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell7, Name = "CWLS7"},
                    new() {ChatType = (int)XivChatType.CrossLinkShell8, Name = "CWLS8"},
                    new() {ChatType = (int)XivChatType.Ls1, Name = "LS1"},
                    new() {ChatType = (int)XivChatType.Ls2, Name = "LS2"},
                    new() {ChatType = (int)XivChatType.Ls3, Name = "LS3"},
                    new() {ChatType = (int)XivChatType.Ls4, Name = "LS4"},
                    new() {ChatType = (int)XivChatType.Ls5, Name = "LS5"},
                    new() {ChatType = (int)XivChatType.Ls6, Name = "LS6"},
                    new() {ChatType = (int)XivChatType.Ls7, Name = "LS7"},
                    new() {ChatType = (int)XivChatType.Ls8, Name = "LS8"},
                    new() {ChatType = (int)XivChatType.TellIncoming, Name = "Tell"},
                    new() {ChatType = (int)XivChatType.Say, Name = "Say"},
                    new() {ChatType = (int)XivChatType.Party, Name = "Party"},
                    new() {ChatType = (int)XivChatType.Yell, Name = "Yell"},
                    new() {ChatType = (int)XivChatType.Shout, Name = "Shout"},
                    new() {ChatType = (int)XivChatType.FreeCompany, Name = "Free Company"},
                    new() {ChatType = (int)XivChatType.Alliance, Name = "Alliance"}
            ];
        }

        //InitializeRegex();

        //if (configuration.Reactions.Count == 0)
        //{
        //    configuration.Reactions.Add(new Reaction() { Name = "Reaction" });
        //}

        //if (configuration.CustomChannels.Count == 0)
        //{
        //    configuration.CustomChannels.Add(new ChannelSetting() { Name = "SystemMessage", ChatType = 57 });
        //}

        // Always set to false on load
        configuration.DebugLogTypes = false;

        configuration.Save();
    }
}
