using System.Collections.Generic;
using Dalamud.Configuration;
using Lumina.Excel.GeneratedSheets;

namespace ContextCommands
{
    /// <summary>
    /// Maps a context to a set <see cref="CommandSet"/>.
    /// </summary>
    internal class CommandItem
    {
        public Context Context = new Context();
        public CommandSet Commands = new CommandSet();

        public string Name => $"[{Context.Job ?? "Any"}] {Context.State?.Name() ?? "Unknown"}";
    }

    internal class Config : IPluginConfiguration
    {
        public int Version { get; set; }

        public List<CommandItem> Items = new List<CommandItem>();
    }
}
