using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;

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
            // !td
            var configTd = Service.configuration.ReactToExclamTd;
            if (ImGui.Checkbox("##td_checkbox", ref configTd))
            {
                Service.configuration.ReactToExclamTd = configTd;                
                Service.configuration.Save();
            }
            ImGui.SameLine(0, 0);
            ImGui.PushStyleColor(ImGuiCol.Text, GreenText);
            ImGui.Text(" !td ");
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 0);
            ImGui.Text("in say chat makes you start a new game");


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

            // !foreplay
            var configForeplay = Service.configuration.ReactToExclamForeplay;
            if (ImGui.Checkbox("##foreplay_checkbox", ref configForeplay))
            {
                Service.configuration.ReactToExclamForeplay = configForeplay;
                Service.configuration.Save();
            }
            ImGui.SameLine(0, 0);
            ImGui.TextColored(GreenText, "!foreplay");            
            ImGui.SameLine(0, 0);
            ImGui.Text("in say chat makes you yell a random foreplay combination from six dice");

            // Time for rolls
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 50.0f));            
            ImGui.SameLine(0, 20);
            ImGui.SetNextItemWidth(25.0f);
            if (plugin.IsRunning)
            {
                ImGui.BeginDisabled();
            }
            // Setting it to two makes it just two characters input
            if (ImGui.InputText("##rolls_time", ref inputBuffer, 2))
            {
                showValidationMessage = true;
                int rollsNumber;
                if (int.TryParse(inputBuffer, out rollsNumber))
                {
                    if (rollsNumber < Configuration.MinRollTime)
                    {
                        isValidRollsInput = false;
                        rollsInputErrorMessage = $" Invalid input: must be at least {Configuration.MinRollTime} seconds.";
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
                    rollsInputErrorMessage = " Invalid input: Not a number.";
                }
                validationTime = DateTime.Now.AddSeconds(10);
            }
            if (plugin.IsRunning)
            {
                ImGui.EndDisabled();
            }
            ImGui.SameLine(60, 0);
            ImGui.Text("Rolls time, in seconds.  Suggested is 45.");

            if (showValidationMessage)
            {                
                // Display error message if invalid
                if (!isValidRollsInput)
                {
                    ImGui.TextColored(RedText, rollsInputErrorMessage);
                }
                else if (!string.IsNullOrEmpty(inputBuffer))
                {
                    ImGui.TextColored(GreenText, "✓ Valid input");
                }
                // After ~10sec, hide text and snap back to previously valid config time
                if (DateTime.Now >= validationTime)
                {
                    showValidationMessage = false;
                    inputBuffer = Service.configuration.RollsTime.ToString();
                }
            }


            ImGui.EndTabItem();
        }

    }
   
}
