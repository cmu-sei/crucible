# Feedback Form

In the Gameboard application, it is possible to create feedback forms to capture participant feedback on the game and individual challenges. Collecting and analyzing participant feedback can help you refine and improve your games and challenges.

To create a feedback form, you must have the *Administrator* role in the Gameboard. For more information about roles, please see the [Gameboard Roles Guide](admin-roles.md).

Creating a feedback form is not required to build a game or challenge. It is up to you whether or not you choose to implement it. If feedback is not configured in the gameboard administration section, then the feedback form does not appear for the participant. You can create questions for *just* a game, *just* challenges, or *both* game and challenges. See the "Configuration YAML" sections below for additional detail.

## Creating feedback form for the game

This documentation assumes that you have been granted the Administrator role in gameboard, you are logged in, and you have a game created.

In the top-right corner, click **Admin**.

Hover over an existing game, then click **Settings**.

Scroll down to the **Feedback Questions** sections. If this is blank, then you will have to add your own YAML here. The YAML format has to be correct and there is some validation built in. 

**Feedback Questions** is blank when:

- no defaults are set and a new game is created

**Feedback Questions** is populated when: 

- some YAML has been saved from a previous session
- defaults have been established and a new game is created
- a game is cloned where original game contained save YAML

!!! note

    When cloning a game, the feedback YAML will always be copied *exactly* from the original game, even if the game's feedback YAML is blank. In this instance, the default template is never used. 

If your questions are built correctly, you will see a message confirming this:

`x game, y challenge questions configured`

Where `x` and `y` are the numbers of questions in your YAML template.

If your questions are not built correctly, you will see a message stating:

`Invalid YAML format`

Invalid YAML format prevents the feedback form from appearing on a game or challenge to a participant. Invalid YAML format does not prevent saving the game. The game is saved - the gameboard functions as expected - however, it will look to participants as if no questions were configured.

### Configuration YAML

Review the following sample YAML for a *game* feedback form to understand the keys and their values. Notice in the example below there is `game:` but not `challenge:`. The feedback questions are for the game only; no questions appear for challenges.

```yaml
# Default Template for Game and Challenge Feedback
message: This feedback is not monitored during competition.
game:
- id: q1
  prompt: Please rate the difficulty of this game.
  shortName: Difficulty
  type: likert
  max: 10
  minLabel: Very Easy
  maxLabel: Very Difficult
  required: true
- id: q2
  prompt: What did you like about this game?
  type: text
```

- **message:** in the example above, _message_ is a communication meant for the participants; here the message informs participants that the feedback form is not monitored by competition user support. They would need to seek technical support through other established channels. Message is configurable and can be left blank. If left blank, no message appears to the participants.
- **id:** *ids* must be unique within the `game` list or `challenge` list; i.e., game and challenge could each have a `q1` id. If ids in a single list are not unique, you are presented with a warning. The warning does not prevent saving the configuration and the feedback form is still visible to a participant.
- **prompt:** this is the question you want the participant to answer or the property you want them to rate.
- **shortName:** an abbreviated version of the prompt. `shortName` is optional, but is helpful for use in tables as the column header. Good examples are "Difficult" or "Quality". 
- **type:** *likert* or *text*; if the type is Likert, then defining the scale (`max`, `minLabel`, `maxLabel`) of how much a participant can agree or disagree with your prompt is required. If the type is *text*, a participant is free to answer your prompt however they like. Text type questions have a 2,000 character limit.
- **max:** this is the upper extreme of your Likert scale; 10 is the recommended upper limit. Any integer greater than 1 will work, but a scale that goes past 10 may become unwieldy or awkward.
- **minLabel:** specify the labels for the extremes of your Likert scale; examples of the negative extreme might be "very easy", "strongly disagree", or "very dissatisfied".
- **maxLabel:** specify the labels for the extremes of your Likert scale; examples of the positive extreme might be " very difficult", "strongly agree", or "very satisfied".
- **required:** this key is optional; set `required` to `true` if you want to make your question required. 

## Creating feedback form for a challenge

The procedure for creating challenge feedback questions is the same as creating game feedback questions, except that you specify `challenge` in the YAML template. Review the following sample YAML with *game* and *challenge* feedback questions specified. You could specify `challenge:` and omit `game:` here. In that case, no questions would appear for the game; just the challenges. 

You can't configure a unique set of questions for each challenge. Each challenge in a game gets the same questions as defined in the YAML template. So, `Challenge abc` gets the same set of questions as `Challenge xyz` provided that `Challenge abc` and `Challenge xyz`  are attached to the same game. 

```yaml
# Default Template for Game and Challenge Feedback
message: This feedback is not monitored during competition.
game: 
- id: q1
  prompt: Please rate the difficulty of this game.
  shortName: Difficulty
  type: likert
  max: 10
  minLabel: Very Easy
  maxLabel: Very Difficult
  required: true
- id: q2
  prompt: Please rate the quality of this game.
  shortName: Quality
  type: likert
  max: 5
  minLabel: Low Quality
  maxLabel: High Quality
challenge:
- id: q1
  prompt: Please rate the difficulty of this challenge.
  shortName: Difficulty
  type: likert
  max: 5
  minLabel: Low Quality
  maxLabel: High Quality
- id: q2
  prompt: Please rate the quality of this challenge.
  shortName: Quality
  type: likert
  max: 10
  minLabel: Very Easy
  maxLabel: Very Difficult
```

## Changing questions mid-game

It is possible to change these keys in the YAML question configuration at any point during a game: `prompt`, `shortName`, `minLabel`, and `maxLabel`.  These keys are for display purposes, and the administrator should be able to fix a typo or a spelling error on the fly.

Do not change these keys in the YAML question configuration during a game, especially if participants have already submitted feedback: question `id`, `type`, `max` (when `likert`), the order of questions, or the number of questions in the list.

## Configuration settings

**Defaults_FeedbackTemplateFile** is the file name/location in the app directory where the default yaml file is found. To configure the **Defaults_FeedbackTemplateFile**:

1. Create a yaml file with the template that is desired to be the default of any new games.
2. Add that file to app files prior to deployment. 
3. In `appsettings.conf` or via whatever method you choose to set environment variables, set **Defaults_FeedbackTemplateFile** as the file name of the new file (for example: `default-template.yaml`). 

### Additional notes 

Configuring default questions is completely optional and not required to have feedback working in a deployment. It is an additional feature to make things more efficient, especially in the case where it is likely all games will use the same or similar questions. It could also be used as an example of valid yaml with placeholder values that the creator needs to change like "prompt 1 here". 

If no defaults are set (i.e., there is no yaml file and no envar set), then any new game will have a blank text area to start from or to copy/paste something into. If there is a default question file provided, any new game is created with the contents of the default questions copied into the text area for yaml. 

Once a game is created, changing the feedback questions of a game does not change the original defaults of the app for any new games, and questions can be completely changed or removed. Finally, changing the default feedback question file after a game is created (this is only done at deployment) does not change the questions of any existing game as the YAML was copied and not linked to it anymore.

## Reporting

Viewing and exporting survey responses is also a function of the Administrator role. Assuming that you are logged into gameboard as an administrator, in the top-right corner, select **Admin**. Then click **Reports**, and **Feedback Reports**.

To view *game* feedback, toggle **Game Feedback**, and select a game. Game feedback refers to feedback at the game level; it does not include challenge-specific feedback.

- **Game Overview:**  Provides some general metrics like the count of configured questions, types, and required and the number of responses **submitted**, **in progress**, and **not started**. **Not started** indicates participants who started a game and the survey was available to them, but they chose not to respond.
- **Question Summary:** Provides metrics for the numerical questions like **Average** (based upon submitted responses), **Lowest Rating**, **Highest Rating** and a count of questions answered. All aggregates are based on submitted only, even min and max which are "Lowest Rating" and "Highest Rating".
- **Export to CSV (summary):** Allows you to export summary statistics per question as a comma-separated values file. From here, you can perform more detailed analysis.
- **Individual Submissions:** Provides a list of questions and answers by player.
  - **Export to CSV (submission):** Allows you to export submitted feedback report results as a comma-separated values file. From here, you can perform more detailed analysis. Each row is a single user's response. The number of columns depends on the number of questions configured.

To view *challenge* feedback, toggle **Challenge Feedback**, then select a game and a challenge. Selecting **All Challenges** here aggregates the feedback for all challenges for a particular game.

- **Challenge Overview:**  Provides some general metrics like the count of configured questions, types, and required versus the number of responses **submitted**, **in progress**, and **not started**. **Not started** indicates participants who started a challenge and the survey was available to them, but they chose not to respond. 
- **Question Summary:** Provides metrics for the numerical questions like **Average** (based upon submitted responses), **Lowest Rating**, **Highest Rating** and a count of questions answered. 
  - **Export to CSV (summary):** Allows you to export summary statistics per question as a comma-separated values file. From here, you can perform more detailed analysis.
- **Individual Submissions:** Provides a list of questions and answers by player.
  - **Export to CSV (submission):** Allows you to export submitted feedback report results as a comma-separated values file. From here, you can perform more detailed analysis. Each row is a single user's response. The number of columns depends on the number of questions configured.