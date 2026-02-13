# Creating a Game in Gameboard

Gameboard organizes challenges into games and manages how users interact with them, either as part of a competition or in the **Practice Area**.

This tutorial walks you through creating and running a cybersecurity competition game in [Gameboard](../../gameboard/index.md) in three steps. In **Step 1** and **Step 2**, you create and configure a new game, including metadata, settings, challenges, and the game map, and prepare it for participants. In **Step 3**, you facilitate the game and manage participant interaction during gameplay.

Before starting this tutorial, review [Gameboard Key Concepts](../../gameboard/index.md#gameboard-concepts) in the *Gameboard Guide*.

??? question "How Does Gameboard Communicate with TopoMojo?"

    Gameboard communicates with the TopoMojo API by authenticating as a dedicated TopoMojo user, commonly called `gameboard-client` or `gameboard-bot` (shown in the screenshot of the Admin Dashboard in TopoMojo below).  This user, configured by a TopoMojo administrator to have the desired permissions/scope (see permissions section for more details), is used for all Gameboard interactions to the TopoMojo API. Setup details appear in Gameboard's Helm chart documentation under [Game Engine Integration](https://github.com/cmu-sei/helm-charts/tree/main/charts/gameboard#game-engine-topomojo-integration).

    ![The gameboard-client user in TopoMojo's admin dashboard](../../tutorials/img/topo-gameboard-client-user.png)

## Assumptions

This tutorial assumes the following:

- You understand basic Gameboard concepts, such as games, challenges, and the Practice Area.
- You have access to a working Gameboard instance, integrated with a TopoMojo game engine.
- You have the required permissions to create and manage games in Gameboard (**Director** or **Admin** role).
- TopoMojo challenges already exist and are accessible to Gameboard. If you *don't* have a TopoMojo challenge, follow the steps in the [Creating a TopoMojo Challenge](../topomojo-challenge/index.md) tutorial to make one!

:blue_book: As you work through this tutorial, you may want to learn more about the Crucible applications or features. If so, refer to the [Related Resources](#related-resources) section below for additional detail and reference information.

!!! example

     This tutorial uses screenshots with sample data to illustrate the steps. Any values shown in a monospace font and wrapped in backticks (for example, `Tutorial` for **Game Name**) are placeholders. Replace these values with your own to follow along and create a new game.

## Step 1: Creating the New Game

1. Log in to Gameboard.
2. In the top navigation bar, click **Admin**.
3. Click **New Game** to open the new game in the Game Center.

### Configuring Game Metadata

Click the **Settings Cog** in **Game Center** to configure the game. Start by configuring the game metadata. Descriptions of key settings appear below. For additional details on game metadata, see [Game Center Settings](../../gameboard/index.md#metadata) in the *Gameboard Administrator Guide*.

- **Name:** Enter the game name that players see on the home page and in the game lobby. For the purposes of this tutorial, enter `Tutorial`.
- **Card Image:** Select a graphic to visually identify the game. Use an image sized `750x1080`. Add text to appear on the card here too. In **Card Text Middle**, enter `tutorial`.
- **Publish:** **Hidden** requires elevated permissions to view; **Visible** allows all users to see the game. We recommend keeping the game **Hidden** while you configure it.
- **Featured:** Enable this to display the game at the top of the homepage.
- **Lobby Markdown:** Add a Markdown-formatted welcome message that users see when they enter the game. Add the following text: `Welcome to the **Tutorial Game**.`
- **Feedback Templates:** (Optional) Configure game-level and challenge-level feedback to collect participant input, such as experience level or comments.
- **Completion Certificates:** (Optional) Assign a certificate that the system awards automatically. Users can earn separate certificates for competitive play and for completing challenges in the **Practice Area**.

### Choosing the Game Mode

Expand **Modes**. Gameboard supports two **Player Modes**: **Competition** and **Practice**. Each mode controls how users access and interact with the challenges in the game.

- In **Competition** mode, Gameboard runs the game as a timed competition. Participants solve as many challenges as possible within the allotted time to earn points and placement on the scoreboard.
- In **Practice** mode, Gameboard makes the challenges available in the **Practice Area**. Users can play challenges multiple times at their own pace to practice and improve their skills.

For additional details on game modes, see [Modes](../../gameboard/index.md#modes) in the *Gameboard Administrator Guide*. For additional details on how users access challenges in practice mode, see [Practice Area](../../gameboard/index.md#practice-area) in the *Gameboard User Guide*.

### Configuring Game Settings

Expand **Settings** to configure game execution and registration behavior. Descriptions of key settings appear below.

- **Execution Opens/Closes:** Define the time window when users can play the game; users cannot play outside this window, and the system ends active sessions when the window closes. Make the execution range one month starting today (e.g., `2026-01-01T:00:00:00+00:00` and `2026-02-01T:00:00:00+00:00`).
- **Registration Opens/Closes:** Define when users can register; close registration before execution to lock enrollment, allow it during execution to encourage late participation, or set it to **None** to prevent self-enrollment.
- **Session Duration:** Set the number of minutes each team can play the game. Enter `60`.
- **Session Limit:** Set the maximum number of teams that can play at the same time to manage infrastructure load. A `0` here indicates an unlimited number of sessions.
- **Gamespace Limit:** Set the maximum number of challenges a team can launch concurrently. Set it to `1`.
- **Max Submissions:** Set the number of submission attempts per challenge; clicking **Submit** sends answers for *all* questions whether or not a user entered an answer in the answer box.
- **Allow Preview:** Controls the cut line behavior from TopoMojo. Anything below the cut line in the TopoMojo document stays hidden until the user deploys the challenge. If you toggle it to visible, Gameboard ignores the cut line and shows the full document from the start.
- **Allow Reset:** Lets players reset their progress. Use **Allow Reset** for tutorial, practice, or test games when you want users to reset their session and start over as if they have never played before.
- **Late Start** Controls whether players can begin a session when less time remains than the full session duration. If enabled, a player can start shortly before the game ends, even though they will not receive the full session time.
- **Team Size:** Set the minimum and maximum number of players per team.
- **Registration Markdown:** Enter additional markdown instructions here. The markdown you enter here should be related to registering for your game. For example, enter: `Please read the rules before registering!`.

For additional details on game settings, see [Game Center Settings](../../gameboard/index.md#settings) in the *Gameboard Guide*.

## Step 2: Adding Challenges to the Game

Because challenges come from TopoMojo, the Gameboard instance requires TopoMojo integration (see the [deployment documentation](https://github.com/cmu-sei/helm-charts/tree/main/charts/gameboard#game-engine-topomojo-integration) for more details). For more details on TopoMojo workspaces and user permissions, see the TopoMojo [Settings](../../topomojo/index.md#settings) topic and the [Users Tab](../../topomojo/index.md#users-tab) topic in the *TopoMojo Guide*.

1. Click the **Challenges** tab in **Game Center**.
2. Select **Search** and search for challenges to add to the game. You can search by TopoMojo workspace fields such as title, description, or tag. Search for `TopoMojo Walkthrough` (this is the tutorial you made in [Creating a TopoMojo Challenge](../topomojo-challenge/index.md)). You can, of course, search for any integrated challenge here.
3. Click the challenge to add it to the game.
4. After adding all challenges, click **Edit** on the left to adjust challenge configuration.

   - **Points:** Set the number of points the challenge is worth. Assign higher values to more difficult or time-consuming challenges.
   - **Solution Guide URL:** (Optional) Add a URL to a challenge solution guide. The system always shows this guide in practice mode and shows it in competitive mode only if you enable **Show Solution Guide in Competitive Mode**.

### Configuring the Game Map

Gameboard provides a default grid that overlays *click points*, which are the locations participants click to select challenges. As a best practice, upload a custom map image instead of using the default grid.

1. Still in the **Game Center**, click the **Challenges** tab.
2. Click **Map**.
3. Upload a map image, and reposition the challenge click points. Hold down the **Alt** key while dragging to resize the click point.

Congratulations! You have a game! Let's test it.

### Testing the New Game

1. In the top navigation bar, click **Home**.
2. Under **Featured Games**, hover over the **Tutorial** game, and click **Open Game**.
3. Notice our registration markdown.
4. Click **Enroll**, then **Confirm**.
5. Click **Start Session**, then **Confirm**.
6. Click **Continue to Gameboard**.
7. Hover over the click point to see your **TopoMojo Walkthrough** challenge worth 100 points, then select it.
8. Click **Start Challenge**, then **Confirm**.

Gameboard sends a request to TopoMojo to create a gamespace, and TopoMojo communicates with the hypervisor to deploy the virtual machines. That completes the process of creating a game, adding a challenge to it, and launching a challenge in the game.

## Step 3: Facilitating the Game

Administrators have several options for monitoring and supporting an active game. Brief descriptions of key capabilities appear below. For additional details on administrative features, see the [Gameboard Administrator Guide](../../gameboard/index.md#administrator-guide).

- Navigate to **Admin**, **Live** to view real-time activity on Gameboard, such as active players and active challenges.
- Use the **Observe** tab in **Game Center** (**Admin**, **Games**, select a game card, **Observe**) to watch player consoles as they work through challenges.
- Respond to player questions using the integrated **Support Area**.
- Monitor game, site, and user metrics and review feedback by clicking **Reports** in the top navigation bar.

For additional details on facilitating a game, see:

- [Observe Tab](../../gameboard/index.md#observe-tab) in the *Gameboard Administrator Guide*.
- [Getting In-Game Support](../../gameboard/index.md#getting-in-game-support) in the *Gameboard User Guide*.
- [Reports](../../gameboard/index.md#reports) in the *Gameboard User Guide*.

## Troubleshooting Common Issues

### Challenge Not Appearing in Search

1. Verify that Gameboard can reach the TopoMojo API over the network.
2. Review Gameboard and TopoMojo API logs for additional details.
3. Verify that the TopoMojo workspace **audience** matches the Gameboard user's (`gameboard-client`) TopoMojo **scope** configuration.

??? tip "Understanding Audience and Scope"

     Audience and Scope are defined in TopoMojo.

     - **Audience:** Audience is set on a challenge *workspace* and defines the group of users the challenge is intended for.
     - **Scope:** Scope is set on a user and defines which audiences a user can access. A user can only deploy a challenge if the challenge's audience matches in that user's scope. If audience and scope don't match, the user can't launch the challenge.

     Gameboard uses a dedicated TopoMojo user (called `gameboard-client`). That user, like any TopoMojo user, has a scope. Therefore:

     - If the Gameboard user is scoped to `prescup`
     - Then Gameboard can only deploy challenges with an audience of `prescup`

     That scope controls what Gameboard is allowed to launch.

     If you set the audience to `everyone`, any TopoMojo user (including the `gameboard-client` user) can deploy that challenge through the **Play** interface in TopoMojo and through a game in Gameboard.

### Players Can't Deploy Challenges

1. Verify that the user has an active session in the game.
2. Verify that the game execution window is open.
3. Verify that the user has not reached the gamespace limit.
4. Verify that Gameboard can reach the TopoMojo API over the network.
5. Review Gameboard and TopoMojo API logs for additional details.

## Related Resources

- [Challenge Development Guidelines for Cybersecurity Competitions](https://www.sei.cmu.edu/library/challenge-development-guidelines-for-cybersecurity-competitions): This paper draws on the SEI's experience to provide general-purpose guidelines and best practices for developing effective cybersecurity challenges.
- [Crucible Documentation](../../index.md)
- [Gameboard Guide](../../gameboard/index.md)
- [Gameboard GitHub Repository](https://github.com/cmu-sei/Gameboard)
- [Creating a TopoMojo Challenge Tutorial](../topomojo-challenge/index.md): Learn how to create and configure a TopoMojo challenge.
