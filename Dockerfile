# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/MundoPet.Api -c Release -o /out

# --- execução ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app/api
COPY --from=build /out .
# O Program.cs procura o cliente em ../../client (relativo à pasta da API)
COPY client /app/client
EXPOSE 8080
ENTRYPOINT ["dotnet", "MundoPet.Api.dll"]
