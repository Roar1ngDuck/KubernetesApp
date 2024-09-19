#!/bin/bash

docker build ./TodoApp/TodoAppFrontend/ -t dwk-todoapp-frontend
docker image tag dwk-todoapp-frontend:latest roaringduck/dwk-todoapp-frontend:latest
docker image push roaringduck/dwk-todoapp-frontend:latest

docker build ./TodoApp/TodoAppBackend/ -t dwk-todoapp-backend
docker image tag dwk-todoapp-backend:latest roaringduck/dwk-todoapp-backend:latest
docker image push roaringduck/dwk-todoapp-backend:latest