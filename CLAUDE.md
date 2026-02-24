# CLAUDE.md

## Project Overview

This is the documentation repository for Crucible, an open-source application framework developed by Carnegie Mellon University's Software Engineering Institute (SEI) for creating and managing virtual environments to support cybersecurity training, education, and exercises.

The documentation is built using MkDocs with the Material theme and deployed to GitHub Pages at <https://cmu-sei.github.io/crucible/>.

## Essential Commands

### Development

```bash
# Serve documentation locally with live reload (port 8000)
mkdocs serve

# Build static site to site/ directory
mkdocs build

# Deploy to GitHub Pages (main branch only, handled by CI)
mkdocs gh-deploy --force --clean --verbose
```

### Linting

```bash
# Run all linters (Vale + markdownlint-cli2)
./lint-docs.sh

# Run Vale prose linter only
vale . --config .vale.ini

# Run markdownlint only
markdownlint-cli2 "**/*.md" --config .markdownlint-cli2.yaml
```

## Documentation Architecture

### Content Organization

All documentation lives in the [docs/](docs/) directory with the following structure:

- **Core Application Guides** ([docs/alloy/](docs/alloy/), [docs/player/](docs/player/), [docs/caster/](docs/caster/), [docs/steamfitter/](docs/steamfitter/), [docs/topomojo/](docs/topomojo/), [docs/gameboard/](docs/gameboard/), [docs/blueprint/](docs/blueprint/), [docs/cite/](docs/cite/), [docs/gallery/](docs/gallery/)): Documentation for each Crucible application component
- **Role-Based Guides** ([docs/roles/](docs/roles/)): Organized by user role - Infrastructure Administrator, Range Builder, Instructor, and Participant
- **Tutorials** ([docs/tutorials/](docs/tutorials/)): Step-by-step guides for specific tasks
- **Getting Started** ([docs/getting-started/](docs/getting-started/)): Introduction and onboarding
- **Installation** ([docs/install/](docs/install/)): Installation and deployment guides
- **Integrations** ([docs/integrations/](docs/integrations/)): Third-party tool integrations

### Navigation

Site navigation is defined in [mkdocs.yml](mkdocs.yml) under the `nav:` key. When adding new pages, update this configuration to make them accessible in the site navigation.

### Assets

Store images, icons, and media files in [docs/assets/](docs/assets/). Use relative links when referencing assets from markdown files.

## Writing and Style Guidelines

### Vale Prose Linting

Vale enforces writing style and terminology consistency. Configuration:

- **Config file**: [.vale.ini](.vale.ini)
- **Custom styles**: [.github/styles/crucible/](.github/styles/crucible/)
- **Vocabulary**: [.github/styles/config/vocabularies/crucible/accept.txt](.github/styles/config/vocabularies/crucible/accept.txt)

Key Vale rules to follow:

- Use preferred terms (enforced in [PreferredTerms.yml](.github/styles/crucible/PreferredTerms.yml)):
  - "allowlist" not "whitelist"
  - "blocklist" not "blacklist"
  - "secondary" not "slave"
- Provide alt text for all images ([AltText.yml](.github/styles/crucible/AltText.yml))
- Use "click" not "click on" ([Click.yml](.github/styles/crucible/Click.yml))
- Avoid passive voice where possible ([Passive.yml](.github/styles/crucible/Passive.yml))
- Use smart quotes consistently ([SmartQuotes.yml](.github/styles/crucible/SmartQuotes.yml))

### Markdown Linting

Markdownlint-cli2 enforces markdown formatting standards. Configuration: [.markdownlint-cli2.yaml](.markdownlint-cli2.yaml)

Key markdown rules:

- Use ATX-style headings (`#` characters)
- Use dashes (`-`) for unordered lists
- Surround headings with exactly 1 blank line above/below
- Code blocks must specify a language (e.g., ` ```bash `)
- Files must end with a newline
- Use asterisks (`*`) for emphasis and bold
- Tables must have leading/trailing pipe characters
- No trailing whitespace

### Material for MkDocs Features

This site uses Material for MkDocs extensions:

- **Admonitions**: Use for callouts (e.g., `!!! note`, `!!! warning`, `!!! abstract`)
- **Code annotations**: Add numbered markers in code blocks
- **Emoji**: Enabled via `:emoji_name:` syntax
- **Syntax highlighting**: Specify language for all code blocks

Example admonition:

```markdown
!!! note "Important Information"
    This is a note that will be highlighted in a blue box.
```

## CI/CD Pipeline

### Automated Checks

GitHub Actions runs on all pull requests ([.github/workflows/lint.yml](.github/workflows/lint.yml)):

- Vale prose linting (fails on error)
- markdownlint-cli2 markdown linting

### Deployment

On pushes to `main` branch ([.github/workflows/deploy.yml](.github/workflows/deploy.yml)):

1. Creates a release tag with format `MM.DD.YYYY-N` (incrementing N for same-day releases)
2. Builds site with `mkdocs-material`
3. Deploys to GitHub Pages using `mkdocs gh-deploy`

## Development Workflow

### Adding New Documentation

1. Create markdown files in the appropriate [docs/](docs/) subdirectory
2. Update [mkdocs.yml](mkdocs.yml) navigation if adding new sections
3. Run `mkdocs serve` to preview changes locally
4. Run `./lint-docs.sh` to check for linting errors before committing
5. Fix any Vale or markdownlint errors
6. Create a pull request (linting runs automatically in CI)

### Modifying Existing Content

1. Edit markdown files in [docs/](docs/)
2. Preview with `mkdocs serve`
3. Lint with `./lint-docs.sh`
4. Commit and create pull request

### Working with Custom Terminology

If documenting new Crucible-specific terms that Vale flags incorrectly:

- Add accepted terms to [.github/styles/config/vocabularies/crucible/accept.txt](.github/styles/config/vocabularies/crucible/accept.txt)
- VSCode spell checker custom words are in [.vscode/settings.json](.vscode/settings.json)
