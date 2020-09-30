using Dalamud.Plugin;

namespace ContextCommands
{
    internal class CommandSet
    {
        public const uint CommandsMaxLength = 4000;

        public string OnEnterCommand = string.Empty;
        public string OnExitCommand = string.Empty;

        public void OnEnter(DalamudPluginInterface pi) => Run(OnEnterCommand, pi);
        public void OnExit(DalamudPluginInterface pi) => Run(OnExitCommand, pi);

        private static void Run(string command, DalamudPluginInterface pi)
        {
            foreach (var line in command.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                pi.CommandManager.ProcessCommand(line);
            }
        }
    }
}
