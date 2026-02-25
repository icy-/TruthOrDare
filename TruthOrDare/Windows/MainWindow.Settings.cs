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
            ImGui.Text("This is the content of Tab 1.");

            var configValue = Service.configuration.SomePropertyToBeSavedAndWithADefault;
            if (ImGui.Checkbox("Random Config Bool", ref configValue))
            {
                Service.configuration.SomePropertyToBeSavedAndWithADefault = configValue;
                // Can save immediately on change if you don't want to provide a "Save and Close" button
                Service.configuration.Save();
            }
            ImGui.EndTabItem();
        }

    }
}
