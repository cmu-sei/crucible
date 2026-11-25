# Roles in the Crucible Ecosystem

When we discuss "roles" in Crucible, we're discussing two concepts:

1. **Organizational roles:** That is, what a *person* does in their program (e.g., Infrastructure Administrator, Range Builder, Instructor, and Participant).
2. **Application roles:** Permission bundles applied to *users* in each application (e.g., Administrator, Content Developer, Observer, View Admin, View Member).

A *Permission* defines what action (or actions) a user can perform within a Crucible application. For example: an Alloy Content Developer can create an Event Template.

In the *Role-Based Guides* we provide helpful information for the following organizational roles within a typical Crucible ecosystem: Infrastructure Administrator, Range Builder, Instructor, and Participant.

## Organizational Roles

Your organization may have different names for similar roles. Each organization is unique.

1. **[Infrastructure Administrator](administrator/index.md):** Keeps Crucible platforms healthy, secure, and scalable; provisions environments, manages access, and monitors day-to-day operations.
2. **[Range Builder](range-builder/index.md):** Designs and delivers complete cyber ranges; combines infrastructure, scenarios, and learner-facing content into cohesive exercises.
3. **[Instructor](instructor/index.md):** Leads and executes Crucible exercises and training sessions; guides participants, evaluates performance, and ensures teams meet learning objectives.
4. **[Participant](participant/index.md):** Engages in exercises to build and demonstrate skills. (Sometimes called a *player* in Player, *competitor* in Gameboard.)

## Application Roles and Permissions

In the *Core Application Guides* we provide detailed information on each application's permissions and roles:

1. [Alloy Roles and Permissions](../alloy/index.md#permissions-and-roles)
2. [Player Roles and Permissions](../player/index.md#permissions-and-roles)
3. [Caster Roles and Permissions](../caster/index.md#permissions-and-roles)
4. [Steamfitter Roles and Permissions](../steamfitter/index.md#permissions-and-roles)
5. [Blueprint Roles and Permissions](../blueprint/index.md#permissions-and-roles)
6. [Gallery Roles and Permissions](../gallery/index.md#roles)
7. [CITE Roles and Permissions](../cite/index.md#permissions-and-roles)

The table below maps each Crucible organizational role to its Alloy, Player, Caster, and Steamfitter permissions. Use it to see what each person can do in each application.

| Role | Alloy | Player | Caster | Steamfitter |
| ------ | -------- | --------- | --------- | -------------- |
| **[Infrastructure Administrator](administrator/index.md)** | **[Administrator](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)**: Full control of Alloy configuration and integrations. | **[Administrator](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Full control of Player, including role delegation and notifications. | **[Administrator](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)**: Full control of Caster projects, modules, and system settings. | **[Administrator](https://github.com/cmu-sei/Steamfitter.Api/blob/development/Steamfitter.Api/docs/Permissions.md)**: Full control of Steamfitter scenarios, tasks, and system settings. |
| **[Range Builder](range-builder/index.md)** | **[Content Developer](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)**: Create and manage event templates (requires Player integration to preview views). | **[View Admin](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Build and maintain views, including toggling inactive states.<br>**[Content Developer](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Create views with Terraform and manage ISO/file upload toggles. | **[Content Developer](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)**: Create and manage projects and membership for projects they author. | **[Content Developer](https://github.com/cmu-sei/Steamfitter.Api/blob/development/Steamfitter.Api/docs/Permissions.md)**: Create and manage scenarios and templates they own. |
| **[Instructor](instructor/index.md)** | **[Observer](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)**: View and execute events assigned to their classes. | **[View Member](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Access Player views assigned to their cohorts, with ability to revert snapshots on VMs when enabled. | **[Observer](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)**: Review projects tied to their events; no edits or workspace runs. | **[Observer](https://github.com/cmu-sei/Steamfitter.Api/blob/development/Steamfitter.Api/docs/Permissions.md)**: Review scenarios assigned to their events; no edits or task runs. |

Blueprint, Gallery, and CITE grant access at two levels. A *system role* applies across the whole application. A role on a single MSEL, exhibit, evaluation, or team applies only to that item. Blueprint can assign the item-level roles in Gallery and CITE when it pushes a MSEL. The table below maps each organizational role to the roles it typically holds in Blueprint, Gallery, and CITE.

| Role | Blueprint | Gallery | CITE |
| ------ | --------- | ------- | ---- |
| **[Infrastructure Administrator](administrator/index.md)** | **[Administrator](https://github.com/cmu-sei/Blueprint.Api/blob/main/Blueprint.Api/docs/Permissions.md)** system role: All permissions, including users, roles, groups, and the shared templates in the Administration View. | **[Administrator](https://github.com/cmu-sei/Gallery.Api/blob/main/Gallery.Api/docs/Permissions.md)** system role: All permissions, including users, roles, and groups. | **[Administrator](https://github.com/cmu-sei/CITE.Api/blob/main/Cite.Api/docs/Permissions.md)** system role: All permissions, including users, roles, groups, and team types. |
| **[Range Builder](range-builder/index.md)** | **[Content Developer](https://github.com/cmu-sei/Blueprint.Api/blob/main/Blueprint.Api/docs/Permissions.md)** system role: Create MSELs, and view, edit, and push integrations for any MSEL. | **[Content Developer](https://github.com/cmu-sei/Gallery.Api/blob/main/Gallery.Api/docs/Permissions.md)** system role: Create collections and exhibits, view all collections and exhibits, and manage and advance any exhibit. | **[Content Developer](https://github.com/cmu-sei/CITE.Api/blob/main/Cite.Api/docs/Permissions.md)** system role: Create, view, edit, and manage any scoring model or evaluation, and advance moves and observe teams in any evaluation. |
| **[Instructor](instructor/index.md)** | **[Owner](../blueprint/index.md#contributors)** MSEL role: Push the MSEL's integrations and manage its contributors.<br>**[Evaluator](../blueprint/index.md#assessor-view)** MSEL role: Rate teams against the MSEL's competencies in the Assessor View. | **[Manager](../gallery/index.md#exhibit-roles)** exhibit role: Full access to the exhibit, including advancing it.<br>**[Observer](../gallery/index.md#observers)**: Monitor all teams during the exercise. | **Facilitator** evaluation role: Observe all teams and advance moves.<br>**Advancer** evaluation role: Advance moves without observing teams.<br>**Official Scorer** team role: View and enter the official score.<br>**Observer** evaluation role: Read-only access to all teams up to the current move. |
| **[Participant](participant/index.md)** | Membership on a MSEL team: View the MSEL, for example in the [Exercise View](../blueprint/index.md#exercise-view). | Membership on an exhibit team: Read the cards and articles shared with that team. | **[Contributor](../blueprint/index.md#cite-team-roles)** team role: Answer for the team.<br>**[Submitter](../blueprint/index.md#cite-team-roles)** team role: Answer for and submit the team's assessment. |

!!! abstract "SEI Digital Library"

       For more background, the SEI has a long history of building cyber ranges and training, exercise, and challenge environments. Some of our relevant research publications include:

       - :material-book: [Challenge Development Guidelines for Cybersecurity Competitions](https://www.sei.cmu.edu/library/challenge-development-guidelines-for-cybersecurity-competitions/)
       - :material-book: [Foundation of Cyber Ranges](https://www.sei.cmu.edu/library/foundation-of-cyber-ranges/)
       - :material-book: [R-EACTR: A Framework for Designing Realistic Cyber Warfare Exercises](https://www.sei.cmu.edu/library/r-eactr-a-framework-for-designing-realistic-cyber-warfare-exercises/)
       - :material-book: [The CERT Approach to Cybersecurity Workforce Development](https://www.sei.cmu.edu/library/the-cert-approach-to-cybersecurity-workforce-development/)
