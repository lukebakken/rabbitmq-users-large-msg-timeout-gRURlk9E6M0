#!/bin/bash

# set -o xtrace
set -o nounset
set -o errexit
set -o pipefail

readonly arg1="${1:-undefined]}"

if [[ $arg1 == 'init' ]]
then
    # Note: POST is inferred
    curl -vd '{"name" : "rabbitmq-producer", "listen" : "proxy:55000", "upstream" : "rabbitmq:5672"}' http://proxy:8474/proxies
    curl -vd '{"name" : "rabbitmq-consumer", "listen" : "proxy:55001", "upstream" : "rabbitmq:5672"}' http://proxy:8474/proxies

    # Add bandwidth toxic for consumer right away
    curl -vd '{"type":"bandwidth","stream":"upstream","attributes":{"rate":512}}' http://proxy:8474/proxies/rabbitmq-consumer/toxics
    curl -vd '{"type":"bandwidth","stream":"downstream","attributes":{"rate":512}}' http://proxy:8474/proxies/rabbitmq-consumer/toxics
else
    # We must be running a command like this:
    # docker-compose run proxy-sidecar curl -4vvvd '{"type" : "latency", "attributes" : {"latency" : 2000}}' http://proxy:8474/proxies/rabbitmq-producer/toxics
    "$@"
fi
