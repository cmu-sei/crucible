# CLAUDE.md

## Project Overview

This is the documentation repository for Crucible. Crucible is an open source application framework developed by Carnegie Mellon University's Software Engineering Institute (SEI). It supports cybersecurity training, education, and exercises by creating and managing virtual environments.

The documentation site uses MkDocs with the Material theme and deploys to GitHub Pages at <https://cmu-sei.github.io/crucible/>.

## Style Guide

**IMPORTANT:** All documentation writing must follow the conventions in [style-guide.md](style-guide.md). Read it before drafting or editing content.

The style guide covers voice and tone, page structure, headings, procedures, screenshots, formatting, lists, admonitions, glossaries, terminology, and the Oxford comma. Vale and markdownlint enforce many of these rules automatically, but the style guide is the source of truth for anything they cannot catch.

Key conventions to keep in mind while writing:

- Address the reader as "you." Use active voice and short sentences.
- Do not use "please" in procedures. Lead with the action.
- Use title case for all headings. Do not end headings with punctuation.
- Bold UI element names. Italicize defined terms on first use. Use code formatting for filenames, paths, and field values.
- Always use the Oxford comma.
- Place screenshots after the step they illustrate, with descriptive alt text.

When you write or edit prose in this repo, also apply these rules to any code comments, commit messages, and pull request descriptions you produce.

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
- **Getting Started** ([docs/getting-started/](docs/getting-started/)): Introduction and onboarding guides
- **Installation** ([docs/install/](docs/install/)): Installation and deployment guides
- **Integrations** ([docs/integrations/](docs/integrations/)): Third-party tool integrations

### Navigation

The `nav:` key in [mkdocs.yml](mkdocs.yml) defines site navigation. When adding new pages, update this configuration to make them accessible in the site navigation.

### Assets

Store images, icons, and media files in [docs/assets/](docs/assets/). Use relative links when referencing assets from markdown files.

## Linting and Tooling

Vale and markdownlint enforce style and formatting rules. All edits made should ensure that all linting checks pass. Fix linting errors that you introduce by editing the text, rather than defaulting to editing a linting rule. Only edit linting rules if absolutely necessary - explain any linting rule edits (why did the rule have to change instead of the edited text).

### Vale Prose Linting

Vale enforces the documented writing style and terminology rules.

- **Config file:** [.vale.ini](.vale.ini)
- **Custom styles:** [.github/styles/crucible/](.github/styles/crucible/)
- **Vocabulary:** [.github/styles/config/vocabularies/crucible/accept.txt](.github/styles/config/vocabularies/crucible/accept.txt)

To add a Crucible-specific term that Vale flags incorrectly, add it to the vocabulary file using the `(?i)` prefix for case-insensitive matching.

### Markdown Linting

markdownlint-cli2 enforces markdown formatting standards. Configuration: [.markdownlint-cli2.yaml](.markdownlint-cli2.yaml).

### Material for MkDocs Features

This site uses Material for MkDocs extensions:

- **Admonitions:** Call-outs such as `!!! note`, `!!! warning`, and `!!! abstract`.
- **Code annotations:** Numbered markers inside code blocks.
- **Emoji:** Enabled via `:emoji_name:` syntax.
- **Syntax highlighting:** Specify the language on every code block.

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

1. Create markdown files in the appropriate [docs/](docs/) subdirectory.
2. Update the `nav:` key in [mkdocs.yml](mkdocs.yml) if you add a new section.
3. Run `mkdocs serve` to preview changes locally.
4. Run `./lint-docs.sh` to check for linting errors before committing.
5. Fix any Vale or markdownlint errors.
6. Create a pull request. Linting runs automatically in CI.

### Modifying Existing Content

1. Edit the relevant markdown files in [docs/](docs/).
2. Preview with `mkdocs serve`.
3. Lint with `./lint-docs.sh`.
4. Commit and create a pull request.

### Working with Custom Terminology

If you document new Crucible-specific terms that Vale flags incorrectly:

- Add accepted terms to [.github/styles/config/vocabularies/crucible/accept.txt](.github/styles/config/vocabularies/crucible/accept.txt).
- Add VS Code spell checker custom words to [.vscode/settings.json](.vscode/settings.json).
