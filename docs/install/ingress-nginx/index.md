# Install ingress-nginx

# Resources

* [ingress-nginx Documentation](https://docs.nginx.com/nginx-ingress-controller/)
* [ingress-nginx Helm Chart](https://github.com/kubernetes/ingress-nginx/blob/main/charts/ingress-nginx)
* [ingress-nginx Values File](https://github.com/kubernetes/ingress-nginx/blob/main/charts/ingress-nginx/values.yaml)
* [Installing ingress-nginx with Helm](https://docs.nginx.com/nginx-ingress-controller/installation/installation-with-helm/)

> Example of a install script that just uses a one liner to install ingress-nginx into the Kubernetes environment.

```bash
function ingress-nginx () {
    helm upgrade -i nginx ingress-nginx/ingress-nginx --namespace nginx --create-namespace --set controller.watchIngressWithoutClass=true --set controller.kind=Deployment --set controller.ingressClassResource.name=nginx --set controller.ingressClassResource.default=true --set controller.ingressClass=nginx
}
```

The Ingress is a Kubernetes resource that lets you configure an HTTP load balancer for applications running on Kubernetes. This is necessary to allow traffic to direct to the applications from outside the cluster.

In the install script, we create a new namespace `nginx`. We set `controller.watchIngressWithoutClass=true` so that you are able to create an Ingress resource without a classname but you are also able to set the class name to `nginx`.

For additional documentation, please use this [link](https://docs.nginx.com/nginx-ingress-controller/).
