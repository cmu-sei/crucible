# Permissions

The following grid aligns roles and their application-level permissions across:

- [Alloy](../alloy/index.md)
- [Player](../player/index.md)
- [Caster](../caster/index.md)
- [Steamfitter](../steamfitter/index.md)

<div class="roles-matrix">
  <table>
    <thead>
      <tr>
        <th>Role</th>
        <th>Alloy</th>
        <th>Player</th>
        <th>Caster</th>
        <th>Steamfitter</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><strong><a href="/administrator/">Infrastructure Administrator</a></strong></td>
        <td><strong><a href="https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md" target="_blank">Administrator</a>:</strong> Full control of Alloy configuration and integrations.</td>
        <td><strong><a href="https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md" target="_blank">Administrator</a>:</strong> Full control of Player, including role delegation and notifications.</td>
        <td><strong><a href="https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md" target="_blank">Administrator</a>:</strong> Full control of Caster projects, modules, and system settings.</td>
        <td><strong><a href="https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df" target="_blank">Administrator</a>:</strong> Full control of Steamfitter scenarios, tasks, and system settings.</td>
      </tr>
      <tr>
        <td><strong><a href="../range-builder/index.md">Range Builder</a></strong></td>
        <td>
          <p><strong><a href="https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md" target="_blank">Content Developer</a>:</strong> Create and manage event templates (requires Player integration to preview views).</p>
        </td>
        <td>
          <p><strong><a href="https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md" target="_blank">View Admin</a>:</strong> Build and maintain views, including toggling inactive states.</p>
          <p><strong><a href="https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md" target="_blank">Content Developer</a>:</strong> Create views with Terraform and manage ISO/file upload toggles.</p>
        </td>
        <td>
          <p><strong><a href="https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md" target="_blank">Content Developer</a>:</strong> Create and manage projects and membership for projects they author.</p>
        </td>
        <td>
          <p><strong><a href="https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df" target="_blank">Content Developer</a>:</strong> Create and manage scenarios and templates they own.</p>
        </td>
      </tr>
      <tr>
        <td><strong><a href="../instructor/index.md">Instructor</a></strong></td>
        <td><strong><a href="https://github.com/cmu-sei/Alloy.Api/blob/development/docs/Permissions.md" target="_blank">Observer</a>:</strong> View and execute events assigned to their classes.</td>
        <td>
          <p><strong><a href="https://github.com/cmu-sei/Player.Api/blob/main/docs/Permissions.md" target="_blank">View Member</a>:</strong> Access Player views assigned to their cohorts, with ability to revert snapshots on VMs when enabled.</p>
        </td>
        <td><strong><a href="https://github.com/cmu-sei/Caster.Api/blob/development/docs/Permissions.md" target="_blank">Observer</a>:</strong> Review projects tied to their events; no edits or workspace runs.</td>
        <td><strong><a href="https://github.com/cmu-sei/Steamfitter.Api/commit/d5515ce341b76bf4089639ecca7e87280d7f73df" target="_blank">Observer</a> :</strong> Review scenarios assigned to their events; no edits or task runs.</td>
      </tr>
    </tbody>
  </table>
</div>
