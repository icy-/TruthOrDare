using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using System;
using System.Numerics;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkUIColorHolder.Delegates;

namespace TruthOrDare.Windows.Main;

public partial class MainWindow : Window, IDisposable
{
    private readonly string boundImagePath;
    private readonly Plugin plugin;

    private bool showConfirmationModal = false;
    private bool tableCleared = false;
    private bool setDefaultTab = true;


    // Reusable ImGui text colors. If I start making lot of these, then later a common file for them
    public Vector4 RedText { get; } = new System.Numerics.Vector4(1.0f, 0.0f, 0.0f, 1.0f);

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

    // Examples that came with starter plugin to display player job and zone
    private void DrawExamplePlayerJobAndZone()
    {
        // Example for other services that Dalamud provides.
        // PlayerState provides a wrapper filled with information about the player character.

        var playerState = Plugin.PlayerState;
        if (!playerState.IsLoaded)
        {
            ImGui.Text("Our local player is currently not logged in.");
            return;
        }

        if (!playerState.ClassJob.IsValid)
        {
            ImGui.Text("Our current job is currently not valid.");
            return;
        }

        // If you want to see the Macro representation of this SeString use `.ToMacroString()`
        // More info about SeStrings: https://dalamud.dev/plugin-development/sestring/
        ImGui.Text($"Our current job is ({playerState.ClassJob.RowId}) '{playerState.ClassJob.Value.Abbreviation}' with level {playerState.Level}");

        // Example for querying Lumina, getting the name of our current area.
        var territoryId = Plugin.ClientState.TerritoryType;
        if (Plugin.DataManager.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
        {
            ImGui.Text($"We are currently in ({territoryId}) '{territoryRow.PlaceName.Value.Name}'");
        }
        else
        {
            ImGui.Text("Invalid territory.");
        }
    }

    private void DrawRollsTableInline()
    {
        // Maybe I insert my table just to the right in the next 'column'
        // Attempt to draw my rolls?!
        ImGui.SameLine(0.0f, ImGui.GetStyle().ItemSpacing.X);
        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Sortable | ImGuiTableFlags.RowBg |
            ImGuiTableFlags.Borders; // | ImGuiTableFlags.ScrollY;
        ImGui.BeginTable("Rolls Table", 2, tableFlags);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 220.0f);
        ImGui.TableSetupColumn("Roll", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        var sortSpecs = ImGui.TableGetSortSpecs();
        if (sortSpecs.SpecsDirty)
        {
            Service.configuration.Rolls.Sort((a, b) =>
            {
                for (int i = 0; i < sortSpecs.SpecsCount; i++)
                {
                    var colSpec = sortSpecs.Specs[i];
                    int col = colSpec.ColumnIndex;
                    int dir = (colSpec.SortDirection == ImGuiSortDirection.Ascending) ? 1 : -1;

                    if (col == 0) // hopefully the first column...?
                    {
                        return string.Compare(a.Name, b.Name) * dir;
                    }
                    else if (col == 1) // 2nd column...?
                    {
                        return a.Value.CompareTo(b.Value) * dir;
                    }
                }
                return 0;
            });
            sortSpecs.SpecsDirty = false;
        }

        foreach (var roll in Service.configuration.Rolls)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextUnformatted(roll.Name);

            ImGui.TableSetColumnIndex(1);
            ImGui.TextUnformatted(roll.Value.ToString());
        }
        ImGui.EndTable();
    }

    public override void Draw()
    {
        
        if (ImGui.BeginTabBar("MyTabBar"))
        {
            // Tab 1
            Settings();  // A partial class within MainWindowSettings file (TODO rename file from ConfigWindow)

            Game();
            ImGui.EndTabBar();
        }   
        
    }
}
