FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Crpg.sln .

RUN dotnet sln Crpg.sln remove \
	src/DumpItemsMod/DumpItemsMod.csproj \
	src/GameMod/GameMod.csproj \
	test \
	test/Application.UTest/Application.UTest.csproj \
	test/Persistence.UTest/Persistence.UTest.csproj \
	test/Common.UTest/Common.UTest.csproj \
	test/Infrastructure.UTest/Infrastructure.UTest.csproj

COPY src/Application/Application.csproj src/Application/
COPY src/Common/Common.csproj src/Common/
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Persistence/Persistence.csproj src/Persistence/
COPY src/WebApi/WebApi.csproj src/WebApi/

RUN dotnet restore

# Copy everything else and build
COPY src/stylecop.json src/
COPY src/stylecop.ruleset src/
COPY src/Application/ src/Application/
COPY src/Common/ src/Common/
COPY src/Domain/ src/Domain/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/Persistence/ src/Persistence/
COPY src/WebApi/ src/WebApi/

RUN dotnet build -c Release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Crpg.WebApi.dll"]
