FROM python:3.11-alpine3.21
ENV architecture=amd64
RUN apk update
RUN apk add curl gpg gpg-agent

#Download the desired package(s)
RUN curl -O https://download.microsoft.com/download/fae28b9a-d880-42fd-9b98-d779f0fdd77f/msodbcsql18_18.5.1.1-1_$architecture.apk
RUN curl -O https://download.microsoft.com/download/7/6/d/76de322a-d860-4894-9945-f0cc5d6a45f8/mssql-tools18_18.4.1.1-1_$architecture.apk

#(Optional) Verify signature, if 'gpg' is missing install it using 'apk add gnupg':
RUN curl -O https://download.microsoft.com/download/fae28b9a-d880-42fd-9b98-d779f0fdd77f/msodbcsql18_18.5.1.1-1_$architecture.sig
RUN curl -O https://download.microsoft.com/download/7/6/d/76de322a-d860-4894-9945-f0cc5d6a45f8/mssql-tools18_18.4.1.1-1_$architecture.sig

# RUN curl https://packages.microsoft.com/keys/microsoft.asc  | gpg --import -
# RUN gpg --verify msodbcsql18_18.5.1.1-1_$architecture.sig msodbcsql18_18.5.1.1-1_$architecture.apk
# RUN gpg --verify mssql-tools18_18.4.1.1-1_$architecture.sig mssql-tools18_18.4.1.1-1_$architecture.apk

#Install the package(s)
RUN apk add --allow-untrusted msodbcsql18_18.5.1.1-1_$architecture.apk
RUN apk add --allow-untrusted mssql-tools18_18.4.1.1-1_$architecture.apk

RUN apk add --no-cache libpq gcc git cmake && \
    apk add --no-cache --virtual .build-deps build-base python3-dev musl-dev linux-headers && \
    apk add --no-cache --virtual .run-deps libpq && \
    apk add --no-cache --virtual .msodbcdeps unixodbc unixodbc-dev && \
    apk add --no-cache --virtual .mssqltoolsdeps libgcc libstdc++ && \
    apk cache clean

WORKDIR /code

RUN python3 -m pip install --no-cache-dir --upgrade pip
RUN python3 -m pip install --no-cache-dir --upgrade setuptools==78.1.1

RUN git clone --no-checkout https://github.com/apache/arrow.git /arrow \
    && cd /arrow \
    && git checkout tags/apache-arrow-19.0.1 \
    && cd cpp \
    && mkdir build \
    && cd build \
    && cmake -DARROW_CSV=ON -DARROW_JSON=ON -DARROW_FILESYSTEM=ON .. \
    && make -j$(nproc) \
    && make install

RUN python3 -m pip install --no-cache-dir --upgrade torch --index-url https://download.pytorch.org/whl/cpu 
RUN python3 -m pip install --no-cache-dir --upgrade pyarrow==19.0.1 
