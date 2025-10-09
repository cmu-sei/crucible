# User Management

Infrastructure administrators are responsible for managing user access, permissions, and authentication integration across all Crucible applications. This guide walks you through the key user management functions.

## Authentication and Identity

Crucible uses an identity provider (typically Keycloak or IdentityServer) for centralized authentication. Configure your identity provider following the [installation guide's OAuth provider recommendations](../../install/index.md#recommended).

### Identity Provider Integration

- **Keycloak** - [Documentation](https://www.keycloak.org/documentation){ target=_blank }
- **IdentityServer** - [Documentation](https://identityserver4.readthedocs.io/en/latest/){ target=_blank }
- Configure OIDC/OAuth2 settings in each Crucible application's Helm values

### Multi-Factor Authentication

Configure MFA policies through your identity provider to enforce additional security for administrative and privileged accounts.

## Application-Specific Permissions

Each Crucible application has its own permission system. As an Administrator, you'll need to understand and configure permissions in each application:

### Permission Documentation

- [Alloy Permissions](https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md){ target=_blank } - Event launching and exercise management
- [Player Permissions](https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md){ target=_blank } - Virtual environment access and team membership
- [Caster Permissions](https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md){ target=_blank } - Infrastructure design and deployment
- [Steamfitter Permissions](https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df){ target=_blank } - Scenario task organization and execution

### Common Permission Roles

While each application has specific permissions, common administrative roles include:

- **Administrator** - Full application access including user/permission management
- **ContentDeveloper/Designer** - Create and manage exercises and scenarios (varies by application)
- **User/Member** - Standard participant access

See individual application permission documentation for complete role definitions and capabilities.

## User Account Management

### Creating Users

Users are typically created and managed through your identity provider (Keycloak/IdentityServer):

1. Access your identity provider's admin interface
2. Create user accounts with appropriate attributes
3. Assign application-specific roles via the identity provider's role mapping
4. Users will receive permissions based on IdP claims when they authenticate

### Managing Permissions

To grant permissions in Crucible applications:

1. Ensure users exist in your identity provider
2. Access each application's admin interface (requires Administrator permission)
3. Assign application-specific permissions following the permission documentation above
4. Permissions may also be managed via application APIs for bulk operations

### Team Management

**Player** provides team management functionality for organizing users into collaborative groups for exercises:

- Create teams for exercises and events
- Assign users to teams
- Manage team membership during active exercises
- See [Player documentation](../../player/index.md) for team management details

## Security Considerations

### Access Control Best Practices

- Follow the Principle of Least Privilege - grant only required permissions
- Review the [Security and Compliance Checklist](../security/index.md) for identity management requirements
- Regularly audit user permissions and role assignments
- Configure role mapping at the identity provider level for centralized control

### Audit Logging

Enable audit logging in each Crucible application to track:

- User authentication events
- Permission changes
- Administrative actions
- Exercise access and participation

Configure audit log forwarding to your SIEM as described in the [security guide](../security/index.md).

## User Workflows by Role

### For Range Builders

Range Builders need permissions in:

- **Caster** - To design infrastructure topologies
- **Steamfitter** - To create scenario tasks
- **Player** - To view and test exercises

See the [Range Builder Guide](../range-builder/index.md) for their typical workflows.

### For Instructors

Instructors primarily interact with:

- **Alloy** - To launch events and manage exercises
- **Player** - To monitor participant progress
- **Gameboard** (if used) - To manage competitions

See the [Instructor Guide](../instructor/index.md) for their typical workflows.

## Troubleshooting

### Common Authentication Issues

- **Users cannot log in** - Verify identity provider configuration and network connectivity
- **Missing permissions** - Check role mappings in identity provider and application-specific permission assignments
- **Permission denied errors** - Review application logs and verify user has required permissions in that specific application

For detailed troubleshooting, see the [Troubleshooting Guide](../troubleshooting/index.md).
