# Admin Game Settings

You arrive at the **Admin Game Settings** by creating a new game or by editing an existing one. With the appropriate role granted to you, click **Admin** then either:

- click **+ New Game** to create a new game, or
- hover over an existing game card and click **Settings**

**Game id:** The game id is a unique string of alpha numeric characters displayed here and in the game URL. It is the primary key for a game in the database. The game id is automatically generated upon creating a new game and really only becomes important when an admin needs to investigate an issue in the database. 

## Metadata

**Name:** The title of your game. Displayed in the game lobby and on the scoreboard.

**Publish:** Toggle *Hidden* or *Visible* to make the game visible to players on the Home screen.

!!! info

    When a game is hidden, a user with permissions that can view both hidden and visible games -- such as Designer or Tester -- will see the game card on the Home screen; however, the game card appears with an "eye-slash" icon to denote that it is hidden.

**Key:** A short, unique key distinguishing *this* event.

**Series:** The name of the series; perhaps the same event is run annually making it a *series*.

**Track:** A course of action in your event. An event may have different categories for teams and individuals to compete in or an event may have an offensive skills track and defensive skills track.

**Season:**  A fixed time period for when a series occurred. The *series* describes the event---for example, a fictitious "Cyber Cup". The *season* is the iteration of that event---Cyber Cup: Season 1, Cyber Cup: Season 2, and Cyber Cup: Season 3.

**Division:** The tier or level of the audience participating in the event. Is this for working professionals or just students?

**Card Image:** Upload an image to become game tile or card to identify your game in the lobby.

**Card Text Top:** Enter text that will appear superimposed on your card at the top.

**Card Text Middle:** Enter text that will appear superimposed on your card in the middle.

**Card Text Bottom:** Enter text that will appear superimposed on your card along the bottom.

**Lobby Markdown:** Using Markdown enter any information you would like players to see when they enter the game lobby. For help with Markdown syntax, see this [Markdown Guide](https://www.markdownguide.org/).

**Feedback Questions:** It's possible -- but not required -- to create questions to capture participant feedback on the game and individual challenges. Create your feedback questions here using Yaml. For help with the feedback feature, see the [Feedback Form documentation](admin-feedback-form.md).

**About feedback templates:** This modal provides instructions for configuring questions presented to players after they complete a challenge or game.

**Paste Example Configuration:** Pastes sample feedback configuration into the Feedback Questions field for you to modify and use for your own needs.

**Certificate Template:** Design a certificate template here by entering HTML into the Certificate Template field. More information on certificates can be found in the [Playing in the Gameboard](participating.md) documentation under "The Profile screen" heading. For your convenience, selecting the **i** button displays instructions for designing a certificate that can dynamically display information related to a game, such as leaderboard rankings, player scores, and other details.

## Modes

**Player Mode:** Toggle to set the game to **Competition** or **Practice**. When at least *one* game is set to Practice in your environment, a link to Practice is visible in the top-right corner of gameboard for authenticated players. Players can click the Practice link and select a challenge start their practice session.

**Require Synchronized Start:** Toggle on to require the game to have a synchronized start. When enabled, no player can start a session until *all* players have indicated that they are "ready to play" in the game lobby. Use this feature for games when you want all players to start at the same time and end at the same time. Synchronized start adheres to other gameboard settings: for example, team size minimum and maximum.

!!! note

    The Admin Start feature bypasses the "ready to play" feature; that is, users assigned the `Admin` role can Admin Start regardless of whether all players have "readied up" or not.

**Show On Homepage When In Practice Mode:** 

**Engine Mode:** Specify the game mode (Standard, External, Legacy Unity Games). In VM mode, the gameboard reaches out to TopoMojo to start the VMs.

## Settings

These settings pertain to registration, execution, and general game and challenge limitations.

### Execution

**Opens:** The date and the time that your game begins.

**Closes:** The date and the time that your game ends.

**Session Duration:** The duration of game session in minutes. Games are created with a default session time of 60 minutes.

**Session Limit:** The maximum number of sessions -- a session is when a game is started and challenges can be deployed and solved -- per game.

**Gamespace Limit:** The maximum number of concurrent "gamespaces" allowed. A *gamespace* is the virtual environment that participants use to compete in a challenge. The default value is 0; the value that you enter here is inherited by a newly created board. For example, if you set this value to 5 in the game, any board created will inherit the 5 concurrent gamespace setting. 

**Max Submissions:** The maximum number of solutions a participant can send to the grading server per challenge---whether that submission is correct, incorrect, or blank. Once the submission amount is reached, the competitor is locked out of further submissions for that challenge.

**Allow Preview:** Toggle *Hidden* or *Visible* to allow participants to view a challenge and documentation prior to starting. You may want to prevent too much information from being given away before a challenge start.

**Allow Reset:** Toggle *Forbidden* or *Allowed* to permit participants to restart their game and attempt challenges again. This option is generally allowed on a "practice" game since that game is meant to help users get their bearings on how a competition works; however, you may decide players will be forbidden to reset an "official" game.

**Allow Late Starts:** Toggle *Forbidden* or *Allowed* to permit players to start within a session length of the execution period end. When toggled to allow, players whose session would end prematurely due to the execution window closing will be allowed to play; however, their session will be shortened to match the end of the game.

**Allow Public Scoreboard Access:** Toggle *Forbidden* or *Allowed* to permit players to view the complete scoreboard after the game ends. Not that the scoreboard itself is public, but if toggled to forbid, players can't view detailed score information for competing players.

### Registration

Offering a different execution period from registration period is an option. This gives participants the opportunity to register for a period of time prior to round one of the competition getting underway. No need for a registration period for later rounds where a competitor would have had to qualify for the next round to even continue.

**Opens:** The date and the time that your registration period begins.

**Closes:** The date and the time that your registration period ends.

**Team Size:** This is self-explanatory. A matching *minimum* and *maximum* of one means that the challenge is a single player challenge. That is, a "team" of one. In a true team tournament, two or more would probably be the minimum. 

**Team Sponsorship:** Toggle *Open* or *Required*.  When required, members on a team must have the same sponsor. Team sponsors are chosen during enrollment. For more information on sponsors, see [Gameboard Administration](admin.md).

**Registration Markdown:** Using Markdown enter any information you would like players to see when they register for the game. For help with Markdown syntax, see this [Markdown Guide](https://www.markdownguide.org/).

## Challenges

### Search

Search for challenges on TopoMojo to place in the game. *Search* here is limited by *Audience* on the workspace Settings in TopoMojo.

### Edit

Selecting a challenge from the search results adds it to the Edit icon.

#### Challenge Specs

**Sync with Source:** Synchronizes the Gameboard challenge markdown guide with the TopoMojo challenge markdown guide so that the content is the same in both apps.

**Support Key:** Assign a unique "key" here that gets appended to a TopoMojo gamespace id to help troubleshoot problems during competition. For example: `b28c7911 a03` --- **b28c7911** is the uniquely generated gamespace ID from TopoMojo; **a03** is the support key *manually* assigned here to a challenge. For more information on support keys, see [Gameboard Administration](admin.md).

**Points:** Assign a point value to your challenge here.

**Solution Guide URL:** Add links to challenge solution guides here for *practice mode* challenges. When added here, the link to the solution guide is available to players in the challenge instructions. Enabling **Show Solution Guide in Competitive Mode** permits the link to a solution guide to be available to players in a *competitive* game.

**Disabled:** Check to disable this challenge in the game without removing it. Disabled challenges are unavailable to players, don't count toward scores, and are not deployed when an admin initiates deployment on a player’s behalf. Essentially, disabling a challenge removes the spec from the game without deleting any underlying data.

**Hidden:** Hidden challenges can't be manually deployed by players and don't count toward scores. However, they are deployed when an admin initiates deployment on a player's behalf or if the game configured in External mode. The need for a hidden challenge is typically related to the implementation details of an externally hosted game and is not useful for the vast majority of games.

**Remove This Challenge:** Removes the challenge from the game.

##### Prereqs

Prerequisites are for challenges that unlock other challenges. For example: Your game has two challenges tagged **c01** and **c02**. You want to force participants to score 500 points on c01 before c02 is unlocked for them to attempt. Challenge c02 will not be available to deploy until the prerequisite condition is met. Complete the fields so that `c02 requires 500 on c01`.

##### Automatic Bonuses

Gameboard can automatically award bonus points to teams and players based on the order in which they solve a challenge. For instance, the first team to solve a challenge can earn an extra 100 points, the second team 50 points, and so on. To configure this, use the YAML format specified below.

**Paste this example configuration:** Click to paste the "hint" text contained in the textbox. This is a useful starting point for creating your own bonus structure YAML configuration.

**Import this configuration:** When you have finished editing your YAML, click to commit it to Gameboard.

### Map

Here is where the visual representation of the game is arranged.  Challenges are placed on the game map here. The challenges you selected appear as hotspots on the map. Drag them into position on the map.

**Show Grid:** Each map has a grid. By selecting Show Grid, you can toggle displaying the grid. When enabled, the grid overlays the map image and makes it easier for you place your challenge hotspots. 

**Browse:** Select **Browse** to search for an image that will serve as a backdrop to your map.

**Reset:** Removes the image from your map. Reset does not remove challenge hotspots.