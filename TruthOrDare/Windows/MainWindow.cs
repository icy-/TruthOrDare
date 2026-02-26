using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

namespace TruthOrDare.Windows.Main;

public enum Tab
{
    Settings,
    Game
}

public partial class MainWindow : Window, IDisposable
{
    private readonly string boundImagePath;
    private readonly Plugin plugin;

    private bool showConfirmationModal = false;
    private bool tableCleared = false;

    public Tab ActiveTab {  get; set; }
    public bool ShouldSelectTabOnce { get; set; }
    private ImGuiTabItemFlags flagsSettings;
    private ImGuiTabItemFlags flagsGame;

    private string inputBuffer;
    private bool isValidRollsInput;
    private string rollsInputErrorMessage;

    // Reusable ImGui text colors. If I start making lot of these, then later a common file for them
    public Vector4 RedText { get; } = new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f);
    public Vector4 GreenText { get; } = new System.Numerics.Vector4(57/255.0f, 1, 20/255.0f, 1);
    public Vector4 PinkText { get; } = new System.Numerics.Vector4(1, 141/255.0f, 161/255.0f, 1);

    // Sets an active tab, one time, before toggling the window
    public void SetTabAndToggle(Tab tab)
    {
        ShouldSelectTabOnce = true;
        ActiveTab = tab;
        Toggle();
    }

    // We give this window a hidden ID using ##.
    // The user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string boundImagePath)
        : base("Truth or Dare##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 330),
            MaximumSize = new Vector2(600, float.MaxValue)
        };

        this.plugin = plugin;
        this.boundImagePath = boundImagePath;
        inputBuffer = Service.configuration.RollsTime.ToString();
        rollsInputErrorMessage = string.Empty;
    }

    public void Dispose() { }
    
    public override void Draw()
    {
        if (ImGui.BeginTabBar("MyTabBar"))
        {            
            Settings(); // First tab for config settings, previously in a pop-out window of its own
            Game();     // Second tab for the meat of the app
            ImGui.EndTabBar();
        }
    }
}
