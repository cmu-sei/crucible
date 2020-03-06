# S3.Vm.Ngconsole

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 1.6.1.

This project contains a prototype for a VMware VM console app utilzing Angular and WebMKS.

Settings are currently configured in `src/environments/`. Please update the URLs in those files to set the approprate host:port for the s3.vm.api and s3.vm.console APIs.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4305/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `-prod` flag for a production build.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).

#### Settings

All configurable values (urls, etc) should be made to use the SettingsService. The SettingsService loads it's values from configuration files located in /assets/config/. There are two files used for this, as follows:

1. settings.json

    This file is committed to source control, and holds default values for all settings. Changes should only be made to this file to add new settings, or change the default value of a setting that will affect everyone who pulls down the project.

2. settings.env.json

    This file is NOT committed to source control, and will differ for each environment. Settings can be placed into this file and they will override settings found in settings.json. Any settings not found in this file will default to the values in settings.json. 

In a production environment, settings.env.json should contain only the settings that need to be changed for that environment, and settings.json serves as a reference for the default values as well as any unchanged settings. settings.json should NOT be altered in a production environment for any reason.
