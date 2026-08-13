# AC Letter Generator

A small HTTP service that renders text as an Animal Crossing style letter image, for use by a Discord
bot. It picks a piece of stationery at random — seasonal designs only appear during the part
of the year they belong to — and draws the title, body and valediction onto it with SkiaSharp.

Text is scaled and wrapped to fit the card, and emoji are drawn as Twemoji artwork.

## Usage

```
POST /letter
{ "title": "...", "body": "...", "valediction": "..." }
```

Responds with `image/webp`. In development, Swagger UI is served at `/swagger`.

## Running

```sh
docker compose up --build              # listens on 127.0.0.1:5258
dotnet run --project LetterGenerator   # listens on localhost:5258
```

## Attribution

Emoji graphics are Twemoji, copyright 2019 Twitter, Inc and other contributors, licensed under
[CC-BY 4.0](https://creativecommons.org/licenses/by/4.0/). See
[LetterGenerator/Emoji/ATTRIBUTION.md](LetterGenerator/Emoji/ATTRIBUTION.md).

Animal Crossing stationery designs and the Seurat typeface are property of Nintendo, used here for a
personal, non-commercial project.
