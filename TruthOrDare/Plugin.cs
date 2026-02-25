using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using InteropGenerator.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TruthOrDare.Windows;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;
using static FFXIVClientStructs.FFXIV.Client.System.Input.PadDevice.Delegates;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentMJIFarmManagement;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

using TruthOrDare.Windows.Main;

namespace TruthOrDare;

public struct Roll(string name, int value)
{
    public string Name { get; set; } = name;
    public int Value { get; set; } = value;

    public readonly bool IsEmpty() { return Name.IsNullOrEmpty(); }

    public override readonly string ToString()
    {
        return $"{Name} rolled a {Value}";
    }
}

public sealed class Plugin : IDalamudPlugin
{
    //[PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    // Signature stuff needed to decorate chat-related methods
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/truthordare";
    private readonly object lockObject = new object();
    private bool isRunning = false;
    private bool isDummyProcessing = false;
    //private MacroSharedLock MacroSharedLock { get; init; }    

    public readonly WindowSystem WindowSystem = new("TruthOrDare");
    //private ConfigWindow ConfigWindow { get; init; }
    //public ConfigWindow ConfigWindow;
    private MainWindow MainWindow { get; init; }

    private readonly List<string> dummyNames;
    private readonly List<string> dummyWorlds;
    private readonly Random random;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        dummyNames = new List<string> {
            "Azalea McDowell", "Lachlan Wiley", "Lauryn McCullough", "Briar Baker", "Isla Aguirre", "Andy Liu", "Kate Delgado", "Colt Luna", "Journey Montgomery", "Maximiliano Salinas", "Royalty Shepherd", "Ronald Walsh", "Leia Gibson", "Tyler Jenkins", "Rylee Anthony", "Shiloh Hull", "Andi Corona", "Darian Morris", "Genesis Martinez", "Alexander Jordan", "Adalynn Chase", "Otis Duffy", "Addisyn Alvarado", "Andres Crane", "Della Banks", "Martin Pacheco", "Paris Warren", "Abel Wise", "Mira Pacheco", "Erik Alvarez", "Leilani Petersen", "Samson Huber", "Raquel Franco", "Gage Boyle", "Aliya Hurley", "Etta Lewis", "Wyatt Davila", "Rayne Waters", "Maximilian Duke", "Melani Reid", "Josue Ball", "Abby Marsh", "Bo Duffy", "Addisyn Bennett", "Leonardo Camacho", "Armani McPherson", "Foster Reed", "Valentina Coffey", "Kody Hicks", "Alina Heath", "Lionel Underwood", "Ensley Jones", "William Zhang", "Sarai Gutierrez", "Luca Smith", "Olivia Trujillo", "Apollo Nolan", "Itzayana Casey", "Armando Decker", "Aleena Henderson", "Beau Castaneda", "Keira Chung", "Ira McFarland", "Annika Parra", "Davion Booth", "Zariyah Costa", "Kenji Daniels", "Ember Small", "Rudy Crosby", "Keily Giles", "Kole Delgado", "Alani Orr", "Benicio Castillo", "Eva Preston", "Vincenzo Vang", "Madisyn Robbins", "Finnegan Ali", "Zelda Salgado", "Trace Thomas", "Elizabeth Johns", "Joziah Sampson" };

        dummyWorlds = new List<string> { "Adamantoise", "Cactuar", "Faerie", "Gilgamesh", "Jenova", "Midgardsormr", "Sargatanas", "Siren", "Balmung", "Brynhildr", "Coeurl", "Diabolos", "Goblin", "Malboro", "Mateus", "Zalera", "Cuchulainn", "Golem", "Halicarnassus", "Kraken", "Maduin", "Marilith", "Rafflesia", "Seraph", "Behemoth", "Excalibur", "Exodus", "Famfrit", "Hyperion", "Lamia", "Leviathan", "Ultros", "Cerberus", "Louisoix", "Moogle", "Omega", "Phantom", "Ragnarok", "Sagittarius", "Spriggan", "Alpha", "Lich", "Odin", "Phoenix", "Raiden", "Shiva", "Twintania", "Zodiark", "Aegis", "Atomos", "Carbuncle", "Kujata", "Typhon", "Bismarck", "Ravana", "Sephirot", "Sophia", "Zurvan" };

        random = Random.Shared;


        // Service
        pluginInterface.Create<Service>();
        Service.plugin = this;

        //MacroSharedLock = new MacroSharedLock(Framework, Service.Logger);
        Service.ChatServer = new ChatServer(SigScanner);
       // Service.ChatSender = new ChatSender(Service.ChatServer, Framework, MacroSharedLock, Service.Logger);

        // You might normally want to embed resources and load them from the manifest stream
        var boundImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "bound181x256.png");

        // Configuration
        Service.InitializeConfig();
        //this.ConfigWindow = new ConfigWindow(this);
        //WindowSystem.AddWindow(ConfigWindow);

        //ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, boundImagePath);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = @"Open settings dialog
/truthordare on    - track chat messages and append rolls to table
/truthordare off   - cease tracking of chat messages
/truthordare clear - clear the table"
        });

        // The magic of watching text begins here
        //  not sure if I want to keep subscribing/unsubscribing with checkbox, or have an early return in its method
        Service.ChatGui.ChatMessage += ChatHandler.OnChatMessage;


        // Tell the UI system that we want our windows to be drawn through the window system
        pluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        //pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        pluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [TruthOrDare] ===A cool log message from TruthOrDare===
        Log.Information($"===A cool log message from {pluginInterface.Manifest.Name}===");
        //backgroundThread.Join();
    }

    public void Dispose()
    {
        Service.ChatGui.ChatMessage -= ChatHandler.OnChatMessage;

        // Unregister all actions to not leak anything during disposal of plugin
        Service.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        //Service.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        Service.PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        //ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }
    
    //public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();


    // Simulates filling up the table one by one with /random rolls, calling the handler directly
    public void DummyRolls()
    {
        if (isDummyProcessing)
            return;
        Service.Logger.Debug($"Dummy Rolls button clicked...");
        isDummyProcessing = true;


        Dalamud.Game.Text.SeStringHandling.SeString dummySender = "";
        Dalamud.Game.Text.SeStringHandling.SeString dummyString;
        bool isHandled = false;

        // Random from 1-10 encounters
        for (int i = 0; i < random.Next(1, 11); i++)
        {
            // find the special character instead of @ sign
            string dummy = $"{dummyNames[random.Next(dummyNames.Count)]}{dummyWorlds[random.Next(dummyNames.Count)]}";
            int roll = random.Next(0, 1000);
            dummyString = $"Random! {dummy} rolls a {roll}.";
            ChatHandler.OnChatMessage((Dalamud.Game.Text.XivChatType)8266, 0, ref dummySender, ref dummyString, ref isHandled);
        }

        isDummyProcessing = false;
    }

    // When "Start" button is clicked
    public void Start()
    {
        // TODO: disable various buttons and config settings while the game is running
        // TODO: maybe turn Start Game into Cancel Game button

        if (isRunning)
            return;
        Service.Logger.Debug($"Start button clicked...");
        isRunning = true;

        // Warning: if you don't wrap Service.ChatServer messages in Framework calls, you'll crash.
        int seconds = 10; // Should be more than three seconds
        string channel = "/yell";
        string intro = $" ♪ Type /random in chat!  Highest number asks the lowest number, \"Truth or Dare?\" {seconds} seconds... Begin!";
        Task.Run(() => {
            Framework.RunOnFrameworkThread(() => Service.ChatServer.SendMessage($"{channel} {intro}"));

            // Waiting until just a few seconds remain.  Then outro.
            Thread.Sleep((seconds - 3) * 1000);
            Framework.RunOnFrameworkThread(() => Service.ChatServer.SendMessage($"{channel} 3 seconds remain..."));            
            Thread.Sleep(3000);
            Framework.RunOnFrameworkThread(() => Service.ChatServer.SendMessage(
                $"{channel} ♪ {Service.configuration.HighRoll.Name} => {Service.configuration.LowRoll.Name}"));
            isRunning = false;
        });
    }

}
