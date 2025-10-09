# Range Builder Guide

Range Builders create and deploy cyber ranges using Crucible platform components. They design infrastructure, scenarios, and user experiences to deliver engaging material that meets training, exercise, or challenge objectives.

This guide provides step-by-step instructions for building complete cyber ranges using the Crucible stack—Alloy for team management, Caster for infrastructure deployment, Steamfitter for automated scenario events, and Player for exercise delivery.

## Overview

As a Range Builder, you'll work with four primary Crucible applications:

- **[Alloy](../alloy/)**: Event orchestration and team management
- **[Caster](../caster/)**: Infrastructure-as-code topology deployment
- **[Steamfitter](../steamfitter/)**: Automated scenario injection and task execution
- **[Player](../player/)**: User interface and exercise delivery

## Prerequisites

Before building ranges, ensure you have:

- **Content Developer** permissions in all Crucible applications
- Access to Terraform modules and templates
- Understanding of your target virtualization environment (VMware vSphere, Azure, or Proxmox)
- Basic knowledge of cyber range concepts

## Common Range Patterns

There are many ways to structure cyber ranges depending on your training goals, audience, and available resources. Here are some common patterns to consider:

1. Basic Lab Environment

    - Single team with individual workstations
    - Pre-configured tools and documentation
    - Self-paced learning objectives

2. Red vs Blue Exercise

    - Opposing teams with different objectives
    - Automated inject schedule
    - Real-time monitoring and scoring

3. Incident Response Scenario

    - Timeline-driven event progression
    - Evidence discovery points
    - Collaborative investigation tools

4. Penetration Testing Lab

    - Vulnerable target environment
    - Progressive difficulty levels
    - Documentation and reporting tools

## Best Practices

1. Infrastructure Design

    - Use modular Terraform configurations
    - Implement proper resource naming conventions
    - Plan for scalability and resource limits
    - Test destruction/cleanup procedures

2. Scenario Development

    - Start with simple manual tasks before automation
    - Use clear, descriptive task names
    - Create a clear timeline for all events and injects
    - Test timing and dependencies thoroughly
    - Provide meaningful feedback to participants

3. User Experience

    - Design intuitive navigation paths
    - Provide clear instructions and objectives
    - Test from participant perspective
    - Plan for different skill levels

## Support

For additional help join the [community discussions](https://github.com/cmu-sei/crucible/discussions){ target=_blank }.
