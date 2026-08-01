using LetterGenerator.Configuration;
using LetterGenerator.DTOs;
using LetterGenerator.Interfaces;
using LetterGenerator.Rendering;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<LetterTemplateConfiguration>(
    builder.Configuration.GetSection(LetterTemplateConfiguration.SectionName));
builder.Services.AddSingleton<IStationarySource, LocalStationarySource>();
builder.Services.AddSingleton<ILetterRenderer, LetterRenderer>();

var app = builder.Build();

// Resolved eagerly so failures happen at startup rather than upon first request
app.Services.GetRequiredService<IStationarySource>();
app.Services.GetRequiredService<ILetterRenderer>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Serves the OpenAPI document at /openapi/v1.json
    app.MapOpenApi();
    // NSwag is used only for the UI, pointed at the document above
    app.UseSwaggerUi(settings => settings.DocumentPath = "/openapi/v1.json");
}

app.MapPost("/letter", async (GenerateLetterRequest request, ILetterRenderer renderer, CancellationToken cancellationToken) =>
    {
        byte[] image = await renderer.RenderAsync(request, cancellationToken);
        return Results.File(image, "image/webp", "letter.webp");
    })
    .WithName("GenerateLetter")
    .Produces(StatusCodes.Status200OK, contentType: "image/webp");

app.Run();
