using Dalamud.Bindings.ImGui;

namespace TruthOrDare.Windows.Main;


public partial class MainWindow
{
    public void Settings()
    {
        // Activating once when Dalamud "Settings" button pressed; no more or will tab-lock
        if (ShouldSelectTabOnce && ActiveTab == Tab.Settings)
        {
            flagsSettings = ImGuiTabItemFlags.SetSelected;
            ShouldSelectTabOnce = false;
        }
        else
        {
            flagsSettings = ImGuiTabItemFlags.None;
        }
        if (ImGui.BeginTabItem("Settings", flagsSettings))
        {
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 50.0f));
            // !tod
            var configTod = Service.configuration.ReactToExclamTod;            
            if (ImGui.Checkbox("##tod_checkbox", ref configTod))
            {
                Service.configuration.ReactToExclamTod = configTod;
                // Can save immediately on change if you don't want to provide a "Save and Close" button
                Service.configuration.Save();
            }
            ImGui.SameLine(0, 0);
            ImGui.PushStyleColor(ImGuiCol.Text, GreenText);
            ImGui.Text(" !tod ");
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 0);
            ImGui.Text("in say chat makes you start a new game");

            // !truth
            var configTruth = Service.configuration.ReactToExclamTruth;
            if (ImGui.Checkbox("##truth_checkbox", ref configTruth))
            {
                Service.configuration.ReactToExclamTruth = configTruth;
                Service.configuration.Save();
            }
            ImGui.SameLine(0, 0);
            ImGui.PushStyleColor(ImGuiCol.Text, GreenText);
            ImGui.Text(" !truth ");
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 0);
            ImGui.Text("in say chat makes you yell a random truth");

            // !dare
            var configDare = Service.configuration.ReactToExclamDare;
            if (ImGui.Checkbox("##dare_checkbox", ref configDare))
            {
                Service.configuration.ReactToExclamDare = configDare;
                Service.configuration.Save();
            }
            ImGui.SameLine(0, 0);
            ImGui.PushStyleColor(ImGuiCol.Text, GreenText);
            ImGui.Text(" !dare ");
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 0);
            ImGui.Text("in say chat makes you yell a random dare");

            // Time for rolls
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 50.0f));
            var rollsTime = Service.configuration.RollsTime;
            ImGui.SameLine(0, 20);
            ImGui.SetNextItemWidth(25.0f);
            if (plugin.IsRunning)
            {
                ImGui.BeginDisabled();
            }
            if (ImGui.InputText("##rolls_time", ref inputBuffer, 2))
            {
                int rollsNumber;
                if (int.TryParse(inputBuffer, out rollsNumber))
                {
                    if (rollsNumber < 3)
                    {
                        isValidRollsInput = false;
                        rollsInputErrorMessage = "Invalid input: must be more than 3 seconds.";
                    }
                    else
                    {
                        isValidRollsInput = true;
                        Service.configuration.RollsTime = rollsNumber;
                        Service.configuration.Save();
                    }
                }
                else
                {
                    isValidRollsInput = false;
                    rollsInputErrorMessage = "Invalid input: Not a number.";
                }

            }
            if (plugin.IsRunning)
            {
                ImGui.EndDisabled();
            }
            ImGui.SameLine(60, 0);
            ImGui.Text("Rolls time, in seconds.  Default is 45.");

            // Display error message if invalid
            if (!isValidRollsInput)
            {
                ImGui.TextColored(RedText, rollsInputErrorMessage);
            }
            else if (!string.IsNullOrEmpty(inputBuffer))
            {
                ImGui.TextColored(GreenText, "Valid input");
            }


            ImGui.EndTabItem();
        }

    }
   
}
