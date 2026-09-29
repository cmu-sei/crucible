# Learning Data (xAPI)

## Overview

Crucible applications emit Experience API (xAPI) statements that record what participants do during an exercise. Each statement describes an actor, a verb, and an object, and applications send statements to a Learning Record Store (LRS).

Use this data to answer questions that no single application can answer alone:

- Which participants opened a console, executed a task, or evaluated an incident.
- When each team reached a given move in the scenario.
- Which competencies an evaluator asserted, and on what evidence.

Six applications emit statements today: Blueprint, CITE, Gallery, Player, Player VM, and Steamfitter. For the verbs and triggers each one sends, see [Statement Reference](statements.md).

Moodle also sends statements through the `logstore_xapi` plugin when you integrate Crucible with Moodle. That path is configured in Moodle, not in the Crucible applications, and is not covered here.

## Actor Identity

Every application builds the xAPI actor the same way, from the Keycloak token of the signed-in user:

| Statement field | Value |
| --- | --- |
| `actor.account.homePage` | The `IssuerUrl` option. When unset, the application falls back to the `iss` claim in the token. |
| `actor.account.name` | The Keycloak `sub` claim. |
| `actor.name` | The user's display name. |

Because every application derives the account from the same Keycloak subject, statements from different applications resolve to the same learner in the LRS without further mapping.

Teams appear as an xAPI Group rather than an Agent. The group's `account.homePage` is the `UiUrl` option and its `account.name` is the team ID. Where an application sets `EmailDomain`, the group also carries an `mbox` built from the team short name and that domain.

## Configuration

Each application reads its own `XApiOptions` section. The options are consistent across applications, with minor differences noted below.

| Option | Purpose |
| --- | --- |
| `Enabled` | Turns statement emission on or off. |
| `Endpoint` | LRS endpoint that receives statements. |
| `Username` | LRS basic-auth key. |
| `Password` | LRS basic-auth secret. |
| `IssuerUrl` | Identity provider URL used as `actor.account.homePage`. |
| `ApiUrl` | Base URL used to build activity IRIs for API resources. |
| `UiUrl` | Base URL used to build activity IRIs for user-facing pages, and the team group's `account.homePage`. |
| `Platform` | Value written to `context.platform`. |
| `EmailDomain` | Domain used to build the team group's `mbox`. |
| `RetentionDays` | How long the application keeps processed statements in its local queue. |
| `ProcessingTimeoutMinutes` | When a statement stuck in processing is retried. |
| `ProcessingDelaySeconds` | Delay between queue processing passes. |

Differences to expect:

- Player API and Player VM API do not use `EmailDomain`.
- Blueprint does not use `ProcessingDelaySeconds`.
- Player VM API uses `PlayerApiUrl` to resolve the parent Player view, and does not use `UiUrl`.

Applications queue statements locally and send them with a background service, so an unreachable LRS delays delivery rather than failing the user's action.

## Current Limitations

Know these constraints before you build against this data:

- **Statements are not correlated across applications.** No shared exercise identifier appears in statements yet, so joining a participant's Blueprint, CITE, and Steamfitter activity requires matching on actor and time rather than on a single key.
- **Some verb IRIs do not resolve.** Player VM API uses verbs under `https://crucible.sei.cmu.edu/xapi/verbs/`, and Blueprint uses `https://w3id.org/xapi/tla/verbs/asserted`. Neither namespace serves a definition today. Treat the IRIs as stable identifiers, not as documentation you can retrieve.
- **No published xAPI profile.** Crucible does not yet publish a machine-readable profile that defines its verbs, activity types, and extensions.
- **Extension naming is inconsistent.** Steamfitter writes mixed-case extension names, such as `expectedOutput`, while Player VM API writes hyphenated names, such as `vm-name`. Both styles appear in the same LRS.
