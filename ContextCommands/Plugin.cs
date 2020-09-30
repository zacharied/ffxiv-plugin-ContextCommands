using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace ContextCommands
{
    public class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/pcontext";

        public string Name => "ContextCommands";

        private DalamudPluginInterface _pi;
        private Ui _ui;
        private Watcher _watcher;
        private Config _config;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;
            _config = (Config)_pi.GetPluginConfig() ?? new Config();
            _pi.SavePluginConfig(_config);

            _pi.CommandManager.AddHandler(CommandName, PluginCommand());

            _ui = new Ui(this, _pi, _config);

            _pi.UiBuilder.OnBuildUi += _ui.BuildUi;
            _pi.UiBuilder.OnOpenConfigUi += _ui.OnOpenConfigUI;

            _watcher = new Watcher(_pi, _config.Items);

            _pi.Framework.OnUpdateEvent += _watcher.FrameworkUpdateEvent;

            PluginLog.Log($"{Name} loaded");
        }

        public void Dispose()
        {
            _pi.CommandManager.RemoveHandler(CommandName);

            _pi.UiBuilder.OnBuildUi -= _ui.BuildUi;
            _pi.UiBuilder.OnOpenConfigUi -= _ui.OnOpenConfigUI;

            _pi.Framework.OnUpdateEvent -= _watcher.FrameworkUpdateEvent;
        }

        public CommandInfo PluginCommand()
        {
            return new CommandInfo((string command, string args) => {})
            {
                HelpMessage = $"Open the {Name} configuration UI"
            };
        }
    }
}
