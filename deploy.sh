#!/bin/bash

docker build ./LogOutput/LogOutputWriter -t dwk-logoutput-writer
docker image tag dwk-logoutput-writer:latest roaringduck/dwk-logoutput-writer:latest
docker image push roaringduck/dwk-logoutput-writer:latest

docker build ./LogOutput/LogOutputReader -t dwk-logoutput-reader
docker image tag dwk-logoutput-reader:latest roaringduck/dwk-logoutput-reader:latest
docker image push roaringduck/dwk-logoutput-reader:latest

docker build ./TodoApp/TodoAppFrontend/ -t dwk-todoapp-frontend
docker image tag dwk-todoapp-frontend:latest roaringduck/dwk-todoapp-frontend:latest
docker image push roaringduck/dwk-todoapp-frontend:latest

docker build ./TodoApp/TodoAppBackend/ -t dwk-todoapp-backend
docker image tag dwk-todoapp-backend:latest roaringduck/dwk-todoapp-backend:latest
docker image push roaringduck/dwk-todoapp-backend:latest

docker build ./PingPongApp/ -t dwk-pingpongapp
docker image tag dwk-pingpongapp:latest roaringduck/dwk-pingpongapp:latest
docker image push roaringduck/dwk-pingpongapp:latest