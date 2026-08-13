using LetterGenerator.Rendering;

namespace LetterGenerator.Tests.Rendering;

/// <summary>
/// Runs against the artwork the app actually ships, so a sequence that these prove is recognised is one
/// the renderer can draw.
/// </summary>
public class EmojiCatalogTests
{
    private const string Sunglasses = "\U0001F60E";
    private const string ThumbsUp = "\U0001F44D";
    private const string SkinToneModifier = "\U0001F3FD";
    private const string ThumbsUpWithSkinTone = ThumbsUp + SkinToneModifier;
    private const string Family = "\U0001F468‍\U0001F469‍\U0001F467";
    private const string FlagOfTheUnitedStates = "\U0001F1FA\U0001F1F8";
    private const string KeycapOne = "1️⃣";
    private const string RedHeart = "❤️";

    private EmojiCatalog _catalog = null!;

    [OneTimeSetUp]
    public void CreateCatalog() => _catalog = new EmojiCatalog();

    [OneTimeTearDown]
    public void DisposeCatalog() => _catalog.Dispose();

    [Test]
    public void Tokenize_ReturnsNothing_ForEmptyText()
    {
        Assert.That(_catalog.Tokenize(string.Empty), Is.Empty);
    }

    [Test]
    public void Tokenize_ReturnsOneTextRun_WhenThereAreNoEmoji()
    {
        var runs = Tokenize("Dear villager, thanks for the pears! See you at 3:30.");

        Assert.That(runs, Is.EqualTo(new[] { ("Dear villager, thanks for the pears! See you at 3:30.", false) }));
    }

    /// <summary>
    /// Digits, <c>#</c> and <c>*</c> lead the keycap sequences, so they are the characters most at risk of
    /// being mistaken for artwork when they appear on their own in ordinary text.
    /// </summary>
    [TestCase("1")]
    [TestCase("Ordered 3 apples and 12 pears")]
    [TestCase("#hashtag and *asterisk*")]
    public void Tokenize_LeavesKeycapLeadingCharactersAsText(string text)
    {
        var runs = Tokenize(text);

        Assert.That(runs, Is.EqualTo(new[] { (text, false) }));
    }

    [Test]
    public void Tokenize_SplitsTextAroundAnEmoji()
    {
        var runs = Tokenize($"Cool cool {Sunglasses} indeed");

        Assert.That(runs, Is.EqualTo(new[]
        {
            ("Cool cool ", false),
            (Sunglasses, true),
            (" indeed", false),
        }));
    }

    /// <summary>
    /// Each of these spans several codepoints. Skia cannot join them into one glyph, so any of them coming
    /// back as more than a single run is the bug this catalog exists to prevent.
    /// </summary>
    [TestCase(ThumbsUpWithSkinTone, TestName = "Tokenize_KeepsSequenceWhole_SkinTone")]
    [TestCase(Family, TestName = "Tokenize_KeepsSequenceWhole_ZeroWidthJoinerFamily")]
    [TestCase(FlagOfTheUnitedStates, TestName = "Tokenize_KeepsSequenceWhole_Flag")]
    [TestCase(KeycapOne, TestName = "Tokenize_KeepsSequenceWhole_Keycap")]
    [TestCase(RedHeart, TestName = "Tokenize_KeepsSequenceWhole_VariationSelector")]
    public void Tokenize_KeepsSequenceWhole(string sequence)
    {
        var runs = Tokenize(sequence);

        Assert.That(runs, Is.EqualTo(new[] { (sequence, true) }));
    }

    /// <summary>
    /// The skin toned thumbs up begins with the plain thumbs up, which has artwork of its own. Matching the
    /// shorter one first would draw the modifier as a loose colour swatch.
    /// </summary>
    [Test]
    public void Tokenize_PrefersTheLongestSequence_WhenAShorterOneAlsoHasArtwork()
    {
        Assert.That(Tokenize(ThumbsUp), Is.EqualTo(new[] { (ThumbsUp, true) }), "plain thumbs up has its own artwork");
        Assert.That(Tokenize(ThumbsUpWithSkinTone), Is.EqualTo(new[] { (ThumbsUpWithSkinTone, true) }));
    }

    [Test]
    public void Tokenize_RunsAdjacentEmojiTogetherAsSeparateRuns()
    {
        var runs = Tokenize(Sunglasses + Family + Sunglasses);

        Assert.That(runs, Is.EqualTo(new[]
        {
            (Sunglasses, true),
            (Family, true),
            (Sunglasses, true),
        }));
    }

    [Test]
    public void Tokenize_PreservesTheOriginalText_AcrossEveryRun()
    {
        const string text = "Hi " + Sunglasses + " there " + Family + "!";

        var rejoined = string.Concat(_catalog.Tokenize(text).Select(run => run.Text));

        Assert.That(rejoined, Is.EqualTo(text));
    }

    private (string Text, bool IsEmoji)[] Tokenize(string text) =>
        [.. _catalog.Tokenize(text).Select(run => (run.Text, run.IsEmoji))];
}
