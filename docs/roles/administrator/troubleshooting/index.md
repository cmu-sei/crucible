# Troubleshooting Playbook

Use this playbook to triage common issues before escalating. Capture the findings in your operations log for future reference.

## Common Issues

Services Won't Start (Helm + k3s)

1. Check cluster and node health

    - `kubectl get nodes`
    - `kubectl get pods -A`
    - `kubectl describe node <node-name>` to inspect resource or scheduling issues.

2. Verify Helm deployment

    - `helm list -A` to ensure the release is deployed.
    - `helm status <release-name>` to see resource state and notes.

3. Inspect failing services

    - `kubectl get pods -n <namespace>`
    - `kubectl describe pod <pod-name>` for events and errors.
    - `kubectl logs <pod-name> [-c <container-name>]` to view logs.

4. Check configurations and manifests

    - `helm get values <release-name>` for current config.
    - Validate any YAML files with `kubectl apply --dry-run=client -f <file.yaml>`.

5. Confirm networking and ports

    - `kubectl get svc -n <namespace>` for service exposure.
    - `kubectl port-forward` or `curl` to test access.
    - Ensure no host-level firewall or port conflict.

### Database Connection Issues

- Verify database is running
- Check connection string format
- Confirm network connectivity

### SSL Certificate Problems

- Verify certificate paths in configuration
- Check certificate validity dates
- Ensure proper certificate chain

## Environment Health

1. Run `kubectl get pods -A` to confirm control-plane and application pods are healthy.
2. Check cluster events: `kubectl get events -A --sort-by=.lastTimestamp | tail`.
3. Review monitoring dashboards (Prometheus/Grafana) for resource saturation.

If pods are crash-looping:

- Describe the pod for error output: `kubectl describe pod <name> -n <namespace>`.
- Inspect container logs: `kubectl logs <name> -n <namespace> --tail=200`.
- Compare with the last known good deployment manifest.

## Identity or Login Failures

- Verify Keycloak/IdP availability and certificate validity.
- Confirm OAuth client secrets match the configuration in `values.yaml`.
- Review Player API logs for `401`/`403` responses to determine whether scope assignments changed.

## Application Availability

- Alloy events stuck in pending state often indicate Steamfitter or Caster API connectivity problems. Check service endpoints and network policies.
- Range Builder reports of missing Player views commonly originate from misaligned permissions. Validate the affected team's View Admin or Content View User access.
- Instructors unable to launch labs should confirm the event template still references valid Player exercises, Caster directories, and Steamfitter scenarios.

## Data Integrity

- For PostgreSQL incidents, use `pg_isready -U <user> -h <host>` to test connectivity.
- Review backup job status to ensure a fallback snapshot exists before performing repair operations.
- If object storage artifacts go missing, audit bucket lifecycle policies and recent delete events.

## Escalation Checklist

- Capture timestamps, affected users, and recent changes.
- Note the exact error messages or logs collected.
- Reference mitigation steps attempted and their outcomes.
- Page the on-call Range Builder or teaching staff when learner-facing content is at risk.
