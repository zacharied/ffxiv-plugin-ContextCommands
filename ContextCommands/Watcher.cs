using System.Collections.Generic;
using Dalamud.Game.Internal;
using Dalamud.Plugin;

namespace ContextCommands
{
    internal class Watcher
    {
        private readonly DalamudPluginInterface _pi;
        private readonly List<CommandItem> _commands;

        private readonly List<Context> _activeContexts = new List<Context>();

        public Watcher(DalamudPluginInterface pi, List<CommandItem> commands)
        {
            _pi = pi;
            _commands = commands;
        }

        public void OnGameStateUpdate()
        {
            foreach (var item in _commands)
            {
                if (!_activeContexts.Contains(item.Context) && item.Context.IsActive(_pi))
                {
                    item.Commands.OnEnter(_pi);
                    _activeContexts.Add(item.Context);
                }
                else if (_activeContexts.Contains(item.Context) && !item.Context.IsActive(_pi))
                {
                    item.Commands.OnExit(_pi);
                    _activeContexts.Remove(item.Context);
                }
            }
        }

        public void FrameworkUpdateEvent(Framework _) => OnGameStateUpdate();
    }
}
