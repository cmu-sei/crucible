# **CITE**
*Collaborative Incident Threat Evaluator*

## Overview

### What is CITE?

**CITE** is a web application created to integrate with the Crucible Framework and allows multiple participants from different organizations to evaluate, score, and comment on cyber incidents. CITE compares a user's score to their organization's score, group average scores, and the official exercise score. Scores are submitted for each move as the exercise progresses and each of the historical scores can be recalled for reference at any time.

In the CITE User Interface, there are two major functional sections:

- CITE Dashboard: The dashboard shows exercise details like the date and time, incident summary, a suggested list of actions for participants to consider taking, and suggested participant roles.
- CITE Scoresheet: The scoresheet compares participant scores to organization scores, group average scores, and the official score.

For installation, refer to these GitHub repositories.

- [CITE UI Repository](https://github.com/cmu-sei/CITE.Ui)

- [CITE API Repository](https://github.com/cmu-sei/CITE.Api)

### CITE Permissions
 
 In order to use CITE, a user must be assigned a scoring permission. 

 There are three levels of permissions in CITE that affect the way a team score is collaborated on and edited. 

 - Basic: Can only view the team score.
 - Modify: Can view and edit the team score.
 - Submit: Can view, edit, and submit the team score.

 Most users will have the modify-level permission; however, one or tow users per team have submit-level permission, meaning that only one or tho users per team can edit and/or submit on the team score.

 However, participants who can submit scores on behalf of their team can also add suggested action and participant roles to the CITE Dashboard. 
 
 Refer to this section for more information. [Actions to Consider and Roles]

## Administrator Guide

## User Guide

### Moves

In CITE, a Move is a defined period of time during an exercise, in which a series of injects are distributed for users to discuss and assess the current incident severity.

When in Dashboard view, users will have two features to interact with moves:

- Displayed Move: Move that will be currently dispalyed on the screen. Here, users can see response to previous moves and scores, but users will not be able to edit a response.

- Current Move: Move that is currently active. There are cases where the Displayed Move and the Current Move might be the same. Here, users are allowed to edit the category of the move.

### CITE Dashboard

The CITE Dashboard shows exercise details like the date and time, incident summary, a suggested list of actions for participants to consider taking, and suggested participant roles.

The following image will show some important hotspots about the CITE Dashboard. Reference the number on the hotspot to know more about this section.

![CITE Dashboard](../assets/img/CITE-Dashboard.png)


#### Active Incident & Moves
*Hotspot 1:* 

The name of the active incident and the move number currently displayed.

#### Situation Description, Date & Time
*Hotspot 2:*

The date and time of the situation displayed and a short description of the event.

#### Actions to Consider
*Hotspot 3:*

Users can see the different actions necessary to be executed during the exercise. These action times are for everyone on the team and "per move", meaning they change at each move of the exercise.

These are added to guide users on an appropriate course of action during an exercise. However, these actions are not connected to the scoresheet.

#### Roles
*Hotspot 4:* 

The roles are added so that each team member will have a clear path of their responsibilities during the exercise. For this, roles can be customized for each team and then the team members are going to decide what role each user is assigned.


#### Score Summary
*Hotspot 5:*

Displays the various scores at the appropriate severity level for the disaplyed move. Here, scores are always visible.

#### Dashboard & Scoresheet Toggle
*Hotspot 6:*

By using this icon, users can toggle between the CITE Dashboard and the CITE Scoresheet.

### CITE Scoresheet

The CITE Scoresheet compares participant scores to organization scores, group average scores, and the oficial score.

The following image will show some important hotspots about the CITE Scoresheet. Reference the number on the hotspot to know more about this section.

![CITE Scoresheet](../assets/img/CITE-Scoresheet.png)

#### Event Name
*Hotspot 1:*

The Event Name is the name of the current event.

#### Displayed Move
*Hotspot 2:*

The move currently displayed on the screen. Clicking < displays previous moves. Clicking > displays the current move. Using Displayed Moves, users can see responses to previous moves and scores but the user cannot edit a previous response.

#### Scoring Features
*Hotspot 3:*

- User: This is the participant's personal score for their reference only. The user score will also appear under the Score Summary range. 

- Team: Toggling the Team icon, displays how the team has scored this move so far. This is the score that the team collaborates on and submits for the current move. This score will be compared to the official score. The Team score appears under the Score Summary range.

- Team Avg: The average of all the users on the team. The Team Avg appears under the Score Summary Range for all moves except the current move.

- Group Avg: The average of all of the teams in the user's group. Group Avg appears under the Score Summary Range for all moves except for the current move.

- Official: The potential score; that is, how the incident should have been scored had it been a real-life scenario. Official score appears under the Score Summary Range for all moves except the current move.

- Submit: Submits the score indicating that the user is done scoring the current move. Click Yes or No. If the user clicks Yes, but changes their mind, click Reopen to edit the scoring.

- Clear: Clears any selections the user has checked but does not clear comments enterred. Selecting Clear returns to a score of 0.00.

- Preset: Sets the user's selections to the previous move score to use as a starting point for the current move.

#### Score Summary
*Hotspot 4:*

Displays the various scores at the appropriate severity level for the displayed move so that the scores are always visible.

#### Categories and Options
*Hotspot 5:*

Categories that are individually scored based upon the current move situation. For each category, select as many options as relevant. Selecting options assigns points to each category which are compiled to create the move score as defined by the scoring model.

**Add, Edit, and Delete a Comment**
When scoring a move, the user can attach a comment (or multiple comments) to a category.

- To add a comment, click ![Comment](../assets/img/comment.png). Enter the comment and click Save.
- To edit an existing comment, click ![Edit](../assets/img/edit.png). Make any changes, then click Save.
- To delete an existing comment, click ![Trash](../assets/img/trash.png). Clcik Yes to delete the comment.

When finished scoring the categories and adding comments, click Submit to submit the scores.

## CITE Tips
test


