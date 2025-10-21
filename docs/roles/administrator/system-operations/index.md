# System Operations

Infrastructure administrators are responsible for the ongoing operational management of Crucible deployments. This guide walks through monitoring, maintenance, and troubleshooting using Kubernetes tools and Crucible application features.

## Overview

System operations for Crucible include:

- **Monitoring and alerting** - Kubernetes cluster health and application status
- **Maintenance procedures** - Updates, backups, and routine tasks
- **Performance optimization** - Resource management and scaling
- **Incident response** - Problem identification and resolution

## Monitoring and Alerting

### Kubernetes Monitoring

Monitor your Crucible deployment using Kubernetes tools:

- **kubectl** - Command-line status checks: `kubectl get pods -A`, `kubectl describe pod <name>`, `kubectl logs <pod>`
- **Rancher** (if installed) - Web-based cluster monitoring and management (see [installation guide](../../install/index.md#rancher))
- **Kubernetes Dashboard** - Alternative web interface for cluster monitoring

### Application Health

Each Crucible application provides health endpoints. Check application status with:

```bash
# Check all pods across namespaces
kubectl get pods -A

# View logs for a specific application
kubectl logs -n <namespace> <pod-name>

# Check pod resource usage
kubectl top pods -A
```

### Infrastructure Monitoring

Key metrics to monitor on your Kubernetes cluster:

- CPU and memory utilization per node (`kubectl top nodes`)
- Disk space on nodes (check via node access or monitoring tools)
- PostgreSQL database performance (via pgAdmin if installed per [installation guide](../../install/index.md#postgresql-and-pgadmin))
- Storage usage if using Longhorn (accessible via Rancher or Longhorn UI)

### Log Management

- **Application logs** - Access via `kubectl logs` for each pod
- **Audit logs** - Configure in each Crucible application's Helm values; forward to SIEM as described in [security guide](../security/index.md)
- **Kubernetes events** - View with `kubectl get events -A`

## Maintenance Procedures

### Application Updates

Update Crucible applications by deploying new versions via Helm:

```bash
# Update a Crucible application to a new version
helm upgrade <release-name> cmu-sei/<chart-name> \
  --namespace <namespace> \
  -f values.yaml \
  --version <new-version>
```

Before updating:

1. Review release notes in the application's GitHub repository
2. Test updates in a non-production environment if available
3. Ensure database backups are current
4. Plan for brief service interruptions during pod restarts

### Database Maintenance

Maintain PostgreSQL databases regularly:

- Use **pgAdmin** (if installed per [installation guide](../../install/index.md#postgresql-and-pgadmin)) for visual database management
- Run `VACUUM ANALYZE` periodically to optimize database performance
- Monitor database size and connection counts
- Configure automated backups using Kubernetes CronJobs or external backup solutions

### Backup and Recovery

#### Backup Strategy

- **Database backups** - Use `pg_dump` or PostgreSQL backup tools; schedule regular automated backups
- **Persistent volume backups** - If using Longhorn, configure snapshot schedules through the Longhorn UI
- **Configuration backups** - Store Helm values files and Kubernetes manifests in version control

#### Recovery Procedures

Restore from backups when needed:

- **Database restore** - Use `pg_restore` with backup files
- **Volume restore** - Use Longhorn snapshot restore or your storage provider's procedures
- **Application redeployment** - Use Helm with backed-up values files

## Performance Optimization

### Resource Management

Adjust Kubernetes resource limits and requests in Helm values files:

```yaml
# Example resource configuration in Helm values
resources:
  limits:
    cpu: 1000m
    memory: 2Gi
  requests:
    cpu: 500m
    memory: 1Gi
```

Monitor resource usage with `kubectl top pods` and `kubectl top nodes` to inform scaling decisions.

### Scaling

Most Crucible applications support horizontal scaling:

- Increase replica counts in Helm values: `replicaCount: 3`
- Kubernetes will distribute load across replicas automatically
- Scale databases vertically by adjusting PostgreSQL resource allocations

Refer to the [installation guide](../../install/index.md#infrastructure) for minimum hardware requirements and scaling considerations.

## Troubleshooting

### Common Operational Issues

For detailed troubleshooting procedures, see the [Troubleshooting Guide](../troubleshooting/index.md).

Common issues include:

- **Pod failures** - Check status with `kubectl describe pod <name>` and review logs
- **Database connection issues** - Verify PostgreSQL pod is running and connection strings are correct in Helm values
- **Certificate errors** - Verify certificate secrets exist: `kubectl get secrets`
- **Resource exhaustion** - Check node and pod resource usage with `kubectl top`

### Diagnostic Commands

Basic Kubernetes diagnostic commands:

```bash
# Check all pods and their status
kubectl get pods -A

# Describe a specific pod (shows events and issues)
kubectl describe pod -n <namespace> <pod-name>

# View pod logs
kubectl logs -n <namespace> <pod-name>

# Check resource usage
kubectl top nodes
kubectl top pods -A

# View recent cluster events
kubectl get events -A --sort-by='.lastTimestamp'
```

## Security Operations

### Security Monitoring

Review the [Security and Compliance Checklist](../security/index.md) for:

- Audit log forwarding to SIEM
- Failed authentication monitoring
- Network policy enforcement
- Regular access reviews

### Incident Response

Follow your organization's incident response procedures. Key steps:

1. Identify and contain the incident using Kubernetes tools
2. Review application and audit logs for the timeframe
3. Use `kubectl` to isolate affected pods if needed
4. Restore from backups if you detect a data integrity compromise
5. Document findings and remediation steps
