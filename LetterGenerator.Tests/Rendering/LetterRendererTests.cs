using LetterGenerator.DTOs;
using LetterGenerator.Interfaces;
using LetterGenerator.Models;
using LetterGenerator.Rendering;
using SkiaSharp;

namespace LetterGenerator.Tests.Rendering;

public class LetterRendererTests
{
    // Out of season for most of the year, so asking for it proves the seasonal filter was skipped
    private const LetterType OutOfSeasonInSummer = LetterType.Snowflake;

    private RecordingStationerySource _stationerySource = null!;
    private EmojiCatalog _emojiCatalog = null!;
    private LetterRenderer _renderer = null!;

    [SetUp]
    public void CreateRenderer()
    {
        _stationerySource = new RecordingStationerySource();
        _emojiCatalog = new EmojiCatalog();
        _renderer = new LetterRenderer(_stationerySource, _emojiCatalog);
    }

    [TearDown]
    public void DisposeCatalog() => _emojiCatalog.Dispose();

    /// <summary>
    /// Both halves matter: the requested stationery has to reach the source that draws it, and it has to
    /// come back on the result. Checking only the result would let the reported stationery be a lie.
    /// </summary>
    [Test]
    public async Task RenderAsync_DrawsOnTheRequestedStationery()
    {
        var letter = await _renderer.RenderAsync(Request(), LetterType.Beach);

        Assert.Multiple(() =>
        {
            Assert.That(_stationerySource.Opened, Is.EqualTo(LetterType.Beach), "stationery opened to draw on");
            Assert.That(letter.Stationery, Is.EqualTo(LetterType.Beach), "stationery reported on the result");
        });
    }

    /// <summary>
    /// The point of asking for a stationery by name: a letter written in December still redraws on the
    /// same design in July.
    /// </summary>
    [Test]
    public async Task RenderAsync_DrawsOnARequestedStationery_EvenOutOfSeason()
    {
        var inSeason = LetterTemplates.GetAvailableLetters(DateTime.Now).Select(letter => letter.Key);
        Assume.That(inSeason, Does.Not.Contain(OutOfSeasonInSummer), "test only means anything out of season");

        var letter = await _renderer.RenderAsync(Request(), OutOfSeasonInSummer);

        Assert.That(letter.Stationery, Is.EqualTo(OutOfSeasonInSummer));
    }

    [Test]
    public async Task RenderAsync_PicksAStationeryInSeason_WhenNoneIsRequested()
    {
        var inSeason = LetterTemplates.GetAvailableLetters(DateTime.Now).Select(letter => letter.Key).ToHashSet();

        var letter = await _renderer.RenderAsync(Request());

        Assert.That(letter.Stationery, Is.AnyOf([.. inSeason]));
    }

    [Test]
    public async Task RenderAsync_ReturnsADecodableImage()
    {
        var letter = await _renderer.RenderAsync(Request(), LetterType.Common);

        using var decoded = SKBitmap.Decode(letter.Image);

        Assert.That(decoded, Is.Not.Null);
    }

    private static GenerateLetterRequest Request() => new()
    {
        Title = "Dear villager",
        Body = "The pears are coming along nicely 😎",
        Valediction = "Your pal, Matt",
    };

    /// <summary>
    /// Serves the real artwork so the renderer draws for real, while recording what it was asked for
    /// </summary>
    private sealed class RecordingStationerySource : IStationerySource
    {
        public LetterType? Opened { get; private set; }

        public Task<Stream> OpenStationery(LetterType letterType, CancellationToken cancellationToken = default)
        {
            Opened = letterType;

            Stream stationery = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Images", $"{letterType}.webp"));
            return Task.FromResult(stationery);
        }
    }
}
