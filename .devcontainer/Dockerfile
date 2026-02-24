# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/devcontainers/python:3.14

ARG TARGETARCH
ARG DEBIAN_FRONTEND=noninteractive

# Configure ZSH History
RUN mkdir -p "/home/vscode/.history" && touch "/home/vscode/.history/zsh_history" && \
    chown -R vscode:vscode /home/vscode/.history && \
    cat >> /home/vscode/.zshrc <<'EOF'
# Persistent zsh history for devcontainer
# Use a dedicated history directory (backed by a Docker volume)
mkdir -p "/home/vscode/.history"
export HISTFILE="/home/vscode/.history/zsh_history"
export HISTSIZE=50000      # in-memory commands
export SAVEHIST=100000     # on-disk commands
# History behavior options
setopt APPEND_HISTORY          # append to history file, don't overwrite
setopt INC_APPEND_HISTORY      # write each command as it's entered
setopt SHARE_HISTORY           # share history across sessions
setopt HIST_IGNORE_DUPS        # ignore immediate duplicates
setopt HIST_IGNORE_ALL_DUPS    # drop older duplicates
setopt HIST_IGNORE_SPACE       # ignore commands starting with a space
setopt HIST_REDUCE_BLANKS      # trim extra spaces
setopt HIST_VERIFY             # don't immediately execute recalled commands
# DEVCONTAINER ZSH HISTORY END
EOF

# Custom Cert Support
COPY .devcontainer/certs /usr/local/share/ca-certificates/custom/
RUN find /usr/local/share/ca-certificates/custom -type f ! -name '*.crt' -delete \
    && if find /usr/local/share/ca-certificates/custom -type f -name '*.crt' -print -quit | grep -q .; then \
    update-ca-certificates; \
    fi

ENV REQUESTS_CA_BUNDLE=/etc/ssl/certs/ca-certificates.crt \
    PIP_CERT=/etc/ssl/certs/ca-certificates.crt \
    NODE_EXTRA_CA_CERTS=/etc/ssl/certs/ca-certificates.crt

# Add new Yarn GPG key
RUN mkdir -p /etc/apt/keyrings && \
    curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | gpg --dearmor -o /etc/apt/keyrings/yarn.gpg && \
    echo "deb [signed-by=/etc/apt/keyrings/yarn.gpg] https://dl.yarnpkg.com/debian/ stable main" > /etc/apt/sources.list.d/yarn.list

# Install prerequisites
RUN --mount=type=cache,target=/var/lib/apt/lists,sharing=locked \
    --mount=type=cache,target=/var/cache/apt,sharing=locked \
    apt-get update && \
    apt-get install -y --no-install-recommends \
    build-essential \
    npm \
    libcairo2-dev \
    libfreetype6-dev \
    libffi-dev \
    libjpeg-dev \
    libpng-dev \
    libz-dev && \
    rm -rf /var/lib/apt/lists/*

# Install Vale
ARG VALE_VERSION=3.13.1
RUN set -eux; \
    case "${TARGETARCH}" in \
    amd64)  VALE_ARCH="64-bit" ;; \
    arm64)  VALE_ARCH="arm64" ;; \
    *)      echo "Unsupported architecture: ${TARGETARCH}" >&2; exit 1 ;; \
    esac && \
    curl -fsSL -o /tmp/vale.tgz \
    "https://github.com/errata-ai/vale/releases/download/v${VALE_VERSION}/vale_${VALE_VERSION}_Linux_${VALE_ARCH}.tar.gz" \
    && tar -xzf /tmp/vale.tgz -C /usr/local/bin vale \
    && rm /tmp/vale.tgz \
    && chmod +x /usr/local/bin/vale \
    && vale --version

# Install markdownlint-cli2
RUN --mount=type=cache,target=/root/.npm \
    npm config -g set fund false && \
    npm install -g markdownlint-cli2@0.21.0

WORKDIR /tmp
COPY ../requirements.txt requirements.txt
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install --upgrade pip && \
    pip install --no-input -r requirements.txt

# Install linting alias
RUN echo "\n# Custom alias for lint-docs.sh\nalias lint='bash /workspaces/crucible-docs/lint-docs.sh'" >> /etc/zsh/zshrc

# Switch back to vscode user
USER vscode

# Claude install
RUN curl -fsSL https://claude.ai/install.sh | bash
