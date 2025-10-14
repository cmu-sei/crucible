# Deployment Management

This section walks infrastructure administrators through deploying and configuring Crucible, referencing the detailed installation guide and architecture documentation.

## Installation

For complete installation instructions, follow the [installation guide](../../install/index.md), which covers:

- **Requirements** - Kubernetes, vCenter/Proxmox, and recommended tools
- **Certificates** - TLS certificate generation and configuration
- **Load Balancer** - MetalLB setup for bare metal installations
- **Ingress** - nginx ingress controller deployment
- **Storage** - Longhorn configuration for persistent volumes
- **Database** - PostgreSQL and pgAdmin installation
- **Crucible Applications** - Helm chart deployment for all framework components

The installation guide includes example configurations and links to the [k3s-install](https://github.com/avershave/k3s-install) and [k3s-production](https://github.com/sei-noconnor/k3s-production) repositories containing production-ready values and setup scripts.

## Deployment Architecture

### Core Services

Crucible's architecture consists of:

- **Identity Management** - Keycloak for authentication
- **Container Orchestration** - Kubernetes (K3s recommended for bare metal)
- **Application Services** - Player, Caster, Steamfitter, Alloy, TopoMojo, Gameboard, Blueprint, Gallery, CITE
- **Supporting Services** - PostgreSQL, Redis, File Storage (S3/MinIO), Message Queue
- **Infrastructure** - Load balancers, SSL termination, monitoring, and logging

See the [Administrator Guide](../index.md) for more information.

## Configuration Management

### Application Configuration

Each Crucible application has specific configuration requirements documented in their respective GitHub repositories:

- [Alloy API Settings](https://github.com/cmu-sei/Alloy.Api)
- [Caster API Settings](https://github.com/cmu-sei/Caster.Api)
- [Player API Settings](https://github.com/cmu-sei/Player.Api)
- [Steamfitter API Settings](https://github.com/cmu-sei/Steamfitter.Api)

Helm chart values for each application are available in the [helm-charts repository](https://github.com/cmu-sei/helm-charts).

### Secrets Management

For sensitive configuration:

- Store secrets in Kubernetes secrets or external secret managers
- Use the certificate procedures from the [installation guide](../../install/index.md#certificates)
- Rotate credentials regularly following security best practices

## Scaling and Performance

### Scaling Considerations

- **Horizontal Scaling** - Most Crucible applications are stateless and can scale horizontally by increasing replica counts in Helm values
- **Vertical Scaling** - Adjust resource limits and requests in Helm chart values based on workload requirements
- **Storage Planning** - The installation guide notes that minimal hardware configurations start at 100-250 GB storage, 8 GB RAM, and 2 cores per node

### Resource Optimization

Monitor resource utilization through:

- Kubernetes metrics and dashboards (Rancher if installed)
- PostgreSQL query performance
- Storage usage via Longhorn (if installed)

## Monitoring and Troubleshooting

### Health Monitoring

- Use Kubernetes health checks and readiness probes defined in Helm charts
- Monitor application logs via `kubectl logs` or centralized logging solutions
- Check service status with `kubectl get pods -A`

### Common Issues

Refer to the [Troubleshooting Guide](../troubleshooting/index.md) for procedures and solutions to aid in common deployment problems.

## Security

For deployment security considerations, see:

- [Security and Compliance Checklist](../security/index.md) - TLS configuration, network policies, audit logging
- [Installation Guide SSL/TLS Section](../../install/index.md#certificates) - Certificate setup procedures
