using LetterGenerator.Rendering;
using SkiaSharp;

namespace LetterGenerator.Tests.Rendering;

public class TextHelpersTests
{
    private const string Sunglasses = "\U0001F60E";
    private const string Family = "\U0001F468‍\U0001F469‍\U0001F467";

    private const float TextSize = 40.0f;

    private EmojiCatalog _catalog = null!;
    private SKTypeface _typeface = null!;
    private SKFont _font = null!;

    [OneTimeSetUp]
    public void CreateFixtures()
    {
        _catalog = new EmojiCatalog();
        _typeface = SKTypeface.FromFile(Path.Combine(AppContext.BaseDirectory, "Fonts", "SeuratProB.otf"))!;
        _font = new SKFont { Typeface = _typeface, Size = TextSize };
    }

    [OneTimeTearDown]
    public void DisposeFixtures()
    {
        _font.Dispose();
        _typeface.Dispose();
        _catalog.Dispose();
    }

    [TestCase("<:blobcatcool:1234567890>", ":blobcatcool:", TestName = "RewriteCustomEmoji_RewritesAStaticEmoji")]
    [TestCase("<a:party:987654321>", ":party:", TestName = "RewriteCustomEmoji_RewritesAnAnimatedEmoji")]
    [TestCase("hi <:wave:1> and <:bye:22>!", "hi :wave: and :bye:!", TestName = "RewriteCustomEmoji_RewritesEveryOccurrence")]
    [TestCase("no markup here", "no markup here", TestName = "RewriteCustomEmoji_LeavesPlainTextAlone")]
    [TestCase(":shortcode:", ":shortcode:", TestName = "RewriteCustomEmoji_LeavesAPlainShortcodeAlone")]
    [TestCase("a < b and c > d", "a < b and c > d", TestName = "RewriteCustomEmoji_LeavesLooseAnglesAlone")]
    public void RewriteCustomEmoji(string text, string expected)
    {
        Assert.That(TextHelpers.RewriteCustomEmoji(text), Is.EqualTo(expected));
    }

    /// <summary>
    /// Emoji are drawn square and as tall as the line box rather than measured through the font, which has
    /// no glyph for them at all.
    /// </summary>
    [Test]
    public void GetLineWidth_MeasuresAnEmojiAsASquareTheHeightOfTheLineBox()
    {
        var expected = TextHelpers.GetGlyphHeight(_font.Metrics);

        Assert.That(TextHelpers.GetLineWidth(Sunglasses, _font, _catalog), Is.EqualTo(expected).Within(0.01f));
    }

    /// <summary>
    /// However many codepoints a sequence spans, it occupies one emoji's worth of the line.
    /// </summary>
    [Test]
    public void GetLineWidth_MeasuresASequenceAsASingleEmoji()
    {
        var single = TextHelpers.GetLineWidth(Sunglasses, _font, _catalog);
        var sequence = TextHelpers.GetLineWidth(Family, _font, _catalog);

        Assert.That(sequence, Is.EqualTo(single).Within(0.01f));
    }

    [Test]
    public void GetLineWidth_IsZero_ForEmptyText()
    {
        Assert.That(TextHelpers.GetLineWidth(string.Empty, _font, _catalog), Is.Zero);
    }

    [Test]
    public void GetLineWidth_AddsEmojiToTheSurroundingText()
    {
        var textOnly = TextHelpers.GetLineWidth("Cool", _font, _catalog);
        var withEmoji = TextHelpers.GetLineWidth($"Cool{Sunglasses}", _font, _catalog);
        var emojiOnly = TextHelpers.GetLineWidth(Sunglasses, _font, _catalog);

        // The two pieces plus the one letter space that joins them
        Assert.That(withEmoji, Is.EqualTo(textOnly + emojiOnly + 3.0f).Within(0.01f));
    }

    /// <summary>
    /// A run of emoji with no whitespace is one long "word", so it takes the same path a long url does.
    /// Breaking it between codepoints would sever a sequence into the pieces it is built from.
    /// </summary>
    [Test]
    public void SplitWordsLongerThanWidth_NeverSplitsAnEmojiSequence()
    {
        var word = string.Concat(Enumerable.Repeat(Family, 8));
        var maxWidth = TextHelpers.GetLineWidth(Family, _font, _catalog) * 3;

        var pieces = TextHelpers.SplitWordsLongerThanWidth(word, _font, _catalog, maxWidth).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(string.Concat(pieces), Is.EqualTo(word), "the pieces should rejoin into the original word");
            Assert.That(pieces, Has.Count.GreaterThan(1), "the word should have been split");
            Assert.That(pieces, Is.All.Matches<string>(piece => piece.Length % Family.Length == 0),
                "every piece should be a whole number of family sequences");
        });
    }

    [Test]
    public void WrapLines_KeepsAnEmojiWithTheLineItBelongsTo()
    {
        var lines = TextHelpers.WrapLines($"Cool cool {Sunglasses}", _font, _catalog, maxWidth: 10_000, maxLines: 10);

        Assert.That(lines, Is.EqualTo(new[] { $"Cool cool {Sunglasses}" }));
    }
}
