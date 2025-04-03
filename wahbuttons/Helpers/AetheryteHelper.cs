using System;
using System.Collections.Generic;
using System.Linq;

namespace WahButtons.Helpers
{
    public static class AetheryteHelper
    {
        // Dictionary of aetheryte IDs to their names, organized by region
        public static readonly Dictionary<string, Dictionary<uint, string>> AetherytesByRegion = new()
        {
            ["La Noscea"] = new Dictionary<uint, string>
            {
                { 2, "Limsa Lominsa Lower Decks" },
                { 8, "Limsa Lominsa Upper Decks" },
                { 4, "Middle La Noscea" },
                { 5, "Lower La Noscea" },
                { 6, "Eastern La Noscea" },
                { 7, "Western La Noscea" },
                { 59, "Upper La Noscea" },
                { 68, "Outer La Noscea" }
            },
            ["The Black Shroud"] = new Dictionary<uint, string>
            {
                { 3, "New Gridania" },
                { 9, "Old Gridania" },
                { 10, "Central Shroud" },
                { 11, "East Shroud" },
                { 12, "South Shroud" },
                { 13, "North Shroud" }
            },
            ["Thanalan"] = new Dictionary<uint, string>
            {
                { 1, "Ul'dah - Steps of Nald" },
                { 14, "Ul'dah - Steps of Thal" },
                { 15, "Western Thanalan" },
                { 16, "Central Thanalan" },
                { 17, "Eastern Thanalan" },
                { 18, "Southern Thanalan" },
                { 19, "Northern Thanalan" }
            },
            ["Ishgard"] = new Dictionary<uint, string>
            {
                { 70, "Foundation" },
                { 71, "The Pillars" },
                { 72, "Coerthas Western Highlands" },
                { 62, "Coerthas Central Highlands" },
                { 74, "The Sea of Clouds" },
                { 75, "Azys Lla" },
                { 76, "The Churning Mists" },
                { 73, "The Dravanian Forelands" },
                { 77, "The Dravanian Hinterlands" }
            },
            ["Gyr Abania"] = new Dictionary<uint, string>
            {
                { 98, "Rhalgr's Reach" },
                { 94, "The Fringes" },
                { 95, "The Peaks" },
                { 96, "The Lochs" }
            },
            ["Far East"] = new Dictionary<uint, string>
            {
                { 111, "Kugane" },
                { 112, "Ruby Sea" },
                { 113, "Yanxia" },
                { 114, "The Azim Steppe" }
            },
            ["Crystarium & Norvrandt"] = new Dictionary<uint, string>
            {
                { 133, "The Crystarium" },
                { 134, "Eulmore" },
                { 135, "Lakeland" },
                { 136, "Kholusia" },
                { 137, "Amh Araeng" },
                { 138, "Il Mheg" },
                { 139, "The Rak'tika Greatwood" },
                { 140, "The Tempest" }
            },
            ["Old Sharlayan & The North"] = new Dictionary<uint, string>
            {
                { 144, "Old Sharlayan" },
                { 145, "Radz-at-Han" },
                { 146, "Labyrinthos" },
                { 147, "Thavnair" },
                { 148, "Garlemald" },
                { 149, "Mare Lamentorum" },
                { 150, "Ultima Thule" },
                { 151, "Elpis" }
            },
            ["Ilsabard"] = new Dictionary<uint, string>
            {
                { 180, "Loporrit Island" },
                { 181, "Phasmascape" }
            }
        };

        // Get all aetherytes
        public static Dictionary<uint, string> GetAllAetherytes()
        {
            var allAetherytes = new Dictionary<uint, string>();
            foreach (var region in AetherytesByRegion)
            {
                foreach (var aetheryte in region.Value)
                {
                    allAetherytes[aetheryte.Key] = aetheryte.Value;
                }
            }
            return allAetherytes;
        }

        // Get all region names
        public static List<string> GetRegions()
        {
            return AetherytesByRegion.Keys.ToList();
        }

        // Get aetherytes in a specific region
        public static Dictionary<uint, string> GetAetherytesByRegion(string region)
        {
            if (AetherytesByRegion.TryGetValue(region, out var aetherytes))
            {
                return aetherytes;
            }
            return new Dictionary<uint, string>();
        }

        // Generate teleport command for a specific aetheryte
        public static string GenerateTeleportCommand(uint aetheryteId)
        {
            return $"/teleport {aetheryteId}";
        }

        // Get a friendly name for an aetheryte ID
        public static string GetAetheryteName(uint aetheryteId)
        {
            foreach (var region in AetherytesByRegion)
            {
                if (region.Value.TryGetValue(aetheryteId, out var name))
                {
                    return $"{name} ({region.Key})";
                }
            }
            return $"Aetheryte {aetheryteId}";
        }
    }
} 