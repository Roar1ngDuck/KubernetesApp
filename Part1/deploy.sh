#!/bin/bash

docker build ./LogOutput/LogOutputWriter -t dwk-logoutput-writer
docker image tag dwk-logoutput-writer:latest roaringduck/dwk-logoutput-writer:latest
docker image push roaringduck/dwk-logoutput-writer:latest

docker build ./LogOutput/LogOutputReader -t dwk-logoutput-reader
docker image tag dwk-logoutput-reader:latest roaringduck/dwk-logoutput-reader:latest
docker image push roaringduck/dwk-logoutput-reader:latest

docker build ./TodoApp/ -t dwk-todoapp
docker image tag dwk-todoapp:latest roaringduck/dwk-todoapp:latest
docker image push roaringduck/dwk-todoapp:latest

docker build ./PingPongApp/ -t dwk-pingpongapp
docker image tag dwk-pingpongapp:latest roaringduck/dwk-pingpongapp:latest
docker image push roaringduck/dwk-pingpongapp:latest