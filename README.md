https://groups.google.com/g/rabbitmq-users/c/gRURlk9E6M0

## List consumer toxics

```
docker compose run proxy-sidecar curl -4vvv http://proxy:8474/proxies/rabbitmq-consumer/toxics
```

## Create consumer timeout toxic (both directions)

```
./add-consumer-toxic.sh
```

## Remove consumer timeout toxic (both directions)

```
./delete-consumer-toxic.sh
```
