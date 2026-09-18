# Build and run with the .NET 10 images so the host needs no .NET installed.
# Two stages: the SDK compiles, the smaller aspnet runtime image ships.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore before copying the rest so that layer is reused whenever only source
# files change — the package graph is the slow part of a rebuild.
COPY ["ABC construction/ABC construction.csproj", "ABC construction/"]
RUN dotnet restore "ABC construction/ABC construction.csproj"

COPY . .
RUN dotnet publish "ABC construction/ABC construction.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Owned by the app user, not root: the upload service creates directories under
# wwwroot/images/uploads at runtime, which a non-root process cannot do inside a
# root-owned tree.
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

# A container process that cannot write outside its own directories limits what
# a compromise reaches.
USER $APP_UID

# Documents the fallback port; the platform still maps whatever it likes.
EXPOSE 8080

# Shell form so $PORT is expanded at start-up, not baked in at build time:
# hosting platforms assign the port per deploy and pass it in the environment.
# Only http is bound, because TLS terminates at the platform's proxy in front
# of this container (see UseForwardedHeaders in Program.cs).
ENTRYPOINT ["sh", "-c", "exec dotnet 'ABC construction.dll' --urls \"http://+:${PORT:-8080}\""]
