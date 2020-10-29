FROM docker.io/library/mono:6.8 as builder

ADD . /

RUN nuget install -o packages
RUN msbuild -p:Configuration=Release

FROM docker.io/library/mono:6.8

COPY --from=builder /bin/Release/ /

ENTRYPOINT [ "mono", "NeoDoc.exe" ]