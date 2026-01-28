# Security and Compliance Checklist

Infrastructure administrators should maintain a repeatable security posture for every Crucible deployment. Use the checklists below to plan regular reviews and document sign-offs.

## Hardening Priorities

- ✅ Enforce TLS certificates for all public endpoints and rotate them at least annually.
- ✅ Restrict administrative portals (Keycloak, Rancher, monitoring) to approved network ranges.
- ✅ Configure Kubernetes network policies so that only required namespaces can reach platform services.
- ✅ Enable audit logging across Alloy, Player, Caster, and Steamfitter, and forward logs to your Security Information and Event Management (SIEM) system.

## Identity Management

- ✅ Integrate Crucible with your enterprise identity provider (IdP) and require MFA for all administrative roles.
- ✅ Review role mappings quarterly to confirm adherence to the principle of least privilege.
- ✅ Disable or rotate shared service accounts and update stored credentials in secrets management.

## Data Protection

- ✅ Schedule recurring PostgreSQL backups and regularly test restore procedures.
- ✅ Encrypt object storage buckets that hold artifacts, uploads, or logs.
- ✅ Verify that your design separates tenant data through namespaces, dedicated databases, or policy enforcement.

## Incident Readiness

- ✅ Document escalation paths for platform outages, security incidents, and learner-impacting issues.
- ✅ Maintain an emergency contact list for range builders and instructional staff.
- ✅ Run tabletop exercises at least twice per year to practice recovery procedures and communications.
