using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace ContextCommands
{
    [SuppressMessage("ReSharper", "InvertIf", Justification = "ImGui code is more clear with the regular `if`-statement control flow")]
    internal class Ui
    {
        private static class Constants
        {
            public const int ListWidth = 230;
            public static readonly Vector2 TextboxSize = new Vector2(590, 150);
        }

        private readonly Plugin _plugin;
        private readonly DalamudPluginInterface _pi;
        private readonly Config _config;

        private bool _visible = false;
        public bool Visible { get => _visible; set => _visible = value; }

        private int _selectedItemIndex = -1;

        public Ui(Plugin plugin, DalamudPluginInterface pi, Config config)
        {
            _plugin = plugin;
            _pi = pi;
            _config = config;
        }

        private void Draw()
        {
            var dirty = false;

            if (!Visible)
                return;

            ImGui.SetNextWindowSize(new Vector2(800, 500), ImGuiCond.Always);

            if (!ImGui.Begin(_plugin.Name, ref _visible, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize))
                return;

            ImGui.Columns(2, "columns", false);

            ImGui.SetColumnWidth(0, Constants.ListWidth);
            DrawCommandSetList(ref dirty);

            ImGui.NextColumn();

            if (_selectedItemIndex >= 0)
                DrawCommandConfig(ref dirty, _config.Items[_selectedItemIndex]);

            ImGui.End();

            if (dirty)
                _pi.SavePluginConfig(_config);
        }

        private void DrawCommandSetList(ref bool dirty)
        {
            if (ImGui.ListBoxHeader("##command-sets", new Vector2(Constants.ListWidth, ImGui.GetWindowHeight() - ImGui.GetTextLineHeightWithSpacing() * 4))) 
            {
                foreach (var (item, index) in _config.Items.Select((value, index) => Tuple.Create(value, index)))
                {
                    if (ImGui.Selectable($"{item.Name}##command-set-item-{index}", _selectedItemIndex == index))
                    {
                        _selectedItemIndex = index;
                    }
                }
                ImGui.ListBoxFooter();
            }

            if (ImGui.Button("Add"))
            {
                _config.Items.Add(new CommandItem());
                dirty = true;
            }

            ImGui.SameLine();

            if (ImGui.Button("Remove") && _selectedItemIndex >= 0)
            {
                _config.Items.RemoveAt(_selectedItemIndex);
                _selectedItemIndex = _config.Items.Count <= _selectedItemIndex ? _config.Items.Count - 1 : _selectedItemIndex;
                dirty = true;
            }
        }

        private void DrawCommandConfig(ref bool dirty, CommandItem selectedItem)
        {
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo("Job##command-config-class", selectedItem.Context.DisplayJob))
            {
                if (ImGui.Selectable(Context.Any))
                {
                    selectedItem.Context.Job = null;
                    dirty = true;
                }

                foreach (var job in _pi.Data.GetExcelSheet<ClassJob>())
                {
                    if (ImGui.Selectable(job.Abbreviation))
                    {
                        selectedItem.Context.Job = job.Abbreviation;
                        dirty = true;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine(0, 50);

            ImGui.SetNextItemWidth(300);
            if (ImGui.BeginCombo("State##command-config-state", selectedItem.Context.DisplayState))
            {
                if (ImGui.Selectable(Context.Any))
                {
                    selectedItem.Context.State = null;
                    dirty = true;
                }

                foreach (var state in (XivState[]) Enum.GetValues(typeof(XivState)))
                {
                    if (ImGui.Selectable(state.Name()))
                    {
                        selectedItem.Context.State = state;
                        dirty = true;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.Text("On enter");
            dirty |= ImGui.InputTextMultiline("##command-config-on-enter", ref selectedItem.Commands.OnEnterCommand, CommandSet.CommandsMaxLength, Constants.TextboxSize);

            ImGui.Text("On exit");
            dirty |= ImGui.InputTextMultiline("##command-config-on-exit", ref selectedItem.Commands.OnExitCommand, CommandSet.CommandsMaxLength, Constants.TextboxSize);
        }

        #region Plugin Interface

        public void BuildUi() => Draw();
        public void OnOpenConfigUI(object sender, EventArgs args) => Visible = true;

        #endregion
    }
}
