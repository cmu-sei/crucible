# Scenario Player - Web
Scenario Player web is the front-end to for the VM player implementing NPM and 
Angular 
version 4.0

Docker
======
The following table defines Docker environment variables that can be set at 
deployment to configure running services.

| Variable | Required | Description |
| -------- | -------- | ----------- |
| SERVER_NAME | Yes | Fully-qualified domain name for the server. |
| API_URL | Yes | URL for the Scenario Player API. |

### Required Software
1. Node.js SDK which includes NPM
2. VS Code
3. Recommended to Update NPM ( npm update -g npm ).
4. Install latest Angular-CLI

1. clone the repo
`git clone https://INTERNAL_SOURCE_REPO/bitbucket/scm/cwd/s3.player.web.git`
2. move to splayer-web directory `cd s3.player.web`
3. install the NPM dependencies
`npm install`
4. run the server `ng serve`

#### Settings

All configurable values (urls, etc) should be made to use the SettingsService. The SettingsService loads it's values from configuration files located in /assets/config/. There are two files used for this, as follows:

1. settings.json

    This file is committed to source control, and holds default values for all settings. Changes should only be made to this file to add new settings, or change the default value of a setting that will affect everyone who pulls down the project.

2. settings.env.json

    This file is NOT committed to source control, and will differ for each environment. Settings can be placed into this file and they will override settings found in settings.json. Any settings not found in this file will default to the values in settings.json. 

In a production environment, settings.env.json should contain only the settings that need to be changed for that environment, and settings.json serves as a reference for the default values as well as any unchanged settings. settings.json should NOT be altered in a production environment for any reason.

