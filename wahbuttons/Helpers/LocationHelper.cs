using System;
using System.Collections.Generic;

namespace WahButtons.Helpers
{
    public static class LocationHelper
    {
        // Dictionary mapping territory IDs to their corresponding regions in AetheryteHelper
        private static readonly Dictionary<uint, string> TerritoryToRegion = new()
        {
            // La Noscea
            { 128, "La Noscea" }, // Limsa Lominsa Lower Decks
            { 129, "La Noscea" }, // Limsa Lominsa Upper Decks
            { 134, "La Noscea" }, // Middle La Noscea
            { 135, "La Noscea" }, // Lower La Noscea
            { 137, "La Noscea" }, // Eastern La Noscea
            { 138, "La Noscea" }, // Western La Noscea
            { 139, "La Noscea" }, // Upper La Noscea
            { 180, "La Noscea" }, // Outer La Noscea

            // The Black Shroud
            { 132, "The Black Shroud" }, // New Gridania
            { 133, "The Black Shroud" }, // Old Gridania
            { 148, "The Black Shroud" }, // Central Shroud
            { 152, "The Black Shroud" }, // East Shroud
            { 153, "The Black Shroud" }, // South Shroud
            { 154, "The Black Shroud" }, // North Shroud

            // Thanalan
            { 130, "Thanalan" }, // Ul'dah - Steps of Nald
            { 131, "Thanalan" }, // Ul'dah - Steps of Thal
            { 140, "Thanalan" }, // Western Thanalan
            { 141, "Thanalan" }, // Central Thanalan
            { 145, "Thanalan" }, // Eastern Thanalan
            { 146, "Thanalan" }, // Southern Thanalan
            { 147, "Thanalan" }, // Northern Thanalan

            // Ishgard
            { 418, "Ishgard" }, // Foundation
            { 419, "Ishgard" }, // The Pillars
            { 397, "Ishgard" }, // Coerthas Western Highlands
            { 155, "Ishgard" }, // Coerthas Central Highlands
            { 401, "Ishgard" }, // The Sea of Clouds
            { 402, "Ishgard" }, // Azys Lla
            { 398, "Ishgard" }, // The Churning Mists
            { 399, "Ishgard" }, // The Dravanian Forelands
            { 400, "Ishgard" }, // The Dravanian Hinterlands

            // Gyr Abania
            { 635, "Gyr Abania" }, // Rhalgr's Reach
            { 612, "Gyr Abania" }, // The Fringes
            { 620, "Gyr Abania" }, // The Peaks
            { 621, "Gyr Abania" }, // The Lochs

            // Far East
            { 628, "Far East" }, // Kugane
            { 613, "Far East" }, // Ruby Sea
            { 614, "Far East" }, // Yanxia
            { 622, "Far East" }, // The Azim Steppe

            // Crystarium & Norvrandt
            { 819, "Crystarium & Norvrandt" }, // The Crystarium
            { 820, "Crystarium & Norvrandt" }, // Eulmore
            { 813, "Crystarium & Norvrandt" }, // Lakeland
            { 814, "Crystarium & Norvrandt" }, // Kholusia
            { 815, "Crystarium & Norvrandt" }, // Amh Araeng
            { 816, "Crystarium & Norvrandt" }, // Il Mheg
            { 817, "Crystarium & Norvrandt" }, // The Rak'tika Greatwood
            { 818, "Crystarium & Norvrandt" }, // The Tempest

            // Old Sharlayan & The North
            { 962, "Old Sharlayan & The North" }, // Old Sharlayan
            { 963, "Old Sharlayan & The North" }, // Radz-at-Han
            { 956, "Old Sharlayan & The North" }, // Labyrinthos
            { 957, "Old Sharlayan & The North" }, // Thavnair
            { 958, "Old Sharlayan & The North" }, // Garlemald
            { 959, "Old Sharlayan & The North" }, // Mare Lamentorum
            { 960, "Old Sharlayan & The North" }, // Ultima Thule
            { 961, "Old Sharlayan & The North" }, // Elpis

            // Ilsabard
            { 1055, "Ilsabard" }, // Loporrit Island
            { 1056, "Ilsabard" } // Phasmascape
        };

        // Dictionary mapping region names to a default aetheryte ID for that region
        private static readonly Dictionary<string, uint> DefaultAetheryteForRegion = new()
        {
            { "La Noscea", 2 }, // Limsa Lominsa Lower Decks
            { "The Black Shroud", 3 }, // New Gridania
            { "Thanalan", 1 }, // Ul'dah - Steps of Nald
            { "Ishgard", 70 }, // Foundation
            { "Gyr Abania", 98 }, // Rhalgr's Reach
            { "Far East", 111 }, // Kugane
            { "Crystarium & Norvrandt", 133 }, // The Crystarium
            { "Old Sharlayan & The North", 144 }, // Old Sharlayan
            { "Ilsabard", 180 } // Loporrit Island
        };

        /// <summary>
        /// Gets the current region based on the player's location
        /// </summary>
        public static string GetCurrentRegion()
        {
            try
            {
                // Get current territory ID
                var territoryId = Plugin.ClientState.TerritoryType;
                
                // Try to get the region from the territory ID
                if (TerritoryToRegion.TryGetValue(territoryId, out var region))
                {
                    return region;
                }
                
                // Default to first region if unknown
                return AetheryteHelper.GetRegions()[0];
            }
            catch (Exception ex)
            {
                Plugin.PluginLog.Error($"Error determining current region: {ex.Message}");
                return AetheryteHelper.GetRegions()[0];
            }
        }

        /// <summary>
        /// Gets the nearest aetheryte ID based on the player's current region
        /// </summary>
        public static uint GetNearestAetheryteId()
        {
            string region = GetCurrentRegion();
            
            // Return the default aetheryte for the region
            if (DefaultAetheryteForRegion.TryGetValue(region, out var aetheryteId))
            {
                return aetheryteId;
            }
            
            // Default to Limsa if no match
            return 2;
        }
    }
} 