FROM alpine:latest
RUN apk update && apk upgrade 
RUN apk add curl bash
COPY --chmod=0755 ./docker-entrypoint.sh /usr/local/bin
ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
CMD ["init"]
