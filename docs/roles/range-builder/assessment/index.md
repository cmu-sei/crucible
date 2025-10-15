# Assessment Integration

???+ tip "Deep Dive into CERT Research"
    Our own experience with team-based assessments is documented in :material-book: *[Self-Assessment in Training and Exercise](https://sei.cmu.edu/library/self-assessment-in-training-and-exercise/)*.

## Suggestions for Scoring

1. Implement scoring based on task completion:

| Task                      | Points | Criteria                  | Weights   |
| ------------------------- | ------ | ------------------------- | --------- |
| **Evidence Collection**   | 100    | Completeness / Timeliness | 60% / 40% |
| **Threat Classification** | 150    | Accuracy / Justification  | 80% / 20% |
| **Mitigation Steps**      | 200    | Effectiveness / Speed     | 70% / 30% |

1. Monitor participant progress throughout scenarios:

| Element      | Description                                                  |
| ------------ | ------------------------------------------------------------ |
| **Name**     | Progress Checkpoint                                          |
| **Purpose**  | Track participant progress                                   |
| **Trigger**  | Time-based (every 10 minutes)                                |
| **Duration** | 2-hour scenario (12 intervals)                               |
| **Metrics**  | Tasks completed, time per task, accuracy rate, help requests |
| **Result**   | Progress data recorded                                       |

## Testing and Validation

### Scenario Testing Checklist

- All task dependencies resolve correctly
- Timing intervals are realistic
- VM targeting works properly
- Expected outputs are achievable
- Failure paths provide recovery options
- Assessment points are fair and measurable

### Common Issues and Solutions

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
  • Assigned team or role tags (e.g., `team_name`, `analyst`)
  • Active and powered-on systems only

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
