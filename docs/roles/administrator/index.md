# Infrastructure Administrator Guide

This guide provides detail for platform administrators, system operators, and technical staff responsible for Crucible deployment and ongoing maintenance.

## Overview

As a Crucible Administrator, you're responsible for maintaining the platform's health, security, and performance. Your role differs from Range Builders (who create exercises) and Instructors (who deliver training) and encompasses platform-wide concerns including:

- Infrastructure planning and capacity management
- User account management and permissions assignment
- System monitoring, maintenance, and troubleshooting
- Security policy enforcement and compliance oversight
- Backup and recovery procedures
- Application-specific configuration and integration management

## API Permissions

Each Crucible application has its own permission system. As an Administrator, you typically need the **Administrator** role in each application to manage users, roles, and system configuration. Below is more information on each application's permissions:

- [Alloy Permissions](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md){ target=_blank }
- [Player Permissions](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md){ target=_blank }
- [Caster Permissions](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md){ target=_blank }
- [Steamfitter Permissions](https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df){ target=_blank }

## Platform Architecture

### Core Components

```
Crucible Platform
├── Identity Management (Keycloak)
├── Container Orchestration (Kubernetes)
├── Application Services
│   ├── Player (User Interface)
│   ├── Caster (Infrastructure)
│   ├── Steamfitter (Scenarios)
│   ├── Alloy (Event Management)
│   ├── TopoMojo (Lab Creation)
│   ├── Gameboard (Competitions)
│   ├── Blueprint (MSEL)
│   ├── Gallery (Incident Data)
│   └── CITE (Threat Evaluation)
├── Supporting Services
│   ├── PostgreSQL (Database)
│   ├── Redis (Caching)
│   ├── File Storage (S3/MinIO)
│   └── Message Queue
└── Infrastructure
    ├── Load Balancers
    ├── SSL Termination
    ├── Monitoring Stack
    └── Logging Aggregation
```

## Related Resources

- [Range Builder Guide](../range-builder/index.md) - For understanding exercise creation
- [Instructor Guide](../instructor/index.md) - For understanding instructor workflows
- [Troubleshooting Reference](troubleshooting/index.md) - Further help on resolving problems
