# Resources and Requirements

## Requirements

- Kubernetes
  - For bare metal/lab installs, we recommend [K3s](https://docs.k3s.io/)
- vCenter/Proxmox (for virtualization)

## Recommended

- Helm
- OAuth Provider: We typically use [Keycloak](https://www.keycloak.org/documentation)

## Crucible Applications and GitHub Pages

- Alloy [API](https://github.com/cmu-sei/Alloy.Api) / [UI](https://github.com/cmu-sei/Alloy.ui)
- Blueprint [API](https://github.com/cmu-sei/Blueprint.Api) / [UI](https://github.com/cmu-sei/Blueprint.Ui)
- Caster [API](https://github.com/cmu-sei/Caster.Api) / [UI](https://github.com/cmu-sei/Caster.Ui)
- CITE [API](https://github.com/cmu-sei/CITE.Api) / [UI](https://github.com/cmu-sei/CITE.Ui)
- Gallery [API](https://github.com/cmu-sei/Gallery.Api) / [UI](https://github.com/cmu-sei/Gallery.Ui)
- Gameboard [API](https://github.com/cmu-sei/Gameboard) / [UI](https://github.com/cmu-sei/Gameboard-ui)
- Player [API](https://github.com/cmu-sei/Player.Api) / [UI](https://github.com/cmu-sei/Player.Ui) / [Console UI](https://github.com/cmu-sei/Console.Ui)
- Player [VM API](https://github.com/cmu-sei/Vm.Api) / [VM UI](https://github.com/cmu-sei/Vm.Ui)
- Steamfitter [API](https://github.com/cmu-sei/Steamfitter.Api) / [UI](https://github.com/cmu-sei/Steamfitter.Ui)
- TopoMojo [API](https://github.com/cmu-sei/TopoMojo) / [UI](https://github.com/cmu-sei/topomojo-ui)

## Crucible Helm Charts

All Crucible applications have Helm charts provided in the [SEI's Helm charts repository](https://github.com/cmu-sei/helm-charts). Documentation for each chart and the applications' settings are provided in README files alongside each application's chart. Modify the settings using the values YAML file in your deployment.

To add the SEI's Helm charts repository:

```bash
helm repo add sei https://helm.cmusei.dev/charts
helm repo update
```

In addition to the application charts, there is a [Crucible Umbrella Chart](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible) that includes all application charts and some third-party charts as dependent sub-charts. Using the Umbrella chart will deploy the full crucible stack under one Crucible Helm deployment. More details on deployment using the Umbrella chart is in the [Umbrella Chart Deployment section](#umbrella-helm-chart-deployment).

### Other Helm Charts

The Crucible stack relies on other opens source services that vendor their own Helm charts.

- [Moodle](https://github.com/bitnami/charts/tree/main/bitnami/moodle) - Open-source learning management system (LMS) for online courses and training
- [MetalLB](https://github.com/metallb/metallb) - Bare-metal load balancer for Kubernetes that assigns external IPs to services
- [Traefik](https://traefik.github.io/charts/traefik/) - Ingress controller and reverse proxy for routing external traffic to cluster services
- [Rancher](https://github.com/rancher/charts) - Kubernetes cluster management and operations platform
- [Longhorn](https://github.com/longhorn/charts) - Distributed block storage system for persistent volumes in Kubernetes
- [StackStorm](https://github.com/StackStorm/stackstorm-k8s) - Event-driven automation and orchestration engine
- [RocketChat](https://github.com/RocketChat/helm-charts) - Open-source team communication and messaging platform
- [Webmail](https://github.com/cmu-sei/helm-charts/tree/main/charts/webmail) - Web-based email client for in-platform messaging

## Kubernetes Operators

- [Keycloak Operator](https://www.keycloak.org/operator/installation) - Manages Keycloak instances
- [CloudNative-PG](https://cloudnative-pg.io/) - Manages PostgreSQL clusters

## Crucible Terraform Provider

- [Crucible Terraform Provider](https://registry.terraform.io/providers/cmu-sei/crucible/latest/docs)


## Example Umbrella Helm Chart Deployment

A Crucible deployment using umbrella Helm charts consists of four Helm charts to orchestrate the entire stack. Review the documentation for each of these charts to determine how to configure the settings for your deployment.

1. [crucible-operators](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible-operators) - Install Kubernetes Operators for Keycloak and Postgres before deploying applications.
2. [crucible-infra](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible-infra) - Install prerequisite infrastructure (e.g., an ingress controller, storage provider, etc.) before deploying applications.
3. [crucible](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible) - Install all Crucible applications.
4. [crucible-monitoring](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible-monitoring) - Install a Grafana logging, open telemetry, and metrics stack to monitor the Kubernetes cluster and Crucible applications.

### Step 1: Install Operators

Crucible uses Kubernetes operators for PostgreSQL and Keycloak. These are **cluster-scoped infrastructure** that install CRDs and watch all namespaces, so they are deployed separately from the application charts. This provides privilege separation (cluster-admin for operators, namespace access for apps), independent upgrade lifecycles, and CRD safety.

The [crucible-operators](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible-operators) Helm chart installs both operators in a single release:

| Operator | Version | Purpose |
|----------|---------|---------|
| [Keycloak Operator](https://www.keycloak.org/operator/installation) | 26.5.6 | Manages Keycloak instances via `Keycloak` and `KeycloakRealmImport` CRs |
| [CloudNative-PG](https://cloudnative-pg.io/) | 0.25.0 (chart) | Manages PostgreSQL clusters via `Cluster` CRs |

```bash
helm install crucible-operators charts/crucible-operators --wait
```

Verify both operators are running:

```bash
kubectl get pods -l app.kubernetes.io/instance=crucible-operators
```

!!! warning
    When uninstalling, remove all Custom Resources (Keycloak, KeycloakRealmImport, CNPG Cluster) **before** removing operators. Deleting CRDs removes all CRs cluster-wide.

### Step 2: Deploy Infrastructure

The [crucible-infra](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible-infra) chart deploys:

1. CloudNative-PG PostgreSQL cluster - Primary database for all Crucible applications. Managed by the CloudNative-PG Operator [above](#step-1-install-operators).
2. Traefik ingress controller - Ingress Controller routes external traffic to services within the cluster
3. NFS storage provisioner - Provides dynamic NFS-backed persistent volumes for shared storage
4. pgAdmin4 - Web-based PostgreSQL management interface

Each of these services can be configured or disabled based on your deployment's needs by configuring the values file.

```bash
helm install crucible-infra charts/crucible-infra -f crucible-infra.values.yaml
```

### Step 3: Deploy Applications

The [crucible](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible) chart deploys all applications that are part of the [Crucible Framework](https://cmu-sei.github.io/crucible/landing/) as well as the following third-party applications:

1. [Keycloak](https://www.keycloak.org/) - Identity provider for authenticating to the platform. Managed by the Keycloak Operator [above](#step-1-install-operators).
2. [Moodle](https://moodle.org/) - Open-source learning management system (LMS) for online courses and training.

```bash
helm install crucible charts/crucible -f crucible.values.yaml
```

### Step 4: Deploy Monitoring (optional)

The [crucible-monitoring](https://github.com/cmu-sei/helm-charts/tree/main/charts/crucible-monitoring) chart deploys a Grafana logging, open telemetry, and metrics stack to monitor the Kubernetes cluster and Crucible applications. The stack includes:

1. [Grafana](https://grafana.com/grafana/) - Observability dashboards for visualizing metrics, logs, and traces.
2. [Prometheus](https://prometheus.io/) - Time-series metrics collection and alerting system.
3. [Loki](https://grafana.com/loki/) - Log aggregation system designed for efficient storage and querying.
4. [Tempo](https://grafana.com/traces/) - Distributed tracing backend for end-to-end request tracking.
5. [Grafana Alloy](https://grafana.com/alloy/) - OpenTelemetry collector for shipping metrics, logs, and traces.

```bash
helm install crucible-monitoring charts/crucible-monitoring -f crucible-monitoring.values.yaml
```
