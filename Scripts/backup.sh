#!/bin/bash
docker run -d -v "<target folder>":<container folder> -e BACKUP_COMMAND="Backup <container folder> <account> $1" --name schedulebackup_backup --rm schedulebackup