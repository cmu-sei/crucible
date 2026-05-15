# Crucible Docs Style Guide

This guide covers writing conventions for the Crucible documentation site. It applies to all contributors, including AI writing assistants.

For setup, workflow, and pull request instructions, see [CONTRIBUTING.md](CONTRIBUTING.md).

When in doubt, consult the *Chicago Manual of Style*. SEI Technical Communications guidance takes precedence where it differs.

## Voice and Tone

- Write directly and professionally. No filler phrases.
- Use short sentences. Rarely more than 20–25 words.
- Use active voice. Rewrite passive constructions where possible.
- Address the reader as "you."
- Do not use "please" in procedures.
- Lead with the action or subject. Avoid preambles.

**Example:**

> *Player* is the centralized interface where participants, teams, and administrators go to engage in a cyber event.

## Page Structure

Each core app guide follows this top-level structure:

```markdown
# AppName: Subtitle
## Overview
## Configuration
## Administrator Guide
## User Guide
## Glossary
```

**Overview** opens with a short paragraph defining what the app does, followed by a bullet list of capabilities, if relevant.

**Configuration** covers app-level setup that isn't performed in the UI. This includes Helm chart deployment settings, `ResourceOwnerAuthorization`, classification banner configuration, and other configuration settings as needed. **Permissions and Roles** is a subsection here.

**Administrator Guide** covers administrative tasks performed in an app. Most apps provide a dedicated Administration View; TopoMojo and Gameboard organize these tasks differently.

**User Guide** covers what participants and "regular" users do in the app.

**Glossary** appears at the end of longer guides. Format: **Term:** Definition.

## Page Title Format

```markdown
# ![AppName Logo](../assets/img/appname-icon.png){: style="height:75px;width:75px"} **AppName:** Subtitle
```

## Headings

- Use ATX style (`#` characters).
- All headings use title case.
- Surround every heading with exactly one blank line above and below.
- Do not end headings with punctuation.
- Do not skip heading levels (e.g., `##` to `####`).

## Procedures

- Use numbered lists for sequential steps.
- Use imperative mood: "Click **Save**." not "You should click **Save**."
- Add sub-bullets under a step for clarifying options, field descriptions, or notes, if needed.
- Screenshots follow the step they illustrate. Never precede it.
- Group related procedures under a shared heading.

**Example:**

```markdown
#### Add a New View

1. From the top menu, click **Administration**.
2. Click **Add New View**.
   - **Name:** Enter a display name for the view.
   - **Description:** (Optional) Enter a brief description.
3. Click **Save**.
```

## Screenshots

Place images after the step or paragraph they illustrate. Avoid lead-in sentences if possible, but use one when it helps clarify what the reader is looking at.

Alt text describes what the image shows:

```markdown
![Player Administration View with Views panel called out](img/player-admin-view.png)
```

- **Good:** `![Add New View dialog with Name and Description fields](img/add-view.png)`
- **Bad:** `![Screenshot showing how to add a view](img/add-view.png)`

Store images in the `img/` subdirectory alongside the guide's `index.md`.

## Formatting

| Element | Convention | Example |
| --- | --- | --- |
| UI element names | Bold | **Administration**, **Add New View**, **Save** |
| UI elements requiring user action | Bold | Select **File**, then **New File** |
| Defined terms (first use) | Italic | *gamespace*, *module*, *lab* |
| File names, paths, field values | Code | `variables.tf.json`, `true`, `ResourceOwnerAuthorization` |
| App names | Plain text, no special formatting | Player, Caster, TopoMojo |

For menu navigation, use commas instead of angle brackets: "Go to **File**, then **New File**" not "Go to **File** > **New File**."

## Lists

- Use `-` for unordered lists.
- Use numbered lists for procedures; bullet lists for non-sequential items.
- Use definition-style bullets for UI field descriptions: **Field Name:** Description.
- Use `!!! note` or sub-bullets (not inline parentheses) for field-level callouts.

## Admonitions

Use Material for MkDocs admonition syntax:

```markdown
!!! note "Title"
    Body text here.
```

| Type | When to use |
| --- | --- |
| `note` | Important information a reader might miss |
| `warning` | Action that could cause data loss or errors |
| `caution` | Action with side effects worth knowing |
| `tip` | Shortcut or efficiency improvement |
| `info` | Background context that aids understanding |
| `example` | Illustrative example of a configuration or concept |

Use admonitions sparingly. Only use them when a reader would genuinely miss the content inline.

## Scope Statement

Open Overview sections with what the tool does:

```markdown
**AppName** is the interface where...

- X happens during a thing
- Y manages this thing
```

## Glossaries

Place at the end of the guide. Use bold term followed by a colon and definition:

```markdown
**Definition:** The combination of a Player View, Caster Directory, and Steamfitter Scenario Template used to run an event.

**Gamespace:** A read-only, isolated copy of a workspace deployed for a participant.
```

## Terminology

Vale enforces most of these automatically. This table covers the most common cases.

### Context-Dependent Compound Words

| Adjective (before noun) | Noun | Verb |
| --- | --- | --- |
| third-party | third party | (n/a) |
| back-end | back end | (n/a) |
| front-end | front end | (n/a) |
| real-time | real time | (n/a) |
| login *(page, credentials)* | login | log in |
| setup *(guide, page)* | setup | set up |

### Preferred Spellings

| Write this | Not this |
| --- | --- |
| open source | `open-source` |
| Boolean | `boolean` |
| click | `click on` |
| allowlist / blocklist | `whitelist` / `blacklist` |
| email | `e-mail` |
| online / offline | `on-line` / `off-line` |
| website / webpage | `web site` / `web page` |
| log in to | `log into` |
| ad hoc | `ad-hoc` |
| antivirus | `anti-virus` |
| double-click | `double click` |
| endpoint | `end point` |
| filename | `file name` |
| homepage | `home page` |
| lifecycle | `life cycle` |
| dataset | `data set` |
| runtime | `run time` |
| timestamp | `time stamp` |
| zero trust | `zero-trust` |

For the full enforced list, see [`.github/styles/crucible/PreferredSpelling.yml`](.github/styles/crucible/PreferredSpelling.yml) and [`.github/styles/crucible/CompoundForms.yml`](.github/styles/crucible/CompoundForms.yml).

### Oxford Comma

Always use the Oxford comma in lists of three or more items:

> Alloy, Player, and Caster (correct) vs. Alloy, Player and Caster (incorrect).

## Linting

Two linters run on every pull request:

- **Vale**: prose style, preferred terms, passive voice, spelling
- **markdownlint-cli2**: markdown formatting

Run both locally before committing:

```bash
./lint-docs.sh
```

To add a Crucible-specific term that Vale flags incorrectly, add it to [`.github/styles/config/vocabularies/crucible/accept.txt`](.github/styles/config/vocabularies/crucible/accept.txt) using the `(?i)` prefix for case-insensitive matching.
