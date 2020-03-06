
## CwdAuthModule  
  
#### Description  
The CWD Auth module is a module that interacts with the CWD Identity server, providing authentication  
to the application.   
#### Dependencies  
- 'oidc-client' - npm   
- CWD Settings Service Located in the [CWD Settings Module](../cwd-settings/README.md)  
  
#### Routes  
The CWDAuth Module provides its own routes for the application.   
- `auth-callback` - Component to process OIDC authorization and validation.  
- `auth-callback-silent` - Component to precess silent authorization as well as token refresh  
- `logout` - Logs out the user from the Identity server and returns them to the home page.   
  
  
#### Implementation  
CWD Auth module only needs to be imported into your NgModule declaration. Keep in mind the CWD  
Auth module is dependent on the CWD Settings Service.   
```typescript  
@NgModule({  
 declarations: [ AppComponent ], 
 imports: [ 
  BrowserModule, 
  AppRoutingModule, 
  CwdSettingsModule.forRoot(settings), 
  CwdAuthModule, ], 
  providers: [], 
  bootstrap: [AppComponent]
})  
```
