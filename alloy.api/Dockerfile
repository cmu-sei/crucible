#
#multi-stage target: dev
#
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS dev

ENV ASPNETCORE_URLS=http://0.0.0.0:4402 \
    ASPNETCORE_ENVIRONMENT=DEVELOPMENT

COPY . /app
WORKDIR /app/Alloy.Api

RUN dotnet publish -c Release -o /app/dist

CMD ["dotnet", "run"]

#
#multi-stage target: prod
#
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS prod
COPY --from=dev /app/dist /app
COPY --from=dev /app/entry.d /app/entry.d

WORKDIR /app
ENV ASPNETCORE_URLS=http://*:80
EXPOSE 80

CMD ["dotnet", "Alloy.Api.dll"]

RUN apt-get update && \
	apt-get install -y jq
