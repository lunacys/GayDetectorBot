# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
RUN echo $APP_UID
COPY ["3rdParty/jint/Jint/Directory.Build.props", "3rdParty/jint/Jint/"]
COPY ["3rdParty/jint/NuGet.config", "3rdParty/jint/"]
COPY ["GayDetectorBot.WebApi/GayDetectorBot.WebApi.csproj", "GayDetectorBot.WebApi/"]
COPY ["3rdParty/jint/Jint/Jint.csproj", "3rdParty/jint/Jint/"]
RUN dotnet restore "./GayDetectorBot.WebApi/GayDetectorBot.WebApi.csproj"
COPY . .
RUN chmod +x "GayDetectorBot.WebApi/.docker/wait-for-postgres.sh"
RUN chmod +x ".docker/wait-for-postgres.sh"
WORKDIR "/src/GayDetectorBot.WebApi"
RUN dotnet build "./GayDetectorBot.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build -a x64

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GayDetectorBot.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish -a x64
RUN chown $APP_UID /app
RUN chmod 777 "/app/publish/wait-for-postgres.sh"
#RUN dotnet tool install --global dotnet-ef --version 9.0
#ENV PATH="$PATH:/root/.dotnet/tools"
#RUN dotnet ef database update

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "GayDetectorBot.WebApi.dll"]
ENTRYPOINT ["/app/wait-for-postgres.sh"]
CMD [ "dotnet", "GayDetectorBot.WebApi.dll" ]
