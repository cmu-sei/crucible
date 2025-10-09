---
title: Gameboard-Admin Challenges
---

# Challenges

The **Challenges** tab helps users with elevated permissions in Gameboard (e.g., Admin, Director, Support) troubleshoot challenge-related issues.

After logging into the Gameboard app with the appropriate role, click **Admin**, then **Challenges**.

**Search:** Search for specific teams, players, challenge IDs, and tags.

**Current:** A green dot indicates an active challenge. A challenge remains active until one of three conditions occurs: a correct solution is submitted, the maximum number of submissions is reached, or the session expires. Once any of these conditions occur, the challenge becomes inactive. Inactive challenges still contribute to total score, rank, and cumulative time.

A challenge remains current and not archived until the **Reset Session** button resets it.

**Archived:** When a participant clicks **Reset Session**, Gameboard archives the participant and challenge event data before deleting the session. Admins can access this archived information here.

**Submissions:** Displays timestamps and the player's submitted answers stored in Gameboard.

**Game Engine Audit:** Click **Audit from game engine?** to query the game engine (for Crucible, this is TopoMojo) and retrieve its record of submitted answers. Use this audit to compare Gameboard's submission records with the game engine's.

**Regrade:** Click **Regrade** to have Gameboard recheck all player submissions against the current list of correct answers. Use regrade when a challenge gains new acceptable answers or when players submit correct answers in an unexpected format. After manually updating the TopoMojo workspace with the new correct answers, regrade the challenge to update player scores accordingly.
