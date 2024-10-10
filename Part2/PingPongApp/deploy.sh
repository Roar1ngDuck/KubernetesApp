#!/bin/bash

docker build ./PingPongApp/ -t dwk-pingpongapp
docker image tag dwk-pingpongapp:latest roaringduck/dwk-pingpongapp:latest
docker image push roaringduck/dwk-pingpongapp:latest