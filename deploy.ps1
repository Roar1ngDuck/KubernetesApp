docker build .\LogOutput\ -t dwk-logoutput
docker image tag dwk-logoutput:latest roaringduck/dwk-logoutput:latest
docker image push roaringduck/dwk-logoutput:latest

docker build .\TodoApp\ -t dwk-todoapp
docker image tag dwk-todoapp:latest roaringduck/dwk-todoapp:latest
docker image push roaringduck/dwk-todoapp:latest

docker build .\PingPongApp\ -t dwk-pingpongapp
docker image tag dwk-pingpongapp:latest roaringduck/dwk-pingpongapp:latest
docker image push roaringduck/dwk-pingpongapp:latest