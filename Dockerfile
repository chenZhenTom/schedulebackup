FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/runtime:6.0
WORKDIR /app
COPY --from=build /app/out ./

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

ENV BACKUP_COMMAND=""

LABEL image.name="schedulebackup"

# $BACKUP_COMMAND 負責帶入程式執行所需參數
CMD ["sh", "-c", "dotnet /app/schedulebackup.dll $BACKUP_COMMAND"]
