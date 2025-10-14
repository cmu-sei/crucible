---
title: Gameboard - Admin Notifications
---

# Notifications

The **Notifications** feature allows those users who have elevated permissions such as Admin and Director roles to post alerts and notices for users in the Gameboard system. When users log in, they will see a banner for each active notification. Once a user dismisses a notification, it does not appear again.

This topic assumes you have been granted a role with the appropriate permissions in Gameboard, you are logged in, and you have a game created.

## Creating a Notification

To create a new notification:

1. In the top navigation, select **Admin**, then **Notifications**.
2. Click **Create Notification**.
3. Enter a **Title**.
4. Enter the **Content** of the notification. The content supports Markdown formatting.
5. Optionally, set the **Availability dates**. These dates determine when the notification is visible. If set, the notification will appear to players only *after* the start date and *until* the end date.
6. Optionally, check the **Dismissible?** box. When unchecked, players can't manually remove or dismiss the notification from their screen. The notification continues to appear every time they log in until the admin deletes the notification or the notification reaches its end date (if an end date is set).
7. Select a **Type**:
   - **General Info** (blue)
   - **Warning** (yellow)
   - **Emergency** (red)
8. Click **Save**.

## Managing Notifications

- You can **edit** or **delete** existing notifications from the list.
- If no availability dates are set, the notification remains visible until a player dismisses it.

!!! note "Announcements vs. Notifications: When to Use Each"

    In Gameboard, Announcements and Notifications both share important messages, but they work differently. Announcements go to users who are currently logged in and can be sent to everyone or just a specific team. These are best for real-time updates, like system issues or challenge changes.

    Notifications, however, appear as banners when users log in and stay until dismissed. They ensure everyone sees the message, even if they weren't online logged into Gameboard when it was posted. Use Announcements for immediate alerts and Notifications for updates that need to reach all users over time.
