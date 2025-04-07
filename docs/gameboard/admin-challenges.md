# Challenges

The Challenges tab is largely for users who have elevated permissions in Gameboard (e.g., Admin, Director, Support) troubleshoot problems with challenges.

After logging into the Gameboard app with the appropriate role, click **Admin**, then **Challenges**.

**Search:** Search for specific teams, players, challenge ids, and tags.

**Current:** Active challenges have a green dot next them. Active indicates a challenge has not yet been solved correctly, maximum submissions have not been attempted, or a session has not expired. A challenge is active until one of those three criteria are met; then, the challenge is over and inactive. When a challenge is inactive, it still counts towards total score, rank, cumulative time.

A challenge is current and not archived because it has not been reset when the **Reset Session** button is clicked.

**Archived:** When a participant clicks **Reset Session**, before the session is deleted, historical information from participant and challenge event is archived. This archived session information is available here for the game admin to access.

**Submissions:** Displays a date and time stamp and the answers submitted by the player and stored in Gameboard.

**Game Engine Audit:** Clicking **Audit from game engine?** queries the game engine (if you're using the Crucible stack, this is TopoMojo) to provide its list of submitted answers. The purpose of game engine audit is to compare Gameboard's record of submissions to the game engine's record of submissions.

**Regrade:** Clicking **Regrade** has the Gameboard check all submissions by a player against expected answers again. Regrade is used when a challenge has two possible answers or players submit in a format that is correct, but that was unexpected by game or challenge developers. When this happens, the TopoMojo workspace is updated manually to include the new correct answers. Then, the challenge is regraded and the player's score updated if a previously entered incorrect answer is now correct.
