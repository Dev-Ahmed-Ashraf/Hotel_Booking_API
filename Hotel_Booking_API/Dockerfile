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

# Inject secrets
ARG JwtSettings__SecretKey
ARG Stripe__ApiKey
ARG Stripe__PublishableKey
ARG Stripe__WebhookSecret
ARG SmtpSettings__Username
ARG SmtpSettings__Password

ENV JwtSettings__SecretKey=$JwtSettings__SecretKey
ENV Stripe__ApiKey=$Stripe__ApiKey
ENV Stripe__PublishableKey=$Stripe__PublishableKey
ENV Stripe__WebhookSecret=$Stripe__WebhookSecret
ENV SmtpSettings__Username=$SmtpSettings__Username
ENV SmtpSettings__Password=$SmtpSettings__Password

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hotel_Booking_API.dll"]