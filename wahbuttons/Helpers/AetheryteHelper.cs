using System;
using System.Collections.Generic;
using System.Linq;

namespace WahButtons.Helpers
{
    public static class AetheryteHelper
    {
        // A data class to represent aetheryte information
        public class AetheryteData
        {
            public class Aetheryte
            {
                public uint Id { get; set; }
                public string Name { get; set; }
                public string Region { get; set; }
                public string Zone { get; set; }

                public Aetheryte(uint id, string name, string region, string zone)
                {
                    Id = id;
                    Name = name;
                    Region = region;
                    Zone = zone;
                }
            }

            private static List<Aetheryte> Aetherytes = new List<Aetheryte>
            {
                // La Noscea
                new Aetheryte(8, "Limsa Lominsa Lower Decks", "La Noscea", "Limsa Lominsa"),
                new Aetheryte(9, "Limsa Lominsa Upper Decks", "La Noscea", "Limsa Lominsa"),
                new Aetheryte(10, "Middle La Noscea", "La Noscea", "Middle La Noscea"),
                new Aetheryte(11, "Lower La Noscea", "La Noscea", "Lower La Noscea"),
                new Aetheryte(12, "Eastern La Noscea", "La Noscea", "Eastern La Noscea"),
                new Aetheryte(13, "Western La Noscea", "La Noscea", "Western La Noscea"),
                new Aetheryte(14, "Upper La Noscea", "La Noscea", "Upper La Noscea"),
                new Aetheryte(15, "Outer La Noscea", "La Noscea", "Outer La Noscea"),

                // Thanalan
                new Aetheryte(1, "Ul'dah - Steps of Nald", "Thanalan", "Ul'dah"),
                new Aetheryte(2, "Ul'dah - Steps of Thal", "Thanalan", "Ul'dah"),
                new Aetheryte(3, "Western Thanalan", "Thanalan", "Western Thanalan"),
                new Aetheryte(4, "Central Thanalan", "Thanalan", "Central Thanalan"),
                new Aetheryte(5, "Eastern Thanalan", "Thanalan", "Eastern Thanalan"),
                new Aetheryte(6, "Southern Thanalan", "Thanalan", "Southern Thanalan"),
                new Aetheryte(7, "Northern Thanalan", "Thanalan", "Northern Thanalan"),

                // The Black Shroud
                new Aetheryte(16, "New Gridania", "The Black Shroud", "New Gridania"),
                new Aetheryte(17, "Old Gridania", "The Black Shroud", "Old Gridania"),
                new Aetheryte(18, "Central Shroud", "The Black Shroud", "Central Shroud"),
                new Aetheryte(19, "East Shroud", "The Black Shroud", "East Shroud"),
                new Aetheryte(20, "South Shroud", "The Black Shroud", "South Shroud"),
                new Aetheryte(21, "North Shroud", "The Black Shroud", "North Shroud"),

                // Coerthas
                new Aetheryte(22, "Coerthas Central Highlands", "Coerthas", "Coerthas Central Highlands"),
                new Aetheryte(23, "Coerthas Western Highlands", "Coerthas", "Coerthas Western Highlands"),
                new Aetheryte(70, "Foundation", "Coerthas", "Ishgard"),
                new Aetheryte(71, "The Pillars", "Coerthas", "Ishgard"),

                // Mor Dhona
                new Aetheryte(24, "Mor Dhona", "Mor Dhona", "Mor Dhona"),

                // Abalathia
                new Aetheryte(76, "The Sea of Clouds", "Abalathia", "The Sea of Clouds"),
                new Aetheryte(77, "Azys Lla", "Abalathia", "Azys Lla"),

                // Dravania
                new Aetheryte(78, "Idyllshire", "Dravania", "Idyllshire"),
                new Aetheryte(79, "The Dravanian Forelands", "Dravania", "The Dravanian Forelands"),
                new Aetheryte(80, "The Dravanian Hinterlands", "Dravania", "The Dravanian Hinterlands"),
                new Aetheryte(81, "The Churning Mists", "Dravania", "The Churning Mists"),

                // Gyr Abania
                new Aetheryte(98, "Rhalgr's Reach", "Gyr Abania", "Rhalgr's Reach"),
                new Aetheryte(99, "The Fringes", "Gyr Abania", "The Fringes"),
                new Aetheryte(100, "The Peaks", "Gyr Abania", "The Peaks"),
                new Aetheryte(101, "The Lochs", "Gyr Abania", "The Lochs"),

                // Hingashi
                new Aetheryte(111, "Kugane", "Hingashi", "Kugane"),

                // Othard
                new Aetheryte(112, "The Ruby Sea", "Othard", "The Ruby Sea"),
                new Aetheryte(113, "Yanxia", "Othard", "Yanxia"),
                new Aetheryte(114, "The Azim Steppe", "Othard", "The Azim Steppe"),

                // Norvrandt
                new Aetheryte(133, "The Crystarium", "Norvrandt", "The Crystarium"),
                new Aetheryte(134, "Eulmore", "Norvrandt", "Eulmore"),
                new Aetheryte(135, "Lakeland", "Norvrandt", "Lakeland"),
                new Aetheryte(136, "Kholusia", "Norvrandt", "Kholusia"),
                new Aetheryte(137, "Amh Araeng", "Norvrandt", "Amh Araeng"),
                new Aetheryte(138, "Il Mheg", "Norvrandt", "Il Mheg"),
                new Aetheryte(139, "The Rak'tika Greatwood", "Norvrandt", "The Rak'tika Greatwood"),
                new Aetheryte(140, "The Tempest", "Norvrandt", "The Tempest"),

                // Sharlayan & Thavnair
                new Aetheryte(144, "Old Sharlayan", "Sharlayan", "Old Sharlayan"),
                new Aetheryte(145, "Radz-at-Han", "Thavnair", "Radz-at-Han"),
                new Aetheryte(146, "Labyrinthos", "Labyrinthos", "Labyrinthos"),
                new Aetheryte(147, "Thavnair", "Thavnair", "Thavnair"),
                new Aetheryte(148, "Garlemald", "Garlemald", "Garlemald"),
                new Aetheryte(149, "Mare Lamentorum", "Mare Lamentorum", "Mare Lamentorum"),
                new Aetheryte(150, "Ultima Thule", "Ultima Thule", "Ultima Thule"),
                new Aetheryte(151, "Elpis", "Elpis", "Elpis")
            };

            public static List<Aetheryte> GetAllAetherytes()
            {
                return Aetherytes;
            }

            public static Aetheryte GetAetheryteById(uint id)
            {
                return Aetherytes.FirstOrDefault(a => a.Id == id);
            }

            public static List<string> GetRegions()
            {
                return Aetherytes.Select(a => a.Region).Distinct().OrderBy(r => r).ToList();
            }

            public static List<Aetheryte> GetAetherytesByRegion(string region)
            {
                if (string.IsNullOrEmpty(region))
                    return Aetherytes;

                return Aetherytes.Where(a => a.Region == region).ToList();
            }
        }

        // Favorites functionality
        private static List<uint> Favorites = new List<uint>();

        public static List<uint> GetFavorites()
        {
            return Favorites;
        }

        public static void AddFavorite(uint aetheryteId)
        {
            if (!Favorites.Contains(aetheryteId))
            {
                Favorites.Add(aetheryteId);
            }
        }

        public static void RemoveFavorite(uint aetheryteId)
        {
            Favorites.Remove(aetheryteId);
        }

        public static bool IsFavorite(uint aetheryteId)
        {
            return Favorites.Contains(aetheryteId);
        }

        // Helper methods for the existing functionality
        public static List<string> GetRegions()
        {
            return AetheryteData.GetRegions();
        }

        public static AetheryteData.Aetheryte GetAetheryteById(uint id)
        {
            return AetheryteData.GetAetheryteById(id);
        }

        public static List<AetheryteData.Aetheryte> GetAetherytesByRegion(string region)
        {
            return AetheryteData.GetAetherytesByRegion(region);
        }

        // Dictionary of aetheryte IDs to their names, organized by region
        public static readonly Dictionary<string, Dictionary<uint, string>> AetherytesByRegionLegacy = new()
        {
            ["La Noscea"] = new Dictionary<uint, string>
            {
                [8] = "Limsa Lominsa Lower Decks",
                [9] = "Limsa Lominsa Upper Decks",
                [10] = "Middle La Noscea",
                [11] = "Lower La Noscea",
                [12] = "Eastern La Noscea",
                [13] = "Western La Noscea",
                [14] = "Upper La Noscea",
                [15] = "Outer La Noscea"
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

        // Generate teleport command from aetheryte ID
        public static string GenerateTeleportCommand(uint aetheryteId)
        {
            return $"/teleport {aetheryteId}";
        }

        // Get all aetherytes in a flat dictionary
        public static Dictionary<uint, string> GetAllAetherytesLegacy()
        {
            var allAetherytes = new Dictionary<uint, string>();
            foreach (var region in AetherytesByRegionLegacy)
            {
                foreach (var aetheryte in region.Value)
                {
                    allAetherytes[aetheryte.Key] = aetheryte.Value;
                }
            }
            return allAetherytes;
        }

        // Get aetherytes in a specific region
        public static Dictionary<uint, string> GetAetherytesByRegionLegacy(string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                return GetAllAetherytesLegacy();
            }

            if (AetherytesByRegionLegacy.TryGetValue(region, out var aetherytes))
            {
                return aetherytes;
            }

            return new Dictionary<uint, string>();
        }

        // Get the name of an aetheryte by ID
        public static string GetAetheryteName(uint id)
        {
            foreach (var region in AetherytesByRegionLegacy.Values)
            {
                if (region.TryGetValue(id, out string name))
                {
                    return name;
                }
            }
            return "Unknown Aetheryte";
        }
    }
} 