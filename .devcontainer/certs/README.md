# Custom Certificates

Place PEM-encoded root CA certificates in this directory with a `.crt` extension before rebuilding the dev container image. The dev container copies `.crt` files into the container and trusts them automatically via `update-ca-certificates`.

If you rely on Zscaler (or another SSL inspection solution), copy the issued root certificate into this folder as `*.crt`, rebuild, and the container will trust outbound TLS through that proxy.
