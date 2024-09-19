#!/bin/bash

docker build ./LogOutput/LogOutputWriter -t dwk-logoutput-writer
docker image tag dwk-logoutput-writer:latest roaringduck/dwk-logoutput-writer:latest
docker image push roaringduck/dwk-logoutput-writer:latest

docker build ./LogOutput/LogOutputReader -t dwk-logoutput-reader
docker image tag dwk-logoutput-reader:latest roaringduck/dwk-logoutput-reader:latest
docker image push roaringduck/dwk-logoutput-reader:latest