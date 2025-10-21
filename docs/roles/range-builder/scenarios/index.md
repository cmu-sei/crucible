# Scenario Development

This section covers reasoning, designing, and implementing automated cyber scenarios using Steamfitter's orchestration capabilities.

Steamfitter enables you to create dynamic, responsive scenarios that adapt to participant actions and progress through predetermined timelines. As a Range Builder, your goal is to combine **automation, decision points,** and **branching logic** to create engaging, realistic training experiences.

---

## Scenario Design Principles

| Principle               | Description                                                                                                          |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------- |
| **Gradual Difficulty**  | Begin with guided tasks, then layer complexity. Offer multiple approaches and stretch challenges for advanced users. |
| **Realistic Timelines** | Align scenario flow with real-world operations: detection → investigation → response → recovery.                     |
| **Adaptive Response**   | Use branching logic: success leads forward, failure redirects to remediation or alternate routes.                    |
| **Learning Resilience** | Build recovery mechanisms so participants can recover from common missteps and still complete the experience.        |

---

## Scenario Architecture

### Task Grouping

Organize activities into logical **phases**:

| Phase                    | Focus                    | Example Tasks                                              |
| ------------------------ | ------------------------ | ---------------------------------------------------------- |
| **1. Initial Detection** | Introduce the incident   | Automated alert, manual acknowledgment, initial assessment |
| **2. Investigation**     | Deepen analysis          | Evidence collection, threat classification, documentation  |
| **3. Response**          | Drive participant action | Containment, communication, remediation                    |
| **4. Recovery**          | Reinforce learning       | Restoration, lessons learned, final reporting              |

---

### Task Relationships

| Relationship Type | Description                         | Example                                     |
| ----------------- | ----------------------------------- | ------------------------------------------- |
| **Sequential**    | Tasks that must complete in order   | Run scan → analyze results                  |
| **Parallel**      | Tasks that can occur simultaneously | Collect logs and review alerts concurrently |
| **Conditional**   | Triggered by specific outcomes      | Continue only if "analysis complete"        |
| **Recovery**      | Activated when failure occurs       | Launch fallback evidence-collection task    |

---

### Timing and Triggers

| Example                | Description                        | Timing                      | Action                   | Expected Outcome       |
| ---------------------- | ---------------------------------- | --------------------------- | ------------------------ | ---------------------- |
| **Initial Compromise** | Simulate attacker scanning network | 5 minutes into scenario     | Network scan executes    | Scan completes         |
| **Lateral Movement**   | Triggered by previous success      | 15 minutes after compromise | Lateral movement attempt | Connection established |

---

### Human Decision Points

| Decision                    | Description                            | Trigger     | Expected Outcome                  | Time Limit |
| --------------------------- | -------------------------------------- | ----------- | --------------------------------- | ---------- |
| **Incident Classification** | Team classifies incident severity      | Manual      | "High severity" response selected | 30 minutes |
| **Management Notification** | Notifies management if severity = high | Conditional | Notification sent                 | None       |

---

### Outcome Branching

| Path              | Description                  | Trigger | Action                              | Expected Result |
| ----------------- | ---------------------------- | ------- | ----------------------------------- | --------------- |
| **Success Path**  | Containment verified         | Success | Run containment verification script | "Contained"     |
| **Recovery Path** | Containment failed, escalate | Failure | Execute escalation procedure        | "Escalated"     |

---

### Assessment Integration

| Assessment Task               | Description                                 | Trigger | Points |
| ----------------------------- | ------------------------------------------- | ------- | ------ |
| **Evidence Collection Check** | Verify that the team gathered evidence correctly | Manual  | 25     |
| **Timeline Reconstruction**   | Evaluate accuracy of event timeline         | Success | 30     |

---

## Common Scenario Patterns

### Incident Response Scenario

| Phase             | Task                 | Description                  | Trigger               | Notes                          |
| ----------------- | -------------------- | ---------------------------- | --------------------- | ------------------------------ |
| **Detection**     | IDS Alert Generation | Generate initial alert       | Time-based (3 min in) | Alert appears on SOC dashboard |
| **Detection**     | SOC Notification     | Display alert to analysts    | After IDS alert       | Confirms visibility            |
| **Investigation** | Initial Triage       | Analyst acknowledges alert   | Manual                | 10-min limit                   |
| **Investigation** | Log Analysis         | Collect logs for review      | After triage          | Automated collection           |
| **Investigation** | Artifact Analysis    | Review artifacts for malware | After log analysis    | 30-min limit                   |

---

### Red vs. Blue Team Exercise

| Team          | Task                   | Purpose                           | Trigger                  | Key Points                   |
| ------------- | ---------------------- | --------------------------------- | ------------------------ | ---------------------------- |
| **Red Team**  | Initial Reconnaissance | Identify targets via network scan | Time-based (5 min)       | Foundational offensive stage |
| **Red Team**  | Initial Access         | Gain system foothold              | After reconnaissance     | Manual task, 1-hour window   |
| **Red Team**  | Persistence            | Maintain access                   | After initial access     | Confirms persistence         |
| **Blue Team** | Monitoring Alert       | Detects Red activity              | Triggered by Red success | 10-min detection delay       |
| **Blue Team** | Threat Hunting         | Investigate alert                 | Manual                   | 40-min task                  |

---

### Penetration Testing Lab (Progressive Difficulty)

| Level            | Task                     | Description              | Trigger          | Difficulty   | Points | Time Limit |
| ---------------- | ------------------------ | ------------------------ | ---------------- | ------------ | ------ | ---------- |
| **Beginner**     | Network Discovery        | Identify live hosts      | Immediate        | Beginner     | 10     | None       |
| **Beginner**     | Port Scanning            | Identify open services   | After discovery  | Beginner     | 15     | None       |
| **Intermediate** | Vulnerability Assessment | Identify vulnerabilities | After scanning   | Intermediate | 25     | —          |
| **Advanced**     | Exploit Development      | Develop custom exploit   | After assessment | Advanced     | 50     | 1 hour     |

---

## Advanced Scenario Techniques

### Evolving Scenarios

| Step                         | Description                        | Trigger       | Timing    | Expected Result                      |
| ---------------------------- | ---------------------------------- | ------------- | --------- | ------------------------------------ |
| **Phase 1 Completion Check** | Verify all initial tasks completed | Time-based    | 30 min    | Phase 1 complete                     |
| **Phase 2 Initialization**   | Launch next phase                  | After success | Immediate | Phase 2 initialized with new injects |

---

### Adaptive Difficulty

| Step                       | Description                | Trigger          | Frequency    | Adjustment                           |
| -------------------------- | -------------------------- | ---------------- | ------------ | ------------------------------------ |
| **Performance Assessment** | Evaluate team progress     | Time-based       | Every 15 min | Record performance                   |
| **Difficulty Adjustment**  | Modify scenario complexity | After assessment | As needed    | Increase complexity or provide hints |

---

### Cross-Team Interaction

| Task                             | Description                          | Team | Trigger     | Delay | Outcome             |
| -------------------------------- | ------------------------------------ | ---- | ----------- | ----- | ------------------- |
| **Red Team: Network Compromise** | Red team compromises network segment | Red  | Manual      | —     | Network compromised |
| **Blue Team: Alert Generation**  | Alert triggered by Red activity      | Blue | Red success | 5 min | Alert sent to SIEM  |

---

## Best Practices

| Focus Area                | Recommendations                                                                                                                           |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| **Scenario Design**       | Define learning objectives clearly; map each task to a skill; balance difficulty; include timing and recovery paths.                      |
| **Task Organization**     | Use descriptive names; group related tasks logically; handle errors gracefully; provide meaningful feedback; document logic.              |
| **Testing & Maintenance** | Test thoroughly before deployment; monitor live execution; collect feedback; revise scenarios after exercises; version control templates. |

---

*For deeper insights on assessment and scenario evaluation, see:*
**[Self-Assessment in Training and Exercise](https://sei.cmu.edu/library/self-assessment-in-training-and-exercise/) :material-open-in-new:**
