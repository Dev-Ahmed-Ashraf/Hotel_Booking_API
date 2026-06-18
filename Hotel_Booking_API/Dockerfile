FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Hotel_Booking_API.csproj", "."]
RUN dotnet restore "Hotel_Booking_API.csproj"

COPY . .
RUN dotnet build "Hotel_Booking_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hotel_Booking_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hotel_Booking_API.dll"]