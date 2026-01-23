# Infrastructure Administrator Guide

This guide helps infrastructure administrators, system operators, and technical staff deploy, operate, and maintain Crucible platforms.

## Overview

As an *Infrastructure Administrator*, you are responsible for the overall health, security, and reliability of the Crucible platform. This role is distinct from *Range Builders*, who create exercises, and *Instructors*, who deliver training. Your responsibilities focus on platform-wide operations rather than exercise content.

Key responsibilities include:

- Infrastructure planning and capacity management
- User account provisioning and permission management
- System monitoring, maintenance, and troubleshooting
- Security policy enforcement and compliance oversight
- Backup, recovery, and disaster preparedness
- Application-level configuration and integration management

## API Permissions

Each Crucible application maintains its own permission model. Infrastructure Administrators typically require an Administrator role within each application to manage users, roles, and system-level configuration.

Refer to the following documentation for application-specific permission details:

- [Alloy Permissions](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md)
- [Player Permissions](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md)
- [Caster Permissions](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md)
- [Steamfitter Permissions](https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df)

## Platform Architecture

### Core Components

```text
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

- [Range Builder Guide](../range-builder/index.md): Learn how to create and deploy exercises using Crucible components.
- [Instructor Guide](../instructor/index.md): Learn how instructors manage and deliver Crucible training.
- [Troubleshooting Playbook](troubleshooting/index.md): Learn how to diagnose and resolve common platform issues.
