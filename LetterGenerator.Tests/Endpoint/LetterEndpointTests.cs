using System.Net;
using System.Net.Http.Json;
using LetterGenerator.DTOs;
using LetterGenerator.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LetterGenerator.Tests.Endpoint;

/// <summary>
/// Covers the contract the caller depends on: which stationery a letter was drawn on comes back on
/// every response, and asking for one back draws on that same stationery. Both live only in the
/// endpoint, so nothing below this level can prove them.
/// </summary>
public class LetterEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void StartApp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void StopApp()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Post_ReportsTheStationery_WhenNoneWasAskedFor()
    {
        var response = await PostLetter(new { title = "Hi", body = "Test", valediction = "Love, Matt" });

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/webp"));
            Assert.That(Stationery(response), Is.Not.Null.And.Not.Empty);
            Assert.That((await response.Content.ReadAsByteArrayAsync()), Is.Not.Empty);
        });
    }

    [Test]
    public async Task Post_ReportsAStationeryTheCallerCanSendBack()
    {
        var response = await PostLetter(new { title = "Hi", body = "Test", valediction = "Love, Matt" });

        var reported = Stationery(response);

        Assert.That(Enum.TryParse<LetterType>(reported, out _), Is.True,
            $"'{reported}' should be a name the stationery field accepts");
    }

    /// <summary>
    /// Snowflake is out of season for most of the year, so this also covers a letter being redrawn on
    /// the stationery it was first written on months later.
    /// </summary>
    [Test]
    public async Task Post_DrawsOnTheStationeryAskedFor()
    {
        var response = await PostLetter(new
        {
            title = "Hi",
            body = "Test",
            valediction = "Love, Matt",
            stationery = nameof(LetterType.Snowflake),
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(Stationery(response), Is.EqualTo(nameof(LetterType.Snowflake)));
        });
    }

    [Test]
    public async Task Post_AcceptsAStationeryInAnyCase()
    {
        var response = await PostLetter(new
        {
            title = "Hi",
            body = "Test",
            valediction = "Love, Matt",
            stationery = "snowflake",
        });

        Assert.That(Stationery(response), Is.EqualTo(nameof(LetterType.Snowflake)));
    }

    /// <summary>
    /// A stationery that cannot be drawn is refused rather than quietly swapped for a random one, so a
    /// caller holding a name that has stopped meaning anything finds out.
    /// </summary>
    [TestCase("\"Snowfalke\"", TestName = "Post_RejectsAStationery_ThatIsNotAName")]
    [TestCase("2", TestName = "Post_RejectsAStationery_GivenAsANumber")]
    [TestCase("999", TestName = "Post_RejectsAStationery_GivenAsANumberNoLetterUses")]
    public async Task Post_RejectsAnUnusableStationery(string stationeryJson)
    {
        var body = $$"""{"title":"Hi","body":"Test","valediction":"Love","stationery":{{stationeryJson}}}""";

        var response = await _client.PostAsync("/letter", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    /// <summary>
    /// Birthday wording puts the excluded cake at about a quarter of draws, so 25 requests make an
    /// exclusion that never reached the draw all but certain to show up.
    /// </summary>
    [Test]
    public async Task Post_KeepsTheExcludedStationeryOutOfTheRandomPick()
    {
        var drawn = new List<string?>();
        for (var request = 0; request < 25; request++)
            drawn.Add(Stationery(await PostLetter(new
            {
                title = "Hi",
                body = "Happy Birthday Tom!",
                valediction = "Love, Matt",
                excludeStationery = nameof(LetterType.BirthdayCake),
            })));

        Assert.That(drawn, Does.Not.Contain(nameof(LetterType.BirthdayCake)));
    }

    /// <summary>
    /// Asking for a stationery is the stronger instruction of the two, so excluding the same one is not a
    /// contradiction the caller has to resolve before sending.
    /// </summary>
    [Test]
    public async Task Post_DrawsOnTheStationeryAskedFor_EvenWhenItIsAlsoExcluded()
    {
        var response = await PostLetter(new
        {
            title = "Hi",
            body = "Test",
            valediction = "Love, Matt",
            stationery = nameof(LetterType.Snowflake),
            excludeStationery = nameof(LetterType.Snowflake),
        });

        Assert.That(Stationery(response), Is.EqualTo(nameof(LetterType.Snowflake)));
    }

    [Test]
    public async Task Post_RejectsAnUnusableStationery_InTheExclusion()
    {
        const string body = """{"title":"Hi","body":"Test","valediction":"Love","excludeStationery":"Snowfalke"}""";

        var response = await _client.PostAsync("/letter", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    /// <summary>
    /// The three field body callers sent before stationery existed has to keep working untouched.
    /// </summary>
    [Test]
    public async Task Post_StillDrawsALetter_ForABodyWithNoStationery()
    {
        var response = await PostLetter(new { Title = "Dear villager", Body = "Thanks for the pears!", Valediction = "Your pal, Matt" });

        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/webp"));
        });
    }

    private Task<HttpResponseMessage> PostLetter(object body) => _client.PostAsJsonAsync("/letter", body);

    private static string? Stationery(HttpResponseMessage response) =>
        response.Headers.TryGetValues(LetterHeaders.Stationery, out var values) ? values.FirstOrDefault() : null;
}
