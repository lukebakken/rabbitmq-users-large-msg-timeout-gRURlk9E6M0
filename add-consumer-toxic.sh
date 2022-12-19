#!/bin/sh

set -eux

docker compose run proxy-sidecar /bin/bash -c 'curl -d '\''{"type":"bandwidth","stream":"upstream","attributes":{"rate":512}}'\'' proxy:8474/proxies/rabbitmq-consumer/toxics && curl -d '\''{"type":"bandwidth","stream":"downstream","attributes":{"rate":512}}'\'' http://proxy:8474/proxies/rabbitmq-consumer/toxics'
