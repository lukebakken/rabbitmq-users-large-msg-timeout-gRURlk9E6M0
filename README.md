https://groups.google.com/g/rabbitmq-users/c/gRURlk9E6M0

## List consumer toxics

```
docker compose run proxy-sidecar curl -4vvv http://proxy:8474/proxies/rabbitmq-consumer/toxics
```

## Create producer bandwidth toxic (both directions)

```
./add-producer-toxic.sh
```

## Remove producer bandwidth toxic (both directions)

```
./delete-producer-toxic.sh
```
