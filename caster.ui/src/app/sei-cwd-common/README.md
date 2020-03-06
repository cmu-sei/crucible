## SEI CWD Common Modules  
#### Description  
SEI CWD Common modules are a set of angular modules that are common between CWD apps. These modules   
are interdependent on each other, however they can be overridden with your own modules.   
  
#### Modules  
- [Settings Module](cwd-settings/README.md)  
- [Auth Module](cwd-auth/README.md) 
- [Toolbar Module](cwd-toolbar/README.md) 
  
#### Replacing Services  
If you would like to replace or extend any of these modules. Angular makes this process very simple.  
  
```typescript  
// Example of replacing the settings service from the CWD settings module.  
import {CwdSettingsService} from './sei-cwd-common'  
  
  
NgModule({  
 declarations: [ AppComponent ], 
 imports: [ 
   BrowserModule, 
   AppRoutingModule, 
   CwdSettingsModule.forRoot(settings), 
   CwdAuthModule, 
   ],
      // Replace the CwdSettingsService in the providers array
        
   providers: [  
    {provide: CwdSettingsService, useClass: <YOUR_CUSTOM_CLASS>},
  ], 
  bootstrap: [AppComponent]
})  
````   
  
Cwd Modules will expect a certain format for dependent modules, services and components. As such you   
should extend existing classes when possible. In the future SEI CWD Common will provide `abstract`   
classes to assist in the implementation of your own modules, services and components.
