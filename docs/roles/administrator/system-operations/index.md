# System Operations

Infrastructure administrators manage the day-to-day operation of Crucible deployments. This procedural guide covers monitoring, maintenance, performance management, and operational troubleshooting using Kubernetes tooling and Crucible application features.

## Operational Scope

System operations for Crucible include:

- **Monitoring and alerting** for cluster and application health
- **Performing maintenance** such as updates and backups
- **Managing performance and scale** through resource tuning
- **Responding to incidents** and resolving operational issues

## Monitoring and Alerting

### Monitoring Kubernetes

Monitor Crucible using standard Kubernetes tools:

- **kubectl:** Command-line inspection and diagnostics (`kubectl get pods -A`, `kubectl describe pod <name>`, `kubectl logs <pod>`)
- **Rancher** (if installed): Web-based cluster monitoring and management; see the [Installation Guide](../../../install/index.md#rancher)
- **Kubernetes Dashboard:** Optional web interface for cluster visibility

### Monitoring Application Health

Each Crucible application exposes health and status information.

Use Kubernetes tooling to inspect application state:

```bash
# Check all pods across namespaces
kubectl get pods -A

# View logs for a specific application
kubectl logs -n <namespace> <pod-name>

# Check pod resource usage
kubectl top pods -A
```

### Monitoring Infrastructure

Track the following cluster metrics:

- CPU and memory utilization per node (`kubectl top nodes`)
- Disk space on nodes
- PostgreSQL performance and connection counts (via pgAdmin if installed per the [Installation Guide](../../../install/index.md#postgresql-and-pgadmin))
- Persistent storage usage (via Longhorn UI or Rancher, if installed)

### Managing Logs

- **Application logs:** Access per pod using `kubectl logs`
- **Audit logs:** Configure via application Helm values and forward to your Security Information and Event Management (SIEM)
- **Kubernetes events:** Review with `kubectl get events -A`

## Performing Maintenance

### Updating Applications

Update Crucible applications using Helm:

```bash
# Update a Crucible application to a new version
helm upgrade <release-name> cmu-sei/<chart-name> \
  --namespace <namespace> \
  -f values.yaml \
  --version <new-version>
```

Before applying updates:

1. Review application release notes
2. Test changes in a non-production environment, if available
3. Verify database backups are current
4. Plan for brief service interruptions during pod restarts

### Maintaining Databases

Perform regular PostgreSQL maintenance:

- Manage databases using pgAdmin (if installed)
- Run `VACUUM ANALYZE` periodically
- Monitor database growth and active connections
- Automate backups using Kubernetes CronJobs or external backup solutions

### Managing Backups and Recovery

#### Backup Strategy

- Databases: Schedule PostgreSQL backups using `pg_dump` or equivalent tools
- Persistent volumes: Configure snapshot schedules if using Longhorn
- Configuration: Store Helm values and Kubernetes manifests in version control

#### Recovery Procedures

Restore services as needed:

- Database restores using `pg_restore`
- Volume restores using storage provider or Longhorn snapshots
- Application recovery using Helm and backed-up values files

## Managing Performance and Scale

### Managing Resources

Tune Kubernetes resource requests and limits through Helm values:

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

Monitor usage with `kubectl top pods` and `kubectl top nodes` to guide adjustments.

### Scaling Applications

Most Crucible services support horizontal scaling:

- Increase `replicaCount` in Helm values
- Kubernetes distributes load across replicas automatically
- Scale PostgreSQL vertically by adjusting allocated resources

Refer to the [Installation Guide](../../../install/index.md#postgresql-and-pgadmin) for minimum hardware requirements and scaling considerations.

## Troubleshooting Operations

### Addressing Common Issues

For detailed troubleshooting tips, see the [Troubleshooting Playbook](../troubleshooting/index.md).

Common operational issues include:

- **Pod failures:** Check status with `kubectl describe pod <name>` and review logs
- **Database connectivity issues:** Verify PostgreSQL pod is running and connection strings are correct in Helm values
- **Certificate errors:** Verify certificate secrets exist: `kubectl get secrets`
- **Resource exhaustion:** Check node and pod resource usage with `kubectl top`

### Using Basic Kubernetes Diagnostic Commands

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

## Managing Security Operations

### Monitoring Security

Use the [Security and Compliance Checklist](../security/index.md) to verify:

- Audit log forwarding to Security Information and Event Management (SIEM)
- Failed authentication monitoring
- Network policy enforcement
- Regular access reviews

### Responding to Incidents

Follow your organization's incident response procedures. Key steps:

1. Identify and contain the incident using Kubernetes tools
2. Review application and audit logs for the timeframe
3. Use `kubectl` to isolate affected pods if needed
4. Restore from backups if you detect a data integrity compromise
5. Document findings and remediation steps
