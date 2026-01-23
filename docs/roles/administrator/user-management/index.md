# Managing Users

Infrastructure administrators manage user access, permissions, and authentication integration across all Crucible applications. This section covers the key user management functions.

## Authentication and Identity

Crucible uses an identity provider (typically Keycloak or Duende IdentityServer) for centralized authentication. Configure your identity provider following our *Installation Guide*'s [OAuth provider recommendations](../../../install/index.md#recommended).

### Identity Provider Integration

- **Keycloak:** Use the official [Keycloak documentation](https://www.keycloak.org/documentation) to configure authentication.
- **IdentityServer:** Use the official [IdentityServer documentation](https://identityserver4.readthedocs.io/en/latest/) to configure OAuth2/OpenID Connect.
- Configure OIDC/OAuth2 settings for each Crucible application in its Helm values.

### Multi-Factor Authentication

Configure MFA policies through your identity provider to enforce additional security for administrative and privileged accounts.

## Application-Specific Permissions

Each Crucible application implements its own permission model. Infrastructure administrators configure and manage permissions within each application as part of platform operations.

### Permission Documentation

- **Alloy:** Review the [Alloy permissions documentation](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md) for event launching and exercise management.
- **Player:** Review the [Player permissions documentation](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md) for virtual environment access and team membership.
- **Caster:** Review the [Caster permissions documentation](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md) for infrastructure design and deployment permissions.
- **Steamfitter:** Review the [Steamfitter permissions documentation](https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df) for scenario task organization and execution roles.

### Common Permission Roles

While each application has specific permissions, common administrative roles include:

- **Administrator:** Full application access including user and permission management.
- **ContentDeveloper/Designer:** Create and manage exercises and scenarios (varies by application).
- **User/Member:** Standard, "regular", participant access.

See the individual application permission documentation linked above for complete role definitions and capabilities.

## Managing User Accounts

### Creating Users

Create and manage users through your identity provider (Keycloak or IdentityServer):

1. Access the identity provider's administrative interface.
2. Create user accounts with the appropriate attributes.
3. Assign application-specific roles using identity provider role mappings.
4. Users receive permissions based on IdP claims during authentication.

### Managing Permissions

Grant permissions within Crucible applications as follows:

1. Confirm the user exists in the identity provider.
2. Access the application's administrative interface (Administrator permission required).
3. Assign application-specific permissions according to the relevant permission documentation.
4. Use application APIs to manage permissions in bulk, if needed.

### Team Management

### Managing Teams

The **Player** application supports team management for organizing users into collaborative groups during exercises:

- Create teams for exercises and events.
- Assign users to teams.
- Modify team membership during active exercises.
- Refer to the [Player Guide](../../../player/index.md) for detailed team management procedures.

## Security Considerations

### Access Control Best Practices

- Follow the principle of least privilege by granting only required permissions.
- Review the [Security and Compliance Checklist](../security/index.md) for identity and access management requirements.
- Audit user permissions and role assignments on a regular basis.
- Configure role mappings at the identity provider level to maintain centralized access control.

### Audit Logging

Enable audit logging in each Crucible application to record:

- User authentication events
- Permission and role changes
- Administrative actions
- Exercise access and participation

Configure audit log forwarding to your Security Information and Event Management (SIEM) system as described in the [Security and Compliance Checklist](../security/index.md).

## User Workflows by Role

### Range Builders

Range Builders typically require permissions in the following applications:

- **Caster:** Design and manage infrastructure topologies
- **Steamfitter:** Create and manage scenario tasks
- **Player:** View and test exercises

Refer to the [Range Builder Guide](../../range-builder/index.md) for detailed workflows.

### Instructors

Instructors primarily work within the following applications:

- **Alloy:** Launch events and manage exercises
- **Player:** Monitor participant progress
- **Gameboard:** Manage competitions and scoring (if used)

Refer to the [Instructor Guide](../../instructor/index.md) for detailed workflows.

## Troubleshooting Common Authentication Issues

- **Users cannot log in:** Verify identity provider configuration and network connectivity.
- **Missing permissions:** Check role mappings in the identity provider and application-specific permission assignments.
- **Permission denied errors:** Review application logs and confirm the user has the required permissions in the affected application.

For step-by-step troubleshooting procedures, see the [Troubleshooting Playbook](../troubleshooting/index.md).
