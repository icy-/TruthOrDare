using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Serilog.Core;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TruthOrDare.Windows;

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
    [PluginService] internal static IFramework IFramework { get; private set; } = null!;

    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/truthordare";
    private readonly object lockObject = new object();
    private bool isRunning = false;
    private Thread backgroundThread = null!;

    private MacroSharedLock MacroSharedLock { get; init; }    

    public readonly WindowSystem WindowSystem = new("TruthOrDare");
    //private ConfigWindow ConfigWindow { get; init; }
    public ConfigWindow ConfigWindow;
    private MainWindow MainWindow { get; init; }

    public Plugin(IDalamudPluginInterface pluginInterface)
    {        

        // Service
        pluginInterface.Create<Service>();
        Service.plugin = this;

        MacroSharedLock = new MacroSharedLock(IFramework, Service.Logger);
        Service.ChatServer = new ChatServer(SigScanner);
        Service.ChatSender = new ChatSender(Service.ChatServer, IFramework, MacroSharedLock, Service.Logger);

        // You might normally want to embed resources and load them from the manifest stream
        var boundImagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "bound181x256.png");

        // Configuration
        Service.InitializeConfig();
        this.ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

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
        pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

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
        Service.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        Service.PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }
    
    public void ToggleConfigUi() => ConfigWindow.Toggle();
    public void ToggleMainUi() => MainWindow.Toggle();

    // A command followed by a sleep
    /*private async Task StaggeredCommand(string command, int milliseconds)
    {
        Service.ChatServer.SendMessage(command);
        await Task.Delay(milliseconds);
    }

    // When "Start Game" button is clicked
    public async Task StartGame()
    {
        string channel = "/yell";
        //ushort seconds = 45; // TODO move to config settings; at least 3 seconds to allow for outro
        ushort seconds = 5;
        string intro = $"""
{channel} ♪ Type /random in chat!  Highest number asks the lowest number, "Truth or Dare?" {seconds} seconds... Begin!
""";
        List<string> outro_messages = new List<string> { "3...", "2...", "1...", "Rolls finished!" };
        Service.ChatServer.SendMessage(intro);

       
        await Task.Delay((seconds-3)*1000);
        foreach (var message in outro_messages)
        {
            await Task.Run(() => StaggeredCommand($"{channel} {message}", 1000));
        }
    }

    */

    // Show each message every interval
    private void StaggeredMessages(List<string> messages, int interval)
    {
        while (isRunning)
        {
            lock (lockObject)
            {
                foreach (var msg in messages)
                {
                    Service.Logger.Debug($"Staggered Task {msg}, {interval}ms");
                    Thread.Sleep(interval);
                }
                isRunning = false;
            }
        }
    }

    // When "Start" button is clicked
    public void Start()
    {
        if (isRunning)
            return;
        Service.Logger.Debug($"Start button clicked...");
        isRunning = true;
        List<string> messages = new List<string> { "3...", "2...", "1...", "Done!" };
        
        System.Threading.Tasks.Task.Run(() => {
            // WARNING NOT THREAD SAFE CALL
            //Service.ChatServer.SendMessage("/say intro");

            // Trying the ChatSender thing
            //System.Threading.Tasks.Task.WaitAny(Service.ChatSender.SendOnFrameworkThread("/say intro", 15));

            var taskId = System.Threading.Tasks.Task.CurrentId!.Value;
            try
            {
                MacroSharedLock.Acquire(taskId);
                Service.ChatSender.SendOnFrameworkThread("/say hi", taskId);
            }
            finally
            {
                //MacroSharedLock.Release(taskId);
            }


            StaggeredMessages(messages, 1000);
        });
        

        // I want a delay of seconds-3 before the first message too
        //int seconds = 5;
        //List<string> messages = new List<string> { "3...", "2...", "1...", "Done!" };
        //Service.Logger.Debug($"Start button clicked...");
        //StaggeredMessages(messages, 1000);
        
    }

}
