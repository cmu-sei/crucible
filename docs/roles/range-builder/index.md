# Range Builder Guide

Range Builders create and deploy cyber ranges using Crucible platform components. They design infrastructure, scenarios, and user experiences to support training, exercises, and challenges.

## Overview

This procedural guide provides step-by-step direction for building complete cyber ranges using the Crucible stack. Range Builders primarily use Alloy for event and team management, Caster for infrastructure deployment, Steamfitter for scenario automation, and Player for exercise delivery.

Refer to the following core application guides for detailed procedures:

- **[Alloy Guide:](../../alloy/index.md)** Event orchestration and team management
- **[Caster Guide:](../../caster/index.md)** Infrastructure-as-code topology deployment
- **[Steamfitter Guide:](../../steamfitter/index.md)** Automated scenario injection and task execution
- **[Player Guide:](../../player/index.md)** Participant interface and exercise delivery

## Prerequisites

Before building ranges, confirm the following:

- **Permissions:** `Content Developer` permissions in all required Crucible applications
- **Infrastructure access:** Access to Terraform modules and templates
- **Environment knowledge:** Familiarity with the target virtualization environment (VMware vSphere, Azure, or Proxmox)
- **Foundational knowledge:** Understanding of basic cyber range concepts

## Common Range Patterns

Range structure varies based on training goals, audience, and available resources. Common patterns include:

### Basic Lab Environment

- Single team with individual workstations
- Pre-configured tools and reference materials
- Self-paced learning objectives

### Red vs Blue Exercise

- Opposing teams with differing objectives
- Automated injects
- Real-time monitoring and scoring

### Incident Response Scenario

- Timeline-driven event progression
- Evidence discovery points
- Collaborative investigation tools

### Penetration Testing Lab

- Intentionally vulnerable target environment
- Progressive difficulty levels
- Documentation and reporting tools

## Best Practices

### Infrastructure Design

- Use modular Terraform configurations
- Apply consistent resource naming conventions
- Plan for scalability and resource limits
- Test tear down and cleanup procedures

### Scenario Development

- Start with simple manual tasks before introducing automation
- Use clear and descriptive task names
- Define a clear timeline for events and injects
- Test timing and dependencies thoroughly
- Provide meaningful feedback to participants

### User Experience

- Design clear navigation paths
- Provide explicit instructions and objectives
- Test workflows from the participant perspective
- Account for varying skill levels

## Support

For additional assistance, join the [Crucible community discussions](https://github.com/cmu-sei/crucible/discussions).
