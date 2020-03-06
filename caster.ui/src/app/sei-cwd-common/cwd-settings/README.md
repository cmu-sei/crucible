## CwdSettingsModule

#### Description
The CWD Settings Module is a module that can load application level settings 
from a JSON file and provide a settings service to the entire application. Typically this 
file will be served by angular in the `assets` folder. 

In most cases a settings file will already exist in your application. however, if you 
need a starting point a template json file is provided in `sei-settings/templates`

#### Implementation
CWD settings module provides a settings service for the entire application through an 
APP_INITIALIZER

in your `app.module.ts` file provide the following code 

In your NgModule declaration be sure to use `CwdSettingsModule.forRoot()` method optionally providing 
a settings config. 

```typescript
import {CwdSettingsModule, CwdSettingsConfig} from './sei-cwd-common';

export const settings: CwdSettingsConfig = {
  url: `assets/config/settings.json`,
  envUrl: `assets/config/settings.env.json`
};

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CwdSettingsModule.forRoot(settings),
  ],
  providers: [],
  bootstrap: [AppComponent]
})
```

#### Classes / Interfaces
##### SeiSettingsConfig
|property|required|default|
|---|---|---|
| url     | no | `/assests/config/settings.json`    |
| envUrl  | no | `/assets/config/settings.env.json` | 



