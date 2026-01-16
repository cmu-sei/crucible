# Creating a Game in Gameboard

Gameboard organizes challenges into games and manages how users interact with them, either as part of a competition or in the **Practice Area**.

This tutorial walks you through creating and running a cybersecurity competition game in [Gameboard](../../gameboard/index.md) in two steps. In **Step 1**, you create and configure a new game, including metadata, settings, challenges, and the game map, and prepare it for participants. In **Step 2**, you facilitate the game and manage participant interaction during gameplay.

Before starting this tutorial, review [Gameboard Key Concepts](../../gameboard/index.md#gameboard-concepts) in the *Gameboard Guide*.

## Assumptions

This tutorial assumes the following:

- You have access to a working Gameboard instance, integrated with a TopoMojo game engine.
- You have the required permissions to create and manage games in Gameboard (**Director** or **Admin** role).
- TopoMojo challenges already exist and are accessible to Gameboard.
- You understand basic Gameboard concepts, such as games, challenges, and the Practice Area.

## Step 1: Creating the New Game

1. Log in to Gameboard.
2. In the top navigation bar, click **Admin**.
3. Click **New Game** to open the new game in the Game Center.

### Configuring Game Metadata

Click the **Settings Cog** in **Game Center** to configure the game. Start by configuring the game metadata. Descriptions of key settings appear below. For additional details on game metadata, see [Game Center Settings](../../gameboard/index.md#metadata) in the *Gameboard Administrator Guide*.

- **Name**: Enter the game name that players see on the home page and in the game lobby.
- **Card Image**: Select a graphic to visually identify the game. Use an image sized `750x1080`.
- **Publish**: Set the game to **Visible** when users can access it. Keep the game **Hidden** while you configure it.
- **Lobby Markdown**: Add a Markdown-formatted welcome message that users see when they enter the game.
- **Feedback Templates**: (Optional) Configure game-level and challenge-level feedback to collect participant input, such as experience level or comments.
- **Completion Certificates**: (Optional) Configure a certificate that the system awards automatically. Users can earn separate certificates for competitive play and for completing challenges in the **Practice Area**.

### Configuring Game Settings

Scroll down and expand **Settings** to configure game execution and registration behavior. Descriptions of key settings appear below. For additional details on game settings, see [Game Center Settings](../../gameboard/index.md#settings) in the *Gameboard Guide*.

- **Execution Opens/Closes**: Define the time window when users can play the game; users cannot play outside this window, and the system ends active sessions when the window closes.
- **Registration Opens/Closes**: Define when users can register; close registration before execution to lock enrollment, allow it during execution to encourage late participation, or set it to **None** to prevent self-enrollment.
- **Session Duration**: Set the number of minutes each team can play the game.
- **Session Limit**: Set the maximum number of teams that can play at the same time to manage infrastructure load.
- **Gamespace Limit**: Set the maximum number of challenges a team can run concurrently to control focus and resource usage.
- **Max Submissions**: Set the number of submission attempts per challenge; clicking **Submit** sends answers for all questions.
- **Team Size**: Set the minimum and maximum number of players per team.

### Choosing the Game Mode

Now expand **Modes**.

Gameboard supports two **Player Modes**: **Competition** and **Practice**. Each mode controls how users access and interact with the challenges in the game.

In **Competition** mode, Gameboard runs the game as a timed competition. Participants solve as many challenges as possible within the allotted time to earn points and placement on the scoreboard.

In **Practice** mode, Gameboard makes the challenges available in the **Practice Area**. Users can play challenges multiple times at their own pace to practice and improve their skills.

For additional details on game modes, see [Modes](../../gameboard/index.md#modes) in the *Gameboard Administrator Guide*. For additional details on how users access challenges in practice mode, see [Practice Area](../../gameboard/index.md#practice-area) in the *Gameboard User Guide*.

### Adding Challenges to the Game

Because challenges come from TopoMojo, the Gameboard instance requires TopoMojo integration (see the [deployment documentation](https://github.com/cmu-sei/helm-charts/tree/main/charts/gameboard#game-engine-topomojo-integration) for more details). For additional details on TopoMojo workspaces and user permissions, see the TopoMojo [Settings](../../topomojo/index.md#settings) topic and the [Users Tab](../../topomojo/index.md#users-tab) topic in the *TopoMojo Guide*.

1. Navigate to the **Challenges** tab in **Game Center**.
2. Select **Search** and search for challenges to add to the game. You can search by TopoMojo workspace fields such as title, description, or tag.

    !!! warning "Challenge Visibility"

         The workspace audience must match the Gameboard user's scope for challenges to appear.

3. Click a challenge to add it to the game.
4. After adding all challenges, click **Edit** on the left to adjust challenge configuration.

   - **Points**: Set the number of points the challenge is worth. Assign higher values to more difficult or time-consuming challenges.
   - **Solution Guide URL**: (Optional) Add a URL to a challenge solution guide. The system always shows this guide in practice mode and shows it in competitive mode only if you enable **Show Solution Guide in Competitive Mode**.

### Configuring the Game Map

Gameboard provides a default grid that overlays *click points*, which are the locations participants click to select challenges. As a best practice, upload a custom map image instead of using the default grid. In **Game Center**, open the **Challenges** tab, click **Map**, upload a map image, and reposition the challenge click points.

## Step 2: Facilitating the Game

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
2. Verify that the TopoMojo workspace audience matches the Gameboard TopoMojo scope configuration.
3. Review Gameboard and TopoMojo API logs for additional details.

### Players Cannot Deploy Challenges

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
