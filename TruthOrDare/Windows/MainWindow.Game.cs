using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Numerics;
namespace TruthOrDare.Windows.Main;

public partial class MainWindow
{
    public void Game()
    {
        if (ImGui.BeginTabItem("Game"))
        {
            ImGui.Text("This is the content of Tab 2.");

            // Labels and dynamic info
            ImGui.Text("Leave your inhibitions at the door! Go");
            ImGui.PushStyleColor(ImGuiCol.Text, RedText);
            ImGui.SameLine(0, 0);
            ImGui.Text(" wild ♥");
            ImGui.PopStyleColor();


            ImGui.Text($"The random config bool is {Service.configuration.SomePropertyToBeSavedAndWithADefault}");

            if (Service.configuration.Rolls.Count > 0)
            {
                ImGui.Text($"high♥ {Service.configuration.HighRoll}");
                ImGui.Text($"low♡ {Service.configuration.LowRoll}");
                tableCleared = false;
            }


            //if (ImGui.Button("Show Settings"))
            //{
            //    plugin.ToggleConfigUi();
            //}
            // Another button to the right of this button, for clearing table
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemSpacing.X);
            if (ImGui.Button("Clear Table"))
            {
                ImGui.OpenPopup("Confirm Clear Table");
                showConfirmationModal = true;
            }
            if (tableCleared)
            {
                ImGui.Text("Table cleared!");
            }
            else
            {
                ImGui.Text($"Table count: {Service.configuration.Rolls.Count}");
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


            if (ImGui.Button("Dummy Rolls"))
            {
                plugin.DummyRolls();
            }
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemSpacing.X);
            // A start button hopefully right-aligned, above the table
            if (ImGui.Button("Start Game"))
            {
                plugin.Start();
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

                    //DrawExamplePlayerJobAndZone();
                }
            }
            ImGui.EndTabItem();
        }
    }
    
}
