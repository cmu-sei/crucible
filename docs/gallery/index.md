# **Gallery**
*Keeping You in the Know!*

## Overview

### What is Gallery?

**Gallery** is a web application where participants receive incident information.

In the Gallery User Interface, there are two major functional sections:

- Gallery Wall: The dashboard that displays "cards" to help participants visualize the incident.
- Gallery Archive: A collection of information that contains relevant reporting, intelligence, news, and social media.

For installation, refer to these GitHub repositories.

- [Gallery UI Repository](https://github.com/cmu-sei/Gallery.Ui)

- [Gallery API Repository](https://github.com/cmu-sei/Gallery.Api)

## Administrator Guide

### Users

The following image shows the Users Administration Page. Here, administrators can add and delete users. Additionally, administrators will be able to assign the necessary permissions to each user.

The available permissions are:

- System Admin: Permission that will grant a user all administration privileges on the Gallery application.
- Content Developer: Will be provided the permission to manage other Gallery Admin pages except the Users Admin page and their permissions.

Most users won't have any permissions assigned in this application.

![Gallery Users Admin OE](../assets/img/galleryUsersAdmin.png)

**Add a User**

Assuming that the user has been granted the appropriate permissions by the exercise admnistrator, follow these steps to add a User.

![Add Gallery User OE](../assets/img/addGalleryUser.png)

1. Under the Users Administration View, click **Add User**.
2. Add a **Name** for the user.
3. Add a **User ID** that will be unique for the user.
4. Add an **Email** for the user.
5. Click **Save**
6. After adding the user to Gallery, select the desired permissions to be assigned by cliking on the checkboxed next to the user.

If necessary, a User can be deleted by clicking on the **Trash Icon** next to the desired User.

In the same way, a User can be edited by cliking on the **Edit Icon** next to the desired User.

### Collections

The following image shows the Collections Administration Page. Here, administrators can add and delete collections. These are where the articles will be assigned to, in the case there are multiple exercises running at the same time.

![Collections Admin OE](../assets/img/collectionsAdmin.png)

**Add a Collection**

Assuming that the user has been granted the appropriate permissions by the exercise administrator, follow these steps to add a Collection.

![Add Collection OE](../assets/img/addCollection.png)

1. Under the Collections Administration View, click **Add Collection**.
2. Add a **Name** for the collection.
3. Add a **Description** about the collection.
4. Click **Save**.

If necessary, a Collection can be deleted by cliking on the **Trash Icon** next to the desired Collection.

In the same way, a Collection can be edited by cliking on the **Edit Icon** next to the desired Collection.

### Cards

The following image shows the Cards Administration Page. Here, administrators can add and delete cards. These are the different cards presented in the Gallery Wall and where different articles related to that card can be found. 

![Cards Admin OE](../assets/img/cardsAdmin.png)

**Add a Card**

Assuming that the user has been granted the appropriate permissions by the exercise administrator, follow these steps to add a Card.

![Add Card OE](../assets/img/addCard.png)

1. Under the Cards Administration View, click **Add Card**.
2. Add a **Name** for the card.
3. Add a **Description** about the card.
4. From the options, select the desired **Collection**.
5. Click **Save**.

If necessary, a Card can be deleted by cliking on the **Trash Icon** next to the desired Card.

In the same way, a Card can be edited by cliking on the **Edit Icon** next to the desired Card.

### Articles

The following image shows the Articles Administraton Page. Here, administrators can add and delete articles. These are different articles providing information from different sources to keep the exercise going.

![Articles Admin OE](../assets/img/articlesAdmin.png)

**Add an Article**

Assuming that the user has been granted the appropriate permissions by the exercise administrator, follow these steps to add an Article.

![Add Article OE](../assets/img/addArticle.png)

1. Under the Article Administration View, click **Add Article**.
2. Add a **Name** for the article.
3. Provide a **Description** about the article.
4. From the options, select the corresponding **Card**.
5. From the options, select the desired **Status**.
6. From the options, select the appropriate **Source Type**.
7. Add the **Source Name** for the article.
8. If necessary, provide a **URL** to redirect users to a PDF or link.
9. If desired, check the **Open URL in new tab** box.
10. Add the appropriate **Move Nubmer** for the article.
11. Add the appropriate **Inject Number** for the article.
12. Specify the appropriate **Date/Time** for the article.
13. Click **Save**.

If necessary, an Article can be deleted by clicking on the **Trash Icon** next to the desired Article.

In the same way, an Article can be edited by clicking on the **Edit Icon** next to the desired Article.

### Exhibits

The following images shows the Exhibits Administration Page. Here, administrators configure the actual exercise to be run based on the teams, collections and articles previously configured.

![Exhibits Admin OE](../assets/img/exhibitsAdmin.png)

**Add an Exhibit**

Assuming that the user has been granted the appropriate permissions by the exercise administrator, follow these steps to add an Exhibit.

![Add Exhibit OE](../assets/img/addExhibit.png)

1. Under the Exhibit Administration View, click **Add Exhibit**.
2. Provide the **Current Move Number**.
3. Provide the **Current Inject Number**.
4. Provide the **Scenario ID**.
5. Click **Save**.

If necessary, an Exhibit can be deleted by clicking on the **Trash Icon** next to the desired Exhibit.

In the same way, an Exhibit can be edited by clicking on the **Edit Icon** next to the desired Exhibit.

**Configure an Exhibit**

To configure an exhibit to be used for an exercise, administrators will need to add Exhibit Teams, Card Teams, and Article Teams. To do this, follow these steps.

![Configure Exhibit OE](../assets/img/configureExhibit.png)

*Exhibit Teams*

![Exhibit Teams OE](../assets/img/exhibitTeams.png)

1. Add a Team to the Exhibit.
2. Select the team to be configured and click on it to expand its configuration details.
3. Under the **All Users** tab, users that have not been assigned to the team will be shown. To add them to the team, click on **Add User**.
4. Under the **Team Users** tab, users that have already been assigned to the team will be shown. To remove a user from the team, click on **Remove**.

*Card Teams*

![Card Teams OE](../assets/img/cardTeams.png)

1. Click on the **+** on the Card Teams section. 
2. From the options, select the desired **Team**.
3. From the options, select the appropriate **Card**.
4. Add the **Move Number**.
5. Add the **Inject Number**.
6. If desired, check the **Is Shown On Wall** box.
7. Click **Save**.

*Article Teams*

![Article Teams OE](../assets/img/articleTeams.png)

1. Select the **Card** to be configured.
2. Under the **Exhibit Teams** tab, teams that haven't been assigned to an article will be shown. To add them to the Article Teams, click on **Add**.
3. Under the **Article Teams** tab, teams that have already been assigned will be shown. To remove a team, click on **Remove**.

## User Guide

### Gallery Wall

The Gallery Wall is the inject delivery dashboard with red, yellow, and green status indicators. Each of these cards have a specifc set of actions, which will help users throughout the in-game exercise. 

The following image will show some important hotspots about the Gallery Wall. Reference the number on the hotspot to know more about this section.

![Gallery Wall OE](../assets/img/galleryWall.png)

#### Title
*Hotspot 1:*

The title of the card.

#### Description
*Hotspot 2:*

A brief description of the event.

#### Date Posted
*Hotspot 3:*

The date and time the card was last updated.

#### Unread Articles
*Hotspot 4:*

The number of articles left to read from the event.

#### Details
*Hotspot 5:*

Provides additional details than those provided in the Gallery Wall Dashboard. All articles related to the event will be filtered and shown to provide more information.

#### Wall & Archive Toggle
*Hotspot 6:*

By using this icon, users can toggle between the Gallery Wall & Gallery Archive.

### Gallery Archive

The Gallery Archive is a collection of information that contains relevant reporting, intelligence, news, and social media data sources.

The following image will show some important hotspots about the Gallery Archive. Reference the number on the hotspot to know more about this section.

![Gallery Archive OE](../assets/img/galleryArchive.png)image.png

#### Search & Sources
*Hotspot 1:*

These articles come from different categories of sources: reporting, news, intel, and social media. The archive contains all "move" data that has been shared up to this point in the exercise. Users can search, sort, and filter information in the archive.

To search the archive, enter the terms in the **Search the Archive** field. The search feature automatically narrows down the results.

#### Archive Information
*Hotspot 2:*

The information in the Gallery Archive is displayed in articles. Each article contains the **Title, Source Type, Source Name, and Date Posted**.

For the information included on the article:

- Title: The title of the intelligence report.
- Source Type: The source of the intelligence report (News, Intel, Reporting, or Social Media).
- Source Name: The specific person or agency who supplied the intelligence.
- Date Posted: The Date and time stamp of when the intelligence report was posted.

#### Read
*Hotspot 3:*

This feature will indicate if the user has read the intelligence or not.

#### Share
*Hotspot 4:*

With this feature, users can share an article with other users using a mail service.

To share an article with another team, click **Share**. In the **Share Article** screen:

1. Under **Share with...**, select a team.
2. Under **Email Contents...**, make any edits to the Subject and Message of the article.
3. Click **Share**.

#### More
*Hotspot 5:*

When enabled, attached documents with additional information will be provided for users to access and read.

#### Filter
*Hotspot 6:*

Users can use the dropdown to further filter intelligence information. Users can sort the Gallery articles based on their categories. This will be useful for users that are searching for information from a specific category or article.

#### Wall & Archive Toggle
*Hotspot 7:*

By using this icon, users can toggle between the Gallery Wall & Gallery Archive.
