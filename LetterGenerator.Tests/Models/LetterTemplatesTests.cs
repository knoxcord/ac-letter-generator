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

    private static HashSet<LetterType> AvailableOn(DateTime date) =>
        [.. LetterTemplates.GetAvailableLetters(date).Select(letter => letter.Key)];

    /// <summary>
    /// Converts the MMdd int used by <see cref="LetterTemplate.AvailableRange"/> into a date in <see cref="Year"/>.
    /// </summary>
    private static DateTime ToDate(int monthDay) => new(Year, monthDay / 100, monthDay % 100);
}
