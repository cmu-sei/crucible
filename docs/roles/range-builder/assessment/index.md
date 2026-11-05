# Assessment Integration

!!! abstract "SEI Digital Library"

    We documented our own experience with team-based assessments in :material-book: [Self-Assessment in Training and Exercise](https://sei.cmu.edu/library/self-assessment-in-training-and-exercise/).

## Suggestions for Scoring

Implement scoring based on task completion:

| Task                      | Points | Criteria                  | Weights   |
| ------------------------- | ------ | ------------------------- | --------- |
| **Evidence Collection**   | 100    | Completeness / Timeliness | 60% / 40% |
| **Threat Classification** | 150    | Accuracy / Justification  | 80% / 20% |
| **Mitigation Steps**      | 200    | Effectiveness / Speed     | 70% / 30% |

Monitor participant progress throughout scenarios:

| Element      | Description                                                  |
| ------------ | ------------------------------------------------------------ |
| **Name**     | Progress Checkpoint                                          |
| **Purpose**  | Track participant progress                                   |
| **Trigger**  | Time-based (every 10 minutes)                                |
| **Duration** | 2-hour scenario (12 intervals)                               |
| **Metrics**  | Tasks completed, time per task, accuracy rate, help requests |
| **Result**   | Progress data recorded                                       |

## Assessment in MSEL-Based Exercises

In an exercise built in Blueprint, Blueprint and CITE serve different purposes.

| Application | What It Captures | Who Uses It |
| ----------- | ---------------- | ----------- |
| **Blueprint** | Ratings of each team against the MSEL's competencies, recorded as xAPI statements | Users with the **Owner** or **Evaluator** role on the MSEL, in the [Assessor View](../../../blueprint/index.md#assessor-view) |
| **CITE** | Each team's assessment of the incident's severity, move by move | Participants, on the [CITE Scoresheet](../../../cite/index.md#cite-scoresheet) |

To prepare competency assessment, follow these steps:

1. On the MSEL's **Competencies** tab, attach a competency framework and add competencies. See [Competencies](../../../blueprint/index.md#competencies).
2. Associate the competencies with teams and scenario events.
3. On the **Data Fields** tab, check the **Assess** column for each data field the evaluators should see.
4. On the **Contributors** tab, give each evaluator the **Evaluator** role.

Blueprint records each rating in the Learning Record Store (LRS), so an administrator must configure xAPI for Blueprint.

!!! note

    CITE doesn't grade participants. Teams compare their answers to the official score to discuss how they judged the incident. Use the [CITE Report](../../../cite/index.md#cite-report) and the **Submissions** page in CITE's Administration View to support the debrief.

## Testing and Validation

The following sections cover a scenario testing checklist and common issues to watch for before going live.

### Scenario Testing Checklist

- All task dependencies resolve correctly
- Timing intervals are realistic
- VM targeting works properly
- Expected outputs are achievable
- Failure paths provide recovery options
- Assessment points are fair and measurable

### Common Issues and Solutions

The following sections describe common problems and how to address them.

#### Timing Problems

When tasks are complex, allow flexibility in timing rather than enforcing strict expiration.

Example (conceptual): Complex Analysis Task

- **Purpose:** Provide learners adequate time for in-depth malware analysis
- **Trigger:** Manual (instructor or participant initiated)
- **Recommended Duration:** 60 minutes, with a 10-minute warning before expiration
- **Extensions:** Permitted when analysis is ongoing or productive

#### VM Targeting Issues

Ensure tasks align with the correct virtual machine or environment context.

Example (conceptual): Targeted Task

- **Purpose:** Deliver activity only to appropriate analyst systems
- **Target Type:** Analyst VM
- **Filters:**
      - Assigned team or role tags (e.g., `team_name`, `analyst`)
      - Active and powered-on systems only

#### Dependency Failures

Prepare fallback paths for tasks that rely on automated or chained actions.

Example (conceptual): Evidence Collection Workflow

Primary Task:

- **Action:** Automated evidence collection script (`auto_collect.sh`)
- **Expected Result:** Evidence successfully gathered

Fallback Task:

- **Trigger:** Automation failure
- **Action:** Manual evidence collection procedure
- **Expected Result:** Evidence successfully gathered
- **Hint:** Reference manual collection steps in documentation

These examples show how to anticipate and design around operational issues without requiring users to modify system configurations.
