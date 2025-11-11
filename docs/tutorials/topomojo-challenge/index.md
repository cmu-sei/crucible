# Tutorial: Creating a Challenge in TopoMojo

This tutorial walks through creating a cybersecurity challenge in [TopoMojo](../../topomojo/index.md), from creating a workspace to configuring grading and deployment.

## Prerequisites

- Access to a TopoMojo instance
- Required role permissions for creating workspaces
- Basic understanding of virtualization and networking concepts
- Familiarity with VMware vCenter (for this tutorial, we use VMware vCenter as the hypervisor, but TopoMojo also supports other hypervisors like Proxmox)

## Overview

TopoMojo is a lab builder and player application for developing cybersecurity challenges. Each challenge lives in its own **workspace** that contains all virtual machines, artifacts, and configuration. When a user starts a challenge, the system deploys a read-only copy called a **game space** for participants to interact with.

## Step 1: Create a New Workspace

1. Log into TopoMojo
1. Click the **Create New Workspace** button
1. You'll get an empty workspace - this is where you'll build your challenge

## Step 2: Configure Workspace Metadata

### Workspace Title

The workspace title should be the actual challenge title that players will see. For example: "Network Traffic Analysis" or "Malware Removal".

### Description Field (Tags)

We suggest adding searchable tags in the description field to make challenges easier to find:

- Competition abbreviation (e.g., "PC" for Presidents Cup)
- Season identifier (e.g., "PC4" for Presidents Cup Season 4)
- Challenge ID (e.g., "PC4-001")
- Any other relevant keywords

### Author

Optionally add your name in the author field.

### Audience

The **audience** field controls which users can access your challenge:

- **Empty**: Challenge is not visible to any Gameboard instance (useful during development)
- **Specific audience** (e.g., "PresCup"): Challenge is visible to the production Gameboard
- **Playtest audience**: Challenge is visible to the Playtest Gameboard instance

**Tip:** Leave the audience empty while developing to prevent premature access.

### Duration

You can configure a time limit for the challenge. This field is optional.

## Step 3: Add Virtual Machines (Templates)

1. Navigate to the **Templates** section
1. Click to add VMs to your workspace
1. For each VM, provide a descriptive name

### VM Naming Best Practices

**For user-facing machines:**

Use clean, descriptive names without challenge IDs, such as:

- Workstation
- File-Server
- Web-Server

**For challenge infrastructure machines:**

You can include challenge IDs since users won't see these:

- challenge-server-g01
- challenge-server-pc4-001

### Hidden VMs

For infrastructure machines that users shouldn't directly access:

1. Select the VM in the templates list
1. Toggle the **Hidden** option to ON

When hidden, users won't see a console button for that VM in the deployed challenge. **Note:** This only hides the console - the VM is still accessible on the network.

### Saving VM Changes

To save changes made to a VM:

1. Click the **Unlink** button for that Template
1. In simple terms, this creates a new disk on the backend (More specifically, "Unlink" creates a new clone of the parent disk in VMware or a clone of the parent template in Proxmox)
1. Now, be sure to click **Save** in order for your changes to persist

**Important:** Click **Unlink** before modifying a VM. If you don't unlink, your changes won't persist. Also, you will have to remove a running linked VM (therefore losing all changes you made) in order to unlink a template, so be sure to unlink before modifying.

**Warning for this demo:** The demo will not click Unlink to avoid consuming unnecessary backend storage.

## Step 4: Configure Challenge Questions and Transforms

### Transforms

Transforms are dynamically generated values (like tokens) that make each deployed challenge unique. Unique challenge deployments increase the reusability value of challenges by avoiding cases where users already know the answers and prevents answer sharing between users.

### Setting Up Transforms

1. Navigate to the **Challenge** section
1. Click **Transforms** to see available types

### Common Transform Types

#### Hex Transform

Generates random hexadecimal characters:

**Key:** `token1`
**Value:** `hex` (generates 8 hex characters)
**Value:** `hex:16` (generates 16 hex characters)

**Note:** Other types like Base64 can include special characters that may break bash scripts or cause execution failures.

#### List Transform

Multi-Word List Transforms

Random words from a list.

**Key:** `list`
**Value:** `list:red blue green	one item`

**Key:** `list2`
**Value:** `list2:alpha beta gamma delta	two items (##key_1##, ##key_2##)`

**Key:** `list3`
**Value:** `list3:one two three four	three items`

Use for human-readable tokens or phrases. Participants can select more than one item from a list type transform.

#### ID Transform

Tracks the running exercise instance.

**Key:** `token_id`
**Value:** `id`

Useful for logging, tagging resources, or embedding run metadata into scripts.

#### Variant Transform

Provides the variant index for exercises with multiple versions.

**Key:** `variant_id`
**Value:** `variant`

Use when rotating content variants (e.g., different flag locations, tool versions).

#### Base64 Transform

Generates a random Base64-encoded token.

**Key:** `token_b64`
**Value:** `b64` (24 chars)
**Value:** `b64:16` (16 chars)

Avoid in shell-sensitive contexts—Base64 may include +, /, =.

#### UUID Transform

**Key:** `token_uuid`
**Value:** `uuid` (decimal string)
**Value:** `uid` (full GUID)

Ideal for correlation IDs inside services or audit trails.

#### Integer Range Transform

Random integer in a specified range.

**Key:** `rand_int`
**Value:** `int:999-9999` (999–9999)

Use for randomized port suffixes, team numbers, usernames.

#### App URL Transform

Reference the app URL from within a challenge (UI-side only).

**Key:** `lab_url`
**Value:** `app_url`

Not exposed to participants; useful in internal scripts or callback endpoints.

#### Grader URL Transform

Injects the grading endpoint for automated scoring flows.

**Key:** `grade_cb`
**Value:** `grader_url`

#### API Key / Grader Key Transform

Internal environment-side access tokens.

**Keys:** `apikey_key`, `grader_key`
**Values:** `same key name`

For backend validation, not participant-visible.

#### IPv4 Transform

Random IP address assignment from CIDR.

**Key:** `target_ip`
**Value:** `ipv4:10.0.2.0/24`

Example output: 10.0.2.137

Use for hiding static answers in networking labs.

### Creating Challenge Questions

1. In the Challenge section, add questions that users will answer
1. Reference transform variables using double-pound notation: `##variable_name##`

**Example Questions:**

**Question 1:**

```text
Text: "Enter token one"
Answer: ##token1##
```

**Question 2:**

```text
Text: "Enter token two"
Answer: ##token2##
```

When the challenge deploys, TopoMojo replaces the `##token1##` and `##token2##` placeholders with the actual generated values.

### Question Weights

You can assign different point values to questions:

- **Default (0):** All questions worth equal points
- **Percentage (0-100):** Question worth that percentage of total points
- **Decimal (0-1):** Also supported for percentage

**Example:**

- Question 1: Weight = 60 (worth 60% of points)
- Question 2: Weight = 40 (worth 40% of points)

### Example Answers

Always provide an example answer for each question showing the expected format.

**Examples:**

- For hex token: `a1b2c3d4e5f6`
- For IP address: `192.168.1.100`

### Grader Types

Select how to validate answers:

**Match:** Exact answer required (most common for transforms)

**Match Any:** Accept any of multiple answers separated by pipes

```text
Answer: Robert|Bob
```

User can enter either "Robert" OR "Bob"

**Match All:** Require all answers separated by pipes (in any order)

```text
Answer: Robert|Bob
```

User must enter "Robert|Bob" or "Bob|Robert"

**Match Alpha:** Strips special characters and matches only letters/numbers. Good for file paths:

```text
Answer: C:\Users\Desktop
```

Accepts `C:\Users\Desktop` or `C:/Users/Desktop`

## Step 5: Challenge Variants

Use variants when you need different VM configurations or questions for different deployments.

**Use variants when the following is true:**

- Different deployed VMs for different versions
- Different attached artifacts
- Different answers based on configuration

**Tip:** Try to avoid variants when possible. Use transforms and dynamic configuration instead.

### Creating Variants

1. Click **Add Variant** or **Clone Variant**
1. Clone Variant copies all settings from an existing variant
1. Modify questions or configurations as needed
1. System randomly deploys one variant when a user starts the challenge

## Step 6: Configure Guest Settings (Passing Transforms to VMs)

To use transform values inside your VMs, pass them as **guest info variables** (VMware guestinfo).

1. Expand a VM template's options
1. Add **Guest Settings**
1. Left side: Guest info variable name
1. Right side: Transform variable name

**Example:**

```text
guestinfo.token1 = ##token1##
guestinfo.token2 = ##token2##
```

### Accessing Guest Info Inside VMs

From within the VM, query guest info using VMware tools:

```bash
vmware-toolsd --cmd "info-get guestinfo.token1"
```

Or for cleaner syntax:

```bash
TOKEN1=$(vmware-toolsd --cmd "info-get guestinfo.token1")
echo $TOKEN1
```

Note: Specific command syntax may vary by VMware tools version (`vmware-toolsd` vs. `vmtoolsd`).

## Step 7: The Challenge Server

The challenge server is a special VM that handles:

- **Startup configuration**: Running scripts to configure other VMs when challenge starts
- **Grading**: Running scripts to verify user actions and provide tokens
- **DNS/DHCP**: Providing network services via dnsmasq
- **File hosting**: Serving files that users need to download

### Challenge Server Setup

The challenge server typically:

- Is **hidden** from users (no console access)
- Has an **unlinked** disk to save changes
- Contains scripts in `/challenge-server/custom-scripts/`
- Configured via `/challenge-server/config.yaml`

### Configuration File

**Startup Scripts:**

```yaml
startup_scripts:
  - my-startup.sh
```

**Hosted Files:**

```yaml
hosted_files:
  enabled: true
```

Files in `/challenge-server/hosted-files/` will be available for download

**Grading:**

```yaml
grading:
  enabled: true
  mode: button  # Options: button, cron, text, text-single
```

**Grading Modes:**

- **button:** User clicks "Grade Challenge" button to run grading script
- **cron:** Grading runs automatically at intervals
- **text:** Provides text input for answers inside the environment
- **text-single:** Single text box regardless of number of questions

**Note:** For most challenges, use Gameboard's built-in question/answer system rather than text-based grading in the environment.

### Grading Configuration

```yaml
grading_script: example-grading.py
token_location: guestinfo  # Read tokens from guest info variables

checks:
  - part_key: grading-check-1
    text: "Description shown to user"
    token_name: token1
  - part_key: grading-check-2
    text: "Another grading check"
    token_name: token2
```

## Step 8: Startup Scripts

Startup scripts configure your challenge environment when it's deployed. They run on the challenge server.

### Basic Startup Script Structure

```bash
#!/bin/bash

# Get transform values from guest info
TOKEN1=$(vmware-toolsd --cmd "info-get guestinfo.token1")

# SSH to another machine and configure it
ssh -i /root/.ssh/key user@hostname << EOF
  echo "$TOKEN1" > /tmp/token.txt
  # Additional configuration commands
EOF

# Log the result
echo "SSH command return value: $?"
echo "Done with startup configuration"
```

### Important Notes

- All startup scripts must be **executable**: `chmod +x script.sh`
- Scripts must be in `/challenge-server/custom-scripts/`
- Scripts run as **root** user
- Non-zero exit codes indicate failure
- All stdout/stderr logged to systemd journal

### Accessing Logs

View challenge server logs:

```bash
sudo journalctl -u challenge-server
```

### Restarting the Challenge Server Service

After modifying config.yaml:

```bash
sudo systemctl restart challenge-server.service
```

### Testing Scripts

**Important:** Test scripts by running them as root (since the challenge server service runs as root):

```bash
sudo /challenge-server/custom-scripts/my-startup.sh
```

## Step 9: Grading Scripts

Grading scripts verify that users completed required tasks.

### Grading Script Output Format

Your grading script must output results in this format:

```text
key: status message
```

**Success requires the word "success" in the value:**

```text
part-one: success
part-two: success - Configuration is correct
```

**Failure is anything without "success":**

```text
part-three: failure - Configuration file is invalid
```

### Example Grading Script (Python)

```python
#!/usr/bin/env python3

import subprocess

# Check if process is running
result = subprocess.run(['pgrep', 'malware.exe'], capture_output=True)

if result.returncode != 0:
    print("grading-check-1: success - Malware process stopped")
else:
    print("grading-check-1: failure - Malware process still running")

# Check if file exists
result = subprocess.run(['ssh', 'user@target', 'test -f /tmp/malware.exe'],
                       capture_output=True)

if result.returncode != 0:
    print("grading-check-2: success - Malware file removed")
else:
    print("grading-check-2: failure - Malware executable still present")
```

### Grading Best Practices

- Make scripts robust with good error handling
- Log all actions and results
- Provide helpful failure messages to guide users
- Test scripts thoroughly before deployment

## Step 10: Team Challenges and Replicas

For team-based challenges where multiple team members need to work simultaneously:

1. Select the VM that team members will use (e.g., an analyzer workstation)
1. Set **Replicas** to `-1`
1. With replicas set to -1, each team member gets their own copy:
   - Team of 3 x 3 copies: analyzer_1, analyzer_2, analyzer_3
   - Each replica has identical configuration and guest info variables

**Important:** Replicas all share the same guest info variables (tokens), so they have the same answers.

## Step 11: Cleanup and Preparation for Deployment

Before marking your challenge as ready:

### Clear Logs

Run the cleanup script to remove development logs:

```bash
/challenge-server/cleanup-journalctl.sh
```

This removes all systemd journal logs so users don't see your development debugging.

### Review Checklist

Let's take a moment to review:

- [ ] All VMs properly named
- [ ] Hidden VMs are actually hidden
- [ ] Unlinked challenge server disk
- [ ] All startup scripts are executable
- [ ] All scripts tested and working
- [ ] Transforms configured correctly
- [ ] Questions have example answers
- [ ] Grading scripts tested
- [ ] Logs cleared
- [ ] No command history or shortcuts left behind
- [ ] Audience field set appropriately

## Step 12: Save and Deploy

1. Save your workspace changes
1. Set the **Audience** field to make it visible to the appropriate Gameboard instance
1. In Gameboard, search for your challenge and add it to a game

## Common Issues and Troubleshooting

### Issue: Changes to VM Are Not Saving

- Solution: Click the Unlink button for that VM

### Issue: Startup Script Fails

- Check logs: `sudo journalctl -u challenge-server`
- Verify script is executable: `ls -la /challenge-server/custom-scripts/`
- Test as root: `sudo /challenge-server/custom-scripts/script.sh`

### Issue: Transform Values Not Appearing in VMs

- Verify properly configured guest settings
- Check variable names match (case-sensitive)
- Deploy a new game space (old one may have cached values)

### Issue: Grading Script Doesn't Provide Tokens

- Verify output format includes the exact key names from config.yaml
- Check that success messages contain the word "success"
- Review logs: `sudo journalctl -u challenge-server`

## Additional Resources

- [TopoMojo GitHub Repository](https://github.com/cmu-sei/TopoMojo)
- [Gameboard Documentation](https://github.com/cmu-sei/Gameboard)
- [CMU SEI Challenge Development Guidelines](https://resources.sei.cmu.edu/library/asset-view.cfm?assetID=889267) - Technical report with best practices
- Crucible Documentation: [Full Documentation Site](/)

## Summary

You've learned how to:

- Create a workspace in TopoMojo
- Add and configure VMs
- Set up transforms for unique challenge instances
- Create challenge questions with dynamic answers
- Configure the challenge server for startup and grading
- Write startup and grading scripts
- Prepare challenges for team competitions
- Clean up and deploy your challenge

**Next Steps:** Try creating a simple challenge following these steps, then explore more advanced features like variants and complex grading scenarios.
