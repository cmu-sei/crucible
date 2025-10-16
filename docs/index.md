# ![An icon representing the Crucible application suite icon](assets/img/home/cruciblelogo.svg "Crucible Application Suite Icon"){: style="height:75px;width:75px"} Introducing Crucible

**Crucible** is an open-source application framework for operating a cyber range. Crucible aims to be both simple and powerful, highly extensible, and cost effective.

Since 2018, Crucible has effectively enabled large-scale Department of Defense (DoD) cyber exercises to increase operator performance. Crucible is now available to the public under open-source licensing.

## Key Features

- Open-source cyber-range application framework
- Modular design with extensive application programming interfaces
- Customizable, immersive, browser-based user interface
- "Infrastructure as code" approach to topology building—enabling scalability, iteration, and reuse
- Flexible integration of powerful, third-party, open-source tools
- Scenario-based exercising
- Efficiency through automation
- Interoperability through open standards

## Addressing Persistent Challenges

Cyber range administrators confront persistent challenges:

- Manual configurations leads to high-labor costs and excessive human error—with limited scalability and automation
- Proprietary range software leads to vendor lock-in and increasing costs
- CMU SEI developed Crucible in response to a decade of experiencing these frictions

## Automating Cyber Experimentation and Exercise

Crucible automates creation of virtual cyber environments featuring modeled topologies, simulated user activity, and scripted scenario events. These environments power individual labs, team-based exercises, and operational experimentation. These simulations can be fully automated or facilitated. Crucible content developers create new templates by specifying a topology, scenario, assessments, and user interfaces. Participants are challenged to perform mission-essential tasks and individual qualification requirements. Each Crucible application is built using the open-source Angular and .NET Core software frameworks.

## Going Simple: Labs/Challenges

[![TopoMojo Logo](assets/img/crucible-icon-topomojo.svg){: style="height:75px;width:75px"}](topomojo/index.md) Crucible’s [**TopoMojo**](/topomojo/index.md) application enables design of simple labs and challenges using form-based configurations. Select and configure virtual machines, define networks, and write a guide. Novice Crucible content developers can easily get productive by using TopoMojo. Choose this app when the benefits of more advanced “infrastructure as code” automation are not needed. TopoMojo supports the configuration and deployment of small virtual environments to two types of hypervisors: VMware vSphere ESXi and Proxmox Virtual Environment KVM (open source). 

## Crafting a Challenge Competition

[![Gameboard Logo](assets/img/crucible-icon-gameboard.svg){: style="height:75px;width:75px"}](gameboard/index.md) Crucible’s [**Gameboard**](gameboard/index.md) application provides game design capabilities and a competition-ready user interface for running your own cybersecurity game. A Crucible content developer can create, clone, manage, and delete games and challenges—for competition or practice.

## Designing User Interfaces

[![Player Logo](assets/img/crucible-icon-player.svg){: style="height:75px;width:75px"}](player/index.md) Crucible's [**Player**](player/index.md) application is the user's window into the virtual environment. Player enables assignment of team membership as well as customization of a responsive, browser-based user-interfaces using various integrated applications. A Crucible system administrator can shape how scenario information, assessments, and virtual environments are presented through the use of integrated applications.

### Player Open-Source Integrations

- **osTicket**, a support ticket system, manages cyber range service requests
- **Mattermost**, a chat service for real-time communications
- **RocketChat**, a chat service for real-time communications
- **Roundcube**, an email service, provides web-based email

## Coding a Topology

[![Caster Logo](assets/img/crucible-icon-caster.svg){: style="height:75px;width:75px"}](caster/index.md) Crucible's [**Caster**](caster/index.md) application enables the "coded" design and deployment of a cyber topology. Using Caster Designs, a novice content developer can avoid scripting OpenTofu code by simply defining variables within pre-configured OpenTofu modules. Caster supports the design and deployment of virtual environments to three types of hypervisors:

- VMware vSphere ESXi
- Microsoft Azure HyperV (public-cloud)
- Proxmox Virtual Environment KVM (open source)

### Caster Open-Source Integrations

- **OpenTofu**, an "infrastructure-as-code" tool, enables scripted deployment of cyber infrastructure
- **GitLab**, a version control system and code-repository, is used to store OpenTofu modules.

## Crafting a Scenario

[![Blueprint Logo](assets/img/blueprint-logo.png)](blueprint/index.md) Crucible's [**Blueprint**](blueprint/index.md) application enables the collaborative creation and visualization of a master scenario event list (MSEL) for an exercise. Scenario events are mapped to specific simulation objectives.

[![Steamfitter Logo](assets/img/crucible-icon-steamfitter.svg){: style="height:75px;width:75px"}](steamfitter/index.md) Crucible's [**Steamfitter**](steamfitter/index.md) application enables the organization and execution of scenario tasks on virtual machines.

### Scenario Open-Source Integrations

- **StackStorm**, an event-driven automation platform, scripts scenario events and senses the virtual environment
- **Ansible**, a software provisioning, configuration management, and application deployment tool, enables post-deployment provisioning of services to infrastructure.

## Animating Activity

[![GHOSTS Logo](assets/img/ghosts_new.png){: style="height:75px;width:75px"}](https://cmu-sei.github.io/GHOSTS/) Crucible's [**GHOSTS**](https://cmu-sei.github.io/GHOSTS/) Non-Player Character (NPC) automation and orchestration framework deploys and shapes the activities of NPCs using Generative AI models.

### GHOSTS Open-Source Integrations

- **Ollama**, a platform designed to run LLaMA 2, Mistral, and other open-source large language models locally on your machine

## Evaluating Threats

[![CITE Logo](assets/img/cite-logo.png)](cite/index.md) Crucible's [**Collaborative Incident Threat Evaluator (CITE)**](cite/index.md) application enables participants from different organizations to evaluate, score, and comment on cyber incidents. CITE also provides a situational awareness dashboard that allows teams to track their internal actions and roles.

## Displaying Incident Information

[![Gallery Logo](assets/img/gallery-logo.png)](gallery/index.md) Crucible's [**Gallery**](gallery/index.md) application enables participants to review cyber incident data by source type, organized by critical infrastructure sector or other categories. Examples of cyber incident data source types include: intelligence, reporting, orders, news, social media, telephone, and email.

## Assessing Performance

![SEER Logo](assets/img/crucible-icon-seer.svg){: style="height:75px;width:75px"} Crucible's **SEER** application enables assessment of team performance. Assessment reports map training objectives to scenario events to performance assessments.

### SEER Open-Source Integrations

- **Moodle/H5P**, an interactive learning management system, eases the embedding of interactive quiz content. Assessments and other user-experience data can be recorded to a learning record store using the Experience API (xAPI).
- **TheHIVE**, a scalable security incident response platform, is tightly integrated with the Malware Information Sharing Platform (MISP).

## Launching a Simulation

[![Alloy Logo](assets/img/crucible-icon-alloy.svg){: style="height:75px;width:75px"}](alloy/index.md) Crucible's [**Alloy**](alloy/index.md) application enables users to launch an on-demand event or join an instance of an already-running simulation. Following the event, reports can provide a summary of knowledge and performance assessments.

## Operational Deployment

Crucible applications implement the OpenID Connect authentication protocol and are integrated with **Keycloak**, an open-source identity authentication service.

Crucible applications are deployed as **Docker** containers, which employ operating system level virtualization to isolate containers from each other. Container deployment, scaling, and management services are obtained using **Kubernetes**, a popular container-orchestration system.

Kubernetes workflow and cluster management are performed using **Argo**, a popular open-source GitOps tool set.

A pre-configured Crucible Appliance virtual machine is available for download.

Beyond government-owned instances, the SEI owns and operates on-premises and cloud-based instances of Crucible:

**Fortress** [fortress.sei.cmu.edu](https://fortress.sei.cmu.edu)

![Fortress Logo](assets/img/fortress-app.svg){: style="height:75px;width:75px"}
