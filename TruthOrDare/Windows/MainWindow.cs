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

        this.boundImagePath = boundImagePath;
        this.plugin = plugin;
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
