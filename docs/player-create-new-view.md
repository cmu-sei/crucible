# Player How to: Create a new View

Follow the procedures below to create a new *view* in Player. These instructions assume that you have been given the appropriate permissions in Player to create a view.

If you have not already done so, in the dropdown next to your username, select **Administration**.

### Step 1: Complete View Information
![player-new-view](assets/img/player-new-view.png)

1. Under Views, click **Add New View**.
2. Add a **Name** and **View Description**.
3. In the Status dropdown, select **Active** or **Inactive**.
   - `Active` means that the new view is available for use immediately.
   - `Inactive` means that the new view will be cloned in Alloy.
4. Select **Applications**.

### Step 2: Add new applications

1. Under Applications, click **Add New Application**. Here, you can add a blank application or an application based upon an existing app template. 
   - **Blank Application:** Adding a blank application requires you to enter the configuration settings manually. These settings can't be applied in another view; they are one-time use only.
   - **Template:** You should have several application templates available to choose from. These are templates that you or another administrator have created to use over and over. The configuration settings are set in the template; the template can be used many times. Application template settings can be overridden for a particular view. For help understanding application templates, see the [Player Guide](https://cmu-sei.github.io/crucible/player-guide) and the Player How to: Create a new App Template.
2. Click **Teams**.

### Step 3: Add new teams

1. Click **Add New Team**. Multiple teams can be added to a view.

2. Enter a **Team Name**.

3. Assign a **Role** to the team.

4. Assign **Permissions** to the team. Each team can be assigned special permissions. You may want to have a team of "admins" who can troubleshoot views in addition to teams comprised of regular users who are participating in the simulation.

5. Click the **User** icon to select users to add to the new team.

   - **Search** for the user whom you want to add. 
   - Click **Add User** to move the user from All Users to Team Users.
   - Under Team Users, you can assign a **Role** to the user at this time.
   - Click **Done** when you are finished adding users to the team.

6. Assign applications to the new team.

   - Next to the new team, click **Add Application**. Select an application from the list. These are the applications you added above. Each team you create gets a list of applications displayed in the Player application bar in the order defined here.

### Step 4: Upload Files

In this step, View Administrators upload a single file or multiple files simultaneously to a View which can be added as an application and attached to a team. 

1. Under Files, click **Choose File** and select the file you want to upload. The file appears under Staged Files - it has not been uploaded yet. 
2. Select the **Team(s)** that you want to access the file and click **Upload Staged File(s)**.
3. The file appears under **Uploaded Files**. From here, you can: **Download** the file, **Delete** the file, **Copy Link** to the file, **Edit** the name and team of the file, and **Add File as Application**.

After adding the file as an application you have to return to **Step 3 Teams** and add that application - the _newly_ uploaded file - to a team just as you would add any new application.

Click **Done** when you are finished adding or updating the view.
