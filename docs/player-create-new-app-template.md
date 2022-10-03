# Player How to: Create a new App Template

An *app template*  contains the settings associated with an app that is added to a team's view.  An app template can be created for common apps that are then added to a view. Default settings that are part of the app template can be overridden by a view admin if needed. An app template can be used by any view admin when adding apps to a particular view.  Think of app templates as helpers for configuring common Crucible apps.

Follow the procedures below to create a new app template in Player. These instructions assume that you have been given the appropriate permissions in Player to create a view.

If you have not already done so, in the dropdown next to your username, select **View Administration**.

![player-new-application-template](assets/img/player-new-application-template.png)
1. Under the Administration nav panel, select **Application Templates**.
2. Click **Add Application Template**. 
   - Enter a **Name** for the app template.
   - Enter a **URL** for the app template.
   - Enter the path for the icon.

3. Enable **Embeddable** if desired. Ebeddable is a true/false attribute that tells Player whether or not the app is supported by iFrames.  The Mattermost chat, for example, is not embeddable and must be opened in a separate browser tab.
4. Enable **Load in background** if desired. Load in background is a true/false attribute that tells Player to load the app in a hidden iFrame when Player loads.  This is important for some apps that may require some initialization.
