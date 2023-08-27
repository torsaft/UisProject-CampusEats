FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /CampusEats
COPY ./src/CampusEats .

RUN dotnet restore ./CampusEats.csproj
RUN dotnet publish -c Release -o /CampusEats/out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /CampusEats
COPY --from=base /CampusEats/out .

EXPOSE 5000 5001
# ENV ASPNETCORE_ENVIRONMENT=Development
# ENV ASPNETCORE_URLS=http://+:5000
# ENV ASPNETCORE_URLS https://+:5001

ENTRYPOINT dotnet CampusEats.dll
# ENTRYPOINT dotnet watch run --no-restore
