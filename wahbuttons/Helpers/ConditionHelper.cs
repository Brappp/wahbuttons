using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WahButtons.Helpers
{
    public static class ConditionHelper
    {
        // Dictionary of condition flags to their descriptions
        private static readonly Dictionary<ConditionFlag, string> ConditionDescriptions = new Dictionary<ConditionFlag, string>
        {
            { ConditionFlag.InCombat, "In Combat" },
            { ConditionFlag.BoundByDuty, "In Duty" },
            { ConditionFlag.BetweenAreas, "Changing Zones" },
            { ConditionFlag.BetweenAreas51, "Loading" },
            { ConditionFlag.OccupiedInCutSceneEvent, "In Cutscene" },
            { ConditionFlag.OccupiedInEvent, "In Event" },
            { ConditionFlag.WatchingCutscene, "Watching Cutscene" },
            { ConditionFlag.WatchingCutscene78, "Watching Cutscene" },
            { ConditionFlag.Mounted, "Mounted" },
            { ConditionFlag.Jumping, "Jumping" },
            { ConditionFlag.Swimming, "Swimming" },
            { ConditionFlag.Diving, "Diving" },
            { ConditionFlag.Crafting, "Crafting" },
            { ConditionFlag.Gathering, "Gathering" },
            { ConditionFlag.Fishing, "Fishing" },
            { ConditionFlag.LoggingOut, "Logging Out" },
            { ConditionFlag.UsingParasol, "Using Parasol" },
            { ConditionFlag.RolePlaying, "Role Playing" },
            { ConditionFlag.ParticipatingInCrossWorldPartyOrAlliance, "In Cross-world Party" }
        };

        /// <summary>
        /// Gets all defined condition flags
        /// </summary>
        public static IEnumerable<ConditionFlag> GetAllConditionFlags()
        {
            return Enum.GetValues<ConditionFlag>();
        }

        /// <summary>
        /// Gets all active condition flags
        /// </summary>
        public static IEnumerable<ConditionFlag> GetActiveConditionFlags()
        {
            return GetAllConditionFlags().Where(flag => Plugin.Condition[flag]);
        }

        /// <summary>
        /// Gets a user-friendly description for a condition flag with its ID
        /// </summary>
        public static string GetConditionDescription(ConditionFlag flag)
        {
            return ConditionDescriptions.TryGetValue(flag, out string description) 
                ? description 
                : flag.ToString();
        }

        /// <summary>
        /// Checks if a specified condition flag is active
        /// </summary>
        public static bool IsConditionActive(ConditionFlag flag)
        {
            return Plugin.Condition[flag];
        }

        public static ConditionFlag[] GetCommonGameConditions()
        {
            return new ConditionFlag[]
            {
                ConditionFlag.InCombat,
                ConditionFlag.BoundByDuty,
                ConditionFlag.Mounted,
                ConditionFlag.Swimming,
                ConditionFlag.Crafting,
                ConditionFlag.Gathering,
                ConditionFlag.OccupiedInCutSceneEvent,
                ConditionFlag.WatchingCutscene
            };
        }
    }
} 