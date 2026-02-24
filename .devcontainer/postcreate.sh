#!/bin/bash

# Show git dirty status in zsh prompt
git config devcontainers-theme.show-dirty 1

sudo chown -R $(whoami): /home/vscode/.claude

# oh-my-zsh plugins
sed -i 's/^\(\s*plugins=(.*\)\s*)/\1 python pyenv pip)/' $HOME/.zshrc
