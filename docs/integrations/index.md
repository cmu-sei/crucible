# **Third-Party Applications**

*Integrate Applications in the Crucible Framework*

The Crucible Framework provides users the necessary tools and resources for integrating open-source third-party applications with the platform's features and data. By leveraging open-source resources, users can save time and resources, and benefit from the expertise of the open-source community.

Additionally, the platform can be tailored to the user's needs by allowing administrators to add applications that will leverage and integrated with the necessary features needed for an exercise. This enables administrators to provide a more personalized experience with the platform and to use applications that are specifically designed to meet their unique needs.

By allowing customization, the platform can become more valuable to users, as it can adapt to their evolving needs and preferences. This can lead to a more engaged user base and a more successful platform overall.

## Third-Party Integration Guide

To be able to add third-party applications to the Crucible Framework, the user must have a System Admin permission.

To integrate a third-party application to the Crucible Framework, follow these next steps:

![Crucible View Admin OE](../assets/img/viewAdmin.png)

1. Click on your username in the top right corner of the screen.
2. Select **Administration**.
3. Click on **Application Templates**.
4. Click on **Add Application Template**.
5. Add the **Name** for the application.
6. Add the **Url** of the application.
7. Add an **icon path** to add the logo of the application.
8. If desired to be **Embeddable**, check the box.
9. If desired to be **Loaded in the Background**, check the box.

After these steps, administrators should follow the Crucible Admin Guide to add the application to the desired set of users and/or teams.

## Third-Party Applications

The following are third-party applications that have already been tested and used within the Crucible Framework.

### Mattermost

Mattermost is an open-source, self-hostable online chat service with file sharing, search, and integrations. It is designed as an internal chat for organizations and companies.

To know more about the application: [Mattermost Documentation](https://docs.mattermost.com)

For installation instructions: [Mattermost Installation](https://github.com/cmu-sei/helm-charts/tree/main/charts/mattermost-team-edition)

### Moodle

Moodle is a free and open-source learning management system. Moodle is used for blended learning, distance education, flipped classroom and other online learning projects in schools, universities, workplaces and other sectors.

To know more about the application: [Moodle Documentation](https://docs.moodle.org/401/en/Main_page)

For installation instructions: [Moodle Installation](https://docs.moodle.org/401/en/Installation_quick_guide)

#### - Crucible Plugin for Moodle

This activity plugin, developed by the Software Engineering Institute (SEI), allows Crucible labs and exercises to be integrated into the Moodle LMS. The Moodle activity may embed the Crucible VM app into an iframe or may provide a link for the student to open the full Crucible lab player window in a new tab or window. With this functionality, Crucible labs can be started, accessed and stopped from a Moodle course.

To know more about the plugin and installation instructions: [Moodle Crucible Plugin Documentation](https://github.com/cmu-sei/moodle-mod_crucible)

### osTicket

osTicket is a widely-used open source support ticket system. It seamlessly integrates inquiries created via email, phone and web-based forms into a simple easy-to-use multi-user web interface. Manage, organize and archive all your support requests and responses in one place while providing your customers with accountability and responsiveness they deserve.

To know more about the application: [osTicket Documentation](https://docs.osticket.com/en/latest/)

For installation instructions: [osTicket Installation](https://docs.osticket.com/en/latest/Getting%20Started/Installation.html)

#### - Crucible Plugin for osTicket

A plugin, developed by the Software Engineering Institute (SEI), that provides authentication against an OAuth2 Identity Server and posts ticket event notifications to the Crucible API.

For installation and configuration instructions: [osTicket Crucible Plugin](https://github.com/cmu-sei/osticket-crucible)

### Rocket.Chat

Rocket.Chat is a customizable open-source communications platform for organizations with high data protection standards. It enables real-time conversations between colleagues, other companies, or your customers across web, desktop, or mobile devices.

To know more about the application: [Rocket.Chat Documentation](https://docs.rocket.chat)

For installation instructions: [Rocket.Chat Installation](https://github.com/RocketChat/helm-charts)

### Roundcube

Roundcube is a web-based IMAP email client. It provides full functionality you expect from an email client, including MIME support, address book, folder manipulation, message searching and spell checking.

To know more about the application: [Roundcube Documentation](https://docs.roundcube.net/doc/help/1.1/en_US/)

For installation instructions: [Roundcube Installation](https://github.com/sei-npacheco/webmail)
