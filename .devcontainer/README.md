# Crucible Docs Dev Container

This [dev container](https://containers.dev/) makes it easy to edit and test changes to the [Crucible documentation](https://cmu-sei.github.io/crucible/).

For more information about dev containers, read the [documentation for using dev containers with VSCode](https://code.visualstudio.com/docs/devcontainers/containers), the documentation for the [VSCode Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers), or the [dev containers homepage](https://containers.dev/)

## Key Features

1. All requirements for editing these docs are already installed including (but not limited to):
   1. [Python](https://www.python.org/)
   2. [MkDocs](https://www.mkdocs.org/)
   3. [Vale](https://vale.sh/)
   4. [markdownlint-cli2](https://github.com/DavidAnson/markdownlint-cli2)
2. Helpful VSCode extensions installed
3. VSCode default settings configured to be helpful by allowing Vale and markdownlint-cli2 make updates to markdown files to automatically resolve a subset of linting errors that may arise

## Using the Container to Edit Documentation

### To Build/Serve the MkDocs Site

```zsh
mkdocs serve
```

Serving the documentation in this way will automatically forward a port from the container to your host so you can visit the site from your web browser. The contents of the site will automatically update with changes to files.

### To Run Markdown File Linting Locally

```zsh
lint
```

The above `lint` command is an alias to the `/workspaces/crucible-docs/lint-docs.sh` script.

Linting markdown files locally is helpful to understand what types of errors the repository's CI job might flag.

The installed VSCode plugins for markdownlint and Vale will also highlight errors in the code editor.
