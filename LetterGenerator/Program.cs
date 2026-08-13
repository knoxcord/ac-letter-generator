using System.Text.Json.Serialization;
using LetterGenerator.Configuration;
using LetterGenerator.DTOs;
using LetterGenerator.Interfaces;
using LetterGenerator.Rendering;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)));
builder.Services.Configure<LetterTemplateConfiguration>(
    builder.Configuration.GetSection(LetterTemplateConfiguration.SectionName));
builder.Services.AddSingleton<IStationerySource, LocalStationerySource>();
builder.Services.AddSingleton<IEmojiCatalog, EmojiCatalog>();
builder.Services.AddSingleton<ILetterRenderer, LetterRenderer>();

var app = builder.Build();

// Resolved eagerly so failures happen at startup rather than upon first request
app.Services.GetRequiredService<IStationerySource>();
app.Services.GetRequiredService<IEmojiCatalog>();
app.Services.GetRequiredService<ILetterRenderer>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serves the OpenAPI document at /openapi/v1.json
    app.MapOpenApi();
    // NSwag is used only for the UI, pointed at the document above
    app.UseSwaggerUi(settings => settings.DocumentPath = "/openapi/v1.json");
}

app.MapPost("/letter", async (GenerateLetterRequest request, ILetterRenderer renderer, HttpResponse response, CancellationToken cancellationToken) =>
    {
        var letter = await renderer.RenderAsync(request, request.Stationery, cancellationToken);

        // Reported whether it was asked for or picked at random, so the caller can always keep hold of it
        response.Headers[LetterHeaders.Stationery] = letter.Stationery.ToString();

        return Results.File(letter.Image, "image/webp", "letter.webp");
    })
    .WithName("GenerateLetter")
    .WithDescription($"Draws the letter and responds with the image. The {LetterHeaders.Stationery} response " +
                     "header names the stationery used; send it back as 'stationery' to draw on that same one again.")
    .Produces(StatusCodes.Status200OK, contentType: "image/webp");

app.Run();

// Top level statements compile to an internal Program, which WebApplicationFactory cannot reach
public partial class Program;
