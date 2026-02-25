using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace TruthOrDare;

internal class Service
{

    [PluginService]
    public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    public static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    public static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    public static ISigScanner SigScanner { get; private set; } = null!;

    [PluginService]
    public static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    public static IFramework Framework { get; private set; } = null!;

    [PluginService] public static IPluginLog Logger { get; set; } = null!;

    public static ChatServer ChatServer { get; set; } = null!;
    public static ChatSender ChatSender { get; set; } = null!;

    public static Plugin? plugin;
    public static Configuration configuration { get; set; } = null!;


    public static void InitializeConfig()
    {
        // This line makes it persist, but for testing maybe I stick with new Configuration()
        // PERSISTING CONFIGURATION?
        //configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // TEST CONFIGURATION?
        configuration = new Configuration();  


        configuration.Initialize(PluginInterface);       


        //if (configuration.Version < ConfigVersion.CURRENT)
        //{
        //    migrateConfiguration(ref configuration);
        //}
      
        configuration.DebugLogTypes = false;

        configuration.Save();
    }
}
