## CwdToolbarModule

#### Description
The CWD toolbar module creates a common CWD module utilizing angular Cdk portals and content projection.
The goal is to allow imported modules to provide buttons, menus and actions on the toolbar in a dynamic
way.

This allows you to build the toolbar navigation and the actions needed for your application based on the modules
and components that are imported or displayed. 

#### Depenencies
- `@angular/material` - npm
- `@angular/cdk` - npm
- `@angular/flex-layout` - npm

#### Components
- `CwdToolbarComponent`
- `CwdToolbarNavigationItemComponent`
- `CwdToolbarActionItemComponent`



#### Implementation
CWD Toolbar module needs to be imported into your NgModule declaration
```typescript
import {CwdToolbarModule} from './cwd-toolbar.module'; 

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CwdToolbarModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
````
#### Example 
You can then use the components to provide navigation and actions to the toolbar from within your components. typically this code 
will be put in a smart component. 

```html
<cas-cwd-toolbar></cas-cwd-toolbar>
<router-outlet></router-outlet>

<cas-cwd-toolbar-navigation-item>
  <!-- This block will show up in your navigation row -->
  <button mat-button [matMenuTriggerFor]="menu">Administrator@this.ws</button>
  <mat-menu #menu="matMenu">
    <button mat-menu-item>Settings</button>
    <button (click)="logout()" mat-menu-item>Logout</button>
  </mat-menu>
</cas-cwd-toolbar-navigation-item>

```
*app.module.html*



Additionaly you will handle any events within your component.
`<button (click)="logout()" mat-menu-item>Logout</button>`
```typescript
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent {
  title = 'Caster';
  
  constructor(authService: AuthService) {}
  
  logout() {
    this.authService.logout()
  }
}
```
*app.component.ts*

#### Theme
 The module can incorporate with your existing material theme in your `styles.scss file`
 
 ```scss
 import "~@angular/material/theming";
 @include mat-core();
 
 $primary: mat-palette($mat-light-blue);
 $accent: mat-palette($mat-orange, A200, A100, A400);
 $warn: mat-palette($mat-yellow, A200);
 $theme: mat-light-theme($primary, $accent, $warn);
 
 @import "src/app/sei-cwd-common/cwd-toolbar/theme/theme";
 @include angular-material-theme($theme);
 @include cwd-toolbar-theme($theme);
 ``` 


