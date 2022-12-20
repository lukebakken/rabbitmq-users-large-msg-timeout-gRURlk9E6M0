#!/bin/sh

set -eux

curl -d '{"type":"bandwidth","stream":"upstream","attributes":{"rate":512}}' http://localhost:8474/proxies/rabbitmq-consumer/toxics
curl -d '{"type":"bandwidth","stream":"downstream","attributes":{"rate":512}}' http://localhost:8474/proxies/rabbitmq-consumer/toxics
