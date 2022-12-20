#!/bin/sh
set -eux
curl -XDELETE http://localhost:8474/proxies/rabbitmq-consumer/toxics/bandwidth_upstream
curl -XDELETE http://localhost:8474/proxies/rabbitmq-consumer/toxics/bandwidth_downstream
