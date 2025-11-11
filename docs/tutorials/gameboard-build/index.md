# Tutorial: Creating a Game in Gameboard

This tutorial walks you through creating and configuring a cybersecurity competition game in the Gameboard application, from initial setup to adding challenges and publishing.

## Prerequisites

- Access to a Gameboard instance
- Required permissions to create and manage games (Gameboard Admin role)
- Challenges already created in TopoMojo (see [TopoMojo Challenge Tutorial](/tutorials/topomojo-challenge/))
- Basic understanding of competition structure

## Overview

Gameboard is the platform for orchestrating cybersecurity competitions. It allows you to create games (competitions) that can be team-based or individual, single or multi-round, and contain multiple challenges that players attempt to solve.

## Step 1: Create a New Game

1. Log into Gameboard
1. Click **Admin** in the navigation
1. Click **New Game**

## Step 2: Basic Game Information

Fill out the essential game details:

### Game Name

This is the title players will see in the game lobby.

**Example:** "Cyber Cup 2026"

### Track

A category for your game, indicating the focus area.

**Examples:**

- Offensive Skills
- Defensive Skills
- Mixed Operations
- Incident Response

### Season

Marks which iteration of your game this is.

**Examples:**

- Season 1
- Season 2
- 2026 Spring

### Division

Designates your target audience.

**Examples:**

- Professionals
- Students
- Open (any participant level)
- Advanced

## Step 3: Game Card Visuals

The game card is what players see when browsing available competitions.

### Card Image

Upload a card image to visually represent your game:

1. Click the image upload area
1. Select your image file
1. **Recommended dimensions:** 750 x 1080 pixels

### Card Text

Optionally add text fields to provide additional information displayed on the card. This might include a brief description or key dates.

## Step 4: Lobby Markdown

The **Lobby Markdown** section allows you to provide detailed information that players see when they enter the game lobby.

You can use Markdown formatting to create:

- Headings
- Lists
- Bold/italic text
- Links
- Tables
- Code blocks

**Example:**

```markdown
## Welcome to Cyber Cup 6!

This competition tests your skills across:

- Network forensics
- Malware analysis
- Web application security
- Incident response

### Important Dates

- Registration Opens: March 1, 2026
- Competition Start: March 15, 2026
- Competition End: March 16, 2026

Good luck!
```

## Step 5: Feedback Configuration

Create feedback questions using YAML to gather insights from players after the competition.

**Example YAML:**

```yaml
questions:
  - id: difficulty
    text: "How would you rate the overall difficulty?"
    type: scale
    min: 1
    max: 5

  - id: favorite
    text: "Which challenge did you enjoy most?"
    type: text

  - id: improvements
    text: "What could we improve?"
    type: textarea
```

This allows you to collect structured feedback to improve future competitions.

## Step 6: Certificate Templates

Design certificates in HTML to recognize participant achievements.

**Example HTML Certificate:**

```html
<div style="text-align: center; padding: 50px;">
  <h1>Certificate of Achievement</h1>
  <p>This certifies that</p>
  <h2>{{player_name}}</h2>
  <p>participated in the Cyber Cup 2026</p>
  <p>Final Score: {{player_score}}</p>
  <p>Date: {{completion_date}}</p>
</div>
```

Variables like `{{player_name}}` will be automatically filled in for each participant.

## Step 7: Game Mode Selection

Choose between two game modes:

### Competitive Mode

- Timed competition
- Scoring and rankings matter
- Limited time to complete challenges

### Practice Mode

- Non-competitive learning environment
- No time pressure
- Players can access solution guides
- Good for training and skill development

Toggle the mode switch based on your needs.

## Step 8: Settings - Game Execution

Configure when and how the game runs:

### Execution Dates

Set the start and end date/time for when the competition is active:

- **Start Date/Time:** When players can begin attempting challenges
- **End Date/Time:** When the competition closes

### Session Limit

The maximum number of players or teams that can play simultaneously.

**Example:**

- 100 for individual competition
- 30 for team-based competition

### Deployment Limits

The maximum number of challenges each team or player can deploy at once.

**Example:**

- Set to 3 if you want players to focus on a few challenges at a time
- Set to 10 if you want them to have many challenges available simultaneously

**Note:** This helps manage infrastructure resources.

### Submission Attempts

Limit how many times a participant can submit answers for a challenge.

**Examples:**

- Unlimited (-1)
- 5 attempts per challenge
- 10 attempts per challenge

**Tip:** Unlimited attempts encourages learning, while limited attempts adds difficulty.

## Step 9: Settings - Gameplay Options

### Preview Option

Enable to let players see challenge details and instructions before starting.

**Use case:** Allows players to read challenge requirements and decide which ones to attempt based on their skills.

### Allow Resets

Decide if players can restart their game progress.

**Use case:** In practice mode, this allows players to retry challenges. Be sure to review this value in competitive mode, as it may not be desirable.

### Allow Late Starts

Let players join after the game has begun.

**Use case:** Useful for multi-day competitions where staggered entry is acceptable.

### Public Scoreboard Access

Enable this to allow players to see everyone's scores.

**Options:**

- During competition: Can create competitive atmosphere but might discourage trailing participants
- After competition: Standard practice to show final rankings
- Never: For scenarios where anonymity is important

## Step 10: Registration Settings

Configure how players sign up for the competition:

### Registration Dates

Define the registration window:

- **Registration Opens:** When players can begin registering
- **Registration Closes:** Deadline for registration

**Tip:** Close registration before the competition starts to finalize team counts.

### Registration Markdown

Provide formatted instructions on how to sign up for your game using Markdown.

**Example:**

```markdown
## How to Register

1. Create an account or log in
1. Complete your player profile
1. Form your team (see requirements below)
1. Submit your registration

### Team Requirements

- All team members must have the same sponsor
- Minimum team size: 2
- Maximum team size: 4
```

### Team Size Configuration

**Minimum Team Size:**

- Set to **1** for individual competition
- Set to **2 or more** for team competition

**Maximum Team Size:**

Define the largest allowed team size.

**Examples:**

- Min: 1, Max: 1 = Individual game
- Min: 2, Max: 4 = Team game with 2-4 members
- Min: 3, Max: 5 = Team game with 3-5 members

### Team Sponsorship Requirement

Enable this if team members must share the same sponsor organization.

**Use case:** For government or organizational competitions where teams must be from the same agency/company.

## Step 11: Adding Challenges

Now add challenges from TopoMojo to your game:

### Searching for Challenges

1. Navigate to the **Challenges** section
1. Use the search function to find challenges from TopoMojo
1. Filter by:
   - Challenge name
   - Tags
   - Difficulty
   - NICE Framework work roles

### Important: Audience Matching

**Critical:** The TopoMojo workspace **audience** must match the Gameboard's configured TopoMojo scope.

If they don't match, the challenge won't be visible in Gameboard.

**Example:**

- TopoMojo workspace audience: "PresCup"
- Gameboard TopoMojo scope: "PresCup"
- Challenge will be visible

### Adding a Challenge

1. Click **Add Challenge** or select from search results
1. Configure challenge settings:

**Points:**

Assign point values based on difficulty and time required.

**Examples:**

- Easy challenges: 100-300 points
- Medium challenges: 400-700 points
- Hard challenges: 800-1000 points

**Tip:** Follow guidelines from challenge testing to set appropriate point values.

**Solution Guide Links (Practice Mode):**
If running in practice mode, you can provide links to solution guides to help players learn.

Repeat for all challenges you want in the game

### Challenge Organization

Challenges can be:

- Organized by difficulty
- Grouped by NICE Framework work role
- Mixed to create varied experiences
- Tagged with specific themes or scenarios

## Step 12: Map Configuration (Optional)

Create a visual representation of your game:

1. Navigate to the **Map** section
1. Upload a map image
1. Place challenge click points on the map
1. Link each point to a specific challenge

### Tips for Map Creation

- Use Ctrl+Click or press and hold **Alt** key to resize challenge click points
- Position points to create a logical flow
- Keep challenge representations consistent
- Test click areas to ensure they're accessible

Gap: Specific key combinations for map editing may vary by browser/OS

## Step 13: Preview Your Game

Before launching, preview what players will see:

1. Navigate to the **Settings** tab
1. Click the **Lobby** link
1. This opens the player-facing view

Review:

- Game card appearance
- Lobby information clarity
- Challenge list and descriptions
- All settings are correct

## Step 14: Final Checklist

Before going live, verify:

- [ ] Game name, track, season, and division are correct
- [ ] Card image uploaded and displays properly
- [ ] Lobby markdown formatted and informative
- [ ] Execution dates set correctly
- [ ] Session and game space limits appropriate for your infrastructure
- [ ] Registration dates configured
- [ ] Team size requirements match your competition type
- [ ] All challenges added with appropriate point values
- [ ] Preview shows everything correctly
- [ ] Feedback questions configured
- [ ] Certificate template tested

## Step 15: Go-Live and Monitor

1. Save all settings
1. Announce the competition to participants
1. Monitor during the competition:
   - Player registrations
   - Active game spaces
   - Infrastructure load
   - Scoreboard progress

## Common Issues and Troubleshooting

Issue: Challenge not appearing in search

- **Solution:** Check that TopoMojo workspace audience matches Gameboard's TopoMojo scope configuration

Issue: Players can't deploy challenges

- **Solution:** Verify session limit and game space limit aren't exceeded

Issue: Certificate not generating

- **Solution:** Check HTML template syntax and verify all variable spelling

Issue: Scoreboard not updating

- **Solution:** Verify Gameboard's connection to TopoMojo and that it is receiving scoring data

## Competition Game Management

### Monitoring Player Progress

Track in real-time:

- Number of registered players/teams
- Active challenges deployed
- Completion rates
- Current standings

### Handling Support Requests

Players may need help with:

- Challenge deployment issues
- Connection problems
- Clarification on challenge requirements
- Technical difficulties

Have a support plan ready with clear communication channels.

### Adjusting Settings

Consider modifying some settings during the competition if necessary:

- Extending time limits
- Adjusting point values (use cautiously)
- Changing submission attempt limits

**Warning:** Major changes during competition can affect fairness. Make adjustments carefully and communicate them to all participants.

## After the Competition

### Post-Competition Tasks

1. **Close registration and competition**
1. **Review feedback** from player surveys
1. **Generate certificates** for participants
1. **Export final scoreboard** for records
1. **Document lessons learned** for future competitions
1. **Release solution guides** (if appropriate)
1. **Archive game data** for future reference

### Analyzing Results

Review:

- Which challenges had highest/lowest completion rates
- Time spent on each challenge
- Point distribution fairness
- Player feedback themes
- Technical issues encountered

Use this data to improve future competitions.

## Additional Resources

- [Gameboard GitHub Repository](https://github.com/cmu-sei/Gameboard)
- [TopoMojo Tutorial](/tutorials/topomojo-challenge/) - Learn to create challenges
- [CMU SEI Challenge Development Guidelines](https://resources.sei.cmu.edu/library/asset-view.cfm?assetID=889267) - Best practices for competition design
- Crucible Documentation: [Full Documentation Site](/)

## Summary

You've learned how to:

- Create a new game in Gameboard
- Configure game metadata and visuals
- Set up game execution parameters
- Configure registration and team settings
- Add challenges from TopoMojo
- Create maps and visual representations
- Preview and publish your competition
- Monitor and manage an active game
- Handle post-competition tasks

**Have fun creating your game, and good luck!**
