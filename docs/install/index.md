# Resources and Requirements

## Requirements

- Kubernetes
  - For bare metal/lab installs, we recommend [K3s](https://docs.k3s.io/)
- vCenter/Proxmox (for virtualization)

## Recommended

- Helm
- OAuth Provider
  + We currently use [IdentityServer](https://identityserver4.readthedocs.io/en/latest/) and [Keycloak](https://www.keycloak.org/documentation)

## Crucible Applications and GitHub Pages

- [Alloy API](https://github.com/cmu-sei/Alloy.Api)
- [Alloy UI](https://github.com/cmu-sei/Alloy.ui)
- [Caster API](https://github.com/cmu-sei/Caster.Api)
- [Caster UI](https://github.com/cmu-sei/Caster.Ui)
- [Player API](https://github.com/cmu-sei/Player.Api)
- [Player Console UI](https://github.com/cmu-sei/Console.Ui)
- [Player UI](https://github.com/cmu-sei/Player.Ui)
- [Player VM API](https://github.com/cmu-sei/Vm.Api)
- [Player VM UI](https://github.com/cmu-sei/Vm.Ui)
- [Steamfitter API](https://github.com/cmu-sei/Steamfitter.Api)
- [Steamfitter UI](https://github.com/cmu-sei/Steamfitter.Ui)
- [CITE API](https://github.com/cmu-sei/CITE.Api)
- [CITE UI](https://github.com/cmu-sei/CITE.Ui)
- [Gallery API](https://github.com/cmu-sei/Gallery.Api)
- [Gallery UI](https://github.com/cmu-sei/Gallery.Ui)
- [Blueprint API](https://github.com/cmu-sei/Blueprint.Api)
- [Blueprint UI](https://github.com/cmu-sei/Blueprint.Ui)

## Crucible Helm Charts

- [Alloy](https://github.com/cmu-sei/helm-charts/tree/main/charts/alloy)
- [Caster](https://github.com/cmu-sei/helm-charts/tree/main/charts/caster)
- [Player](https://github.com/cmu-sei/helm-charts/tree/main/charts/player)
- [Steamfitter](https://github.com/cmu-sei/helm-charts/tree/main/charts/steamfitter)
- [CITE](https://github.com/cmu-sei/helm-charts/tree/main/charts/cite)
- [Gallery](https://github.com/cmu-sei/helm-charts/tree/main/charts/gallery)
- [Blueprint](https://github.com/cmu-sei/helm-charts/tree/main/charts/blueprint)

## Other Helm Charts

- [MetalLB](https://github.com/metallb/metallb)
- [ingress-nginx](https://github.com/kubernetes/ingress-nginx)
- [Rancher](https://github.com/rancher/charts)
- [Longhorn](https://github.com/longhorn/charts)
- [StackStorm](https://github.com/StackStorm/stackstorm-k8s)
- [RocketChat](https://github.com/RocketChat/helm-charts)
- [Moodle](https://github.com/bitnami/charts/tree/main/bitnami/moodle)
- [Webmail](https://github.com/cmu-sei/helm-charts/tree/main/charts/webmail)
- [Keycloak](https://github.com/bitnami/charts/tree/main/bitnami/keycloak)

## Docker Images

!!! note

    These images mean that there isn't a Helm repository being used to deploy these applications but are currently being used by us. To create a deployment, please view the Kubernetes deployment documentation.

We primarily use these images in setting up a email server. The above Helm charts will pull the correct Docker images. This is completely optional but is what we use during certain exercises.

- [ClamAV](https://hub.docker.com/r/clamav/clamav/)
- [MISP](https://github.com/coolacid/docker-misp)

## Infrastructure

Not all applications require virtualization. Gallery, CITE, Blueprint, Player and Steamfitter all can be ran without a hypervisor.

You are able to run the full Crucible stack on minimal hardware. We usually run on four nodes: one server and three agents. Each node has around 100-250 GB of storage, 8 GB RAM, 2 Cores. This is mainly for Longhorn and StackStorm which takes a lot of resources even when limiting their availability. This is only what we recommend. As stated before, you can run this on one node outside of production. The only concern would be storage space.

# Install Overview

## Certificates

This stack is very dependent on TLS. Please create certificates and add them as secrets into the cluster. Down below will create self-signed certificates for testing. If you are going to install this into production, you will have to change these.

??? example

    ``` json
    {
      "names": [
        {
          "C": "US"
        }
      ],
      "key": {
        "algo": "rsa",
        "size": 2048
      },
      "CN": "Foundry Appliance Host",
      "hosts": ["$DOMAIN", "*.$DOMAIN"]
    }
    ```

    ``` bash
    cfssl gencert -initca certificates/root-ca.json | cfssljson -bare root-ca
    cfssl gencert -ca certificates/root-ca.pem -ca-key certificates/root-ca-key.pem -config certificates/config.json \
                -profile intca certificates/int-ca.json | cfssljson -bare int-ca
    cfssl gencert -ca certificates/int-ca.pem -ca-key certificates/int-ca-key.pem -config certificates/config.json \
                -profile server certificates/host.json | cfssljson -bare host
    ```

    ``` bash
    kubectl create secret tls appliance-cert --key certificates/host-key.pem --cert <( cat certificates/host.pem certificates/int-ca.pem ) --dry-run=client -o yaml | kubectl apply -f -
    kubectl create secret generic appliance-root-ca --from-file=appliance-root-ca=certificates/root-ca.pem --dry-run=client -o yaml | kubectl apply -f -
    ```

## Load Balancer

If you're using a cloud provider for your Kubernetes cluster, you do not have to worry about supplying your own load balancer. If you are installing this on bare metal, which would be a majority of the time if you are testing the software, you will have to provide a load balancer. We recommend using MetalLB. The documentation will guide you on how to install this into your cluster.

- [Helm Install MetalLB](https://metallb.universe.tf/installation/#installation-with-helm)
- [Configuring MetalLB](https://metallb.universe.tf/configuration/)

??? example

    ``` bash
    helm upgrade -i metallb metallb/metallb --namespace metallb-system --create-namespace
    ```

## Ingress

In order to access these services, you need to be able to communicate to the cluster. The easiest way to do this is to add `ingress-nginx` to your cluster. Before you install this, you have to have an active load balancer. Here's a one liner using Helm to install `ingress-nginx`:

??? example

    ``` bash
    helm upgrade -i nginx ingress-nginx/ingress-nginx --namespace nginx --create-namespace --set controller.watchIngressWithoutClass=true --set controller.kind=Deployment --set controller.ingressClassResource.name=nginx --set controller.ingressClassResource.default=true --set controller.ingressClass=nginx
    ```

## Rancher

K3s is created by Rancher but Rancher itself is a GUI to help configure your Kubernetes cluster if you are already using K3s. If you are not using K3s, please do not install this application. Rancher will also help you get to and configure Longhorn.

??? example

    ``` bash
    helm upgrade -i rancher rancher-stable/rancher --namespace cattle-system --create-namespace --set bootstrapPassword=$RANCHER_PASS --set replicas=1 --set auditLog.level=2 --set auditLog.destination=hostPath --set hostname=rancher.$DOMAIN --set ingress.tls.source=secret --set ingress.tls.secretName=name-of-certificate
    ```

## Longhorn

Longhorn is used to easily manage, create, and backup persistent volumes (PVs) and persistent volume claims (PVCs). You do not have to install this but you will have to manage your own PVs and PVCs if you are not using a cloud provider.

??? example

    ``` bash
    helm upgrade -i longhorn longhorn/longhorn --namespace longhorn-system --create-namespace --set persistence.defaultClassReplicaCount=1 --wait
    ```

## PostgreSQL and pgAdmin

Majority of the applications above use PostgreSQL. We also use pgAdmin to help manage the database. This may differ if you're using a cloud provider.

??? example

    ``` bash
    helm upgrade -i postgresql bitnami/postgresql --set global.storageClass=longhorn --set global.postgresql.auth.postgresPassword=$POSTGRES_PASS
    ```

[Here's the chart for pgAdmin that we use.](https://github.com/rowanruseler/helm-charts/blob/master/charts/pgadmin4/values.yaml)

??? example

    ``` bash
    helm upgrade -i pgadmin runix/pgadmin4 -f -
    ```

## Crucible Installation

All of the Crucible applications have their settings on the GitHub page and can be modified in the values YAML file on the corresponding Helm chart. There are settings within each application you do have to set up in order for communication. We have populated environment files and scripts that help guide you with this part of the installation. These are located at these two GitHub pages:

- [k3s-install](https://github.com/avershave/k3s-install)
- [k3s-production](https://github.com/sei-noconnor/k3s-production)

These contain the necessary values and setup procedures to install the entire Crucible stack. More information on these settings can be located on the individual GitHub pages.
