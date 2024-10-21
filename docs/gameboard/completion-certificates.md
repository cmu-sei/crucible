# Completion Certificates

Gameboard supports a convenient way for participants to view and print a certificate as proof that they participated in a game. Certificates are unique to the participant and specific to a game.

To create a *certificate template*, you must have the *Administrator* role in the Gameboard. For more information about roles, please see the [Gameboard Roles Guide](C:\Users\rgreeder\OneDrive - Software Engineering Institute\Desktop\gameboard-docs).

To generate and print a *certificate* of completion, you must be a participant in a game that is completed and the scoreboard is final.

## Generating certificates as a player

Upon completing a game, a player can view and print a customized certificate specific to a completed game.

In the Gameboard, click **Home**. Under **Completed Games**, select a completed game card.

Expand **Certificate** to view and print the certificate as a .PDF. The options to print the certificate are the same as printing any other .PDF.

Players can view a list of past certificates from their gameboard Profile page too. In gameboard, click **Profile**. Then click **Certificates**.  A table view of all game certificates is displayed. You can view and print the certificate as a .PDF from the table view.

!!! info

        Certificates are not available during live games.

## Configuring certificates as an administrator

This section assumes that you have been granted the Administrator role in gameboard, you are logged in, and you have a game created.

In the top-right corner, click **Admin**.

Hover over an existing game, then click **Settings**. Under **Metadata**, see the **Certificate Template** field.  Certificate Template holds default HTML that you can use to start your template. For help, toggle the `i` icon.

Of course, you can substitute your own HTML to design the certificate template.

There is an optional app configuration to specify an HTML filename to use as a site default for all new games.

This allows an admin to copy in an HTML template for a game in the settings. 

### Cloning a game with a configured certificate template

When cloning a game, the HTML from the Certificate Template field will always be copied *exactly* from the original game, even when the Certificate Template field is blank.