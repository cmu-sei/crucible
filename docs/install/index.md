# Crucible Install

## Resources

## Importance of Certificates

This installation requires the creation of certificates that are placed in the correct place. This also includes the current vsphere certificate that is located on the landing page at `/downloads`. There is an example of the installation of the certificates in this documentation but make sure that these are correctly located and created. **This is the most troubleshooted issue when it comes to the install. You are able to see these issues by viewing the logs of the application.**

## Importance of DNS for Lab Installs

If you are running this install in a lab, please note that it is highly recommended you also include a DNS server. There are tricky configurations that could throw you into a spiral. For example, the DNS server that's included in the K3S (Rancher) Kubernetes cluster is CoreDNS. This has a configmap that is customizable and during the many different installs had to be accessed and altered. **Most issues that you run into the install will most likely be DNS related and should be troubleshooted first.**

## Usage of Helm

To get the newest version of the helm chart, you are able to pull it down using `helm show values` and using the helm repository. You can direct the output to a yaml file by using `> example.values.yaml`.

For most/all of the values files in this example, they use a combination of the `env` file and substituting the variables with `envsubst`. With the combination of the `helm upgrade -i`, this will install the needed applications into your Kubernetes cluster. This looks like this in the example of player:

```bash
envsubst < values/crucible/player.values.yaml | helm upgrade -i player sei/player -f -
```

This also applies to the other values files included with this documentation.

The reason behind `helm upgrade -i` is so that you are able to install the application if it doesn't exist. If it does, it will then do a `helm upgrade` instead, only applying your changes.

## Note to Application Settings and Environment Variables

These are all located at the related GitHub pages. It's important to say this because a lot of troubleshooting can be done by simply following the application logs and reference the GitHub page. There also could be updates or more information located in the application settings and should be routine to check the settings if there is an update.

## Seed Data for Applications

Most of the applications take in seed data to automate up the process of getting the application up and running. This data is somewhat documented on most of the GitHub pages and also included in the values file on the Helm chart. The documentation is currently limited but describes itself in the examples.
