#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /data
RUN mkdir -p shared && \
    mkdir -p shared/testfolder1/subfolder1 && \
    mkdir -p shared/testfolder2/subfolder2 && \
    touch shared/testfolder1/testfile1.txt && \
    touch shared/testfolder1/subfolder1/subfoldertestfile1.txt && \
    touch shared/testfolder2/testfile2.txt && \
    touch shared/testfolder2/subfolder2/subfoldertestfile2.txt
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FileListener/FileListener.csproj", "FileListener/"]
RUN dotnet restore "FileListener/FileListener.csproj"
COPY . .
WORKDIR "/src/FileListener"
RUN dotnet build "FileListener.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileListener.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
COPY mydevcerts/*.pfx /app/certs/
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileListener.dll"]