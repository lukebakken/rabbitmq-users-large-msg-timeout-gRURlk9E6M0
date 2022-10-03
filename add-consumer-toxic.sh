#!/bin/sh

set -eux

docker compose run proxy-sidecar /bin/bash -c 'curl -d '\''{"type":"timeout","stream":"upstream","attributes":{"timeout":1000}}'\'' proxy:8474/proxies/rabbitmq-consumer/toxics && curl -d '\''{"type":"timeout","stream":"downstream","attributes":{"timeout":1000}}'\'' http://proxy:8474/proxies/rabbitmq-consumer/toxics'
