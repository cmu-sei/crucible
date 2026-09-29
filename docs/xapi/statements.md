# xAPI Statement Reference

This page lists the xAPI statements each Crucible application emits, the verb Internationalized Resource Identifier (IRI) it uses, and the action that triggers it. For the actor model and Learning Record Store (LRS) configuration, see [Learning Data (xAPI)](index.md).

Verbs come from published vocabularies where one fits: Advanced Distributed Learning (ADL) (`adlnet.gov`), TinCan (`id.tincanapi.com`), Activity Streams (`activitystrea.ms`), and Department of Defense Instructional Systems Design (ISD) (`w3id.org/xapi/dod-isd`). Player VM API uses Crucible-specific verbs where no published verb matches.

## Blueprint

Blueprint describes a MSEL as an ADL simulation activity (`http://adlnet.gov/expapi/activities/simulation`). Statements carry the MSEL ID as the xAPI registration.

| Statement | Verb IRI | Emitted When |
| --- | --- | --- |
| MSEL Viewed | `http://id.tincanapi.com/verb/viewed` | You open a MSEL. |
| Exercise Started | `http://adlnet.gov/expapi/verbs/launched` | A MSEL owner starts the exercise. |
| Exercise Stopped | `http://adlnet.gov/expapi/verbs/terminated` | A MSEL owner stops the exercise. |
| Join Page Viewed | `http://id.tincanapi.com/verb/viewed` | You open the join page. The object is an Activity Streams page. |
| MSEL Joined | `http://activitystrea.ms/schema/1.0/join` | You join a MSEL, through an invitation or directly. |
| Competency Asserted | `https://w3id.org/xapi/tla/verbs/asserted` | An Evaluator records a competency assessment. |
| Checkbox Changed | `https://w3id.org/xapi/dod-isd/verbs/selected` or `.../reset` | You select or clear a checkbox data field on a scenario event. `selected` when checked, `reset` when cleared. |

Competency assertions carry more than the verb. Each one includes `result.score` with raw, minimum, maximum, and scaled values, `result.completion`, a competency identifier extension, a confidence extension, and the competency framework IRI in `context.contextActivities.grouping`. The framework IRI distinguishes competencies that share an identifier across frameworks.

Blueprint also reads statements back out of the LRS to populate the Assessor View. It queries by registration, using the MSEL ID together with the integration IDs for the linked Player view, Gallery exhibit, CITE evaluation, and Steamfitter scenario.

## CITE

CITE statements carry the Evaluation ID as the registration. Submissions, scoring categories, and moves appear in `context.contextActivities`.

| Statement | Verb IRI | Emitted When |
| --- | --- | --- |
| Evaluation Dashboard Viewed | `http://id.tincanapi.com/verb/viewed` | You open the dashboard for an evaluation. |
| Evaluation Scoresheet Viewed | `http://id.tincanapi.com/verb/viewed` | You open the scoresheet for an evaluation. |
| Evaluation Dashboard Observed | `https://w3id.org/xapi/dod-isd/verbs/observed` | An observer opens another team's dashboard. |
| Evaluation Scoresheet Observed | `https://w3id.org/xapi/dod-isd/verbs/observed` | An observer opens another team's scoresheet. |
| Submission Created | `https://w3id.org/xapi/dod-isd/verbs/initiated` | A submission is created for a team, move, or role. |
| Submission Edited | `https://w3id.org/xapi/dod-isd/verbs/edited` | You update a submission that is not yet complete. |
| Submission Submitted | `https://w3id.org/xapi/dod-isd/verbs/submitted` | You update a submission whose status becomes Complete. |
| Option Selected | `https://w3id.org/xapi/dod-isd/verbs/selected` | You select a scoring option. |
| Option Cleared | `https://w3id.org/xapi/dod-isd/verbs/reset` | You clear a scoring option. |
| Submission Evaluated | `https://w3id.org/xapi/dod-isd/verbs/evaluated` | The submission score is recalculated after a selection changes. |
| Selections Cleared | `https://w3id.org/xapi/dod-isd/verbs/reset` | You clear all selections on a submission. |
| Selections Preset | `https://w3id.org/xapi/dod-isd/verbs/initialized` | You preset selections on a submission from another submission. |
| Comment Added | `https://w3id.org/xapi/dod-isd/verbs/stated` | You add a comment to a submission. The comment text appears in `result.response`. |
| Comment Edited | `https://w3id.org/xapi/dod-isd/verbs/edited` | You edit a submission comment. |
| Comment Deleted | `https://w3id.org/xapi/dod-isd/verbs/deleted` | You delete a submission comment. |
| Action Completed | `https://w3id.org/xapi/dod-isd/verbs/completed` | You check a team action on the dashboard. |
| Action Reset | `https://w3id.org/xapi/dod-isd/verbs/reset` | You clear a team action on the dashboard. |
| Duty Assigned | `https://w3id.org/xapi/dod-isd/verbs/assigned` | A user is added to a role. |
| Duty Removed | `https://w3id.org/xapi/dod-isd/verbs/removed` | A user is removed from a role. |

A team's scoring selections represent that team's assessment of the incident. They are not a grade on the participant.

Observer statements deliberately omit team context. An observer viewing another team's data is not a member of that team, so CITE suppresses the team group on `observed` statements.

## Gallery

Gallery statements carry the Exhibit ID as the registration. Exhibits, collections, moves, and injects appear in `context.contextActivities`.

| Statement | Verb IRI | Emitted When |
| --- | --- | --- |
| Article Viewed | `http://id.tincanapi.com/verb/viewed` | You open an article. |
| Article Previewed | `http://id.tincanapi.com/verb/previewed` | You preview an article without opening it. |
| Card Viewed | `http://id.tincanapi.com/verb/viewed` | You open a card. |
| Exhibit Archive Viewed | `http://id.tincanapi.com/verb/viewed` | You open the archive for an exhibit. |
| Exhibit Wall Viewed | `http://id.tincanapi.com/verb/viewed` | You open the wall for an exhibit. |
| Exhibit Archive Observed | `https://w3id.org/xapi/dod-isd/verbs/observed` | An observer opens another team's archive. |
| Exhibit Wall Observed | `https://w3id.org/xapi/dod-isd/verbs/observed` | An observer opens another team's wall. |
| Article Created | `https://w3id.org/xapi/dod-isd/verbs/created` | A content developer creates an article. |
| Article Edited | `https://w3id.org/xapi/dod-isd/verbs/edited` | A content developer updates an article. |
| Article Deleted | `https://w3id.org/xapi/dod-isd/verbs/deleted` | A content developer deletes an article. |
| Article Shared | `https://w3id.org/xapi/dod-isd/verbs/shared` | You share an article with other users or teams. |
| Article Marked Read | `https://w3id.org/xapi/dod-isd/verbs/read` | You mark an article read. |
| Article Marked Unread | `http://id.tincanapi.com/verb/marked-unread` | You mark an article unread. |

As in CITE, Gallery suppresses team context on `observed` statements.

## Player

Player statements carry the View ID as the registration and describe a view as an ADL simulation activity.

| Statement | Verb IRI | Emitted When |
| --- | --- | --- |
| View Viewed | `http://id.tincanapi.com/verb/viewed` | You open a view. |
| Application Accessed | `http://activitystrea.ms/schema/1.0/access` | You switch to an application within a view. |
| Team Joined | `http://adlnet.gov/expapi/verbs/attended` | You are added to a team in a view. |
| View Terminated | `http://adlnet.gov/expapi/verbs/terminated` | You leave a view. The statement includes `result.duration`. |
| Team Switched | `https://w3id.org/xapi/verbs/switched` | You change your primary team within a view. |

## Player VM

Player VM API records hands-on actions against virtual machines. Its statements parent to the Player view, so VM activity resolves to the view a participant was working in.

These verbs use the `https://crucible.sei.cmu.edu/xapi/verbs/` namespace because no published vocabulary covers VM console and power operations.

| Statement | Verb IRI | Emitted When |
| --- | --- | --- |
| Console Opened | `.../verbs/console-opened` | You open a VM console. |
| Console Closed | `.../verbs/console-closed` | You close a VM console. |
| Power On | `.../verbs/power-on` | You power on a VM. |
| Power Off | `.../verbs/power-off` | You power off a VM. |
| Shutdown | `.../verbs/shutdown` | You request a guest shutdown. |
| Reboot | `.../verbs/reboot` | You request a guest reboot. |
| Snapshot Reverted | `.../verbs/snapshot-reverted` | You revert a VM to its snapshot. |
| ISO Mounted | `.../verbs/iso-mounted` | You mount an ISO on a VM. |
| ISO Uploaded | `.../verbs/iso-uploaded` | You upload an ISO to a view. |
| ISO Deleted | `.../verbs/iso-deleted` | You delete an uploaded ISO. |
| Network Changed | `.../verbs/network-changed` | You change a VM network adapter. |
| Follow Started | `.../verbs/followed` | You start following another user's console. |
| Follow Stopped | `.../verbs/unfollowed` | You stop following another user's console. |

VM statements carry these extensions on the statement object's activity definition, under `https://crucible.sei.cmu.edu/xapi/extensions/`: `vm-id`, `vm-name`, `vm-type`, `vm-ip-addresses`, `team-ids`, `active-team-ids`, `iso-scope`, `followed-user-id`, and `followed-team-id`. Power statements also carry a `power-operation` extension, and network statements carry the adapter and network.

## Steamfitter

Steamfitter statements carry the Scenario ID as the registration.

| Statement | Verb IRI | Emitted When |
| --- | --- | --- |
| Scenario Started | `http://adlnet.gov/expapi/verbs/launched` | A scenario starts. |
| Scenario Ended | `http://adlnet.gov/expapi/verbs/terminated` | A scenario ends. |
| Task Executed | `http://adlnet.gov/expapi/verbs/executed` | A task finishes running against its target virtual machines. |

Task statements are the richest performance evidence Crucible produces. Each one includes `result.completion`, `result.success`, and `result.score` with raw, minimum, maximum, and scaled values, taken from the score the task earned against the score available. Activity extensions record the task action, VM mask, API URL, and expected output.

Steamfitter derives move and group context from the task name when it follows the `<move>-<group> name` convention.

Tasks that run in the background have no signed-in user. Steamfitter attributes those statements to the scenario's creator, so planner-attributed statements can appear alongside participant activity.
