# Alloy API Readme

The **Alloy API** project is a restful api for Alloy functionality in the Crucible ecosystem. By default, `alloy.api` is available at `localhost:4402`, with the Swagger page at `localhost:4402/swagger/index.html`.

## postgresql migrations

From the **Alloy.Api** folder ... $ dotnet ef migrations add --project ..\Alloy.Api.Migrations.PostgreSQL\Alloy.Api.Migrations.PostgreSQL.csproj

## Alloy API Data

### Definition

- `Guid Id`
- `Guid? ExerciseId` (Player, an exercise definition)
- `Guid? DirectoryId` (Caster)
- `Guid? ScenarioId` (Steamfitter)
- `string Name`
- `string Description`
- `int DurationHours`

### Implementation

- `Guid Id`
- `Guid DefinitionId` (Alloy)
- `Guid UserId`
- `Guid? ExerciseId` (Player)
- `Guid? WorkspaceId` (Caster)
- `Guid? RunId` (Caster)
- `Guid? SessionId` (Steamfitter)
- `string Name`
- `string Description`
- `ImplementationStatus Status`
- `DateTime? LaunchDate`
- `DateTime? EndDate`
- `DateTime? ExpirationDate`

## Caster Interface

Alloy will deploy a lab/odx as a Caster workspace, including the workspace specific variables file. Variables include:
- Player Exercise ID
- Player Team Names

Then, Alloy will:
- Create a run
- Create a plan
- Apply the plan

Caster will write the `ExerciseId` and the `TeamId` from Player to each VM's guest info variables.

Alloy will get all authorized exercise/directories from `caster.api`.

## Player Interface

`Player.api` must have an idea of an Exercise definition. Alloy will get all authorized Exercise definitions from the `player.api`. `Player.api` must have a `CreateExerciseFromExerciseDefinition` endpoint.

## Steamfitter Interface

Alloy will get all authorized scenarios from `steamfitter.api`. Alloy will call the `CreateSessionFromScenario` endpoint.

## Preparing for Launch

Determine if an implementation already exists that can be rejoined or if a new implementation needs to be created.
   - `GET /definitions/{definitionId}/implementations/mine`
   - check for `active` status

If there is an `active` implementation, then present option to rejoin and skip launch.

## Launch

1. Create a new implementation from the definition.
   - `POST /definitions/{definitionId}/implementations`
2. Clone the Player exercise.
3. Deploy a new Caster workspace in the Caster directory.
   - `CreateWorkspaceFromDirectory`
   - Create a `Run` for the workspace
   - Wait for the `Run` to be `Planned` status
   - Apply the `Run`
   - Wait for the `Run` to be `Applied` status
4. Create a Steamfitter session from the scenario.
5. Set the Implementation status to `active`.

## End

1. Set Player exercise to `Inactive`.
2. End the Steamfitter session.
3. Destroy the Caster workspace.
   - Create an `isDestroy` `Run` for the workspace
   - Wait for the `Run` to be `Planned` status
   - Apply the `Run`
   - Wait for the `Run` to be `Applied` status
4. Set the Implementation status to `ended` or `expired`.

## Background Tasks

The entire **Launch** and **End** processes will be single background tasks that poll the Caster API for each status change, update the implementation database record appropriately, and then move on to the next step in the process.

## Alloy documentation

In addition to the **Alloy API Readme** and [Alloy UI Readme](https://github.com/cmu-sei/crucible/blob/master/alloy.ui/README.md) found in their respective repositories, Alloy documentation can be found in the Crucible wiki. Get started with Alloy [here](https://github.com/cmu-sei/crucible/wiki/Alloy-Quick-Start).

## License
Copyright 2020 Carnegie Mellon University. See the [LICENSE.txt](https://github.com/cmu-sei/crucible/blob/master/alloy.api/license.txt) files for details.
