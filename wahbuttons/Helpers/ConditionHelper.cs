using Dalamud.Game.ClientState.Conditions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WahButtons.Helpers
{
    public static class ConditionHelper
    {
        // Dictionary of condition flags to their descriptions
        private static readonly Dictionary<ConditionFlag, string> ConditionDescriptions = new()
        {
            { ConditionFlag.InCombat, "In Combat" },
            { ConditionFlag.BoundByDuty, "In Duty" },
            { ConditionFlag.BetweenAreas, "Between Areas" },
            { ConditionFlag.BetweenAreas51, "Between Areas 51" },
            { ConditionFlag.WatchingCutscene, "Watching Cutscene" },
            { ConditionFlag.WatchingCutscene78, "Watching Cutscene 78" },
            { ConditionFlag.OccupiedInCutSceneEvent, "In Cutscene Event" },
            { ConditionFlag.OccupiedInQuestEvent, "In Quest Event" },
            { ConditionFlag.OccupiedSummoningBell, "Using Summoning Bell" },
            { ConditionFlag.OccupiedInEvent, "In Event" },
            { ConditionFlag.RolePlaying, "Role Playing" },
            { ConditionFlag.Fishing, "Fishing" },
            { ConditionFlag.Gathering, "Gathering" },
            { ConditionFlag.Crafting, "Crafting" },
            { ConditionFlag.Mounted, "Mounted" },
            { ConditionFlag.Mounting, "Mounting" },
            { ConditionFlag.ParticipatingInCrossWorldPartyOrAlliance, "In Cross-World Party" },
            { ConditionFlag.LoggingOut, "Logging Out" },
            { ConditionFlag.CarryingObject, "Carrying Object" },
            { ConditionFlag.UsingParasol, "Using Parasol" },
            { ConditionFlag.Performing, "Performing" },
            { ConditionFlag.Diving, "Diving" },
            { ConditionFlag.Jumping, "Jumping" },
            { ConditionFlag.CarryingItem, "Carrying Item" }
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
            string description = ConditionDescriptions.TryGetValue(flag, out var desc) 
                ? desc 
                : flag.ToString();
                
            return $"{description} (ID: {(int)flag})";
        }

        /// <summary>
        /// Checks if a specified condition flag is active
        /// </summary>
        public static bool IsConditionActive(ConditionFlag flag)
        {
            return Plugin.Condition[flag];
        }
    }
} 