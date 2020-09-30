#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState;
using Dalamud.Plugin;

namespace ContextCommands
{
    internal class Context
    {
        public const string Any = "Any";

        /// <summary>The job abbreviation, or null for any job</summary>
        public string? Job;
        public XivState? State;

        public bool IsActive(DalamudPluginInterface pi)
        {
            var playerJob = Job != null ? pi.ClientState.LocalPlayer.ClassJob.GameData.Abbreviation : null;
            return (State?.IsActive(pi) ?? true) && Job == playerJob;
        }

        public string DisplayJob => Job ?? Any;
        public string DisplayState => State?.Name() ?? Any;
    }

    internal enum XivState
    {
        WeaponDrawn,
        InCombat,
        InInstance,
        InCrafting,
        InGathering,
        InFishing,
        InPerforming,
        RolePlaying,
        Swimming,
        Mounted
    }

    internal static class XivStateExt
    {
        private const int ActorOffset = 0x1906;

        private static byte GetStatusRaw(DalamudPluginInterface pi) => Marshal.ReadByte(pi.TargetModuleScanner.ResolveRelativeAddress(pi.ClientState.LocalPlayer.Address, ActorOffset));
        private static bool CheckCondition(ClientState state, params ConditionFlag[] conds) => conds.Any(c => state.Condition[c]);

        /// <summary><seealso cref="Name"/></summary>
        private static readonly Dictionary<XivState, string> ConditionNames = new Dictionary<XivState, string>()
        {
            { XivState.WeaponDrawn, "Weapon drawn" },
            { XivState.InCombat, "In combat" },
            { XivState.InInstance, "In instance" },
            { XivState.InCrafting, "Crafting" },
            { XivState.InGathering, "Gathering" },
            { XivState.InFishing, "Fishing" },
            { XivState.InPerforming, "Performing" },
            { XivState.RolePlaying, "Role-playing" },
            { XivState.Swimming, "Swimming" },
            { XivState.Mounted, "Mounted" }
        };

        /// <summary><seealso cref="IsActive"/></summary>
        private static readonly Dictionary<XivState, Func<DalamudPluginInterface, bool>> ConditionChecks = new Dictionary<XivState, Func<DalamudPluginInterface, bool>>()
        {
            { XivState.WeaponDrawn, (pi) => (GetStatusRaw(pi) & 4) > 0 },
            { XivState.InCombat, (pi) => CheckCondition(pi.ClientState, ConditionFlag.InCombat) },
            { XivState.InInstance, pi => CheckCondition(pi.ClientState, ConditionFlag.BoundByDuty, ConditionFlag.BoundByDuty56, ConditionFlag.BoundByDuty95, ConditionFlag.BoundToDuty97) },
            { XivState.InCrafting, (pi) => CheckCondition(pi.ClientState, ConditionFlag.Crafting, ConditionFlag.Crafting40) },
            { XivState.InGathering, (pi) => CheckCondition(pi.ClientState, ConditionFlag.Gathering, ConditionFlag.Gathering42) },
            { XivState.InFishing, (pi) => CheckCondition(pi.ClientState, ConditionFlag.Fishing) },
            { XivState.InPerforming, (pi) => CheckCondition(pi.ClientState, ConditionFlag.Performing) },
            { XivState.RolePlaying, (pi) => CheckCondition(pi.ClientState, ConditionFlag.RolePlaying) },
            { XivState.Swimming, (pi) => CheckCondition(pi.ClientState, ConditionFlag.Swimming) },
            { XivState.Mounted, (pi) => CheckCondition(pi.ClientState, ConditionFlag.Mounted, ConditionFlag.Mounted2, ConditionFlag.Mounting, ConditionFlag.Mounting71) }
        };

        /// <summary>
        /// Get the full, human-readable name of this condition.
        /// </summary>
        public static string Name(this XivState condition)
        {
            if (ConditionNames.TryGetValue(condition, out string name))
                return name;

            PluginLog.LogError($"No name string found for condition '{condition.ToString()}'");
            return condition.ToString();
        }

        /// <summary>
        /// Check if the client state currently meets this condition.
        /// </summary>
        public static bool IsActive(this XivState condition, DalamudPluginInterface pi)
        {
            if (ConditionChecks.TryGetValue(condition, out var checkFunc))
                return checkFunc(pi);

            PluginLog.LogError($"No check function found for condition '{condition.ToString()}'");
            return false;
        }
    }
}
