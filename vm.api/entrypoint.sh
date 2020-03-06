#!/bin/bash
set -e

if [ -z "$ENTRY_DIR" ]; then ENTRY_DIR=entry.d; fi

for f in $(find $ENTRY_DIR/*.sh); do
    echo [entrypoint] running $f ...
    chmod 755 $f
    $f
done

exec "$@"