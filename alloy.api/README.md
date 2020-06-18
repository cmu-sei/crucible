# Alloy API Readme

The **Alloy API** project is a restful api for Alloy functionality in the Crucible ecosystem. By default, `alloy.api` is available at `localhost:4402`, with the Swagger page at `localhost:4402/swagger/index.html`.

## postgresql migrations

From the **Alloy.Api** folder ... $ dotnet ef migrations add --project ..\Alloy.Api.Migrations.PostgreSQL\Alloy.Api.Migrations.PostgreSQL.csproj

## Alloy API Data

### EventTemplate

- `Guid Id`
- `Guid? ViewId` (Player, a view eventTemplate)
- `Guid? DirectoryId` (Caster)
- `Guid? ScenarioTemplateId` (Steamfitter)
- `string Name`
- `string Description`
- `int DurationHours`

### Event

- `Guid Id`
- `Guid EventTemplateId` (Alloy)
- `Guid UserId`
- `Guid? ViewId` (Player)
- `Guid? WorkspaceId` (Caster)
- `Guid? RunId` (Caster)
- `Guid? ScenarioId` (Steamfitter)
- `string Name`
- `string Description`
- `EventStatus Status`
- `DateTime? LaunchDate`
- `DateTime? EndDate`
- `DateTime? ExpirationDate`

## Caster Interface

Alloy will deploy a lab/odx as a Caster workspace, including the workspace specific variables file. Variables include:
- Player View ID
- Player Team Names

Then, Alloy will:
- Create a run
- Create a plan
- Apply the plan

Caster will write the `ViewId` and the `TeamId` from Player to each VM's guest info variables.

Alloy will get all authorized view/directories from `caster.api`.

## Player Interface

`Player.api` must have an idea of an View eventTemplate. Alloy will get all authorized View eventTemplates from the `player.api`. `Player.api` must have a `CreateViewFromViewEventTemplate` endpoint.

## Steamfitter Interface

Alloy will get all authorized scenarioTemplates from `steamfitter.api`. Alloy will call the `CreateScenarioFromScenarioTemplate` endpoint.

## Preparing for Launch

Determine if an event already exists that can be rejoined or if a new event needs to be created.
   - `GET /eventTemplates/{eventTemplateId}/events/mine`
   - check for `active` status

If there is an `active` event, then present option to rejoin and skip launch.

## Launch

1. Create a new event from the eventTemplate.
   - `POST /eventTemplates/{eventTemplateId}/events`
2. Clone the Player view.
3. Deploy a new Caster workspace in the Caster directory.
   - `CreateWorkspaceFromDirectory`
   - Create a `Run` for the workspace
   - Wait for the `Run` to be `Planned` status
   - Apply the `Run`
   - Wait for the `Run` to be `Applied` status
4. Create a Steamfitter scenario from the scenarioTemplate.
5. Set the Event status to `active`.

## End

1. Set Player view to `Inactive`.
2. End the Steamfitter scenario.
3. Destroy the Caster workspace.
   - Create an `isDestroy` `Run` for the workspace
   - Wait for the `Run` to be `Planned` status
   - Apply the `Run`
   - Wait for the `Run` to be `Applied` status
4. Set the Event status to `ended` or `expired`.

## Background Tasks

The entire **Launch** and **End** processes will be single background tasks that poll the Caster API for each status change, update the event database record appropriately, and then move on to the next step in the process.

## Alloy documentation

In addition to the **Alloy API Readme** and [Alloy UI Readme](https://github.com/cmu-sei/crucible/blob/master/alloy.ui/README.md) found in their respective repositories, Alloy documentation can be found in the Crucible wiki. Get started with Alloy [here](https://github.com/cmu-sei/crucible/wiki/Alloy-Quick-Start).

## License
Copyright 2020 Carnegie Mellon University. See the [LICENSE.txt](https://github.com/cmu-sei/crucible/blob/master/alloy.api/license.txt) files for details.
