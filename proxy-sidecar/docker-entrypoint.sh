#!/bin/bash

set -o xtrace
set -o errexit
set -o nounset
set -o pipefail

# Note: POST is inferred
curl -4vvvd '{"name" : "rabbitmq", "listen" : "proxy:55672", "upstream" : "rabbitmq:5672"}' http://proxy:8474/proxies
