# Tutorial: Creating a Game in Gameboard

This tutorial walks you through creating and configuring a cybersecurity competition game in [Gameboard](../../gameboard/index.md), from initial setup to adding challenges and publishing.

## Prerequisites

- Access to a Gameboard instance with a TopoMojo game engine
- Required permissions to create and manage games in Gameboard (**Director** or **Admin** role)
- TopoMojo challenges accessible to Gameboard (see [TopoMojo Challenge Tutorial](../topomojo-challenge/index.md))


## Overview

Gameboard is a platform for orchestrating cybersecurity competitions. Admins can create games (competitions) for users to participate in. Users complete challenges as part of games or practice their skills in the **Practice Area**. Review [Gameboard Key Concepts](../../gameboard/index.md#gameboard-concepts) before reading this guide.

## Table of Contents

- [Tutorial: Creating a Game in Gameboard](#tutorial-creating-a-game-in-gameboard)
  - [Prerequisites](#prerequisites)
  - [Overview](#overview)
  - [Table of Contents](#table-of-contents)
  - [Create a New Game](#create-a-new-game)
    - [Configure Game Metadata](#configure-game-metadata)
    - [Configure Game Settings](#configure-game-settings)
    - [Add Challenges to the Game](#add-challenges-to-the-game)
    - [Configure the Game Map](#configure-the-game-map)
    - [Game Modes](#game-modes)
  - [Facilitating a Game](#facilitating-a-game)
  - [Common Issues and Troubleshooting](#common-issues-and-troubleshooting)
    - [Issue: Challenge Not Appearing in Search](#issue-challenge-not-appearing-in-search)
    - [Issue: Players Can't Deploy Challenges](#issue-players-cant-deploy-challenges)
  - [Additional Resources](#additional-resources)

## Create a New Game

1. Log into Gameboard.
2. Click **Admin** in the navigation bar.
3. Click **New Game**. This takes you to **Game Center** for the new game.

### Configure Game Metadata

Click the **Settings Cog** from **Game Center** to configure the game settings. The first configuration area is for game metadata. Descriptions of a few key settings are below; see the [Gameboard documentation on the Game Center Settings](../../gameboard/index.md#metadata) for full details and additional settings.

1. **Name**: Name of the game that players will see on the home page and game lobby.
2. **Card Image**: Give this game a uniquely identifiable graphic. The suggested image size for cards is `750x1080`.
3. **Publish**: Toggle the game to **Visible** when you are ready for users to see it. Keep the game **Hidden** while you are still configuring.
4. **Lobby Markdown**: Include a markdown-formatted welcome message that users will see when they enter the game.
5. **Feedback Templates**: Optionally configure Game- and Challenge-level feedback. Feedback is useful for getting user thoughts on the competition and collecting demographic information (e.g., years of experience).
6. **Completion Certificate**: Optionally configure a completion certificate that users are automatically awarded. Users can earn a different certificate for competing in the competitive-mode game and for completing challenges in the **Practice Area**.

### Configure Game Settings

Scroll down and click to expand **Settings**. This area configures game execution and registration settings. Descriptions of a few key settings are below; see the [Gameboard documentation on the Game Center Settings](../../gameboard/index.md#settings) for full details and additional settings.

1. **Execution Open / Close**: The time window when users can play the game. Users cannot play the game beyond the end of the execution window. Users with active sessions will have their game session end at the close of the execution period. Games will automatically become available for users to begin playing at the start of the execution window.
2. **Registration Open / Close**: The time window when users can register to play the game. After registration closes, users cannot enroll in the game. Close registration before the execution period begins if you want to lock enrollment before the competition starts. Allow registration to continue to the end of the execution period to encourage users to play even if the game has already started. Open registration before the execution period begins to give users time to enroll and read instructions. Set registration to **None** to prevent users from self-enrolling; in this case, administrators will enroll users on their behalf.
3. **Session Duration**: The number of minutes users will have to compete in the game.
4. **Session Limit**: The number of teams that can play simultaneously. For example, set Session Limit to `5` to allow five teams to play at the same time; the sixth team to attempt will see a "Capacity full. Try again later." message until another team finishes their session. Limiting the number of simultaneous sessions can help manage infrastructure load.
5. **Gamespace Limit**: The number of challenges that each team can have open at one time. Set this to `1` to force teams to focus on a single challenge at a time, encouraging team focus and collaboration. Set this to `5` to allow teams to work on five challenges at once, encouraging a "divide and conquer" strategy. Limiting the number of challenges a team can deploy at once can help manage infrastructure load.
6. **Max Submissions**: The number of times a team can attempt submitting answers to a challenge. When a team uses all submission attempts, the team cannot submit more answers. *Note: clicking the **Submit** button submits answers for all questions in a challenge. You cannot submit an answer to an individual challenge question.*
7. **Team Size**: Set the minimum and maximum number of players that a team can have in the game. Set both the minimum and maximum to `1` to create an individual game. Set the minimum to `2` and the maximum to `5` to allow teams of between two and five players.

### Add Challenges to the Game

Challenges come from TopoMojo. The Gameboard instance requires a TopoMojo game engine configured with permissions to access challenges via an assigned **Scope** for the Gameboard user. See the [TopoMojo documentation on creating a workspace](../../topomojo/index.md#settings) and the [TopoMojo administrator guide](../../topomojo/index.md#users-tab) for more details.

1. Navigate to the **Challenges** tab of the **Game Center**.
2. Select **Search** and search for the challenges to add to the game. You can search by various TopoMojo workspace fields (e.g., title, description, tag). *Recall that the workspace must have an **audience** that matches the Gameboard user's **scope** for challenges to be visible to Gameboard.*
3. Click the challenge to add it to the game.
4. After adding all challenges to the game, click **Edit** on the left to adjust challenge configurations.
   1. **Points**: Assign the number of points this challenge is worth. Best practice is to award more points for challenges that are harder or take more time.
   2. **Solution Guide URL**: Optionally add a URL to a challenge solution guide. This solution guide will appear for all users when the game is in practice mode and shown to competitive mode users if you select the option to **Show Solution Guide in Competitive Mode**.

### Configure the Game Map

Gameboard provides a default grid where challenge click-points (where the users click to select a challenge) are overlaid, but best practice is to upload a custom map image. Click **Map** from the **Game Center Challenges** tab to upload a map image and rearrange the challenge click points.

### Game Modes

Gameboard supports two game modes. Competitive mode configures a timed competition where users solve as many challenges as possible in the allotted time to earn their spot on the leaderboard. Practice mode places the challenges in this game in the **Practice Area** where users can play the challenges as many times as they'd like to hone their skills.

Configure the **Game Mode** by selecting **Modes** from the **Game Center** settings area. See the [Gameboard documentation on the Practice Area](../../gameboard/index.md#practice-area) for more details.


## Facilitating a Game

Administrators have several options for monitoring and supporting an ongoing game. Brief overviews of a few key points are below. See the [Gameboard administrator documentation](../../gameboard/index.md#administrator-guide) for more details.

1. Navigate to **Admin > Live** for a live look at activity on Gameboard (e.g., active players, active challenges, etc.).
2. Use **Observe** from the **Game Center** to watch player consoles as they solve challenges. More details in the [Observe Challenges documentation](../../gameboard/index.md#observe-tab).
3. Support the competition by responding to user inquiries using the integrated **Support Area**. More details in the [Support Area documentation](../../gameboard/index.md#getting-in-game-support).
4. Monitor game / site / user metrics / feedback by clicking **Reports** in the navigation bar. More details about reports are available in the [Reports documentation](../../gameboard/index.md#reports).


## Common Issues and Troubleshooting

### Issue: Challenge Not Appearing in Search

1. Verify that Gameboard can reach the TopoMojo API over the network.
2. Verify that the TopoMojo workspace audience matches Gameboard's TopoMojo scope configuration.
3. Observe Gameboard and TopoMojo API logs for more information.

### Issue: Players Can't Deploy Challenges

1. Verify that the user has an active session in the game.
2. Verify that the game execution window in ongoing.
3. Verify that the user has not reached the game space limit.
4. Verify that Gameboard is able to reach the TopoMojo API over the network.
5. Observe Gameboard and TopoMojo API logs for more information.

## Additional Resources

- [Gameboard GitHub Repository](https://github.com/cmu-sei/Gameboard)
- [Gameboard Documentation](../../gameboard/index.md)
- [TopoMojo Tutorial](../topomojo-challenge/index.md) - Learn to create challenges
- [CMU SEI Challenge Development Guidelines](https://resources.sei.cmu.edu/library/asset-view.cfm?assetID=889267) - Best practices for competition design
- Crucible Documentation: [Full Documentation Site](../../index.md)
