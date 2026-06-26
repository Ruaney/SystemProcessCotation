# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY SystemProcessCotation/SystemProcessCotation.csproj SystemProcessCotation/
RUN dotnet restore SystemProcessCotation/SystemProcessCotation.csproj

COPY SystemProcessCotation/ SystemProcessCotation/
RUN dotnet publish SystemProcessCotation/SystemProcessCotation.csproj -c Release -o /app --no-restore

# --- runtime ---
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SystemProcessCotation.dll"]
