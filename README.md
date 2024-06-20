# 操作說明
## 更換 gcp 專案憑證 
`google_client_secrets.json`
此檔案為 GCP 專案中的 OAuth 2.0 用戶端憑證，請將憑證的 json 檔下載後，把內容覆蓋進來，以替換正確的 OAuth 用戶端

**用戶端應用程式類型請創建 "電腦版應用程式"**

## `新增 google 帳號`

執行 
```
Scripts/auth_user.sh {account}
```

{account} 替換為使用者名稱

執行完畢後會在 Credentials 資料夾中產生此 user name 的 credential 檔案 "Google.Apis.Auth.OAuth2.Responses.TokenResponse-{account}"

檔案出現之後重新打包 docker 即可。

## 打包 docker image (進入以下之前 務必 務必 務必 要先取得使用者授權憑證 json 檔案)
```
docker build -t schedulebackup .
```

or
```
sh Script/build.sh
```


## `執行備份排程`
crontab 排程執行 

```
Scripts/backup.sh {date}
```
## `執行刪除排程`
crontab 排程執行 

```
Scripts/delete.sh {date}
```
## `Docker 指令參數說明`

```
docker run -d -v "{本地目標目錄}":"{容器掛載目錄}" -e BACKUP_COMMAND="{執行動作} {容器掛載目錄} {使用者名稱} {日期}" --name schedulebackup_backup --rm schedulebackup
```

- **{本地目標目錄}**: 本地要執行備份 or 刪除的資料夾路徑

- **{容器掛載目錄}**: 容器中用來掛載目標目錄的資料夾路徑(此資料夾名稱會影響雲端上的根資料夾名稱)

- **{執行動作}**: 分為 "Auth"、"Backup"、"Delete"

- **{使用者名稱}**: 此處應輸入一開始新增 google 帳號時輸入的 {account}，來指定備份帳號。

- **{日期}**: 判斷用日期. ex: 2024/06/12

- -d: 背景執行
- -v: 掛載資料夾
- -e: 環境變數
- --name: 容器名稱
- --rm: 運行完畢後自動刪除容器