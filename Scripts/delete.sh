#!/bin/bash
docker run -d -v "<target folder>":<container folder> -e BACKUP_COMMAND="Delete <container folder> $1" --name schedulebackup_delete --rm schedulebackup