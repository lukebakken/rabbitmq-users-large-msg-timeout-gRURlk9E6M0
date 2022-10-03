#!/bin/bash

# set -o xtrace
set -o nounset
set -o errexit
set -o pipefail

readonly arg1="${1:-undefined]}"

if [[ $arg1 == 'init' ]]
then
    # Note: POST is inferred
    curl -sd '{"name" : "rabbitmq-producer", "listen" : "proxy:55000", "upstream" : "rabbitmq:5672"}' http://proxy:8474/proxies
    curl -sd '{"name" : "rabbitmq-consumer", "listen" : "proxy:55001", "upstream" : "rabbitmq:5672"}' http://proxy:8474/proxies
else
    # We must be running a command like this:
    # docker-compose run proxy-sidecar curl -4vvvd '{"type" : "latency", "attributes" : {"latency" : 2000}}' http://proxy:8474/proxies/rabbitmq-producer/toxics
    "$@"
fi
