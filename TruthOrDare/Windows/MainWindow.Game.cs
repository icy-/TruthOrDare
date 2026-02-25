using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;
using Lumina.Excel.Sheets;
using System;

namespace TruthOrDare.Windows.Main;

public partial class MainWindow
{   

    public void Game()
    {       
        // Activating once when Dalamud "Open" button pressed; no more or will tab-lock
        if (ShouldSelectTabOnce && ActiveTab == Tab.Game)
        {
            flagsGame = ImGuiTabItemFlags.SetSelected;
            ShouldSelectTabOnce = false;
        }
        else
        {
            flagsGame = ImGuiTabItemFlags.None;
        }
        if (ImGui.BeginTabItem("Game", flagsGame))
        {
            // Labels and dynamic info
            ImGui.Text("Leave your inhibitions at the door! Go");
            ImGui.PushStyleColor(ImGuiCol.Text, RedText);
            ImGui.SameLine(0, 0);
            ImGui.Text(" wild ♥");
            ImGui.PopStyleColor();


            ImGui.Text("");
            //ImGui.Text($"The random config bool is {Service.configuration.ReactToExclamTod}");

            if (Service.configuration.Rolls.Count > 0)
            {
                ImGui.Text($"high♥ {Service.configuration.HighRoll}");
                ImGui.Text($"low♡ {Service.configuration.LowRoll}");
                tableCleared = false;
            }

            // Another button to the right of this button, for clearing table
            //ImGui.SameLine(0.0f, ImGui.GetStyle().ItemSpacing.X);
                        

            if (tableCleared)
            {
                ImGui.Text("Table cleared!");
            }
            else
            {
                ImGui.Text($"Table count: {Service.configuration.Rolls.Count}");
            }


            if (ImGui.Button("Dummy Rolls"))
            {
                plugin.DummyRolls();
            }

            if (plugin.IsRunning)
            {
                ImGui.BeginDisabled();
            }
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemSpacing.X);
            // A start button hopefully right-aligned, above the table
            if (ImGui.Button("Start Game"))
            {
                plugin.Start();
            }
            if (plugin.IsRunning)
            {
                ImGui.EndDisabled();
                // Progress bar hack
                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemSpacing.X);
                var time = ImGui.GetTime();
                var fraction = (MathF.Sin((float)time) + 1.0f) * 0.5f;
                ImGui.ProgressBar(fraction, new Vector2(100f, 30f), "Processing...");
            }

            ImGui.Spacing();

            // Normally a BeginChild() would have to be followed by an unconditional EndChild(),
            // ImRaii takes care of this after the scope ends.
            // This works for all ImGui functions that require specific handling, examples are BeginTable() or Indent().
            using (var child = ImRaii.Child("SomeChildWithAScrollbar", Vector2.Zero, true))
            {
                // Check if this child is drawing
                if (child.Success)
                {
                    var boundImage = Plugin.TextureProvider.GetFromFile(boundImagePath).GetWrapOrDefault();
                    if (boundImage != null)
                    {
                        using (ImRaii.PushIndent(55f))
                        {
                            var size = new Vector2(181, 256);
                            ImGui.Image(boundImage.Handle, size);
                            DrawRollsTableInline();

                        }
                    }
                    else
                    {
                        ImGui.Text("Image not found.");
                    }
                    ImGuiHelpers.ScaledDummy(20.0f);

                    DrawClearTableButton();
                }
            }
            ImGui.EndTabItem();
        }
    }

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

    // And its logic
    private void DrawClearTableButton()
    {
        if (plugin.IsRunning)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Clear Table"))
        {
            ImGui.OpenPopup("Confirm Clear Table");
            showConfirmationModal = true;
        }
        if (plugin.IsRunning)
        {
            ImGui.EndDisabled();
        }

        if (ImGui.BeginPopupModal("Confirm Clear Table", ref showConfirmationModal, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Are you sure you want to Clear the Rolls?\n This action cannot be undone.");
            ImGui.Separator();

            // Add buttons for user choice
            if (ImGui.Button("Yes, Clear", new Vector2(120, 0)))
            {
                // Perform the action here
                Service.configuration.ClearRolls();
                tableCleared = true;
                // Close the modal
                ImGui.CloseCurrentPopup();
                showConfirmationModal = false;
            }
            ImGui.SetItemDefaultFocus(); // Set initial keyboard focus to this button

            ImGui.SameLine();

            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                // User cancelled, close the modal
                tableCleared = false;
                ImGui.CloseCurrentPopup();
                showConfirmationModal = false;
            }
            ImGui.EndPopup();
        }
    }

}
