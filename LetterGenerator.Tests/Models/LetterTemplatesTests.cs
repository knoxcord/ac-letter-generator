using LetterGenerator.Models;

namespace LetterGenerator.Tests.Models;

public class LetterTemplatesTests
{
    // A non-leap year keeps the arithmetic below honest: no template range touches 29 February.
    private const int Year = 2025;

    private static readonly DateTime[] DaysOfYear = [.. Enumerable.Range(0, 365).Select(offset => new DateTime(Year, 1, 1).AddDays(offset))];

    private static IEnumerable<LetterType> SeasonalLetters => LetterTemplates.Metadata
        .Where(letter => letter.Value.AvailableRange.HasValue)
        .Select(letter => letter.Key);

    private static IEnumerable<LetterType> YearRoundLetters => LetterTemplates.Metadata
        .Where(letter => !letter.Value.AvailableRange.HasValue)
        .Select(letter => letter.Key);

    [TestCaseSource(nameof(SeasonalLetters))]
    public void GetAvailableLetters_ReturnsSeasonalLetter_OnFirstAndLastDayOfItsRange(LetterType letterType)
    {
        var (start, end) = LetterTemplates.Metadata[letterType].AvailableRange!.Value;

        Assert.Multiple(() =>
        {
            Assert.That(AvailableOn(ToDate(start)), Does.Contain(letterType), $"first day of range ({start:0000})");
            Assert.That(AvailableOn(ToDate(end)), Does.Contain(letterType), $"last day of range ({end:0000})");
        });
    }

    [TestCaseSource(nameof(SeasonalLetters))]
    public void GetAvailableLetters_OmitsSeasonalLetter_OnDaysBracketingItsRange(LetterType letterType)
    {
        var (start, end) = LetterTemplates.Metadata[letterType].AvailableRange!.Value;

        Assert.Multiple(() =>
        {
            Assert.That(AvailableOn(ToDate(start).AddDays(-1)), Does.Not.Contain(letterType), $"day before range ({start:0000})");
            Assert.That(AvailableOn(ToDate(end).AddDays(1)), Does.Not.Contain(letterType), $"day after range ({end:0000})");
        });
    }

    [TestCaseSource(nameof(YearRoundLetters))]
    public void GetAvailableLetters_ReturnsYearRoundLetter_OnEveryDayOfTheYear(LetterType letterType)
    {
        var missingDays = DaysOfYear.Where(day => !AvailableOn(day).Contains(letterType));

        Assert.That(missingDays, Is.Empty);
    }

    // Range contained within the year.
    [TestCase(LetterType.Beach, 05, 31, false)]
    [TestCase(LetterType.Beach, 06, 01, true)]
    [TestCase(LetterType.Beach, 07, 15, true)]
    [TestCase(LetterType.Beach, 08, 31, true)]
    [TestCase(LetterType.Beach, 09, 01, false)]
    // Range wrapping the year boundary.
    [TestCase(LetterType.Snowflake, 11, 25, false)]
    [TestCase(LetterType.Snowflake, 11, 26, true)]
    [TestCase(LetterType.Snowflake, 12, 31, true)]
    [TestCase(LetterType.Snowflake, 01, 01, true)]
    [TestCase(LetterType.Snowflake, 02, 24, true)]
    [TestCase(LetterType.Snowflake, 02, 25, false)]
    [TestCase(LetterType.Snowflake, 07, 04, false)]
    // Single-month range.
    [TestCase(LetterType.Halloween, 09, 30, false)]
    [TestCase(LetterType.Halloween, 10, 31, true)]
    [TestCase(LetterType.Halloween, 11, 01, false)]
    // No range at all.
    [TestCase(LetterType.Common, 01, 01, true)]
    [TestCase(LetterType.Common, 10, 31, true)]
    public void GetAvailableLetters_MatchesTheSeason(LetterType letterType, int month, int day, bool expected)
    {
        var available = AvailableOn(new DateTime(Year, month, day)).Contains(letterType);

        Assert.That(available, Is.EqualTo(expected));
    }

    [Test]
    public void GetAvailableLetters_IgnoresTheTimeOfDay()
    {
        var startOfDay = AvailableOn(new DateTime(Year, 10, 31, 00, 00, 00));
        var endOfDay = AvailableOn(new DateTime(Year, 10, 31, 23, 59, 59));

        Assert.That(startOfDay, Is.EquivalentTo(endOfDay));
    }

    /// <summary>
    /// <see cref="LetterTemplates.GetLetter"/> indexes straight into the metadata, so a letter type
    /// without an entry would be a 500 rather than a miss.
    /// </summary>
    [Test]
    public void Metadata_CoversEveryLetterType()
    {
        Assert.That(LetterTemplates.Metadata.Keys, Is.EquivalentTo(Enum.GetValues<LetterType>()));
    }

    /// <summary>
    /// The random pick indexes into the available letters, so a day with none available would throw
    /// rather than return nothing.
    /// </summary>
    [Test]
    public void GetAvailableLetters_IsNeverEmpty_OnAnyDayOfTheYear()
    {
        var emptyDays = DaysOfYear.Where(day => AvailableOn(day).Count == 0);

        Assert.That(emptyDays, Is.Empty);
    }

    [TestCase(01, 15, TestName = "GetRandomLetter_OnlyReturnsLettersInSeason_InWinter")]
    [TestCase(07, 15, TestName = "GetRandomLetter_OnlyReturnsLettersInSeason_InSummer")]
    public void GetRandomLetter_OnlyReturnsLettersInSeason(int month, int day)
    {
        var date = new DateTime(Year, month, day);
        var available = AvailableOn(date);

        // Enough draws to make an out of season letter slipping through very unlikely to be missed
        var drawn = Enumerable.Range(0, 500).Select(_ => LetterTemplates.GetRandomLetter(date)).ToHashSet();

        Assert.That(drawn, Is.SubsetOf(available));
    }

    [TestCase("Happy Birthday Tom!", TestName = "MatchesKeywords_IgnoresCase")]
    [TestCase("happy bday!", TestName = "MatchesKeywords_MatchesEveryKeywordInTheList")]
    [TestCase("It's Tom's b-day", TestName = "MatchesKeywords_MatchesKeywordsContainingPunctuation")]
    // A bare substring match is deliberate, so wording around the keyword does not have to be anticipated
    [TestCase("two birthdays this week", TestName = "MatchesKeywords_MatchesAPluralisedKeyword")]
    [TestCase("come to the birthdayparty", TestName = "MatchesKeywords_MatchesAKeywordRunTogetherWithAnotherWord")]
    public void MatchesKeywords_MatchesBirthdayWording(string letterText)
    {
        Assert.That(LetterTemplates.Metadata[LetterType.BirthdayCake].MatchesKeywords(letterText), Is.True);
    }

    [TestCase("")]
    [TestCase("The bridge repairs are finished")]
    public void MatchesKeywords_DoesNotMatchUnrelatedText(string letterText)
    {
        Assert.That(LetterTemplates.Metadata[LetterType.BirthdayCake].MatchesKeywords(letterText), Is.False);
    }

    /// <summary>
    /// Keywords are opt-in, so a template without them must never be boosted no matter what the letter says.
    /// </summary>
    [Test]
    public void MatchesKeywords_IsFalse_ForTemplatesWithoutKeywords()
    {
        var boosted = LetterTemplates.Metadata
            .Where(letter => letter.Key != LetterType.BirthdayCake)
            .Where(letter => letter.Value.MatchesKeywords("Happy Birthday Tom! bday b-day"))
            .Select(letter => letter.Key);

        Assert.That(boosted, Is.Empty);
    }

    private const string UnrelatedText = "The bridge repairs are finished";
    private const string BirthdayText = "Happy Birthday Tom!";

    [TestCase(LetterType.Common, UnrelatedText, LetterTemplates.DefaultWeight, TestName = "GetWeight_IsUnboostedForAYearRoundLetter")]
    [TestCase(LetterType.Halloween, UnrelatedText, LetterTemplates.SeasonalWeight, TestName = "GetWeight_BoostsASeasonalLetter")]
    [TestCase(LetterType.BirthdayCake, BirthdayText, LetterTemplates.KeywordWeight, TestName = "GetWeight_BoostsAKeywordMatch")]
    [TestCase(LetterType.BirthdayCake, UnrelatedText, LetterTemplates.DefaultWeight, TestName = "GetWeight_DoesNotBoostAKeywordLetterWithoutAMatch")]
    public void GetWeight_MatchesTheTier(LetterType letterType, string letterText, int expected)
    {
        var weight = LetterTemplates.GetWeight(LetterTemplates.Metadata[letterType], letterText);

        Assert.That(weight, Is.EqualTo(expected));
    }

    /// <summary>
    /// The tier assertions above cover the weights themselves; this covers the draw actually honouring them.
    /// The bound is the share a keyword match would win if weights were ignored, scaled up by a wide margin
    /// to keep the test off the knife edge of its real expectation.
    /// </summary>
    [Test]
    public void GetRandomLetter_UsuallyDrawsAKeywordMatch()
    {
        var date = new DateTime(Year, 7, 15);
        const int draws = 2000;
        var unweightedShare = (double)draws / AvailableOn(date).Count;

        var cakes = Enumerable.Range(0, draws)
            .Count(_ => LetterTemplates.GetRandomLetter(date, BirthdayText) == LetterType.BirthdayCake);

        Assert.That(cakes, Is.GreaterThan(unweightedShare * 5));
    }

    /// <summary>
    /// A keyword boost must not become a guarantee, or m-bot's background reroll would have nothing left
    /// to reroll to on a letter that mentions a birthday.
    /// </summary>
    [Test]
    public void GetRandomLetter_StillDrawsOtherLetters_WhenAKeywordMatches()
    {
        var date = new DateTime(Year, 7, 15);

        var drawn = Enumerable.Range(0, 2000)
            .Select(_ => LetterTemplates.GetRandomLetter(date, BirthdayText))
            .ToHashSet();

        Assert.That(drawn, Is.SupersetOf(AvailableOn(date)));
    }

    private static HashSet<LetterType> AvailableOn(DateTime date) =>
        [.. LetterTemplates.GetAvailableLetters(date).Select(letter => letter.Key)];

    /// <summary>
    /// Converts the MMdd int used by <see cref="LetterTemplate.AvailableRange"/> into a date in <see cref="Year"/>.
    /// </summary>
    private static DateTime ToDate(int monthDay) => new(Year, monthDay / 100, monthDay % 100);
}
