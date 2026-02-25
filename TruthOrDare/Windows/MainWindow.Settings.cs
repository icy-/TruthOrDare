using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkTimer.Delegates;

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
            ImGui.PushStyleColor(ImGuiCol.Text, NeonGreenText);
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
            ImGui.PushStyleColor(ImGuiCol.Text, NeonGreenText);
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
            ImGui.PushStyleColor(ImGuiCol.Text, NeonGreenText);
            ImGui.Text(" !dare ");
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 0);
            ImGui.Text("in say chat makes you yell a random dare");

            // Time for rolls
            ImGui.Dummy(new System.Numerics.Vector2(0.0f, 50.0f));



            ImGui.EndTabItem();
        }

    }
}
