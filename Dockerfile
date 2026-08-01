# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG RID=linux-x64

# Restored in its own layer so NuGet is only re-fetched when the project file changes.
COPY LetterGenerator/LetterGenerator.csproj LetterGenerator/
RUN dotnet restore LetterGenerator/LetterGenerator.csproj -r $RID

COPY LetterGenerator/ LetterGenerator/
RUN dotnet publish LetterGenerator/LetterGenerator.csproj -c Release -o /app \
    -r $RID --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
# ContentRootPath resolves to the working directory, and LetterTemplates:DirectoryPath ("Images")
# is resolved against it, so this has to be the directory the publish output lands in.
WORKDIR /app
COPY --from=build /app .

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "LetterGenerator.dll"]
