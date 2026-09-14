# AC Letter Generator

A small HTTP service that renders text as an Animal Crossing style letter image, for use by a Discord
bot. It picks a piece of stationery at random — seasonal designs only appear during the part
of the year they belong to — and draws the title, body and valediction onto it with SkiaSharp.

The draw is weighted rather than even. A design that is in season is favored over a year-round one,
since only a few are ever in season at once, and a design whose keywords appear in the letter is
favored much more strongly — mention a birthday and you will usually, though not always, get the
birthday cake.

Text is scaled and wrapped to fit the card, and emoji are drawn as Twemoji artwork.

## Usage

```
POST /letter
{ "title": "...", "body": "...", "valediction": "...", "stationery": "Snowflake" }
```

Responds with `image/webp`. The `Letter-Stationery` response header names the stationery used.

`stationery` is optional; leave it out and one in season is picked as described above. Send back a name from
the header to draw on that same stationery again — useful for redrawing a letter with a different
valediction without the design changing. A named stationery is used whether it is in season or not,
and an unknown one is a 400 rather than a silent fallback to random.

In development, Swagger UI is served at `/swagger`.

## Running

```sh
docker compose up --build              # listens on 127.0.0.1:5258
dotnet run --project LetterGenerator   # listens on localhost:5258
./scripts/update-emoji.sh              # refresh the bundled Twemoji artwork
```

## Attribution

Emoji graphics are [Twemoji](https://github.com/jdecked/twemoji), copyright (c) 2022-present Jason
Sofonia & Justine De Caires and copyright (c) 2014-2021 Twitter, Inc and other contributors,
licensed under [CC-BY 4.0](https://creativecommons.org/licenses/by/4.0/).

Animal Crossing stationery designs and the Seurat typeface are property of Nintendo, used here for a
personal, non-commercial project.
