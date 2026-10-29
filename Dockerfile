FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Emby.UserData/ ./Emby.UserData/
RUN dotnet publish Emby.UserData/Emby.UserData.csproj -c Release -o /out
FROM scratch AS artifact
COPY --from=build /out/ /
