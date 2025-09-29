# SeniorProject — Meow 會員與運動訓練管理

一個以 ASP.NET Core 打造的「會員管理 + 運動訓練」範例專案，包含 Web API 與 MVC 網站兩個服務，並以共用 DTO 專案串接。功能涵蓋註冊／登入、會員中心（個人檔案、頭像設定）、訓練影片與訓練組合的管理、訓練紀錄（Session）與統計分析，以及後台管理區域。

## 專案結構
- `Meow.api`：ASP.NET Core Web API（.NET 8、EF Core、SQL Server、Swagger）
- `Meow.web`：ASP.NET Core MVC 前端（.NET 8），透過 Typed `HttpClient` 呼叫後端 API
- `Meow.Shared`：前後端共用的 DTO 專案
- `shared-keys/`：API 與 Web 共享的 Data Protection 金鑰目錄（已在程式設定為 `..\\shared-keys`）

## 主要功能
- 帳號系統
  - 註冊、登入（Cookie 驗證）、變更密碼（BCrypt 雜湊）
  - 會員中心：基本資料（生日、性別、身高、體重）與頭像設定（內建圖庫或上傳自訂圖片）
- 訓練相關
  - 訓練影片（TrainingVideos）：查詢、標籤管理、縮圖上傳，支援 Draft/Published/Archived 狀態
  - 訓練組合（TrainingSets）：查詢、建立/更新/刪除、封面上傳、標籤綁定、項目排序
  - 訓練紀錄（TrainingSessions）：建立 Session、更新單一項目結果、完成 Session（紀錄耗時、熱量與點數）
- 標籤（Tags）：CRUD、依名稱/分類查詢
- 分析（Analytics）
  - Admin Weekly：近一週總時數、完成次數、活躍會員數、Top 活躍會員
  - Member Weekly：個人週統計（日別分鐘數、總次數、總點數）
  - Member Stats：累積次數、總分鐘數、平均每次分鐘、當前與最佳週連續天數
- 後台（`/Admin` 範圍）：儀表板、會員清單、標籤、訓練影片/組合管理

## 技術棧
- 後端：ASP.NET Core Web API、EF Core（Database First）、SQL Server、Swagger
- 前端：ASP.NET Core MVC、Bootstrap、jQuery（僅基本 UI）
- 其他：Cookie 驗證、BCrypt、Typed `HttpClient`、Data Protection 共用金鑰

## 本機開發環境
- .NET 8 SDK
- SQL Server（本機或遠端皆可）
- 受信任的本機 HTTPS 憑證
  - `dotnet dev-certs https --trust`

## 設定與啟動
1) 設定資料庫連線字串（API）
- 在 `Meow.api` 目錄使用 User Secrets（或以環境變數／`appsettings.*.json` 配置）：
  - `dotnet user-secrets set "ConnectionStrings:AppDb" "Server=.;Database=SeniorProject;Trusted_Connection=True;TrustServerCertificate=True"`

2) 設定後端 API BaseUrl（Web）
- 預設在 `Meow.web/appsettings.json` 已指向 `https://localhost:7001/`，若執行連線異常，可改用 User Secrets 覆寫：
  - 於 `Meow.web` 目錄：
  - `dotnet user-secrets set "BackendApi:BaseUrl" "https://localhost:7001"`

3) 啟動服務（分別開兩個終端機）
- 啟動 API：
  - `cd Meow.api`
  - `dotnet run`（開發預設為 `https://localhost:7001`）
- 啟動 Web：
  - `cd Meow.web`
  - `dotnet run`（開發預設為 `https://localhost:7002`）
- Swagger：瀏覽 `https://localhost:7001/swagger`

4) Data Protection 共用金鑰
- API 與 Web 皆設定為 `..\\shared-keys`，請確保兩個專案皆能讀寫該資料夾，以共享 Cookie/保護金鑰。

## 資料庫初始化與測試帳號
- 專案採 Database First，請先建立資料庫與資料表（若需 DDL，請依 `Meow.api/Data/*.cs` 的模型定義建立，或向作者索取）。
- 建議直接透過網站註冊建立一般帳號（`/Auth/Register`）。
- 提升為管理員（以 Email 為例）：
  - `UPDATE dbo.Member SET IsAdmin = 1 WHERE Email = 'someone@example.com';`
- 注意：密碼欄位使用 BCrypt，請勿以 SQL 直接塞入雜湊為 SHA-256 的字串，否則登入會失敗。

## 常用 API（節錄）
- Auth：
  - `POST /api/Auth/login`（回傳使用者基本資料，用於前端建立 Cookie）
- Members：
  - `GET /api/Members`、`GET /api/Members/{id}`、`POST /api/Members`
  - `PUT /api/Members/{id}`（更新暱稱）、`PUT /api/Members/{id}/password`
  - `GET /api/Members/recent?take=5`、`GET /api/Members/count`
- Member Profile：
  - `GET /api/Members/{memberId}/profile`、`PUT /api/Members/{memberId}/profile`
  - `PUT /api/Members/{memberId}/avatar/{avatarId}`、`POST /api/Members/{memberId}/avatar/upload`（支援 PNG/JPG/WEBP、<= 5MB）
- Tags：
  - `GET /api/Tags?keyword=`、`POST /api/Tags`、`PUT /api/Tags/{id}`、`DELETE /api/Tags/{id}`
- TrainingVideos：
  - `GET /api/TrainingVideos?keyword=&status=&tagIds=`、`GET /api/TrainingVideos/{id}`
  - `POST /api/TrainingVideos`、`PUT /api/TrainingVideos/{id}`、`DELETE /api/TrainingVideos/{id}`
  - `PUT /api/TrainingVideos/{id}/status`、`PUT /api/TrainingVideos/{id}/tags`
  - `POST /api/TrainingVideos/{id}/thumbnail`（上傳縮圖，PNG/JPG/WEBP、<= 5MB）
- TrainingSets：
  - `GET /api/TrainingSets?keyword=&status=&difficulty=&tagId=`、`GET /api/TrainingSets/{id}`、`GET /api/TrainingSets/meta`
  - `POST /api/TrainingSets`、`PUT /api/TrainingSets/{id}`、`DELETE /api/TrainingSets/{id}`
  - `POST /api/TrainingSets/{id}/cover`（上傳封面，PNG/JPG/WEBP、<= 5MB）
- TrainingSessions：
  - `GET /api/TrainingSessions?memberId=&from=&to=&page=&pageSize=&tagIds=`
  - `POST /api/TrainingSessions?memberId=`（開始 Session）
  - `GET /api/TrainingSessions/{id}`（查明細）
  - `PUT /api/TrainingSessions/{id}/complete`（完成 Session）
  - `PUT /api/TrainingSessions/items`（更新單一項目結果）
- Analytics：
  - `GET /api/Analytics/admin/weekly?start=YYYY-MM-DD&take=5`
  - `GET /api/Analytics/admin/popular-sets?start=...&end=...&take=10`
  - `GET /api/Analytics/weekly?memberId=&start=YYYY-MM-DD`
  - `GET /api/Analytics/member/stats?memberId=`

## 常見疑難排解
- 無法登入或 401/403：
  - 確認 API 與 Web 的 Cookie 名稱、Data Protection 金鑰一致（本專案已設為 `.AspNetCore.Cookies` 與 `..\\shared-keys`）。
  - 確認 Web 的 `BackendApi:BaseUrl` 指向 API 的實際位址與通訊埠。
- HTTPS 憑證錯誤：
  - 先執行 `dotnet dev-certs https --clean` 再 `dotnet dev-certs https --trust`。
- SQL Server 連線錯誤：
  - 檢查 `ConnectionStrings:AppDb` 是否正確、遠端連線與 `TrustServerCertificate=True` 設定。

## Roadmap（後續規劃）
- 訓練中即時計時／回合控管（更貼近 HIIT/重量訓練流程）
- 更多前端 UI/UX 打磨（Bootstrap 元件、RWD、易用性）
- 角色／權限細緻化、審核流程
- 完整資料庫 DDL 與初始化腳本整理

---

開發說明：解更多端點與欄位，請直接參考 Swagger（`https://localhost:7001/swagger`）與程式碼（API Controllers、`Meow.Shared` DTO）。若需要協助或修正 README，歡迎開 Issue。 
