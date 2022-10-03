#!/bin/sh
set -eux
docker compose run proxy-sidecar /bin/bash -c 'curl -XDELETE http://proxy:8474/proxies/rabbitmq-consumer/toxics/timeout_upstream; curl -XDELETE http://proxy:8474/proxies/rabbitmq-consumer/toxics/timeout_downstream'
