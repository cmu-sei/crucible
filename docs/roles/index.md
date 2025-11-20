# Roles in the Crucible Ecosystem

When we discuss "roles" in Crucible, we're discussing two concepts:

1. **Organizational roles:** That is, what a *person* does in their program (e.g., Infrastructure Administrator, Range Builder, Instructor, and Participant).
2. **Application roles:** Permission bundles applied to *users* in each application (e.g., Administrator, Content Developer, Observer, View Admin, View Member).

A *Permission* defines what action (or actions) a user can perform within a Crucible application. For example: an Alloy Content Developer can create an Event Template.

In the *Role-Based Guides* we provide detailed information for the following organizational roles within a typical Crucible ecosystem:

1. Infrastructure Administrator
2. Range Builder
3. Instructor
4. Participant

## Organizational Roles

### Infrastructure Administrator

[Infrastructure Administrator](administrator/index.md): Keeps Crucible platforms healthy, secure, and scalable; provisions environments, manages access, and monitors day-to-day operations.

### Range Builder

[Range Builder](range-builder/index.md): Designs and delivers complete cyber ranges; combines infrastructure, scenarios, and learner-facing content into cohesive exercises.

### Instructor

[Instructor](instructor/index.md): Leads and executes Crucible exercises and training sessions; guides participants, evaluates performance, and ensures teams meet learning objectives.

### Participant

[Participant](participant/index.md): Engages in exercises to build and demonstrate skills. (Sometimes called a *player* in Player, *competitor* in Gameboard.)

## Application Roles and Permissions

In the *Core Application Guides* we provide detailed information on each application's permissions and roles:

1. [Alloy Roles and Permissions](../alloy/index.md#permissions-and-roles)
2. [Player Roles and Permissions](../player/index.md#permissions-and-roles)
3. [Caster Roles and Permissions](../caster/index.md#permissions-and-roles)
4. [Steamfitter Roles and Permissions](../steamfitter/index.md#permissions-and-roles)

The table below maps each Crucible organizational role to its Alloy, Player, Caster, and Steamfitter permissions. Use it to see what each person can do in each application.

| Role | Alloy | Player | Caster | Steamfitter |
| ------ | -------- | --------- | --------- | -------------- |
| **[Infrastructure Administrator](administrator/index.md)** | **[Administrator](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)**: Full control of Alloy configuration and integrations. | **[Administrator](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Full control of Player, including role delegation and notifications. | **[Administrator](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)**: Full control of Caster projects, modules, and system settings. | **[Administrator](https://github.com/cmu-sei/Steamfitter.Api/blob/development/Steamfitter.Api/docs/Permissions.md)**: Full control of Steamfitter scenarios, tasks, and system settings. |
| **[Range Builder](range-builder/index.md)** | **[Content Developer](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)**: Create and manage event templates (requires Player integration to preview views). | **[View Admin](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Build and maintain views, including toggling inactive states.<br>**[Content Developer](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Create views with Terraform and manage ISO/file upload toggles. | **[Content Developer](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)**: Create and manage projects and membership for projects they author. | **[Content Developer](https://github.com/cmu-sei/Steamfitter.Api/blob/development/Steamfitter.Api/docs/Permissions.md)**: Create and manage scenarios and templates they own. |
| **[Instructor](instructor/index.md)** | **[Observer](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)**: View and execute events assigned to their classes. | **[View Member](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)**: Access Player views assigned to their cohorts, with ability to revert snapshots on VMs when enabled. | **[Observer](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)**: Review projects tied to their events; no edits or workspace runs. | **[Observer](https://github.com/cmu-sei/Steamfitter.Api/blob/development/Steamfitter.Api/docs/Permissions.md)**: Review scenarios assigned to their events; no edits or task runs. |

??? tip "Deep Dive into CERT Research"

      For more background, CERT has a long history of building cyber ranges and training/exercise/challenge environments. Some of our relevant research publications include:

      - :material-book: *[Challenge Development Guidelines for Cybersecurity Competitions](https://www.sei.cmu.edu/library/challenge-development-guidelines-for-cybersecurity-competitions/)*
      - :material-book: *[Foundation of Cyber Ranges](https://www.sei.cmu.edu/library/foundation-of-cyber-ranges/)*
      - :material-book: *[R-EACTR: A Framework for Designing Realistic Cyber Warfare Exercises](https://www.sei.cmu.edu/library/r-eactr-a-framework-for-designing-realistic-cyber-warfare-exercises/)*
      - :material-book: *[The CERT Approach to Cybersecurity Workforce Development](https://www.sei.cmu.edu/library/the-cert-approach-to-cybersecurity-workforce-development/)*
