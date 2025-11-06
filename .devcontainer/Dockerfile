FROM mcr.microsoft.com/devcontainers/python:3.14

# Custom Cert Support
COPY .devcontainer/certs /usr/local/share/ca-certificates/custom/
RUN find /usr/local/share/ca-certificates/custom -type f ! -name '*.crt' -delete \
    && if find /usr/local/share/ca-certificates/custom -type f -name '*.crt' -print -quit | grep -q .; then \
    update-ca-certificates; \
    fi

ENV REQUESTS_CA_BUNDLE=/etc/ssl/certs/ca-certificates.crt \
    PIP_CERT=/etc/ssl/certs/ca-certificates.crt \
    NODE_EXTRA_CA_CERTS=/etc/ssl/certs/ca-certificates.crt

# Install prerequisites
RUN apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    build-essential \
    curl \
    git \
    npm \
    libcairo2-dev \
    libfreetype6-dev \
    libffi-dev \
    libjpeg-dev \
    libpng-dev \
    libz-dev&& \
    rm -rf /var/lib/apt/lists/*

# Install Vale
ARG VALE_VERSION=3.13.0
RUN set -euo pipefail; \
    arch="$(uname -m)"; \
    case "$arch" in \
      x86_64) VALE_ARCH=64-bit ;; \
      aarch64|arm64) VALE_ARCH=ARM64 ;; \
      *) echo "Unsupported arch: $arch" && exit 1 ;; \
    esac; \
    curl -fsSL "https://github.com/errata-ai/vale/releases/download/v${VALE_VERSION}/vale_${VALE_VERSION}_Linux_${VALE_ARCH}.tar.gz" \
      -o /tmp/vale.tgz && \
    tar -xzf /tmp/vale.tgz -C /tmp && \
    install -m 0755 /tmp/vale /usr/local/bin/vale && \
    rm -rf /tmp/vale /tmp/vale.tgz && \
    vale --version

# Install markdownlint-cli2
RUN npm config -g set fund false && npm install -g markdownlint-cli2@0.18.1

WORKDIR /workspace
COPY ../requirements.txt requirements.txt
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install --upgrade pip && \
    pip install --no-input -r requirements.txt

# Install linting alias
RUN echo "\n# Custom alias for lint-docs.sh\nalias lint='bash /workspaces/crucible-docs/lint-docs.sh'" >> /etc/zsh/zshrc
