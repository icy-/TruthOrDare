
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    public bool IsRunning { get; private set; }
    private bool isDummyProcessing = false;
    //private MacroSharedLock MacroSharedLock { get; init; }    

    public readonly WindowSystem WindowSystem = new("TruthOrDare");
    //private ConfigWindow ConfigWindow { get; init; }
    //public ConfigWindow ConfigWindow;
    private MainWindow MainWindow { get; init; }

    private readonly List<string> dummyNames;
    private readonly List<string> dummyWorlds;
    private readonly List<string> truths;
    private readonly List<string> dares;    
    private readonly Random random;

    private ChatHandler chatHandler;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        dummyNames = new List<string> {
            "Azalea McDowell", "Lachlan Wiley", "Lauryn McCullough", "Briar Baker", "Isla Aguirre", "Andy Liu", "Kate Delgado", "Colt Luna", "Journey Montgomery", "Maximiliano Salinas", "Royalty Shepherd", "Ronald Walsh", "Leia Gibson", "Tyler Jenkins", "Rylee Anthony", "Shiloh Hull", "Andi Corona", "Darian Morris", "Genesis Martinez", "Alexander Jordan", "Adalynn Chase", "Otis Duffy", "Addisyn Alvarado", "Andres Crane", "Della Banks", "Martin Pacheco", "Paris Warren", "Abel Wise", "Mira Pacheco", "Erik Alvarez", "Leilani Petersen", "Samson Huber", "Raquel Franco", "Gage Boyle", "Aliya Hurley", "Etta Lewis", "Wyatt Davila", "Rayne Waters", "Maximilian Duke", "Melani Reid", "Josue Ball", "Abby Marsh", "Bo Duffy", "Addisyn Bennett", "Leonardo Camacho", "Armani McPherson", "Foster Reed", "Valentina Coffey", "Kody Hicks", "Alina Heath", "Lionel Underwood", "Ensley Jones", "William Zhang", "Sarai Gutierrez", "Luca Smith", "Olivia Trujillo", "Apollo Nolan", "Itzayana Casey", "Armando Decker", "Aleena Henderson", "Beau Castaneda", "Keira Chung", "Ira McFarland", "Annika Parra", "Davion Booth", "Zariyah Costa", "Kenji Daniels", "Ember Small", "Rudy Crosby", "Keily Giles", "Kole Delgado", "Alani Orr", "Benicio Castillo", "Eva Preston", "Vincenzo Vang", "Madisyn Robbins", "Finnegan Ali", "Zelda Salgado", "Trace Thomas", "Elizabeth Johns", "Joziah Sampson" };

        dummyWorlds = new List<string> { "Adamantoise", "Cactuar", "Faerie", "Gilgamesh", "Jenova", "Midgardsormr", "Sargatanas", "Siren", "Balmung", "Brynhildr", "Coeurl", "Diabolos", "Goblin", "Malboro", "Mateus", "Zalera", "Cuchulainn", "Golem", "Halicarnassus", "Kraken", "Maduin", "Marilith", "Rafflesia", "Seraph", "Behemoth", "Excalibur", "Exodus", "Famfrit", "Hyperion", "Lamia", "Leviathan", "Ultros", "Cerberus", "Louisoix", "Moogle", "Omega", "Phantom", "Ragnarok", "Sagittarius", "Spriggan", "Alpha", "Lich", "Odin", "Phoenix", "Raiden", "Shiva", "Twintania", "Zodiark", "Aegis", "Atomos", "Carbuncle", "Kujata", "Typhon", "Bismarck", "Ravana", "Sephirot", "Sophia", "Zurvan" };

        random = Random.Shared;
        truths = GenerateTruths();
        dares = GenerateDares();

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
            HelpMessage = @"Open the app"
//truthordare on    - track chat messages and append rolls to table
//truthordare off   - cease tracking of chat messages
//truthordare clear - clear the table"
        });


        chatHandler = new ChatHandler(this);
        // The magic of watching text begins here
        //  not sure if I want to keep subscribing/unsubscribing with checkbox, or have an early return in its method
        Service.ChatGui.ChatMessage += chatHandler.OnChatMessage;


        // Tell the UI system that we want our windows to be drawn through the window system
        pluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        pluginInterface.UiBuilder.OpenConfigUi += ToggleSettings;

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
        Service.ChatGui.ChatMessage -= chatHandler.OnChatMessage;

        // Unregister all actions to not leak anything during disposal of plugin
        Service.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        //Service.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        Service.PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        //ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    // Maybe just store it in a file eventually
    private List<string> GenerateTruths()
    {
        return
@"""Picture your partner or crush -- what's your favorite body part?
Where were you when you erotically roleplayed for the first time?
Who was your first in-game kiss?
What's the most embarrassing thing that's happened to you during sex or roleplay?
What's the dirtiest thing anyone's ever asked you to do, and did you do it?
Would you have a threesome with any of this group? Who would it be with?
What's your steamiest sexual fantasy?
How often do you masturbate?
What's your biggest turn-off?
Have you ever made a sex in-game pose or video?
What's your favorite sex position?
Do you ever sext/ERP flirt? If so, you recall a spicy one?
Who's the sexiest person you've ever been with?
Have you ever fantasized about an NPC?
Have you ever had an orgy? Would you like to have an orgy?
What's your best pick-up line?
If you could only do one sex position for the rest of your life, which would you choose?
Who would you rather kiss in this group?
Have you ever had a sex dream about anyone in this group?
Who was the first person you had a crush on?
What's your favorite sexual guilty pleasure?
Who was your best sexual experience with? What made it so good?
Have you ever in-game cheated on someone?
Have you ever had sex with other people in the room?
Where's the most unusual place you've had sex?
What's the dirtiest thing you want someone to do to you?
Have you ever tainted a wholesome home or area by having sex there?
What's your kinkiest turn-on?
Have you ever used sex toys with a partner?
What's the cringiest thing you've said while trying to flirt?""".Split("\n").ToList();
    }

    private List<string> GenerateDares()
    {
        return
@"""Leave a steamy ERP message for an ex or another friend.
Write out a detailed post of licking peanut butter, whipped cream, or chocolate sauce off of someone's body.
Fake an orgasm in a silly message, detailing what aroused you so much.
Write out a passionate kiss with someone in the room, if they consent.
Act out your favorite sex position with the person to your left.
Give oral sex to someone in the room, if they consent.
If there's a pool or a hot tub, go skinny-dipping.
Embrace someone you know least in the group for three rounds.
Take a body shot or get one taken from you (ERP). Balance the glass on cleavage, torso -- have fun wtih it!
Go bottomless for five rounds, or demand someone else goes bottomless for five rounds.
Give a nice sensual twerk for someone in the room.
Perform a pole dance, and slowly strip over the course of five rounds.
Remove an item of clothing or demand someone removes an item of clothing, if feeling dominant.
Give a lap dance to anyone of your choice, with consent.
Roleplay a fantasy of your choice with another member of the group.
Give a foot massage and/or worship the feet of the person to your right for five rounds.
Embrace your inner animal for five rounds, complete with sounds, and other fun things. 
Have someone in the room to dress you for five rounds.  If you're submissive, beg them eloquently.
Have someone in the room to gag and lock you for five rounds.  Beg them if you're submissive, or promise them a punishment if not.
Act out your favorite sex position with the person to your right.
Simp for someone for five rounds, or demand worship from someone for five rounds.
Have someone in the room spank you twenty times (or if dommy, spank someone, with consent, twenty times.)  The receiver counts every five out loud.
Ask the room for up to three humiliating moodles and put them on for five rounds.
If submissive, get put on display, nude, preferably with an Amborella animation, for five rounds.  Or put someone on display if dominant.
Go topless for five rounds, or demand someone else goes topless for five rounds.
Play a song or perform a bard midi piece while nude.  Then stay nude for five rounds.
Eloquently beg to have someone finger and/or toy your ass, or demand it of someone if feeling dominant.  Wear a visible butt plug for five rounds.
Ask someone to blindfold and lock it in for five rounds.  Or demand it of someone, if feeling dominant.""".Split("\n").ToList();
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }

    // We open the same window but on different tabs, to address validation issue with config binding
    public void ToggleSettings() => MainWindow.SetTabAndToggle(Tab.Settings);
    public void ToggleMainUi() => MainWindow.SetTabAndToggle(Tab.Game);


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
            string dummy = $"{dummyNames[random.Next(dummyNames.Count)]}{dummyWorlds[random.Next(dummyWorlds.Count)]}";
            int roll = random.Next(0, 1000);
            dummyString = $"Random! {dummy} rolls a {roll}.";
            chatHandler.OnChatMessage((Dalamud.Game.Text.XivChatType)8266, 0, ref dummySender, ref dummyString, ref isHandled);
        }

        isDummyProcessing = false;
    }

    public void RandomTruth()
    {
        
        string channel = "/yell";
        int index = random.Next(truths.Count);
        Service.ChatServer.SendMessage($"{channel} [Truth #{index}] {truths[index]}");
    }

    public void RandomDare()
    {
        string channel = "/yell";
        int index = random.Next(dares.Count);
        Service.ChatServer.SendMessage($"{channel} [Dare #{index}] {dares[index]}");
    }    

    // When "Start" button is clicked, or if ReactToExclamTod config setting is turned on
    public void Start()
    {
        // TODO: maybe turn Start Game into Cancel Game button

        if (IsRunning)
            return;
        Service.Logger.Debug($"Start button clicked...");
        IsRunning = true;
        Service.configuration.ClearRolls();

        // Warning: if you don't wrap Service.ChatServer messages in Framework calls, you'll crash.
        int seconds = Service.configuration.RollsTime; // Should be more than three seconds
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
            IsRunning = false;
        });
    }

}
