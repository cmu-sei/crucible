# Installing Longhorn

> This is an example script installing Longhorn.

```bash
# Not setting ingress because we're going to be using Rancher
# If not using Rancher, set the ingress
function longhorn () {
    helm upgrade -i longhorn longhorn/longhorn --namespace longhorn-system --create-namespace --set persistance.defaultClassReplicaCount=1 --wait

    kubectl create secret tls appliance-cert --key certificates/host-key.pem --cert <( cat certificates/host.pem certificates/int-ca.pem ) --dry-run=client -o yaml | kubectl apply -f - --namespace longhorn-system
}
```

Longhorn, created by Rancher, is a lightweight, reliable and easy-to-user distributed block storage system for Kubernetes. This makes it so that you do not have to create your own Persistant Volumes and Persistant Volume Claims.

Although it is recommended to install Rancher with Longhorn (and the entire K3s stack), it is not required. To access the Longhorn UI, you would have to create an additional Ingress resource to point to the Longhorn UI service. In addtion, you would also have to add your certificate for TLS if needed.

**Please note:** Longhorn has to be placed into it's own namespace, `longhorn-system`, as shown in the above script.
