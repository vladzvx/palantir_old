FROM postgres:13.3 as build
ENV DEBIAN_FRONTEND=noninteractive


RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y git

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
      build-essential postgresql-server-dev-13
RUN apt install systemtap-sdt-dev
RUN git clone https://github.com/postgrespro/rum

RUN cd rum
WORKDIR /rum/
RUN make USE_PGXS=1 install
RUN make USE_PGXS=1 DESTDIR=/tmp/build install

FROM postgres:13.3 AS runtime
COPY --from=build /tmp/build/ /

COPY NotificationDB.sql ./docker-entrypoint-initdb.d
