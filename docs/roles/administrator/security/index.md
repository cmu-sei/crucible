# Security and compliance checklist

Infrastructure administrators should maintain a repeatable security posture for every Crucible deployment. Use the checklists below to plan regular reviews and document sign-offs.

## Hardening priorities

- Enforce TLS certificates for all public endpoints; rotate them at least annually.
- Restrict administrative portals (Keycloak, Rancher, monitoring) to approved network ranges.
- Configure Kubernetes network policies so only required namespaces can reach platform services.
- Enable audit logging across Alloy, Player, Caster, and Steamfitter; forward logs to your SIEM.

## Identity management

- Integrate Crucible with your enterprise IdP and require MFA for all administrative roles.
- Review role mappings quarterly to confirm the Principle of Least Privilege.
- Disable or rotate shared service accounts and update any stored credentials in secrets management.

## Data protection

- Schedule recurring PostgreSQL backups and test restore procedures.
- Encrypt object storage buckets that hold artifacts, uploads, or logs.
- Verify that tenant data is separated through namespaces, dedicated databases, or policy enforcement.

## Incident readiness

- Document escalation paths for platform outages, security incidents, and learner-impacting issues.
- Maintain an emergency contact list for range builders and instructional staff.
- Run tabletop exercises twice per year to practice recovery patterns and communications.
