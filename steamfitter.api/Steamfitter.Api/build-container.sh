#!/bin/bash

dotnet publish -c Release -o bin/publish

docker build . -t steamfitter/api --no-cache

docker-compose up -d