# How to create an Alloy definition

Before creating an Alloy definition, the **Player exercise**, **Caster directory**, and **Steamfitter scenario** should be created. All of these components are actually optional for a definition, so a definition can have any combination of the three components.

 1. Add a new Definition.
 2. Complete the Name, Description, and Duration fields.
 3. Copy and paste the ID's of the Player exercise, Caster directory, and Steamfitter scenario.
 4. Save the Definition.

To give the user the ability to end the implementation or lab from inside Player, add a link to the definition as a Player application in the associated Player exercise.

## Understanding the launch process

When launching an implementation from a definition, the Alloy API goes through the following process:

1.  Clones the definition Player exercise into a new Player exercise and adds the Player exercise ID to the Alloy implementation.
2.  Creates a Steamfitter session from the definition Steamfitter scenario and adds the Steamfitter scenario ID to the Alloy implementation.
3.  Creates a Caster workspace in the definition Caster directory and adds the Caster Workspace ID to the Alloy implementation.
4.  Creates a Terraform `auto.tfvars` file in the Caster workspace that contains the following:
    - Exercise ID
    - Team Name and ID of every team in the exercise
    - User Name and ID
5.  Plans and applies the Caster workspace to deploy the infrastructure.
6.  Starts the Steamfitter scenario.

## Understanding the end process

There are two ways the end process can be triggered:
- user initiated, and
- `AlloyQueryService` of the Alloy API initiated because expiration time has been reached.

When the end process is initiated, the Alloy API goes through the following process:

 1. Deletes the Player exercise.
 2. Deletes the Steamfitter session.
 3. Plans and applies the *destroy* of the Caster workspace.
 4. Deletes the Caster workspace.
