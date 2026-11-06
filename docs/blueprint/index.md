# ![Blueprint Logo](../assets/blueprint-logo.png) Blueprint: Building a MSEL

## Overview

[**Blueprint**](#glossary) is a web app that helps users create a [Master Scenario Event List (MSEL)](#glossary) and [events](#glossary) more easily. It lets users choose and define simulated entities, attacks, time frames, and regulators affected by the scenario. Users can view, edit, create, and approve events in the MSEL using Blueprint.

Blueprint makes it easier to build and view the MSEL compared to using an Excel spreadsheet. It also helps teams collaborate, even if they are from different teams. Blueprint can connect with Player, Gallery, CITE, and Steamfitter to help automate exercise setup.

For installation, refer to these GitHub repositories.

- [Blueprint UI Repository](https://github.com/cmu-sei/Blueprint.Ui)
- [Blueprint API Repository](https://github.com/cmu-sei/Blueprint.Api)

## Configuration

Configure and deploy Blueprint using the [Blueprint Helm Chart](https://github.com/cmu-sei/helm-charts/tree/main/charts/blueprint). The Helm Chart README provides detailed instructions for all deployment settings.

### Classification Banner

Blueprint UI supports an optional, customizable classification banner that displays persistently at the top of the application. The banner can show classification labels (such as "UNCLASSIFIED" or "SECRET"), maintenance messages, or any other persistent notification. Configure the banner through `HeaderBarSettings` in the Helm chart. See the [Classification Banner](https://github.com/cmu-sei/helm-charts/tree/main/charts/blueprint#classification-banner) section of the Blueprint Helm Chart README for configuration details.

### AI Drafting

Blueprint can draft a MSEL, its Gallery articles, and its team and organization profiles with a model on Amazon Bedrock. AI drafting is off by default. Turn it on in the `Ai` settings of the Blueprint API:

- **`Ai:Enabled`:** `true` turns on every AI feature.
- **`Ai:Region`:** the Amazon Web Services (AWS) region of the Bedrock endpoint, such as `us-east-1`.
- **`Ai:ModelId`:** the Bedrock model or inference profile ID that writes the drafts.
- **`Ai:FallbackModelId`:** a second model to try once when the first declines a request. Leave it empty for no retry.
- **`Ai:MaxTokens`:** the longest answer the model may write. The default is `64000`.
- **`Ai:TimeoutSeconds`:** how long a request may run. The default is `600`.
- **`Ai:MaxInjects`:** the most injects a draft may have. The default is `60`.
- **`Ai:MaxCompetencies`:** the most competencies a draft may choose from. The default is `2500`.
- **`Ai:ImageModelId`:** an optional Bedrock image model that makes news photos for Gallery articles, such as `stability.stable-image-core-v1:1`. Leave it empty for no photos.
- **`Ai:ImageRegion`:** the AWS region of the image model. The default is `us-west-2`, because Bedrock offers the image models in fewer regions than the text models.
- **`Ai:Prices`:** what each model costs, keyed by model ID, so Blueprint can estimate what building each MSEL cost. For each model, set `InputPerMillion`, `OutputPerMillion`, `CacheReadPerMillion`, and `CacheWritePerMillion` in US dollars per million tokens, and `PerImage` for an image model. Blueprint counts a model without a price in tokens only. As an environment variable, write the model ID with underscores for its dots and hyphens, such as `Ai__Prices__us_anthropic_claude_sonnet_5_5__InputPerMillion`.
- **`Ai:Effort`:** how hard the model thinks before it answers: `low`, `medium`, `high`, `xhigh`, or `max`. Leave it empty for the model's own default. Lower is faster and spends fewer tokens.
- **`Ai:Strict`:** `true`, the default, constrains each answer to its schema on Bedrock, so an answer cannot come back as markup or broken JSON.
- **`Ai:Parallel`:** how many of a drafted MSEL's rows, articles, and profiles the browser writes at once. The default is `6`.
- **`Ai:Models`:** a provider, model, and effort for each AI task, where it differs from the defaults above. The tasks are `design`, `injects`, `rows`, `article`, `profile`, `value` (Write with AI), `page` (Write a page with AI), `metadata` (the catalog metadata at the finish), `assist` (Ask AI), and `draft`. Each task takes `Provider`, `ModelId`, `FallbackModelId`, `Region`, `BaseUrl`, `ApiKey`, `MaxTokens`, `Effort`, and `Strict`. Blueprint ships `injects` at `medium` effort and `rows`, `article`, and `profile` at `low`, because the outline specifies them closely.

The provider is `Bedrock`, the default, or `OpenAI`, which is any endpoint that offers the OpenAI chat completions API: the OpenAI API, or a server such as vLLM or Ollama that runs a local model. For `OpenAI`, set `BaseUrl`, such as `https://api.openai.com/v1`, `ApiKey`, and `ModelId`. A local model usually needs a smaller `MaxTokens`, such as `16000`. For example, to write the Gallery articles with a cheaper Bedrock model and the profiles with a local one:

```json
"Models": {
  "article": { "ModelId": "us.anthropic.claude-haiku-5-5" },
  "profile": { "Provider": "OpenAI", "BaseUrl": "https://models.example.com/v1", "ApiKey": "...", "ModelId": "gpt-oss-120b", "MaxTokens": 16000 }
}
```

The API uses the default AWS credential chain of its host. Caster uses the same `Ai` settings for its AI features.

#### Audio and Video

**Write with AI** can also voice an inject: read its article aloud, or make a voicemail, an intercepted call, a broadcast clip, or a news reel. These options appear only when the API has a speech provider in `Ai:Speech`:

- **`Ai:Speech:Provider`:** `Polly` for Amazon Polly, or `OpenAI` for any server that offers the OpenAI `/v1/audio/speech` API, such as a local text-to-speech server on a network with no internet. Leave it empty, the default, for no audio.
- **`Ai:Speech:Region`:** Polly's AWS region. The default is `us-east-1`. Polly uses the same default AWS credential chain as Bedrock, and those credentials need the `polly:SynthesizeSpeech` permission.
- **`Ai:Speech:Engine`:** Polly's engine. The default is `neural`, the only engine with the newscaster style that broadcast and reel anchors read in.
- **`Ai:Speech:BaseUrl`**, **`Ai:Speech:ApiKey`**, and **`Ai:Speech:Model`:** for `OpenAI`, the endpoint, such as `https://api.openai.com/v1`, its key, and the speech model, such as `tts-1`. This API returns no timings, so Blueprint estimates when each sentence and word starts from its length.
- **`Ai:Speech:Voices`:** the voices a clip may use, each with a `Name`, a `Gender`, a `Label` such as `British English`, and `News` when it reads in the newscaster style. Leave it empty for Polly's English neural voices or OpenAI's six voices.
- **`Ai:Speech:MaxCharacters`:** the most characters one clip may read aloud. The default is `6000`, about seven minutes.

The text model writes the voicemail, call, broadcast, and reel scripts, so they also need `Ai:Enabled`. A reel without a photo of its own gets one from `Ai:ImageModelId` when it's set. Price what the speech provider voices in `Ai:Prices` with `PerMillionCharacters`, keyed `polly-neural` for Polly's neural voices or by the OpenAI model ID. For example:

```json
"Speech": { "Provider": "Polly", "Region": "us-east-1" },
"Prices": { "polly-neural": { "PerMillionCharacters": 16 } }
```

!!! note

    The AI never changes a MSEL on its own. You review everything it writes, and nothing is saved until you confirm it.

## Permissions and Roles

To use Blueprint, a System Admin must assign Content Developer permissions to the user and add them to a team.

There are three levels of permissions in Blueprint that affect the way a user interacts with the Blueprint application and collaborates on the MSEL creation.

- [System Admin](#glossary): Can add users to a team, as well as assign the required permissions to users. Additionally, users with this permission can view, edit, create, and approve events on the MSEL.
- [Content Developer](#glossary): Can view, edit, create, and approve events on the MSEL.
- [Facilitator](#glossary): Manages the exercise, can advance moves, execute events, and check events as completed.

Most users will have the Content Developer permission, because it is the minimum required permission to create and collaborate with other teams on MSEL creation.

Refer to the [Administrator Guide](#administrator-guide) for more information on additional administrative actions.

## Administrator Guide

Blueprint administrators use the Administration View to manage users and units, and to configure organization, Gallery card, CITE action, and CITE role templates for content developers building MSELs.

### Administration View

Across the Crucible exercise applications, the **Administration View** is where privileged users configure the platform and control access. It includes user and team management, role and permission assignment, and setup and maintenance of app-specific templates and content. The Administration View is where admins prepare and manage the environment so events run smoothly for participants.

Accessing the Administration View is the same in all Crucible exercise applications: expand the dropdown next to your username in the top-right corner and select **Administration**.

![The Administration dropdown in the top right-corner](img/crucible-administration.png)

#### Units

The Units page lists all the units in Blueprint. A unit is an organizational entity, such as a team, department, or agency, that groups users for exercise purposes. Administrators can add, edit, and delete units, and assign users to their respective units.

![Blueprint Units administration page showing several units with edit and delete icons](img/bp-admin-units-page.png)

##### Add a Unit

1. Expand the dropdown next to your username in the top-right corner and select **Administration**.
2. Under **Units**, click **+** (**Add Unit**).
3. In **Name**, enter the unit name (for example, `Carnegie Mellon University`).
4. In **Short Name**, enter a short name or acronym (for example, `CMU`).
5. Click **Save**.

##### Edit, Delete, and Search for a Unit

- **Edit:** Click **Edit** (pencil icon) next to the unit, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the unit.
- **Search:** In the **Search** field, enter the name of the unit.

##### Add/Remove Users from a Unit

To add a user to a unit:

1. Click the unit row to expand its configuration.
2. Under **All Users**, find the user and click **Add User**.

To remove a user from a unit, under **Unit Users**, find the user and click **Remove**.

![Blueprint Units page with unit expanded showing Users and Unit Members panels](img/bp-add-remove-users.png)

#### Data Fields

The Data Fields page lists all data field templates available for use in MSEL scenario events. Data fields define the columns that appear on a MSEL - similar to column headers in a spreadsheet. The **Info** and **Facil** checkboxes control whether the field displays for information events and facilitation events, respectively. Content developers can pull these templates into a MSEL from the **Data Fields** tab.

##### Add a Data Field Template

1. Under **Data Fields**, click **+** (**Add a template data field**).
2. In **Name**, enter the field name (for example, `Delivery Method`).
3. In **Data Type**, select the data type. Available types: Boolean, Card, Checkbox, Competency, Datetime, Double, HTML, Integer, IpAddress, Move, Network, Organization, SourceType, Status, String, Team, TeamsMultiple, Url, User. See [Data Value Checks](#data-value-checks) for the values IpAddress, Network, and Datetime fields accept.
4. Check **Use Option List** to provide a predefined list of selectable values for the field (for example, `Low, Medium, High`).
5. Click **Save**.

##### Edit, Delete, and Search for a Data Field Template

- **Edit:** Click **Edit** (pencil icon) next to the field, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the field.
- **Search:** In the **Search** field, enter the name of the data field template.

#### Inject Types

The Inject Types page lists all inject type templates available for use in MSELs. An inject type defines the structure and data fields for a category of scenario events.

##### Add an Inject Type

1. Under **Inject Types**, click **+** (**Add Inject Type**).
2. In **Name**, enter the inject type name (for example, `Email`).
3. In **Description**, enter a brief description (for example, `Email-based scenario events`).
4. Click **Save**.

##### Edit, Delete, and Search for an Inject Type

- **Edit:** Click **Edit** (pencil icon) next to the inject type, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the inject type.
- **Search:** In the **Search** field, enter the name of the inject type.

#### Catalogs

The Catalogs page lists all catalog templates available for use in MSELs. A catalog is a reusable collection of scenario events associated with a specific inject type.

##### Add a Catalog

1. Under **Catalogs**, click **+** (**Add Catalog**).
2. In **Name**, enter the catalog name.
3. In **Description**, enter a brief description.
4. In **Inject Type**, select the inject type for this catalog.
5. Check **Is a Public Catalog** to make the catalog available to all users.
6. Under **Data Fields to Show on Inject List**, check the fields to display.
7. Click **Save**.

##### Edit, Delete, and Search for a Catalog

- **Edit:** Click **Edit** (pencil icon) next to the catalog, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the catalog.
- **Search:** In the **Search** field, enter the name of the catalog.

#### Organizations

The Organizations page lists all organization templates available for use in MSELs. An organization template defines a reusable entity  -  such as a government agency, company, or media outlet  -  that can be added to MSEL events. Content developers can pull these templates into a MSEL from the **Organizations** tab.

##### Add an Organization Template

1. Under **Organizations**, click **+** (**Add Organization Template**).
2. In **Long Name**, enter the full organization name (for example, `Cybersecurity and Infrastructure Security Agency`).
3. In **Short Name**, enter a short name or acronym (for example, `CISA`).
4. In **Summary**, enter a brief description (for example, `Security agency`).
5. In **Email**, enter the organization's email address (for example, `john@cisa.gov`).
6. In **Description**, enter the full organization description.
7. Click **Save**.

##### Edit, Delete, and Search for an Organization Template

- **Edit:** Click **Edit** (pencil icon) next to the organization template, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the organization template.
- **Search:** In the **Search** field, enter the name of the organization template.

#### Gallery Cards

The Gallery Cards page lists all Gallery card templates available for use in MSELs. A Gallery card template defines a reusable card category that can be pushed to Gallery during an exercise. Content developers can pull these templates into a MSEL from the **Gallery Cards** tab.

##### Add a Gallery Card Template

1. Under **Gallery Cards**, click **+** (**Add Gallery Card Template**).
2. In **Name**, enter the card name (for example, `Information Technology Sector`).
3. In **Card Description**, enter a brief description (for example, `Status of the Information Technology Sector`).
4. Click **Save**.

##### Edit, Delete, and Search for a Gallery Card Template

- **Edit:** Click **Edit** (pencil icon) next to the card template, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the card template.
- **Search:** In the **Search** field, enter the name of the card template.

#### CITE Actions

The CITE Actions page lists all CITE action templates available for use in MSELs. A CITE action template defines a reusable task that teams complete during an exercise. Content developers can pull these templates into a MSEL from the **CITE Actions** tab.

##### Add a CITE Action Template

1. Under **CITE Actions**, click **+** (**Add CITE Action Template**).
2. In **Description of the Action**, enter the action description (for example, `Score the incident`).
3. Click **Save**.

##### Edit, Delete, and Search for a CITE Action Template

- **Edit:** Click **Edit** (pencil icon) next to the action template, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the action template.
- **Search:** In the **Search** field, enter the name of the action template.

#### CITE Duties

The CITE Duties page lists all CITE duty templates available for use in MSELs. A CITE duty template defines a reusable role or responsibility assigned to team members during an exercise. Content developers can pull these templates into a MSEL from the [CITE Roles](#cite-roles) tab.

##### Add a CITE Duty Template

1. Under **CITE Duties**, click **+** (**Add CITE Duty Template**).
2. In **Name**, enter the duty name (for example, `Reviewer`).
3. Click **Save**.

##### Edit, Delete, and Search for a CITE Duty Template

- **Edit:** Click **Edit** (pencil icon) next to the duty template, make the desired changes, and click **Save**.
- **Delete:** Click **Delete** (trash can icon) next to the duty template.
- **Search:** In the **Search** field, enter the name of the duty template.

#### Users

The following image shows the Users Administration Page. Here, administrators can add and delete users. Additionally, administrators can assign the necessary permissions to each user.

The available permissions are:

- **System Admin:** Can use all administration privileges on the Blueprint application.
- **Content Developer:** Can view, edit, create, and approve events on the MSEL.

![Blueprint Users Admin OE](img/blueprintUsersAdmin-v4.png)

##### Add a User

If the exercise administrator has granted the appropriate permissions, follow these steps to add a user.

![Blueprint Add User OE](img/blueprintAddUser-v3.png)

1. Under the Users Administration View, click **+**.
2. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field         | Data Type | Description            | Example                              |
| ------------- | --------- | ---------------------- | ------------------------------------ |
| **User ID**   | GUID      | Unique ID for the user | 9dd4e3d8-5098-4b0a-9216-697cda5553f8 |
| **User Name** | String    | User name identifier   | user-2                               |

Click the **Save** icon (a user with a **+** sign), then select the permissions you want to assign by checking the boxes next to the user's name.

##### Delete a User

To delete a user, follow these steps:

1. Click the **Gear** icon in the top-right corner.
2. Under the Users Administration View, select the user you want to delete and click the **Trash Can** next to the user.

##### Search for a User

To search for a specific user, follow these steps:

1. Click the **Gear** icon in the top-right corner.
2. Under the Users Administration View, click the **Search Bar** and type the name of the desired user.

#### Roles

<!-- TODO: Document Roles administration page -->

#### Groups

<!-- TODO: Document Groups administration page -->

## User Guide

Content developers use Blueprint to build and manage MSELs.

### Event Dashboard

The **Event Dashboard** is Blueprint's homepage. It shows a card for each thing the current user can do: **Join**, **Launch**, **Build**, and **Assess**. Each card shows how many events it applies to. While data loads, an **Initializing Data** card is displayed with a progress spinner.

Below the mission cards, three cards describe what Blueprint does: **Competency-Based Assessment**, **Collaborative Planning**, and **Hands-On and Tabletop Exercises**. Click one to read more.

#### Join an Event

The **Join** card (*Active Exercises*) counts the in-progress events the user can join. Click **Join Event** to go to `/join`, which shows a card for each of those events with its name and description. A name or description too long for the card ends in an ellipsis; hover over it, or tab to it, to see it in full. Click **Join** on a card to open the event in Player. **Join** is a link, so it can also be opened in a new tab. When there is nothing to join, the card shows **No active events available to join** and cannot be clicked.

#### Start an Event

If the user can launch any template MSELs, the **Launch** card (*Initiate New Missions*) is shown. Click **Start Event** to go to `/launch` and choose a template to launch. Launching the event creates its teams in a new Player view and proceeds to Player.

#### Manage an Event

If the user holds the `CreateMsels` permission or is a contributor on any MSEL, the **Build** card (*Design & Planning*) is shown. Click **Manage Events** to go to `/build`, where the user can create, edit, delete, and open MSELs.

#### Assess an Event

If the user is a System Admin, or an Owner or Evaluator on any MSEL (a MSEL's creator counts as its Owner), the **Assess** card (*Evaluation & Scoring*) is shown, with the number of those MSELs that are running (`Deployed`). Click **Assess Events** to go to `/assess`, which shows a card for each running event the user can assess, with its name, description, and number of competencies. Click **Assess** on a card to open that event's [Assessor View](#assessor-view). An event with no competencies says **No competencies to assess** but still opens, because its Assessor View still shows the xAPI evidence and **Checkbox** data fields. When there is nothing to assess, the page shows **No running events available to assess**.

If the user can do none of these, a single **No Access** card is shown instead.

### Player Manage Event Page

!!! note

    Most invitation management now happens directly from the MSEL's [Invitations](#invitations) tab in Blueprint. The Player Manage Event application described below is still used by participants after a launched event is running, but MSEL owners typically create and manage invitations in Blueprint before launch.

When a user starts an event (as described above), Blueprint adds a **Manage Event** application to the left side panel of the Player view. The Manage Event application lets the user change the event end time, end the event, and invite others to join.

![Blueprint End Event OE](img/blueprintEndEvent-v2.png)

#### Launched Event User Invitations

To invite other users to the launched event, follow these steps:

![Blueprint Launched Event Invites OE](img/blueprintEventInvitations.png)

1. Navigate to the **Manage Event** application from Player's side panel.
2. Click **Invite Others**.
3. Fill the fields as necessary following the Data Format Table specifications.

#### Data Format Table

| Field                                                      | Data Type     | Description                                       | Example                              |
| ---------------------------------------------------------- | ------------- | ------------------------------------------------- | ------------------------------------ |
| **Team**                                                   | Dropdown Text | User's team                                       | SEI - Software Engineering Institute |
| **Email Domain**                                           | String        | User's email domain tied to the invitation        | @sei.cmu.edu                         |
| **Expiration Date/Time**                                   | Datetime      | Date/Time when the invitation should expire       | 04/20/2024 11:17:25                  |
| **Maximum Number of Users Allowed to Use this Invitation** | Integer       | Number of users allowed to use the invitation     | 5                                    |
| **Can Invite Others to this team**                         | Boolean       | Allows new users to invite others to the exercise | False                                |

After you've added all desired configurations, click **Save**.

#### Ending a Launched Event

To end an event, follow these steps:

1. Navigate to the **Manage Event** application from Player's side panel.
2. Click **End Event**.
3. On the confirmation screen, click **Yes** to successfully end the event.

### MSEL Catalog

The MSEL Catalog shows created MSELs. Users can select a MSEL to work on, create a new one, and delete an existing one.

![Blueprint MSEL Catalog](img/blueprintDashboard-v4.png)

#### Create or Upload a MSEL

Two actions are available in the MSEL list table header:

- **Add blank MSEL** (**+** icon): creates a new MSEL with default settings. The new MSEL appears with status `Pending` and a description such as "Created from Default Settings by [username]".
- **Upload a new MSEL from a file** (upload icon): choose **Upload xlsx file** or **Upload JSON file**, then select the file to create a new MSEL from its contents. A `.json` file is one exported with **Download JSON file** from a MSEL's row actions. Blueprint refuses a file that isn't valid JSON or isn't a Blueprint MSEL, and creates nothing.

##### JSON Import Report

After a JSON upload, Blueprint shows a report for the imported MSEL:

- **Imported:** the number of **Moves**, **Scenario Events**, **Cards**, **Teams**, **Data Fields**, and **Pages** created.
- **Changed:** what Blueprint adjusted to fit this Blueprint, such as a new name when a MSEL with the file's name already exists, a CITE scoring model or team type linked by name, team members that could not be imported, and which integrations are on.
- **Failed:** steps that could not be completed, such as checking the scoring model in CITE, with what to do before pushing.

Click **Open MSEL** to open the new MSEL, or **Close** to stay on the MSEL list.

#### Draft a MSEL with AI

When AI drafting is on, you can draft a whole MSEL from a description of the exercise. The draft follows an exercise design workflow: the scenario first, then the training objectives, then the moves and the injects that drive them. You need the `CreateMsels` permission.

Drafting has two steps. First the model writes the design, with a few questions for you. You review and change the design, answer the questions, and click **Create MSEL**. Blueprint creates the MSEL at once and then writes the injects, Gallery articles, and profiles into it, step by step, while you watch on the MSEL's page.

1. In the MSEL list table header, click **Draft MSEL with AI**.
2. In **Template**, choose the template to draft from. The draft uses the template's columns, teams, organizations, Gallery cards, and integrations. With several templates, **Crucible MSEL** is chosen to start with.
3. Optionally, in **Competency frameworks**, choose the frameworks whose competencies the draft may assign. With none chosen, the draft uses the template's own competencies.
4. In **Exercise description**, describe the audience, the scenario, the objectives, the organizations to simulate, and the competencies to exercise.
5. Optionally, expand **Design choices (optional)** and answer the questions that change an exercise most. The draft decides any you leave open and says what it chose.
    - **Format:** a discussion-based tabletop, a functional exercise with hands-on work in the range, or a hybrid.
    - **CITE assessment:** how CITE assesses the teams. **Let the model choose**, the default, leaves it to the design step, which picks the one that fits the scenario and shows it on the **Design** tab. See [CITE Assessment Choices](#cite-assessment-choices). This appears only when the template uses CITE.
    - **Critical infrastructure sectors:** the CISA sectors the exercise covers, from the 16. With none chosen, the draft decides and says what it chose.
    - **Gallery cards:** how the articles are grouped on the Gallery wall: by source or thread, by organization, by critical infrastructure sector, by country, or by network segment and key cyber terrain. This appears only when the template uses Gallery.
    - **Number of moves**, **Length (minutes)**, and **Scenario time covered:** the pacing. The scenario time covered is how much story time the exercise spans, such as `three days`.
    - **Participant structure:** how you organize participants, such as one combined staff or parallel national teams.
    - **Article marking:** the banner on the MSEL's Gallery articles and pages. See [Article Markings](#article-markings). This appears only when the template uses Gallery.
    - **Add Team Profiles and Organization Profiles pages:** clear this to skip the profile pages.
6. Click **Draft**. Writing the design takes a minute or two. The progress stays at the bottom of the dialog, showing what the model has written so far and how long it has run. Click **Stop** to discard the design; your description and choices stay, but what the model wrote before you stopped it is still billed. If the model's answer cannot be read as a design with moves and teams, the dialog says so and asks you to draft again; the answer differs each time.
7. Review the design in its tabs:
    - **Questions:** up to five questions whose answers change the exercise, each with a suggested answer and why it matters. Change any answer you disagree with; click **Use it** to put a suggestion back. The injects are written to your answers.
    - **Design:** the name, description, problem, premise, escalation ceiling, the critical infrastructure sectors the scenario touches, and the objectives as task, condition, and standard. The **Check before creating** list shows anything Blueprint corrected or the model assumed.
    - **Moves:** each move's start on the exercise clock, in minutes, its scenario time, title, and purpose. Change a start and the moves are put in order and numbered again. Click **Add move** for another, half an hour after the last and a day later on the scenario clock, or the **X** to remove one.
    - **Teams and organizations:** each one's short name, name, and description. Each organization also has labelled **Sector** (a CISA critical infrastructure sector, or none for a news outlet or the adversary), **Size** (**Small** under 100 people, **Medium** to 999, **Large** to 9,999, **Enterprise** 10,000 or more), **Industry** (what it does within its sector, such as a regional wastewater utility), and **Country**, each with a tooltip; they keep its staff, systems, and reach in scale across the articles. Each team shows its **Competencies** as chips, which you can remove or add from the chosen frameworks, and for a CITE template its **CITE duties** as chips, which members pick from in CITE; type a duty and press Enter to add one. Click the **X** to remove a team or organization.
    - **Gallery cards:** the cards on the Gallery wall. Click **Add card** for another.

    On the **Design** tab, click **Add objective** for another objective, and on **Teams and organizations**, **Add team** or **Add organization**. What you add is written for like the rest: the injects, articles, and profiles are written to the design as you leave it.
8. Click **Create MSEL**. Click **Edit description** to change the description and draft again, or **Discard** to close without saving.

The dialog closes and the MSEL's page opens at once, with **Creating the MSEL** in a banner at the top while Blueprint creates it from the template; if that fails, the banner says why and offers **Try again** with the same design. The banner then shows each step as it is written, with the step's time and the whole build's: the outline of the injects, each move's situation briefing, CITE tasks, and read-in, with the move briefings counted as the model writes them, then the injects against the design's plan; then the text of each inject, each Gallery article, and each profile, several at once, with how many are being written and the last one done; then the pages, Player applications, CITE, and the catalog metadata. Everything is saved as it is written, so you can work on the MSEL, or leave it, while it builds. Click **Cancel** to stop.

A MSEL whose build stopped, after **Cancel**, an error, or a closed tab, shows **Drafting with AI stopped before it finished** on its page with what is left, and offers **Continue drafting**, which picks up from the last saved step, **Finish without them**, which skips the articles and profiles still to write, and **Discard draft**, which deletes the MSEL.

Only one browser tab builds a MSEL at a time, so two tabs, or two people, never write the same injects or pages twice. While another tab is building it, the MSEL's page names who is building it, or says **You are building this MSEL with AI in another tab**, and offers **Take over** instead of **Continue drafting**. **Take over** builds the MSEL in this tab; the other tab stops and says who took over, and what it wrote stays. Closing the tab that is building a MSEL frees it at once for another tab to continue.

##### What Create MSEL Adds

Besides the moves, teams, organizations, Gallery cards, and injects, creating a drafted MSEL adds:

- **A pre-exercise move and a hot wash:** move 0 opens fifteen minutes before `STARTEX` and holds the read-in, so participants who arrive early have somewhere to wait. `STARTEX` opens move 1, the first move of play, and `ENDEX` opens the last move, the after-action hot wash. When the MSEL uses Steamfitter, each is a Steamfitter task that changes the move in CITE and Gallery.
- **Two clocks:** each move has a scenario time, which can jump ahead between moves. Blueprint dates each Gallery article within its move's scenario time.
- **An Expected Actions column:** what participants should do for each inject. It shows on the event list for controllers but not on the exercise view.
- **A Gallery status color on every row:** the model chooses each article's **Status** color as Gallery shows it on the wall: Gray for background or routine news, Green for good news or a problem resolved, Yellow for a developing situation, Orange for a serious one, and Red for a critical one. Rows that are not Gallery articles, and any row the model gives no color, are Gray.
- **Source Type on the list:** the column that holds each inject's Gallery source type shows on the **Scenario Events** list and its default tab.
- **Gallery articles are injects only:** an Information or Facilitation row, such as a controller's step, never goes to Gallery. If the model fills its **Source Type**, **Source Name**, **Date Posted**, **From Org**, or **To Org**, Blueprint clears them and says so in the draft's notes.
- **Columns marked for each view**, when the template marks none: the title and Expected Actions show on Information and Facilitation rows; and the title, Expected Actions, competency, and recipient organization columns, with each row's move and time, show on the **Assessor View**.
- **Pages:**
  - **Exercise Overview:** the objectives, teams and roles, schedule, and assessment. Every participant can see it.
  - **[MSEL name] Read-In:** the read-in. Every participant can see it.
  - **Facilitator Guide:** each move's purpose, expected decisions, controller notes, and questions; the expected action and objective of every inject; and the after-action review. Participants cannot see it.
  - **Team Profiles** and **Organization Profiles:** each team's and organization's profile, unless you cleared the option.
- **Player applications**, when the MSEL uses Player: **CITE Dashboard**, **Gallery Wall**, **Gallery Archive**, **Read-In**, the profile pages, and **Exercise Timeline** for every team, and the Steamfitter applications for exercise control. An application the template already has for the same view is not added again, whatever its name: a template's **CITE** application opens on the dashboard, so it stands in for **CITE Dashboard**, and every team gets it, not only the teams the template gave it to. Each team's first tab, the one Player opens on, is CITE's dashboard when the MSEL uses CITE, and otherwise the **Read-In**. The template's **Virtual Machines** application, and a map on it, stay only when the MSEL uses Caster and the format is functional or hybrid, because a tabletop's teams have no machines.
- **CITE**, when the MSEL uses CITE: each team gets one of CITE's built-in team types (**Official Score Contributor** for participant teams, **Other** for `EXCON`), its CITE duties, the CITE tasks, and the scoring model you chose, authored into the MSEL.
- **Steamfitter**, when the MSEL uses Steamfitter: one `STARTEX` row and one `ENDEX` row, each with a Player notification to every participant. When the AI drafted its own row for either, such as "`ENDEX` and hot wash", that row is used instead of adding another.
- **Exercise control:** your units become contributors, you join the exercise control team, and you become a CITE facilitator and a Gallery observer, so exercise control can watch every team. Every team sees every Gallery card; exercise control can post to them.
- **Catalog metadata:** the Institute of Electrical and Electronics Engineers (IEEE) 2881 fields on the **Info** tab: difficulty, purpose, mode, subject, keywords, and language, written last, from the finished MSEL.
- **A picture:** when the server has an image model, the MSEL gets a picture made from its scenario. On the **Info** tab, click the picture to see it at full size, and click **Regenerate Image** for another; **Generate Image** makes one for a MSEL that has none.
- **Article markings:** the banner you chose on every Gallery article. See [Article Markings](#article-markings).
- **One set of scenario indicators:** the hosts, addresses, domains, file hashes, and senders every alert, report, email, and Cyber Threat Intelligence (CTI) event names, so participants can pivot from one to the next. When the template has **IP Address** and **Network** columns, each row's values come from the set. See [Scenario Indicators](#scenario-indicators).

When you push the integrations, Gallery gets the drafted articles. Pages and articles sit on white paper with dark text, so they read the same in light and dark themes.

##### CITE Assessment Choices

- **Let the model choose:** the default. The design step picks one of the choices below for the scenario: questions by move for most tabletops, a Department of Defense (DoD) matrix where the exercise assesses mission impact or likelihood, `CISA NCISS` where teams score an incident, the template's own model where it has one and fits, or none. The **Design** tab shows the choice; choose another and draft again if it does not fit.
- **Keep the template's scoring model:** offered when the template already links a CITE scoring model or authors its own. The seeded **Crucible MSEL** template authors `CISA NCISS`, so a MSEL drafted from it scores with that model unless you choose otherwise.
- **Questions by move:** the draft writes discussion questions for each move. The MSEL gets its own scoring model, `[MSEL name] Questions by Move`: one category per move of play with the move's questions, comment boxes on, and scores hidden, and a final category in the hot wash for the after-action review.
- **DoD CTT mission impact matrix:** teams score the mission impact of each move, following the Department of Defense Cyber Table Top (CTT) guide. The MSEL gets its own copy of the `DoD CTT Mission Impact Assessment` model.
- **DoD CTT likelihood matrix:** teams score each attack's cost and chance of success. The MSEL gets its own copy of the `DoD CTT Likelihood Assessment Matrix` model, with the matrix beside the score sheet.
- **CISA NCISS:** each move, teams score the incident with the Cybersecurity and Infrastructure Security Agency (CISA) National Cyber Incident Scoring System (NCISS): functional impact, observed activity, location of observed activity, actor characterization, information impact, how recoverable the systems are, cross-sector dependency, and potential impact. The MSEL gets its own copy of the `CISA NCISS` model.
- **None:** no scoring model.

The model is authored in Blueprint, not created in CITE: review and edit it under the CITE settings on the **Info** tab, where **Author it in Blueprint** is selected. The push creates it in CITE for the evaluation, so a draft you discard leaves nothing in CITE. A template that authors its own model hands every MSEL drafted from it a copy, which the draft keeps.

##### Article Markings

A drafted MSEL has the marking you choose in **Design choices** when drafting, saved as its **Marking** on the **Info** tab; see [Marking](#marking) for the banners. A draft starts with **Exercise banner**, unless you or the template choose another. Blueprint also puts the banner on each Gallery article as it writes it, so you see it while you review the MSEL, and the push puts the current **Marking** on every article it sends.

##### Scenario Indicators

The outline of the injects includes one set of indicators for the whole scenario, which every row and Gallery article uses:

- **The defenders' hosts:** each host's name, role, address, and network. When the MSEL uses Caster and has a Caster workspace, or its template has one, the hosts are the machines in the workspace's plan, by their names, so a report names a host the teams can reach. Blueprint uses the addresses and networks Caster gives for each machine and makes up private ones where Caster gives none. A machine with the **Attacker** role in Caster's topology view, or one only on an internet or red team network, is the adversary's. When Blueprint can't read the workspace, it makes up the hosts, the build goes on, and the warnings say why.
- **Each organization's own domain**, for its people's email addresses and its website.
- **The adversary's infrastructure:** command and control addresses and domains, files, phishing senders, and user agents, each with what it's for, and the threat actor's name.

Every value is fictional and safe. Hosts have private addresses: `10.0.0.0/8`, `172.16.0.0/12`, or `192.168.0.0/16`. Adversary addresses come from the documentation ranges, `192.0.2.0/24`, `198.51.100.0/24`, and `203.0.113.0/24`, or from the range's own internet network. Every domain ends in `.example` or `.test`, and Blueprint makes up each file's hashes. Blueprint replaces any value the model wrote outside these, and the warnings list them.

The model writes each row and article with only the indicators it names. Blueprint then checks what it wrote. Any IP address, network, domain, file hash, or email address not in the set becomes one of the same kind from the set, the same one each time. The MSEL's own organization and team email addresses stay. The build's warnings say how many values it replaced in each row or article, and which. Version numbers, times, phone numbers, network masks, and file names stay as written.

The model writes two to four Intel articles as a CTI event, as a threat-sharing platform such as the Malware Information Sharing Platform (MISP) shows one:

- **Event header:** the event ID, event info, date, threat level, analysis state, and distribution.
- **Tags:** the TLP level when the article marking is TLP, the threat actor, and, as `mitre-attack-pattern` tags, the attack techniques of the inject and of the injects that share its indicators.
- **Attributes:** a table of category, type, value, Intrusion Detection System (IDS) flag, and comment, from the indicator set only.
- **Related events:** the other articles that share its indicators.

#### Filter Display

Use the filter options to narrow down MSELs shown in the catalog.

##### Types

Filter MSELs by categorization. Options are: All Types, Templates, and Not Templates.

##### Statuses

Filter MSELs by current status. Options are: `All Statuses`, `Pending`, `Entered`, `Approved`, `Deployed`, and `Completed`.

!!! note

    The filter dropdown uses `Completed` (past tense). The MSEL Status dropdown on the Config tab (see [Basic Information](#basic-information)) uses `Complete` and also exposes `Archived`. Both names refer to the same lifecycle state; the filter simply does not expose Archived MSELs by default.

##### Search

Search for a specific MSEL by name.

The MSEL list header also exposes:

- **Show only MSELs I own or have access to** toggle (funnel icon with subtitle **All MSELs** / **My MSELs**): for admins, switches between viewing every MSEL and only MSELs the user owns or contributes to.
- **Administration** gear: opens the Administration View (see [Administrator Guide](#administrator-guide)).

#### MSEL Row Actions

Each MSEL row in the catalog exposes four icon buttons in the first column:

- **Download** (down-arrow icon): choose **Download xlsx file** or **Download JSON file** to export the MSEL. A `.json` export can be uploaded again with **Upload JSON file**.
- **Upload .xlsx file to [MSEL name]** (up-arrow icon): replaces the MSEL's contents with data from the chosen file.
- **Delete [MSEL name]** (trash can icon): permanently removes the MSEL and its associated data. Template MSELs cannot be deleted; the trash can is disabled for them.
- **Copy [MSEL name]** (copy icon): duplicates the MSEL into a new, editable copy.

Click the MSEL's name (a link in the Name column) to open it for editing. The Description column shows one line of each description; hover over a shortened one to see it in full.

!!! important

    When importing MSELs into Blueprint, verify that events and settings align with your preferences. Some fields may require reconfiguration after import.

### MSEL Definition

After uploading or creating a MSEL, users can edit any information and add additional information to the MSEL. Users edit the same MSEL, instead of working on separate copies and sending edits to one person. This ensures everyone has the same version during the live scenario.

#### Change a MSEL with AI

When AI drafting is on, you can change a MSEL by describing what you want.

1. At the top of the MSEL, click **Ask AI**.
2. In **What should change?**, describe the change, such as `Split BLUE into a CERT team and a legal team`. You can also click one of the examples.
3. Click **Ask**. The assistant proposes changes, or asks you a question.
4. Clear the check box beside any change you do not want.
5. Click **Apply** to make the checked changes. The button shows how many changes it applies.

The assistant can add or edit teams, organizations, Gallery cards, moves and their scenario times, injects, and inject columns (data fields), and add or remove competencies. It offers only the changes your MSEL role allows: the owner, an editor, or a user with `EditMsels` can change columns.

To give injects something no column holds, ask for it, such as `Add a Severity column with Low, Medium, and High and set it on every inject`. The assistant proposes the column, with its type, its choices, the Gallery article parameter it fills, and whether it shows on the **Exercise View** and the scenario event list, and fills it on the injects in the same set of changes. Columns are added before the injects, so one **Apply** does both. It can also rename a column, change its type or choices, or move it on or off a view. It leaves out a column whose name is taken, choices on a column that is not text, or a Gallery parameter another column already fills, and says why.

It can also turn on an integration the MSEL does not use yet, with what the integration needs, such as `Use Gallery and CITE for this exercise`. The MSEL's owner, or a user with `EditMsels`, can do this:

- **Gallery:** Blueprint adds the article columns, which the injects in the same changes can fill, and the assistant proposes the Gallery cards and the article values on the injects.
- **CITE:** Blueprint gives the teams CITE team types, and authors the scoring model the assistant chose (DoD CTT mission impact, DoD CTT likelihood, or CISA NCISS) when the MSEL has none.
- **Player, Steamfitter, and Caster:** turned on; finish setting them up on their tabs, such as the Caster directory on the **Info** tab.

It cannot change the integrations of a MSEL that has been pushed until it is pulled.

It cannot delete injects, moves, or columns, turn an integration off, or change users or pages.

##### AI Usage

Blueprint records every call to an AI model made for a MSEL, including the calls that wrote its draft. The MSEL's owner, and users with the `ViewMsels` permission, see an **AI usage** table at the end of the **Config** section of the **Info** tab. It lists each step, such as **Design**, **Inject outline, CITE tasks and read-in**, **Inject text**, **Gallery articles**, **Ask AI**, and **News photos**, with its calls, input and output tokens, photos, and estimated cost. **Audio scripts** are the model's calls that wrote voicemails, calls, broadcasts, and reels. **Audio** counts each request to the speech provider. Once the provider voices anything, a **Characters voiced** column appears. Polly bills a clip twice, once for the audio and once for its timings, so the table counts both. The estimate uses the prices in `Ai:Prices`; it totals the calls whose models have a price and names the models that have none. It includes calls the model declined, because the provider bills those too. The table updates as the calls end, without a refresh.

To write one value with AI:

1. Open a scenario event, and click **Write [field name] with AI** beside a long text field: an HTML column, or the Gallery **Summary**. Short fields such as the title, sender, and source have no button; they are quicker to type than to ask for.
2. Describe what you want, and click **Write**.
3. Compare the **Current** and **Proposed** values side by side. A Gallery article is shrunk to fit its half of the dialog, so you see the whole page; in a narrow window the two are stacked.
4. Click **Use this**, **Try again**, or **Discard**.

To add audio or video to an inject, open **Write [field name] with AI** on its article field and choose from **What to do**:

- **Add 'Listen to this article':** reads the article as written, from its own headline, without the inject's title, and puts a **Listen to this article** player under the headline and byline, as a news site does, or under the marking when the article has no headline. No model writes anything. Each outlet, by its source name, always gets the same voice. The marking, small-print captions, and any transcript aren't read.
- **Make a voicemail**, on a Phone inject: the sender's voicemail, 20 to 45 seconds, as if heard over a phone line.
- **Make an intercepted call**, on an Intel inject: a call between two or more speakers, each in their own voice, as if heard over a phone line, with speaker-labeled captions.
- **Make a news broadcast clip**, on a News inject: the outlet's 30 to 60 second spoken version of the story, its anchor in the outlet's voice, with any quoted official in their own.
- **Make a news reel**, on a Social or News inject: a 15 to 30 second vertical video of the story for social media. The article's photo, or one made for it, pans behind the outlet's name and handle and a **BREAKING** headline. Captions burned into the video show a few words at a time, with the spoken word in yellow. The last three seconds are an end card. The reel comes with the post's text.

For a voicemail, call, or broadcast, choose **Audio only**, **Still card video**, or **Waveform video** under **Format**. A video has closed captions and a title card: **VOICEMAIL**, **INTERCEPT**, or the outlet's name, under the MSEL's marking. A transcript follows each clip, in a **Transcript** section participants can expand.

Write with AI shows the clip in the article, with its length and format. **Use this** puts it where the article's body starts. A reel belongs on a Social inject. To move one made on a News inject, click **Copy HTML**, then paste it into a field of the Social inject in the editor's HTML view.

Your browser makes the video, so it takes a few seconds and needs a browser that can encode H.264 video, such as Chrome or Edge. A browser with no Advanced Audio Coding (AAC) encoder gives the video Opus audio. Older Safari plays Opus in MP4 without sound, so Blueprint keeps the MP3 player beside the video. In a browser that can't encode video, the clip is audio only.

To write a profile with a logo, edit a team or an organization, and click **Write the team's profile with AI** or **Write the organization's profile with AI**.

To write a page with AI, such as the read-in or the facilitator guide, open the page on the **Info** tab, click **Edit Page**, then **Write [page name] with AI** beside **Save Changes**. Say what should change, such as `Add the two new teams` or `Cut this to one screen`; for an empty page, the instructions are optional and the page is written for its name. The assistant writes from the MSEL's scenario, moves, teams, organizations, and other pages, and keeps what you did not ask to change. Compare the current and proposed pages, then click **Use this**, **Try again**, or **Discard**. **Use this** puts the page in the editor; click **Save Changes** to keep it. The AI usage table lists these calls as **Pages**.

To write a move's situation briefing with AI, edit the move on the **Moves** tab and click **Write the situation briefing with AI** beside **Situation Description**. It writes what participants are told when the move opens, from the scenario, the moves, and the injects before the move, keeping clear of what the move's own injects bring. **Use this** puts it in the move; click **Save** to keep it.

To make the MSEL's picture with AI, click **Generate Image** or **Regenerate Image** beside the card image on the **Info** tab, or click the picture itself to see it at full size. In the dialog, say what the picture should show or change, or leave the box empty; what you ask for leads the picture, set in the MSEL's scenario, or in its description when it was not drafted with AI, and the dialog shows the new picture beside the current one. Click **Use this** to keep it, **Try again** for another, or **Discard** to keep the current one; nothing is saved until you click **Use this**. Pictures are news photos without text, signs, flags or recognizable faces, so a request for those is not followed. The same scenario asked again gives much the same picture, so say what should change. A server without an image model (`Ai:ImageModelId`) says so. The AI usage table lists these calls as **MSEL picture**, apart from the **News photos** written into articles.

#### Basic Information

On this tab, users can edit and configure MSEL settings, as well as enable/disable integrations.

![Blueprint Info Tab OE](img/blueprintBasicInfo-v5.png)

To edit the MSEL's basic information and configuration, follow these steps:

1. Navigate to the **Info** tab.
2. Ensure you are in the **Config** section.
3. Fill the fields as necessary following the Data Format Table specifications.

#### Data Format Table

| Field | Data Type | Description | Example |
| --- | --- | --- | --- |
| **Name** | String (max 70 characters) | Name of the MSEL | `Hurricane Delta MSEL` |
| **Description** | String (max 600 characters) | Details and/or characteristics of the MSEL | `Hurricane Delta Fort Myers Scenario` |
| **Marking** | Dropdown | Banner on the MSEL's participant material: `None`, `Exercise banner`, `TLP` for a Traffic Light Protocol (TLP) level, or `Classification banner`. Defaults to `None`. See [Marking](#marking). | `TLP` |
| **TLP level** | Dropdown | Shown when **Marking** is `TLP`: `TLP:CLEAR`, `TLP:GREEN`, `TLP:AMBER`, `TLP:AMBER+STRICT`, or `TLP:RED`. | `TLP:AMBER` |
| **Classification banner** | String (max 120 characters) | Shown when **Marking** is `Classification banner`: the banner text, in your own words. Displays in uppercase. | `UNCLASSIFIED // FOR EXERCISE USE ONLY` |
| **Is a Template** | Boolean | Marks this MSEL as a reusable template for creating new MSELs. Template MSELs cannot be deleted. | `True` |
| **Set a Start Time** | Boolean | When checked, exposes a **Start Date / Time** picker and schedules a specific start moment for the MSEL. When unchecked, the MSEL is open-ended. | `True` |
| **Start Date / Time** | Datetime | Start date/time of the MSEL (only visible when **Set a Start Time** is checked). | `09/10/2024 20:00:00` |
| **Duration (d / h / m)** | Integer × 3 | End time relative to the start, expressed as days / hours / minutes. Click **+** to increment or type directly into each field. | `1 d 12 h 00 m` |
| **Applications to Integrate** | Boolean checkboxes | Enable any of: Player, Gallery, CITE, Steamfitter, Caster. Enabling an integration activates additional MSEL sidebar tabs for that app. Caster requires Player. | Player: `True`, CITE: `True` |
| **MSEL Status** | Dropdown | MSEL lifecycle state: `Pending`, `Entered`, `Approved`, `Complete`, `Archived`. Blueprint sets `Pushing` and `Pulling` itself while it pushes or removes integrations, and `Deployed` once they are pushed; while the MSEL is in one of those, the dropdown is locked. | `Approved` |
| **Header Row Metadata (Height)** | Integer | Height of the header row when exporting this MSEL to `.xlsx`. | `30` |
| **Difficulty Rating** | Dropdown | Metadata tag describing the exercise's difficulty. | (per framework) |
| **Exercise Purpose** | Dropdown | Metadata tag describing why the exercise is being run. | (per framework) |
| **Exercise Mode** | Dropdown | Metadata tag describing how the exercise is delivered. | (per framework) |
| **Subject** | String | Free-text subject tags. | `incident response, network defense` |
| **Keywords** | String | Searchable keyword tags. | `ransomware, financial services` |
| **Date Created** | Date (read-only) | Date this MSEL was first saved. | `2026-05-08` |
| **Created By** | String (read-only) | Name of the user who created this MSEL. | `Admin User` |

To save Config-tab changes, click **Save Changes** at the top of the tab. **Cancel Changes** reverts unsaved edits. Both buttons are disabled until a change is made.

#### Direct-Access URLs

The Config tab exposes three copyable, shareable URLs. Each row shows a **Copy** (clipboard) button and a direct link.

- **Exercise View URL** (`http://<host>/msel/{mselId}/view`): use this URL for direct read-only access to the Exercise MSEL (see [Exercise View](#exercise-view)).
- **MSEL Starter URL** (`http://<host>/starter/?msel={mselId}`): use this URL for direct access to edit the Scenario Events list in starter mode.
- **Assessor View URL** (`http://<host>/assess/?msel={mselId}`): use this URL for direct access to the Assessor View (see [Assessor View](#assessor-view)).

#### Associating Competencies with a MSEL

The Config tab includes a **Competencies** row that points the user at the MSEL's [Competencies](#competencies) tab, where a competency framework can be attached to the MSEL.

#### Metadata

The **Metadata** block on the Config tab captures non-functional descriptors that help with searching and filtering MSEL catalogs: **Difficulty Rating**, **Exercise Purpose**, **Exercise Mode**, **Subject** (free text), and **Keywords** (free text).

#### Marking

The **Marking** setting puts a banner on the MSEL's participant material. It defaults to `None`, so a MSEL shows no banner until you choose one.

- **Exercise banner:** `EXERCISE · EXERCISE · EXERCISE` on gold, with the MSEL name and `FICTIONAL SCENARIO`.
- **TLP:** the chosen TLP level on black, in the level's color.
- **Classification banner:** your text, in the color of the classification it names (for example, red for `SECRET`, purple for `CUI`).

The banner shows in two places:

- **Gallery articles:** Blueprint adds the banner to each article's body when it pushes the MSEL to Gallery. A banner already in the article is replaced; `None` removes it. The article text stored in the MSEL doesn't change. To change the banner on pushed articles, remove and push the integrations again.
- **MSEL pages:** a page shows the banner at its top and bottom. Changes show the next time the page opens.

#### Integrations

- **[Gallery](#glossary):** Blueprint will add the collections, exhibits, cards, articles, teams, and users specified on the MSEL. To learn more about this integration, reference the [Gallery](#gallery) section.
- **[CITE](#glossary):** Blueprint will add the evaluation, moves, actions, roles, teams, and users specified on the MSEL. To learn more about this integration, reference the [CITE](#cite) section.
- **[Player](#glossary) & [Steamfitter](#glossary):** Blueprint will automate the adding of events specified on the MSEL, as well as configure exercise details in Player.
- **[Caster](#glossary):** Blueprint will deploy the MSEL's virtual machine topology from a Caster directory into a new Caster workspace, with the VMs added to the Player view and its teams. To learn more about this integration, reference the [Caster](#caster) section.

#### MSEL Pages

Beyond the **Config** section, the **Info** tab is a container for user-authored notes pages. Click **Add Page** on the Info tab to create a new page; each page can be restricted to elevated users or exposed to every MSEL user. A page shows the MSEL's [Marking](#marking) banner at its top and bottom.

![Blueprint Notes Tab OE](img/blueprintNotes-v3.png)

To add MSEL notes, follow these steps:

1. Navigate to the **Info** tab.
2. Click **Add Page**.
3. Fill the fields as necessary following the Data Format Table specifications.

#### Data Format Table

| Field                                 | Data Type | Description                                                                | Example                         |
| ------------------------------------- | --------- | -------------------------------------------------------------------------- | ------------------------------- |
| **Page Name**                         | String    | Name of the Page                                                           | Core Planning Group Notes       |
| **All MSEL users can view this page** | Boolean   | Enable other users who don't have elevated permissions to access this page | True                            |
| **Notes**                             | Rich Text | Field to add page notes                                                    | Next meeting is on 12/7 at 0800 |

After you've made your changes, click the **checkmark** to save the page.

#### Contributors

On this tab, users can add and assign contributor teams, as well as their respective roles related to the creation and management of the MSEL. Assigned teams can view and edit the MSEL.

![Blueprint Contributors Tab OE](img/blueprintAddContributors-v2.png)

##### Add a Contributor

To add a contributor unit to the MSEL, follow these steps:

1. Navigate to the **Contributors** tab.
2. Click **Add a Contributor Unit**, then select the unit you want to add.
3. After selecting the unit, click the **+** button.

##### Remove a Contributor

To remove a contributor unit from the MSEL, follow these steps:

1. Navigate to the **Contributors** tab.
2. Select the team you want to remove and click the **-** button.

Now that you've added the unit to the MSEL, members can view and edit it based on each unit member's role.

The available roles are:

- **[`Editor`](#glossary):** Can edit the events assigned to them by the MSEL owner, but not other events. They cannot delete events or create new events.
- **[`Approver`](#glossary):** Can view and edit the MSEL, and will have the added feature of approving a MSEL.
- **[`Move Editor`](#glossary):** Can edit moves on the MSEL, as well as increment them during an exercise.
- **[`Owner`](#glossary):** Owner of the MSEL, can view and edit the MSEL, as well as perform all of the functionalities that the MSEL provides (e.g., Add Teams, Add Integrations, Events).
- **[`Evaluator`](#glossary):** Manages the exercise, can advance moves, execute events, and check events as completed.
- **[`Viewer`](#glossary):** Can view the MSEL, but can't edit it.

Each contributor also has a **CITE Role** and a **Gallery Role**, which the push sends to CITE and Gallery. Blueprint fills them in where they are empty, so exercise control and assessors can watch every team:

- A contributor added to the `EXCON` team gets **CITE Role** `Facilitator`, which sees every team and advances the moves, and **Gallery Role** `Observer`.
- A contributor given the `Evaluator` role gets **CITE Role** `Observer`, which sees every team but changes nothing, and **Gallery Role** `Observer`.

A role you set by hand stays, and you can change either on this tab at any time.

#### Competencies

On the **Competencies** tab, MSEL owners attach a competency framework (defined in the Administration Competencies page) and select which work roles, tasks, and specific knowledge, skills, and abilities apply to the MSEL. Selected competencies can then be associated with teams and scenario events, enabling assessment during the Assessor View.

##### Select a Competency Framework

1. Navigate to the **Competencies** tab.
2. In **Competency Framework**, select a framework from the dropdown. If no frameworks appear, define one in the Administration Competencies page.

##### Add Competencies to the MSEL

1. Expand the **Add Competencies** region.
2. Browse work roles and competencies from the selected framework and select the items that apply to this MSEL.

##### View MSEL Competencies

Expand the **MSEL Competencies** region to see the table of competencies associated with the MSEL. Columns include **ID**, **Type**, **Framework**, **Name**, **Teams**, and **Events**. Use the search box to filter by name.

#### Teams

On this tab, users can add and assign teams as well as their respective roles during an event to the MSEL.

![Blueprint Teams Tab OE](img/blueprintAddTeams-v3.png)

##### Add a Team

To add a team to the MSEL, follow these steps:

1. Navigate to the **Teams** tab.
2. Click **Add a Team**, then select the team you want to add.
3. After selecting the team, click the **+** button.

##### Remove a Team

To remove a team from the MSEL, follow these steps:

1. Navigate to the **Teams** tab.
2. Select the team you want to remove and click the **-** button.

Click a team to expand its **Team Members**. Each member's **Type** is their [CITE team role](#cite-team-roles), which decides what they can do in CITE. Check **Open Invite** on a team to let any of its members invite others to it.

#### Data Fields

On this tab, users can add data fields used on the MSEL. Compare these [data fields](#glossary) to the column fields on Excel spreadsheets.

![Blueprint Data Fields Tab OE](img/blueprintDataFields-v5.png)

There are two categories of Data Fields:

- **[System Defined](#glossary):** Added by default in MSEL creation, since data fields under this category are essential for MSEL features to work.
- **[User Defined](#glossary):** Added by the user on an as-needed basis.

##### Who Can Change Data Fields

| What | Who can change it |
| --- | --- |
| A user-defined data field and its option list | Users with EditMsels, the MSEL's Owner (including its creator), and its Editors |
| The **Move**, **Group**, **Execution Time**, and **Integration Target** rows | Users with EditMsels and the MSEL's Owner, because they are MSEL settings |
| **Make a template** from a data field | The above, with ManageDataFields as well |

For everyone else, including users with the Approver role, the tab is read-only. An Approver still edits and approves scenario events.

##### Add a Data Field

![Blueprint Add Data Fields OE](img/blueprintAddDataField.png)

To add a Data Field to the MSEL, follow these steps:

1. Navigate to the **Data Fields** tab.
2. Click the **+** icon from the top left of the screen.
3. Fill the fields as necessary following the Data Format Table specifications.

#### Data Format Table

| Field                                                      | Data Type     | Description                                                                                    | Example          |
| ---------------------------------------------------------- | ------------- | ---------------------------------------------------------------------------------------------- | ---------------- |
| **Display Order**                                          | Integer       | Indicate the order of the data field                                                           | 1                |
| **Name**                                                   | String        | Name of the data field                                                                         | Description      |
| **Data Type**                                              | Dropdown Text | Data type for the data field                                                                   | String           |
| **Use Option List**                                        | Boolean       | Add options to select when adding an event                                                     | True             |
| **Multi-select**                                           | Boolean       | With **Use Option List**, allow more than one option to be selected                            | False            |
| **Display on the Events list**                             | Boolean       | Display this data field on the Events list tab                                                 | True             |
| **Display on the Exercise View**                           | Boolean       | Display this data field on the Exercise View tab                                               | True             |
| **Information Data Field**                                 | Boolean       | Display this data field for information events                                                 | True             |
| **Facilitation Data Field**                                | Boolean       | Display this data field for facilitation events                                                | True             |
| **Display on "Default" edit tab**                          | Boolean       | Display this data field on the Default tab of the event edit dialog                            | True             |
| **Assessor View**                                          | Boolean       | Display this data field on the [Assessor View](#assessor-view)                                 | True             |
| **Display for Content Developers and MSEL Owners Only!!!** | Boolean       | Display this data field for users who are Content Developers or MSEL Owners only               | True             |
| **Gallery Article Parameter**                              | Dropdown Text | If using the Gallery integration, select the Gallery parameter that aligns with the data field | Description      |
| **Column Metadata (Width)**                                | Integer       | Width of the column when displayed                                                             | 30               |
| **Cell Metadata (Color, Tint, Font-Weight)**               | String        | Column color and font style                                                                    | (Red, 20%, bold) |

After you've added all desired configurations, click **Save**.

##### Data Value Checks

Blueprint checks the values typed into these data field types. A value chosen from an option list is not checked.

| Data Type | Accepts | Example |
| --- | --- | --- |
| **IpAddress** | A single IPv4 or IPv6 address, without a prefix length | `192.0.2.10`, `2001:db8::10` |
| **Network** | A network in Classless Inter-Domain Routing (CIDR) form, with no host bits set. Blueprint suggests the corrected network when host bits are set. | `10.1.0.0/24`, `2001:db8::/48` |
| **Datetime** | A date and time chosen with the date picker | `09/10/2024 20:00:00` |

An event with a value that fails its check shows the problem under the field and cannot be saved until it is fixed. A Datetime value that is not a date, such as the text left behind when a field's type is changed to Datetime, is shown as is and marked **Not a date and time**. IpAddress and Network columns sort by address rather than alphabetically.

##### Delete a Data Field

To delete a Data Field from the MSEL, follow these steps:

1. Navigate to the **Data Fields** tab.
2. Select the data field you want to delete and click the **Trash Can**.

##### Edit a Data Field

To edit an existing Data Field, follow these steps:

1. Navigate to the **Data Fields** tab.
2. Select the data field you wish to edit and click **Edit** to make any changes to the existing configurations.
3. After making all necessary changes, click the **checkmark** to save them.

##### Search for a Data Field

To search for a specific Data Field, follow these steps:

1. Navigate to the **Data Fields** tab.
2. Click the **Search Bar** and add the name of the data field desired.

#### Organizations

On this tab, users add all related [organizations](#glossary) (with their information) used on the MSEL and the live exercise. Users can create organizations from scratch or from a template.

![Blueprint Organizations Tab OE](img/blueprintOrganizations-v4.png)

##### Add an Organization Card from Scratch

![Blueprint Add Organization OE](img/blueprintAddOrganization-v2.png)

To add an Organization Card from scratch, follow these steps:

1. Navigate to the **Organizations** tab.
2. Click **Add Organization**.
3. Select **New Organization** from the dropdown.
4. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field           | Data Type | Description                                                   | Example                                                                                                                                                             |
| --------------- | --------- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**        | String    | Name of the organization                                      | Twitter                                                                                                                                                             |
| **Short Name**  | String    | Organization's short name, such as an acronym                 | TW                                                                                                                                                                  |
| **Summary**     | String    | Organization's short summary                                  | Social media platform                                                                                                                                               |
| **Email**       | String    | Organization's email contact                                  | `user@twitter.com`                                                                                                                                                  |
| **Description** | Rich Text | Information, details, and characteristics of the organization | Twitter is a micro-blogging and social networking service on which users post and interact with messages known as "tweets", owned by American company Twitter, Inc. |

After you've added all desired configurations, click **Save**.

##### Add an Organization Card from Template

To create an organization from a template, follow these steps:

1. Navigate to the **Organizations** tab.
2. Click **Add Organization**.
3. Select the template you want to use from the dropdown.
4. Here, users can edit all necessary information to create a new organization.
5. After modifying the desired details, click **Save**.

##### Edit an Organization

To edit an existing organization, follow these steps:

1. Navigate to the **Organizations** tab.
2. Select the card you want to edit and click **Edit** next to the organization name.
3. Here, users can edit all necessary information.
4. Click **Save**.

##### Delete an Organization

To delete an organization, follow these steps:

1. Navigate to the **Organizations** tab.
2. Select the card you want to delete and click the **Trash Can** next to the organization name.

##### Search for an Organization

To search for a specific organization, follow these steps:

1. Navigate to the **Organizations** tab.
2. Click the **Search Bar** and type the name of the desired organization.

#### Moves

On this tab, users can add all related exercise [moves](#glossary) to the MSEL.

![Blueprint Moves Tab OE](img/blueprintMoves-v4.png)

##### Add a Move

![Blueprint Add Moves OE](img/blueprintAddMove-v3.png)

To add a move, follow these steps:

1. Navigate to the **Moves** tab.
2. Click the **+** icon.
3. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field                     | Data Type | Description                                               | Example                                  |
| ------------------------- | --------- | --------------------------------------------------------- | ---------------------------------------- |
| **Move Number**           | Integer   | Designated number for the move                            | 1                                        |
| **Start Date/Time**       | Datetime  | Start date/time of the move real-time                     | 11/20/2023, 09:00:00                     |
| **Move Description**      | String    | Information, details, and characteristics of the move     | Scene Setter                             |
| **Situation Date/Time**   | Datetime  | Start date/time of the move exercise-time                 | 1/24/2024, 14:18:27                      |
| **Situation Description** | Rich Text | Information, details, and characteristics of the exercise | Hurricane Delta has landed in Fort Myers |

After you've added all desired configurations, click **Save**.

##### Edit a Move

To edit the move's details, follow these steps:

1. Navigate to the **Moves** tab.
2. Select the move you want to edit and click **Edit** for the corresponding move.
3. Here, users can edit all desired details.
4. Click **Save**.

##### Delete a Move

To delete a move from the MSEL, follow these steps:

1. Navigate to the **Moves** tab.
2. Select the move you want to delete and click the **Trash Can** for the corresponding move.

##### Search for a Move

To search for a specific move, follow these steps:

1. Navigate to the **Moves** tab.
2. Click the **Search Bar** and type the name of the desired move.

#### Scenario Events

On this tab, users can add all related events to the MSEL.

![Blueprint Events Tab OE](img/blueprintInjects-v2.png)

##### Add an Event

To add a new event, follow these steps:

1. Navigate to the **Events** tab.
2. Click the **Hamburger** icon found on the top left.
3. Click **Add New Event**.

On the **Default** tab, fill the fields as necessary following the Data Format Table specifications.

![Blueprint Add Event OE](img/blueprintAddInjects-v3.png)

##### Data Format Table

| Field               | Data Type     | Description                                                                  | Example                |
| ------------------- | ------------- | ---------------------------------------------------------------------------- | ---------------------- |
| **Control Number**  | String        | Event control number, used to identify and categorize events                 | `ADMIN-1`              |
| **Description**     | String        | Information, details, and characteristics of the event                       | `STARTEX`              |
| **Status**          | Dropdown Text | Approval status of the event used on the MSEL                                | `Approved`             |
| **Time**            | Datetime      | Start date/time of the move real-time                                        | `11/20/2023, 09:00:00` |
| **Move**            | Dropdown Text | Move number the event is part of                                             | `1`                    |
| **Group**           | Integer       | Groups events within a move, often times executed within the same time range | `1`                    |
| **Exercise Date**   | Datetime      | Start date/time of the move exercise-time                                    | `1/24/2024, 14:18:27`  |
| **Type**            | String        | Source type for the event                                                    | `Email`                |
| **Delivery Method** | Dropdown Text | How is the event delivered to participants                                   | `Email`                |

If not already completed on the previous tab, fill the fields from the **Advanced** tab following the Data Format Table specifications. This tab focuses on the MSEL's metadata and style.

![Blueprint Add Event Advanced OE](img/blueprintAddInjectsAdvanced.png)

##### Data Format Table

| Field                  | Data Type | Description                                                         | Example             |
| ---------------------- | --------- | ------------------------------------------------------------------- | ------------------- |
| **Exercise Date/Time** | Datetime  | Start date/time of the move exercise-time                           | 1/24/2024, 14:18:27 |
| **Row Metadata**       | Integer   | Defines the size of this event on the Events and Exercise View tabs | 15,199,21,133       |

If the MSEL owner enabled Gallery integration, the user should verify that all fields on the **Gallery** tab are complete, following the Data Format Table specifications below. This tab includes the fields required for the Gallery integration to function properly.

![Blueprint Add Event Gallery OE](img/blueprintAddInjectsGallery.png)

##### Data Format Table

| Field               | Data Type     | Description                                                                  | Example                                           |
| ------------------- | ------------- | ---------------------------------------------------------------------------- | ------------------------------------------------- |
| **Description**     | String        | Information, details, and characteristics of the event                       | STARTEX                                           |
| **Status**          | Dropdown Text | Approval status of the event used on the MSEL                                | Approved                                          |
| **Move**            | Dropdown Text | Move number that the event is part of                                        | 1                                                 |
| **Group**           | Integer       | Groups events within a move, often times executed within the same time range | 1                                                 |
| **Exercise Date**   | Datetime      | Start date/time of the move exercise-time                                    | 1/24/2024, 14:18:27                               |
| **Delivery Method** | Dropdown Text | How is the event delivered to participants                                   | Gallery                                           |
| **Title**           | String        | Title of the event                                                           | Bank Consortium Falls Victim to Ransomware Attack |
| **From Org**        | Dropdown Text | Select the organization that sends this event                                | CC News                                           |
| **To Org**          | Dropdown Text | Select the organization that receives this event                             | All                                               |

After you've added all desired configurations, click **Save**.

##### Add Color to an Event

To add a color to an existing event, follow these steps:

1. Navigate to the **Events** tab.
2. Select the event you want to edit and click the **Hamburger** icon next to it.
3. Hover over **Highlight**.
4. Here, users can select the desired color.

##### Edit an Event

To edit the event's details, follow these steps:

1. Navigate to the **Events** tab.
2. Select the event you want to edit and click the **Hamburger** icon next to it.
3. Click **Edit**. The system opens the same edit component used when creating a new event.
4. Make your changes, then click **Save**.

##### Delete an Event

To delete an event, follow these steps:

1. Navigate to the **Events** tab.
2. Select the event you want to delete and click the **Trash Can** icon next to it.

##### Search for an Event

To search for a specific event, follow these steps:

1. Navigate to the **Events** tab.
2. Click the **Search Bar** and type the name of the desired event.

#### Exercise View

While the **Events** tab lets MSEL owners and content developers manage events, the **Exercise View** tab gives participants read-only access to track current and future events during an exercise.

![Blueprint Exercise View OE](img/blueprintExerciseView-v3.png)

#### Assessor View

The **Assessor View** tab is where Evaluators watch a live exercise and rate the participating teams against the MSEL's competencies. Each scenario event shows the xAPI statements the exercise applications recorded for it, beside a competency assessment where the Evaluator rates each competency and submits the rating as an xAPI competency assertion. The MSEL's content is read-only here; the only values an Evaluator can change are **Checkbox** data fields.

The Assessor View is also available outside the MSEL editor via the **Assessor View URL** on the Config tab (`/assess/?msel={mselId}`).

##### Access

| MSEL Role | Access |
| --- | --- |
| **Owner**, **Evaluator** | View the Assessor View and check or clear **Checkbox** data fields |
| **Editor**, **Approver** | View the Assessor View |

A MSEL's creator counts as its Owner. System Admins have full access to every MSEL's Assessor View. Users with no role on the MSEL see **Access Denied**.

##### Choose What Assessors See

The Assessor View shows only the columns the MSEL owner marks for it. If none are marked, it shows **No data fields are marked as assessor-visible.** On the Assessor View tab inside the MSEL, it points at the **Data Fields** tab. Opened from its **Assessor View URL**, it offers **Open Data Fields** to users who can change the data fields (EditMsels, the MSEL's creator, or an Owner or Editor), and tells anyone else to ask the MSEL owner.

To choose the Assessor View columns, follow these steps:

1. Navigate to the **Data Fields** tab.
2. Check the **Assessor View** column for each data field the assessors should see. The **Move**, **Group**, **Execution Time**, and **Integration Target** rows control those columns the same way.

To give assessors competencies to rate, attach a framework and its competencies on the [Competencies](#competencies) tab, then associate them with teams and scenario events. The rating levels come from the framework's default proficiency scale.

##### Review xAPI Evidence

Events are grouped under their move and group. Click a move, group, or event row to expand it. Use **Expand All** (Ctrl+Shift+E) and **Collapse All** (Ctrl+Shift+C) in the header to open or close every row.

An expanded row shows **xAPI Evidence** on the left: the statements recorded for that move, group, or event, with their time, platform, actor, team, verb, and object. Click a statement to see its JSON, or click **Copy JSON** to copy it. Narrow the list with the **Team**, **Source** (Blueprint, CITE, Steamfitter, Player, or Gallery), and **Verb** filters, and click **Refresh xAPI statements** to load new ones.

Click **Search Events** in the header to search the events and filter them by the values of their data fields.

##### Submit a Competency Assessment

The **Competency Assessment** panel on the right of an expanded row lists the competencies associated with that move, group, or event. To rate them, follow these steps:

1. In **Assess Team**, select the team being assessed. Only teams associated with at least one of the listed competencies appear. When the evidence is filtered to a single team, that team is preselected.
2. For each competency, select a **Rating** and optionally enter a **Comment**. Hover over the competency ID to see its name.
3. Click **Submit assertion** to submit one rating, or **Submit All** to submit every rated competency.

Each submitted rating is marked **Submitted**. Blueprint records it in the Learning Record Store (LRS) as an xAPI statement with the `asserted` verb, so xAPI must be configured for Blueprint.

##### Print an Assessment Report

Click **Print Assessment Report** in the header to open the browser's print dialog with a summary of the exercise's purpose and results. The report reads the same xAPI statements as the Assessor View, so you don't need to expand any rows first.

The report has these parts:

- **Cover**: the MSEL image (or a band in the theme color), **Assessment Report**, the MSEL name, the exercise **Start** and **Duration**, the number of **Moves**, **Injects**, and **Teams**, the **Assessors**, and when it was **Printed**. Under **Purpose** are the MSEL description and each move's description.
- **Results by objective**: shown when the MSEL has a data field whose name contains "objective". Each value of that field is a row, with the **Injects** that have it, how many were **Assessed**, and the ratings at each proficiency level, for all teams together.
- **Results by competency**: the same columns for each competency the injects exercise, shown by its ID and name. Without an objective field, the report says the competencies measure the results.
- **Results by team**: each team's **Injects assessed**, **Not assessed** count, and ratings at each level.
- A page for each team: one table with the **Time**, **Inject**, and **Rating** of each inject the team was rated on, in time order. Each rating reads as the competency ID and level (for example, "T-1 Expert"), followed by the assessor's comment when there is one. Ratings for a whole move or group appear as "(whole move)" or "(whole group)". A **Not assessed** line names the injects with competencies the team could have been rated on.

When a competency was rated more than once for the same team and inject, the report counts the latest rating. Ratings submitted without a team, or for a team no longer on the MSEL, print on a **No team** page.

#### MSEL Playbook

The **MSEL Playbook** tab presents scenario events in a printable, page-at-a-time layout intended for offline reference by exercise controllers.

Each event page shows: **Execution Time** (absolute + offset), **Move**, **Group**, **Integration Target**, **Control Number**, **Simulated Time**, **Status**, **Summary**, **From Org**, **To Org**, **Card Id**, **Source Type**, **Source Name**, **Delivery Method**, and the **Description** with rendered rich-text content.

Pagination controls (**Previous page**, **Next page**, **Items per page**) appear at the top; the default is one event per page. Visual indicators mark the transition between moves.

Two print buttons appear next to the pagination controls. Each opens the browser's print dialog with only the playbook content.

- **Print Current Page** prints the events on the current page.
- **Print All Events** prints the full playbook:
  - A cover page with the MSEL image, or a band in the theme color when the MSEL has no image. Below it: the MSEL name and description; the start date and time, duration, number of moves and scenario events, and teams; the subject, educational level, and keywords when set; and a contents list of the sections that follow.
  - **Competencies**, when the MSEL has any: its competencies grouped by framework, each with its ID number and name, then each team's competencies.
  - Each MSEL page marked for the playbook, on its own page.
  - Each move on a new page, starting with **Move** and its number, its description, the **Situation Date/Time**, and the **Situation** briefing in a shaded box. The move's events follow.

Audio and video don't print: each player prints as a line saying the clip plays in Gallery, and its transcript prints open. Event tables don't split a row across pages. Every printed page has a footer with the MSEL name and the page number. The page number needs Chrome or Edge 131 or later.

#### Invitations

Users with Administrator, Content Developer, or designated MSEL contributor roles can create invitations from the Invitations tab. These invitations allow others to join or start the MSEL event.

You can specify email domains and a maximum number of users on the invitations. Invitations link to specific teams added to the MSEL. When users join the event using an invitation, the system assigns them to the corresponding team for the duration of the event.

![Blueprint Invitations OE](img/blueprintInvitations-v2.png)

##### Add an Invitation

![Blueprint Add Invitations OE](img/blueprintAddInvitations.png)

To add a new invitation, follow these steps:

1. Navigate to the **Invitations** tab.
2. Click **Invite Others**.
3. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field                                                      | Data Type     | Description                                                                                                          | Example                              |
| ---------------------------------------------------------- | ------------- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------ |
| **Team**                                                   | Dropdown Text | Team tied to the invitation                                                                                          | SEI - Software Engineering Institute |
| **Email Domain**                                           | String        | User's email domain                                                                                                  | `@sei.cmu.edu`                       |
| **Expiration Date/Time**                                   | Datetime      | Date and time when the invitation should expire                                                                      | 04/15/2024 15:12:03                  |
| **Maximum Number of Users Allowed to Use this Invitation** | Integer       | Number of users that can use the invitation                                                                          | 5                                    |
| **Can Invite others to this Team**                         | Boolean       | Permission to add others to the team                                                                                 | False                                |
| **CITE Team Role**                                         | Dropdown Text | With CITE, the [CITE team role](#cite-team-roles) people who join through this invitation get. Starts at **Member**. | Contributor                          |

After you've added all desired configurations, click **Save**.

##### Edit an Invitation

To edit an invitation, follow these steps:

1. Navigate to the **Invitations** tab.
2. Select the invitation you want to edit and click **Edit**. The system opens the same edit component used when creating a new invitation.
3. Make your changes, then click **Save**.

##### Delete an Invitation

To delete an invitation, follow these steps:

1. Navigate to the **Invitations** tab.
2. Select the invitation you want to delete and click the **Trash Can** next to it.

##### Search for an Invitation

To search for a specific invitation, follow these steps:

1. Navigate to the **Invitations** tab.
2. Click the **Search Bar** and type the name of the team tied to the invitation.

### Integrations

Integrations with the following applications simplify configuration steps. This functionality eliminates the need to repeat the same steps in multiple applications.

#### CITE

If enabled, Blueprint can push MSEL information to the CITE application. When the MSEL owner enables CITE integration, additional tabs appear in the Blueprint side panel. The MSEL owner must configure the additional tabs to complete the integration. To enable full CITE integration, the MSEL owner must configure the following tabs:

##### Teams

Use the **Teams** tab to configure CITE settings for teams added to the MSEL. To do this, follow these steps.

First, assign a CITE Team Type to each team.

![CITE Team Type OE](img/citeTeamType.png)

1. Navigate to the **Teams** tab.
2. Select the desired team and click **Edit**.
3. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field              | Data Type     | Description                               | Example                 |
| ------------------ | ------------- | ----------------------------------------- | ----------------------- |
| **Team Email**     | String        | Team's email contact                      | `seicmu@cmu.edu`        |
| **CITE Team Type** | Dropdown Text | Classifies the team within a certain type | Individual Organization |

After you've made all desired configurations, click **Save**.

Next, assign roles to each member of the team. To do this, follow these steps:

![CITE Team Roles OE](img/citeTeamRoles-v2.png)

1. Navigate to the **Teams** tab.
2. Select the desired team, and click it to expand its configurations.
3. In each member's **Type** column, select their CITE team role.

##### CITE Team Roles

The **Type** list shows the team roles defined in CITE. Blueprint gives each member that role in CITE when it pushes the MSEL. CITE comes with these roles:

| Role | Can do in CITE |
| --- | --- |
| **Owner** | View the team, answer, submit the team's score, and manage the team |
| **Submitter** | View the team, answer, and submit the team's score |
| **Contributor** | View the team and answer |
| **Member** | View the team |

A CITE administrator can add roles, such as one that can see or change the official score. Blueprint lists them once they exist.

!!! warning

    A member whose **Type** is **-- None (Member in CITE) --** gets the **Member** role in CITE, which can view the team but not answer. Give every participant who scores a role before pushing: for example, **Submitter** for the team's recorder and **Contributor** for the others.

    People who join a team through an [invitation](#invitations) get the invitation's **CITE Team Role**, which starts at **Member**. Set it to **Contributor** or **Submitter** on invitations for participants who score; a separate invitation for recorders can give them **Submitter**.

##### CITE Actions

On this tab, the MSEL owner can add [CITE Actions](#glossary) to push from Blueprint. These actions allow team members to customize their response by tracking tasks during the exercise.

![Blueprint CITE Actions OE](img/blueprintCiteActions-v3.png)

##### Add a CITE Action

![Blueprint Add CITE Actions OE](img/blueprintAddCiteActions.png)

To add a CITE Action, follow these steps:

1. Navigate to the **CITE Actions** tab.
2. Click the **+** icon.
3. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field                         | Data Type     | Description                                                      | Example                              |
| ----------------------------- | ------------- | ---------------------------------------------------------------- | ------------------------------------ |
| **Move**                      | Dropdown Text | Select the move to display this action                           | 0 - Hurricane Delta Scene Setter     |
| **Team**                      | Dropdown Text | Select the team this action applies to                           | Conference of State Bank Supervisors |
| **Display Order**             | Integer       | Indicates the order to display this action on the CITE Dashboard | 1                                    |
| **Description of the Action** | String        | Information, details, and characteristics of the action          | Assign users to roles                |

After you've added all desired configurations, click **Save**.

##### Edit a CITE Action

To edit the CITE Action's details, follow these steps:

1. Navigate to the **CITE Actions** tab.
2. Select the action you want to edit and click **Edit** next to it. The system opens the same edit component used when creating a new action.
3. Make your changes, then click **Save**.

##### Delete a CITE Action

To delete a CITE Action, follow these steps:

1. Navigate to the **CITE Actions** tab.
2. Select the action you want to delete and click the **Trash Can** next to it.

##### Filter Actions by Team

To filter CITE Actions by teams, follow these steps:

1. Navigate to the **CITE Actions** tab.
2. On the **Team** dropdown, select the desired team to filter by.

##### Search for an Action

To search for a specific action, follow these steps:

1. Navigate to the **CITE Actions** tab.
2. Click the **Search Bar** and type the name of the desired action.

##### CITE Roles

On this tab, the MSEL owner can add [CITE Roles](#glossary) to push from Blueprint. These roles allow team members to customize their response by tracking their responsibilities during an exercise.

![Blueprint CITE Roles OE](img/blueprintCiteRoles-v3.png)

##### Add a CITE Role

![Blueprint Add CITE Roles OE](img/blueprintAddCiteRoles.png)

To add a CITE role, follow these steps:

1. Navigate to the **CITE Roles** tab.
2. Click the **+** icon.
3. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field    | Data Type     | Description                          | Example                              |
| -------- | ------------- | ------------------------------------ | ------------------------------------ |
| **Name** | String        | Name of the role                     | Team Leader                          |
| **Team** | Dropdown Text | Select the team this role applies to | Conference of State Bank Supervisors |

After you've added all desired configurations, click **Save**.

##### Edit a CITE Role

To edit the CITE Role's details, follow these steps:

1. Navigate to the **CITE Roles** tab.
2. Select the role you want to edit and click **Edit** next to it. The system opens the same edit component used when creating a new role.
3. Make your changes, then click **Save**.

##### Delete a CITE Role

To delete a CITE Role, follow these steps:

1. Navigate to the **CITE Roles** tab.
2. Select the role you want to delete and click the **Trash Can** next to it.

##### Filter Roles by Team

To filter Roles by team, follow these steps:

1. Navigate to the **CITE Roles** tab.
2. On the **Team** dropdown, select the desired team to filter by.

##### Search for a Role

To search for a specific role, follow these steps:

1. Navigate to the **CITE Roles** tab.
2. Click the **Search Bar** and type the name of the desired role.

#### Gallery

If enabled, Blueprint can push MSEL information to the Gallery application. When the MSEL owner enables Gallery integration, additional tabs appear in the Blueprint side panel. The MSEL owner must configure the additional tabs to complete the integration.

To enable full Gallery integration, the MSEL owner should configure the following tabs.

##### Gallery Data Fields

On the **Data Fields** tab, configure additional data fields to push MSEL information to Gallery.

![Blueprint Data Fields Tab OE](img/blueprintDataFields-v5.png)

The MSEL owner should add these fields:

##### Data Format Table

| Field                 | Data Type     | Description                                                               | Example                                       |
| --------------------- | ------------- | ------------------------------------------------------------------------- | --------------------------------------------- |
| **Description**       | String        | Information, details, and characteristics of the event                    | `STARTEX`                                     |
| **Date Posted**       | Datetime      | Posted date of the event                                                  | `1/24/2024, 14:18:27`                         |
| **Delivery Method**   | Dropdown Text | How is the event delivered to participants                                | `Gallery`                                     |
| **Name**              | String        | Name of the event                                                         | `Common Operating Picture 1`                  |
| **From Org**          | Dropdown Text | Select the organization that sends this event                             | `CC News`                                     |
| **To Org**            | Dropdown Text | Select the organization that receives this event                          | `All`                                         |
| **Summary**           | Rich Text     | Complete information and details of the event                             | `A ransomware attack affected Bank XYZ...`    |
| **Card**              | Dropdown Text | Select the Gallery Card to categorize this event                          | `Information Technology Sector`               |
| **Status**            | Dropdown Text | Approval status of the event used on the MSEL                             | `Approved`                                    |
| **Source Type**       | Dropdown Text | Select where the event's details come from                                | `News`                                        |
| **Source Name**       | String        | Add the author of the event                                               | `BBC News`                                    |
| **Move**              | Dropdown Text | Move number that the event is part of                                     | `1`                                           |
| **Group**             | Integer       | Groups events within a move, often executed within the same time range    | `1`                                           |
| **Url**               | String        | Provide a URL if more information is necessary for participants to access | `www.bbcnews.com/ransomware-attack-xyzbank`   |
| **Open in a New Tab** | Boolean       | Use this option to open the URL in a new tab                              | `True`                                        |

After adding data fields, map each one to the appropriate Gallery field by selecting an option from the **Gallery Article Parameter** dropdown when adding or editing the field.

On the Scenario Events tab, a field mapped to the **Status** parameter offers Gallery's article statuses as a dropdown of colors: `Gray`, `Green`, `Yellow`, `Orange`, and `Red`. These are the colors the article's card shows on the Gallery wall. Values from older MSELs that use Gallery's former status names show as the matching color.

##### Gallery Cards

On this tab, the MSEL owner can add [Gallery Cards](#glossary) to push from Blueprint. These are the different cards in the Gallery Wall. You can also find articles related to that card here.

![Blueprint Gallery Cards OE](img/blueprintGalleryCards-v2.png)

##### Add a Gallery Card

![Blueprint Add Gallery Cards OE](img/blueprintAddGalleryCards.png)

To add a Gallery card, follow these steps:

1. Navigate to the **Gallery Cards** tab.
2. Click the **+** icon.
3. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field                | Data Type     | Description                                                   | Example                                |
| -------------------- | ------------- | ------------------------------------------------------------- | -------------------------------------- |
| **Name**             | String        | Name of the Gallery card                                      | Energy Sector                          |
| **Card Description** | String        | Information, details, and characteristics of the Gallery card | The current state of the Energy Sector |
| **Move**             | Dropdown Text | Select the move to display this Gallery card                  | 0                                      |

After you've added all desired configurations, click **Save**.

##### Edit a Gallery Card

To edit a Gallery card, follow these steps:

1. Navigate to the **Gallery Cards** tab.
2. Select the card you want to edit and click **Edit** next to it. The system opens the same edit component used when creating a new card.
3. Make your changes, then click **Save**.

##### Delete a Gallery Card

To delete a Gallery card, follow these steps:

1. Navigate to the **Gallery Cards** tab.
2. Select the card you want to delete and click the **Trash Can** next to it.

##### Search for a Gallery Card

To search for a specific card, follow these steps:

1. Navigate to the **Gallery Cards** tab.
2. Click the **Search Bar** and type the name of the desired card.

#### Player

If enabled, Blueprint pushes MSEL information to the Player application. The system adds additional tabs to the Blueprint side panel, which the MSEL owner will need to configure.

To enable the full Player integration, configure the following tabs.

##### Player Apps

On the [Player Apps](#glossary) tab, the MSEL owner can add configurations to push from Blueprint. These are the different applications accessible from the [Player View](#glossary).

![Player Apps OE](img/blueprintPlayerApps-v3.png)

##### Add a Player App

![Blueprint Add Player App OE](img/blueprintAddPlayerApp-v3.png)

To add a Player app, follow these steps:

1. Navigate to the **Player Apps** tab.
2. Click the **+** icon.
3. Select an app template from the provided list.
4. Fill the fields as necessary following the Data Format Table specifications.

##### Data Format Table

| Field                  | Data Type | Description                                          | Example                                         |
| ---------------------- | --------- | ---------------------------------------------------- | ----------------------------------------------- |
| **Name**               | String    | Name of the application                              | Chat                                            |
| **Url**                | String    | URL of the application                               | `{vmUrl}/views/{playerViewId}`                  |
| **Icon Url**           | String    | URL of the application's icon                        | `{galleryUrl}/assets/img/archive-blue.png`      |
| **Embeddable**         | Boolean   | Select whether to embed the app in Player's iFrame   | True                                            |
| **Load in Background** | Boolean   | Select whether the app should load in the background | True                                            |

##### URL Parameters

You can add URL parameters to point to a specific CITE evaluation, Gallery exhibit, Player view, or other content. To do this, include the variables in braces when adding a new application. Consider using the following variables:

- **{citeEvaluationId}:** Add this variable to the URL to point to a specific CITE evaluation.
- **{galleryExhibitId}:** Add this variable to the URL to point to a specific Gallery exhibit.
- **{steamFitterScenarioId}:** Add this variable to the URL to point to a specific Steamfitter scenario.
- **{playerViewId}:** Add this variable to the URL to point to a specific Player view.
- **{casterWorkspaceId}:** Add this variable to the URL to point to the Caster workspace the MSEL deployed.
- **{vmUrl}:** Add this variable to the URL to point to the VM UI, for example `{vmUrl}/views/{playerViewId}`.

After you've added all desired configurations, click **Save**.

Each team shows a view once in Player. Two apps show the same view when they open the same page, whatever their names. CITE opens on the dashboard when the URL names no `section`, and Gallery opens on the archive. Blueprint refuses to give a team an app it has, or another app for a view it has, and names the app the team has. When you push the integrations, a team that still has two apps for one view gets only the first in its order.

##### Edit a Player App

To edit a Player app, follow these steps:

1. Navigate to the **Player Apps** tab.
2. Select the app you want to edit and click **Edit** next to it. The system opens the same edit component used when creating a new app.
3. Make your changes, then click **Save**.

##### Delete a Player App

To delete a Player app, follow these steps:

1. Navigate to the **Player Apps** tab.
2. Select the app you want to delete and click the **Trash Can** next to it.

##### Search for a Player App

To search for a specific app, follow these steps:

1. Navigate to the **Player Apps** tab.
2. Click the **Search Bar** and type the name of the desired app.

#### Caster

If enabled, Blueprint deploys the MSEL's virtual machine topology through [Caster](../caster/index.md). On push, Blueprint creates a workspace named `blueprint-{mselId}` in the chosen Caster directory, writes the Player view and team IDs into the workspace as Terraform variables, and plans and applies it. The VMs land in the MSEL's Player view, so the Caster integration requires the Player integration.

##### Choose the Caster Directory

The directory holds the Terraform template to deploy. To choose it, follow these steps:

1. Navigate to the **Info** tab.
2. Under **Applications to Integrate**, check **Player** and **Caster**.
3. In **Directory**, select the Caster directory, listed as *project / directory*.
4. Click **Save Changes**.

The directory list comes from Caster, so the user choosing it needs access to Caster. Without it, Blueprint shows **Caster directories could not be loaded. You need access to Caster to choose one.**

After a push, the **Workspace** row shows the workspace Blueprint created. Click **Open in Caster** to open the Caster project that holds it.

##### Map Teams to the Template

Blueprint writes each Player team's ID to a Terraform variable named after the team: lower case, with spaces turned into underscores. A template that names its team variables differently can still be used by mapping each MSEL team to one of the template's variables.

To map a team, follow these steps:

1. Navigate to the **Teams** tab. The **Topology Team** column shows each team's variable.
2. Click **Edit** next to the team, then select the template variable in **Topology Team**. Select **By team name** to go back to the variable the team's name gives.

Blueprint lists the string variables the template declares, except sensitive ones, ones the template already assigns, and the ones Blueprint writes itself: `view_id`, `user_id`, `username`, and the deprecated `exercise_id`.

The Teams tab warns when a team is mapped to a variable the template no longer declares, and when a template variable has no MSEL team. A variable with no MSEL team and no default in the template fails the Caster plan. If two teams end up with the same variable, the push fails and names the teams.

##### Give Participants the VMs

Participants reach the deployed VMs through the VM UI. Add a [Player app](#add-a-player-app) whose URL uses `{vmUrl}`, for example `{vmUrl}/views/{playerViewId}`. Until the MSEL has one, the **Info** and **Player Apps** tabs warn that participants will not be able to reach the Caster VMs.

##### Network Map

Blueprint draws a network map of the Caster workspace and gives it to the Player view as a VM [map](../player/index.md#maps), so participants can open a VM's console by clicking it on the map. The map shows each network as a colored bar and each machine as a box joined to the networks it is on. A machine is labeled with its name in Player, the name its console shows; a machine with no Player record keeps its Caster name, is drawn dashed, and cannot be clicked. A role given to a machine in Caster, such as *Domain controller*, appears above its name.

The map appears under **Caster** on the **Info** tab when the MSEL uses Player and Caster. Once the push has created the workspace, the MSEL owner sees a preview of the map, how many machines it shows, and which machines have no Player console.

- **Update map:** draws the map again from the workspace's saved layout and writes it to the Player view, replacing the map an earlier update made. If the view has no Network Map app, Blueprint adds one, named **Network Map**, for the teams that can open the mapped machines and the view's Admin team, and adds it to the MSEL's **Player Apps** too. **Update map** needs the Player view, so it is available after a push.
- **Refresh preview:** draws the preview again without sending anything to Player.
- **Edit layout in Caster:** opens the workspace in Caster. Open its **Topology View** to move machines and networks and to give machines roles, then return to Blueprint and click **Update map**.

Caster's topology view is the one place to edit the layout. Blueprint places machines and networks where they were saved in Caster's tree layout, or its force layout when nothing has a tree position. When nothing has been moved in Caster, Blueprint draws a network diagram: each network is a bar across the map, ordered so that a machine on two networks sits between their bars, and each machine hangs under its network, with links that cross no other machine. When some nodes have been moved in Caster, the ones never moved are placed beside them: networks in a row, each with its machines below it. Either way, the map can differ from what Caster draws for nodes nobody moved.

Changes made to the map in Player's map editor are overwritten by the next **Update map** or push. Other maps in the view are left alone.

Blueprint writes the map to the VM API set in its `ClientSettings:VmApiUrl` setting. When the setting is empty, the push makes no map and **Update map** says no VM API is configured.

#### Push Integrations

After adding all of the MSEL information and performing all necessary configurations, you can push information from enabled integrations to their respective applications.

![Blueprint Push Integrations OE](img/pushIntegrations-v2.png)

To push MSEL information to their respective applications, follow these steps:

1. Navigate to the **Info** tab.
2. Click **Push Integrations**.

When Caster is enabled, Blueprint deploys the topology last, after the other integrations, because the Terraform apply can take a long time.

After the apply, Blueprint makes the [network map](#network-map) in the Player view from the workspace's topology, and adds the Network Map app if the view has none. A workspace with no machines gets no map. If the map cannot be made, the push still finishes, and the status shows **Network map was not made in Player** with the reason; click **Update map** to try again.

#### Remove Integrations

If desired, the MSEL owner can remove MSEL information after the enabled integrations have pushed it to their respective applications.

![Blueprint Remove Integrations OE](img/removeIntegrations-v2.png)

To remove MSEL information from the applications, follow these steps:

1. Navigate to the **Info** tab.
2. Click **Remove Integrations**.

When Caster is enabled, Blueprint destroys the Caster workspace first, while the Player view its VMs belong to still exists. If the destroy fails, Blueprint stops and keeps the other integrations, so **Remove Integrations** can be run again.

## Glossary

This glossary defines key terms and concepts used in the Blueprint application.

**`Approver` Role:** Can view and edit the MSEL, but will have the added feature of approving a MSEL.

**Assessor View:** MSEL view used by Evaluators during an exercise to review xAPI evidence and submit competency ratings for teams. Directly accessible via the Assessor View URL on the Config tab.

**Blueprint**: Web application created to make the development of a MSEL and events easier.

**Caster:** Web application that deploys virtual machine topologies from Terraform templates organized in projects, directories, and workspaces.

**Catalog:** A reusable, named collection of scenario events associated with a specific inject type. Catalogs are defined in Administration → Catalogs and can be pulled into MSELs.

**CITE:** Web application that allows multiple participants from different organizations to evaluate, score, and comment on cyber incidents.

**CITE Action:** Series of steps to guide users on an appropriate course of action during an exercise.

**CITE Team Role:** What a team member can do in CITE: Owner, Submitter, Contributor, Member, or a role a CITE administrator adds. Set in each member's **Type** on the Teams tab and given to them in CITE when the MSEL is pushed. See [CITE Team Roles](#cite-team-roles).

**`CITE` Role:** Provide a set of responsibilities assigned to a user during an exercise.

**Competency:** A work role, task, knowledge, skill, or ability that comes from a competency framework and can be attached to a MSEL, a team, or a scenario event.

**Competency Framework:** A named collection of competencies (for example, NICE Framework) defined in Administration → Competencies and linked to a proficiency scale.

**Content Developer Permission:** Can view, edit, create, and approve events on the MSEL.

**Data Fields:** Structured components containing essential information about the event's characteristics, context, and implications.

**`Editor` Role:** Can edit the events assigned to them by the MSEL owner, but not other events. They cannot delete events or create new events.

**Event Dashboard:** Blueprint's homepage. Presents up to three cards (Join an Event, Start an Event, Manage an Event) tailored to the user's permissions.

**Events**: Specific scenario events or messages within the scenario that prompt users to implement designated actions.

**Exercises**: Structured and simulated activity designed to assess, train, or evaluate the capabilities, preparedness, and responses of individuals, teams, or organizations in dealing with various scenarios, especially emergencies or crises.

**Evaluator Permission**: Manages the exercise, can advance moves, execute events, and check events as completed.

**Gallery:** Web application where participants receive incident information.

**Gallery Card:** The Gallery wall displays Gallery cards. Each Gallery card represents an article category. Cards show a background color based on the most recent article's status and the number of unread articles. Content developers define cards within each Gallery collection.

**Group:** A named collection of users defined in Administration → Groups. Used for bulk permission management.

**Inject Type:** A named template describing the structure and data fields for a category of scenario events. Defined in Administration → Inject Types.

**Move Editor:** Can edit moves on the MSEL, as well as increment them during an exercise.

**Moves:** A defined period of time during an exercise, where exercise organizers distribute a series of events for users to discuss and assess current incident severity.

**MSEL:** (Master Scenario Events List) provides a timeline for all expected events, affiliated users and organizations during an exercise.

**MSEL Playbook:** A printable, paginated view of a MSEL's scenario events intended for use by exercise controllers.

**Organizations:** Entities within an exercise with defined roles, responsibilities, and functions.

**`Owner` Role:** Owner of the MSEL, can view and edit as well as perform all functionality that the MSEL provides (e.g., Add Teams, Add Integrations, Events).

**Player:** Centralized web interface where participants, teams, and administrators go to engage in a cyber event.

**Player Apps:** List of applications accessible from the same Player view.

**Player View:** These are the settings associated with building a view. You can edit, clone, and delete a view.

**Proficiency Scale:** A reusable, named set of performance levels (for example, Novice, Intermediate, Advanced, Expert) attached to a competency framework.

**Steamfitter:** Gives content developers the ability to create scenarios consisting of a series of scheduled tasks, manual tasks, and events which run against virtual machines during an event.

**System Admin Permission:** Can add users to a team, as well as assign the required permissions.

**System Defined Data Fields:** Added by default in MSEL creation, since data fields under this category are essential for MSEL features to work.

**User Defined Data Fields:** The user adds on an as-needed basis.

**`Viewer` Role:** Can view the MSEL, but can't do any edits to it.
