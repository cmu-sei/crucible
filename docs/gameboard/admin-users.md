# Users

To access the **Administration**, **Users** screen click **Admin** in the top navigation menu, then click **Users**. This is where users can be created, updated, deleted, assigned roles, and given API keys. Participant names are approved or disapproved here too. In Gameboard, a *user* is a person and a user has a *Name*.

## Searching, filtering, and sorting

To search for a user across the whole of Gameboard enter a term into the **Search** field.

To filter your results, select **Has Elevated Role**, **Has Pending Name**, or **Has Disallowed Name**.

- **Has Elevated Role:** contains only those participants who have been granted roles with additional permissions (Admin, Director, Support, Tester).
- **Has Pending Name:** contains only those participants whose display names are pending approval or disapproval from an admin.
- **Has Disallowed Name:** contains only those participants whose display names have been disapproved by an admin.

To sort your results, select **Name**, **Last Login**, or **Created On**. Click once to sort in **ascending** order (A → Z, oldest to newest). Click again to sort in **descending** order (Z → A, newest to oldest). The arrow (▲ or ▼), appears in the sort button to show the current sorting direction.

## Viewing a user card

Select **View** in a user's record to show the user's card. Here you can delete a user from Gameboard, change their approved name, add a disapproved reason, and even change their role.

Here you can also generate an API Key for the user.

1. Under API Keys, enter a name for your new API key.
2. Enter an expiration date or leave the date field blank for permanent access.
3. Click **Add**.
4. Click **Copy** to copy the new key and record it in safe place. You can only view it for a short period of time in the Gameboard.

## Adding Users

Administrators can create Gameboard user accounts in advance. Typically, users would register through an identity provider and their Gameboard account is generated at first login. Users select a sponsor and display name.

However, administrators can create Gameboard accounts proactively for users who already have an identity account. This is useful for events where pre-registered participants need to complete a specific set challenges or play in the Practice Area.

To add a new user in the Gameboard:

1. On the Users screen, click **Add Users**.
2. In the **Create Users** window, in the field, enter space- or line-delimited user GUIDs (globally unique identifiers) to create Gameboard user accounts. In this way, multiple accounts can be created at the same time. Example settings to configure initial user settings are provided.
3. Under **Settings**, enable **Show an error if one of these IDs already exists** and/or **Don't force users to select their sponsor before playing** depending upon your needs.
4. Select a role to assign to the users.
5. Select a sponsor to assign to the users.
6. Select a game to enroll the users in.
7. Click **OK**. Gameboard creates the new users. If you sort the users in descending order by **Created On** date you'll see them at the top.
