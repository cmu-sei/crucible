#!/bin/bash

docker build . -t steamfitter/web --no-cache
docker-compose up -d
