# Troubleshooting Playbook

Use this playbook to triage common issues before escalating. Capture your findings in the operations log for future reference.

## Services Won't Start (Helm + k3s)

1. Check cluster and node health

     - `kubectl get nodes`
     - `kubectl get pods -A`
     - `kubectl describe node <node-name>` to inspect resource or scheduling issues

2. Verify Helm deployment

     - `helm list -A` to ensure the release deployed
     - `helm status <release-name>` to review resource state and notes

3. Inspect failing services

     - `kubectl get pods -n <namespace>`
     - `kubectl describe pod <pod-name>` for events and errors
     - `kubectl logs <pod-name> [-c <container-name>]` to view logs

4. Check configurations and manifests

     - `helm get values <release-name>` for current configuration
     - Validate YAML files with `kubectl apply --dry-run=client -f <file.yaml>`

5. Confirm networking and ports

     - `kubectl get svc -n <namespace>` for service exposure
     - Use `kubectl port-forward` or `curl` to test access
     - Ensure no host-level firewall or port conflict exists

## Database Connection Issues

- Verify the database service is running
- Check connection string format
- Confirm network connectivity

## SSL Certificate Problems

- Verify certificate paths in application configuration
- Check certificate validity dates
- Ensure the full certificate chain is present and trusted

## Environment Health

1. Run `kubectl get pods -A` to confirm control-plane and application pods are healthy
2. Review recent cluster events: `kubectl get events -A --sort-by=.lastTimestamp | tail`
3. Check monitoring dashboards (Prometheus/Grafana) for resource saturation

If pods are crash-looping:

- Describe the pod for error output: `kubectl describe pod <name> -n <namespace>`
- Inspect container logs: `kubectl logs <name> -n <namespace> --tail=200`
- Compare with the last known good deployment manifest

## Identity or Login Failures

- Verify Keycloak or identity provider availability and certificate validity
- Confirm OAuth client secrets match values in `values.yaml`
- Review Player API logs for `401` or `403` responses to identify scope or role changes

## Application Availability

- Alloy events stuck in a pending state often indicate Steamfitter or Caster API connectivity issues; verify service endpoints and network policies
- Missing Player views reported by Range Builders commonly originate from misaligned permissions; confirm View Admin or Content View User access
- Instructors unable to launch labs should confirm event templates reference valid Player exercises, Caster directories, and Steamfitter scenarios

## Data Integrity

- For PostgreSQL issues, test connectivity using `pg_isready -U <user> -h <host>`
- Verify backup job status before performing repair operations
- Audit object storage lifecycle policies and recent delete events if artifacts are missing

## Escalation Checklist

- ✅ Capture timestamps, affected users, and recent changes
- ✅ Record exact error messages and relevant logs
- ✅ Note mitigation steps attempted and their outcomes
- ✅ Contact the on-call Range Builder or instructional staff when learner-facing content is at risk
