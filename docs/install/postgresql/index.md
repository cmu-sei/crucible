# Installing PostgreSQL

> This is an example that uses the env file to install PostgreSQL.

```bash
function postgresql () {
    if [ "${HAS_LONGHORN}" != "true" ]; then
        echo "Longhorn Required"
        exit 1
    fi
    helm upgrade -i postgresql bitnami/postgresql --set global.storageClass=longhorn --set global.postgresql.auth.postgresPassword=$POSTGRES_PASS
    echo "Postgres Password: $POSTGRES_PASS"
}
```

**The above script uses Longhorn as it's storage. If you were to use a different type of storage, please make that switch.**

PostgreSQL is one of the databases used by both Identity and Crucible. This is a requirement. The above script uses a one-liner with Helm to install PostgreSQL just by setting the `global.storageClass=longhorn` and the `global.postgresql.auth.postgresPassword=$POSTGRES_PASS`. You can either use the included `env` file or set the `$POSTGRES_PASS` variable to install.

For more customizations, please reference or use the values yaml file by [Bitnami](https://github.com/bitnami/charts/blob/main/bitnami/postgresql/values.yaml).