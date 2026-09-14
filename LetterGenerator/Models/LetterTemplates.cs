using System.Collections.Frozen;

namespace LetterGenerator.Models;

public static class LetterTemplates
{
    public static readonly FrozenDictionary<LetterType, LetterTemplate> Metadata = new Dictionary<LetterType, LetterTemplateOptions>
    {
        [LetterType.Acorn] = new()
        {
            TitleColor = "#fff6d0",
            AvailableRange = (0901, 1125)
        },
        [LetterType.Airmail] = new()
        {
            TitleColor = "#010101"
        },
        [LetterType.BabyGoods] = new()
        {
            TitleColor = "#69544e",
            BodyColor = "#66bdca"
        },
        [LetterType.Balloons] = new()
        {
            TitleColor = "#da8528",
            BodyColor = "#64553b",
            TextBackgroundColor = "#fcfcf0",
            AvailableRange = (0315, 0515)
        },
        [LetterType.Bandage] = new()
        {
            TitleColor = "#fbffff",
            BodyColor = "#1173a3"
        },
        [LetterType.Beach] = new()
        {
            TitleColor = "#843800",
            BodyColor = "#a55100",
            AvailableRange = (0601, 0831)
        },
        [LetterType.BirthdayCake] = new()
        {
            TitleColor = "#533d15",
            Keywords = ["birthday", "bday", "b-day"]
        },
        [LetterType.BlueSky] = new()
        {
            TitleColor = "#a8ecff",
            BodyColor = "#d7f8ff"
        },
        [LetterType.BunnyDay] = new()
        {
            TitleColor = "#6c524a",
            TextBackgroundColor = "#fdfbeb",
            AvailableRange = (0315, 0510)
        },
        [LetterType.CarpetOfLeaves] = new()
        {
            TitleColor = "#ffecc0",
            AvailableRange = (0901, 1125)
        },
        [LetterType.Camo] = new()
        {
            TitleColor = "#e8e280",
            BodyColor = "#fff3ce"
        },
        [LetterType.CherryBlossoms] = new()
        {
            TitleColor = "#fb60a6",
            BodyColor = "#913762",
            AvailableRange = (0225, 0531)
        },
        [LetterType.Chocolate] = new()
        {
            TitleColor = "#f3c99e",
            BodyColor = "#fff3de",
            AvailableRange = (0116, 0214)
        },
        [LetterType.ChocolateHeart] = new()
        {
            TitleColor = "#fff0f4",
            AvailableRange = (0116, 0214)
        },
        [LetterType.FluffyClouds] = new()
        {
            TitleColor = "#313953",
            BodyColor = "#0b509e",
            ValedictionColor = "#fbfdff",
            AvailableRange = (0601, 0831)
        },
        [LetterType.CoolCool] = new()
        {
            TitleColor = "#dedede",
            BodyColor = "#afafaf"
        },
        [LetterType.Common] = new()
        {
            TitleColor = "#806059"
        },
        [LetterType.Dandelion] = new()
        {
            TitleColor = "#217703",
            BodyColor = "#f27625",
            AvailableRange = (0225, 0531)
        },
        [LetterType.DawningYear] = new()
        {
            TitleColor = "#382d20"
        },
        [LetterType.Decorative] = new()
        {
            TitleColor = "#ffffc5",
            BodyColor = "#ac441a"
        },
        [LetterType.ElegantRoses] = new()
        {
            TitleColor = "#ebd989",
            BodyColor = "#f0dbb1"
        },
        [LetterType.Fanciful] = new()
        {
            TitleColor = "#ffe2d4"
        },
        [LetterType.FantasyStars] = new()
        {
            TitleColor = "#ea61dd",
            BodyColor = "#6733e5",
            ValedictionColor = "#fe973d"
        },
        [LetterType.FathersDay] = new()
        {
            TitleColor = "#010101",
            AvailableRange = (0601, 0630)
        },
        [LetterType.FestiveTree] = new()
        {
            TitleColor = "#fdffff",
            AvailableRange = (1120, 0110)
        },
        [LetterType.Fireworks] = new()
        {
            TitleColor = "#ffbaf4",
            BodyColor = "#c9a3ff",
            ValedictionColor = "#92a7f7",
            TextBackgroundColor = "#362a98"
        },
        [LetterType.FlowerBouquet] = new()
        {
            TitleColor = "#673d2c",
            BodyColor = "#933102"
        },
        [LetterType.FullBloom] = new()
        {
            TitleColor = "#983f1c",
            BodyColor = "#e06200",
            AvailableRange = (0225, 0531)
        },
        [LetterType.Gears] = new()
        {
            TitleColor = "#fdfcff",
            BodyColor = "#8de7de"
        },
        [LetterType.Gem] = new()
        {
            TitleColor = "#2d7f86",
            BodyColor = "#398470",
            TextBackgroundColor = "#b9dec7"
        },
        [LetterType.Goldfish] = new()
        {
            TitleColor = "#44483e",
            BodyColor = "#2d7895"
        },
        [LetterType.Graduation] = new()
        {
            TitleColor = "#fffeff"
        },
        [LetterType.Graffiti] = new()
        {
            TitleColor = "#18f6e4",
            BodyColor = "#f4f72d"
        },
        [LetterType.Halloween] = new()
        {
            TitleColor = "#fed04b",
            BodyColor = "#fe9e34",
            ValedictionColor = "#fdd347",
            AvailableRange = (1001, 1031)
        },
        [LetterType.HappyClovers] = new()
        {
            TitleColor = "#6a441b",
            BodyColor = "#1a4a00"
        },
        [LetterType.Hibiscus] = new()
        {
            TitleColor = "#d90b7c",
            BodyColor = "#188da0",
            TextBackgroundColor = "#f3f1f2",
            AvailableRange = (0501, 0915)
        },
        [LetterType.Holiday] = new()
        {
            TitleColor = "#1f7c56",
            BodyColor = "#a70005",
            AvailableRange = (1120, 0110)
        },
        [LetterType.LovelyHearts] = new()
        {
            TitleColor = "#f29e9f",
            BodyColor = "#d15c64"
        },
        [LetterType.MothersDay] = new()
        {
            TitleColor = "#010101",
            AvailableRange = (0501, 0531)
        },
        [LetterType.Mushroom] = new()
        {
            TitleColor = "#f9f2da",
            AvailableRange = (0901, 1125)
        },
        [LetterType.Pumpkin] = new()
        {
            TitleColor = "#faf9de",
            BodyColor = "#fce26b",
            AvailableRange = (1001, 1031)
        },
        [LetterType.RedDragonflies] = new()
        {
            TitleColor = "#ef682a",
            BodyColor = "#b96645"
        },
        [LetterType.Ribbon] = new()
        {
            TitleColor = "#3a020d",
            BodyColor = "#5d554a"
        },
        [LetterType.Shapes] = new()
        {
            TitleColor = "#42bf7e",
            BodyColor = "#635a53"
        },
        [LetterType.ShootingStars] = new()
        {
            TitleColor = "#8dd5f6",
            BodyColor = "#d7eff9"
        },
        [LetterType.Snowflake] = new()
        {
            TitleColor = "#3ea0ba",
            BodyColor = "#02739b",
            AvailableRange = (1126, 0224)
        },
        [LetterType.Snowperson] = new()
        {
            TitleColor = "#3a9eba",
            BodyColor = "#f9ffff",
            AvailableRange = (1126, 0224)
        },
        [LetterType.SoManyHearts] = new()
        {
            TitleColor = "#8d02d3",
            BodyColor = "#fffbff"
        },
        [LetterType.Star] = new()
        {
            TitleColor = "#c24309",
            BodyColor = "#1b3d62"
        },
        [LetterType.StationeryGoods] = new()
        {
            TitleColor = "#6e6e6f"
        },
        [LetterType.Torn] = new()
        {
            TitleColor = "#717171"
        },
        [LetterType.TownView] = new()
        {
            TitleColor = "#5a0100",
            BodyColor = "#7c2100",
            AvailableRange = (1126, 0224)
        },
        [LetterType.TurkeyDay] = new()
        {
            TitleColor = "#b34b00",
            BodyColor = "#7a1d00",
            TextBackgroundColor = "#d89301",
            AvailableRange = (0916, 1130)
        },
        [LetterType.Velvety] = new()
        {
            TitleColor = "#c07d02",
            BodyColor = "#ebbe62"
        },
        [LetterType.WarmSweater] = new()
        {
            TitleColor = "#fffdfd",
            TextBackgroundColor = "#8a0100",
            AvailableRange = (1120, 0110)
        },
        [LetterType.Wedding] = new()
        {
            TitleColor = "#4c8cba",
            BodyColor = "#68ccd6"
        },
        [LetterType.WinterCamellia] = new()
        {
            TitleColor = "#fcfcfc",
            BodyColor = "#404b64",
            AvailableRange = (1126, 0224)
        },
        [LetterType.Zen] = new()
        {
            TitleColor = "#ffcf72",
            BodyColor = "#ffe9d6"
        }
    }.ToFrozenDictionary(entry => entry.Key, entry => new LetterTemplate(entry.Value));

    public static LetterTemplate GetLetter(LetterType letterType) => Metadata[letterType];

    public const int DefaultWeight = 1;

    // Only a handful of the seasonal designs are ever in season at once, so without a boost they are
    //   drowned out by the year-round ones during the short window where they actually fit
    public const int SeasonalWeight = 3;

    // Much larger than the seasonal boost because it competes against the whole pool rather than adding
    //   to a group of them. At the seasonal 3 a matched design would win about 6% of the time, which
    //   reads as unchanged. This lands nearer a coin flip while still leaving room to draw anything else
    public const int KeywordWeight = 40;

    public static LetterType GetRandomLetter(string letterText = "") => GetRandomLetter(DateTime.Now, letterText);

    public static LetterType GetRandomLetter(DateTime dateTime, string letterText = "")
    {
        var availableLetters = GetAvailableLetters(dateTime).ToList();
        var roll = Random.Shared.Next(availableLetters.Sum(letter => GetWeight(letter.Value, letterText)));

        foreach (var letter in availableLetters)
        {
            roll -= GetWeight(letter.Value, letterText);
            if (roll < 0)
                return letter.Key;
        }

        // Unreachable while every weight is positive, but returning the last letter beats throwing
        return availableLetters[^1].Key;
    }

    /// <summary>
    /// How many times more likely <paramref name="template"/> is to be drawn than a plain year-round one.
    /// The tiers are exclusive rather than multiplied, so a template that gains both keywords and a
    /// seasonal range still has one predictable weight
    /// </summary>
    public static int GetWeight(LetterTemplate template, string letterText) =>
        template.MatchesKeywords(letterText) ? KeywordWeight
        : template.AvailableRange.HasValue ? SeasonalWeight
        : DefaultWeight;

    public static IEnumerable<KeyValuePair<LetterType, LetterTemplate>> GetAvailableLetters(DateTime dateTime)
    {
        var date = dateTime.Month * 100 + dateTime.Day;
        return Metadata.Where(metadata => IsAvailable(metadata.Value, date));
    }

    private static bool IsAvailable(LetterTemplate template, int date)
    {
        if (!template.AvailableRange.HasValue)
            return true;

        var (start, end) = template.AvailableRange.Value;
        return start <= end
            ? date >= start && date <= end
            : date >= start || date <= end;
    }
}
