Meow — 會員系統＋訓練紀錄管理平台

一個結合**會員管理**與**訓練紀錄**的 ASP.NET Core 專案（前後端分離：Meow.Api、Meow.Web）。

A. 功能列表（作品集 5 大功能）

- 登入功能（Email／密碼）
- 會員中心（查看／編輯基本資料）
- 變更密碼
- 訓練紀錄列表（查閱個人訓練紀錄）
- 管理後台（基本維護作業）

> 未完成部分（短期規劃）放在文末 Roadmap。

B. 技術棧

- 後端：ASP.NET Core、EF Core、SQL Server
- 前端：ASP.NET Core MVC（HTML / CSS / JavaScript）
- 其他：DI、分層架構、DTO、Swagger

---

C. 如何啟動（本機開發）

> 預設連線：API：https://localhost:7001/、Web：https://localhost:7002/

1. 準備環境

- .NET 8 SDK
- SQL Server（本機或遠端）
- 信任本機 HTTPS 憑證（第一次需要）：

> dotnet dev-certs https --trust
 
---

2. 設定環境變數（或 User Secrets）



2-1. API（Meow\.Api）需要資料庫連線字串：

- 進入 Meow.Api 專案資料夾後執行其一：

- Option A：環境變數
> setx ConnectionStrings__Default "Server=.;Database=SeniorProject;Trusted_Connection=True;TrustServerCertificate=True"

- Option B：User Secrets
> dotnet user-secrets set "ConnectionStrings:Default" "Server=.;Database=SeniorProject;Trusted_Connection=True;TrustServerCertificate=True"



2-2. Web（Meow\.Web）需要後端 API 位址：

- 進入 Meow.Web 專案資料夾後

> dotnet user-secrets set "BackendApi:BaseUrl" "https://localhost:7001"

---

3. 建庫 & 啟動

- 專案是 Database First：

- 請先在 SQL Server 建立資料庫並執行 DDL（如：/db/ddl.sql），完成後在 Meow.Api：

> dotnet run

- 最後啟動前端 Meow.Web（另一個終端機）：

> dotnet run

- API：https://localhost:7001/
- Web：https://localhost:7002/

---

4. 測試帳號 / 密碼

> 登入採 BCrypt 雜湊，可用下方「種子資料（可選）」插入兩個測試帳號：

> admin01@example.com / admin01（系統管理者）

> user01@example.com / user01（一般會員）

---

5. 種子資料（可選）

> 在 SQL Server 執行


USE [SeniorProject];
GO

DECLARE @pwd NVARCHAR(100) = N'P@ssw0rd!';
DECLARE @hash VARBINARY(32) = HASHBYTES('SHA2_256', @pwd);
DECLARE @hashHex NVARCHAR(64) = LOWER(CONVERT(VARCHAR(64), @hash, 2));

-- Admin
INSERT INTO dbo.Member (Email, PasswordHash, Nickname, IsAdmin, Status)
VALUES (N'admin@demo.test', @hashHex, N'Admin', 1, N'Active');

-- Normal User
INSERT INTO dbo.Member (Email, PasswordHash, Nickname, IsAdmin, Status)
VALUES (N'user@demo.test', @hashHex, N'User', 0, N'Active');

---

6. API 清單（精簡版—對應已完成功能）

> 端點以 https://localhost:7001 為例，實際以 Swagger 或程式為準。

6-1. **Auth**

  * POST /api/auth/login：登入（回傳使用者基本資訊／Token〔如有〕）

6-2. **Members**

  * GET /api/members/me：取得自己的會員資料
  * PUT /api/members/me：更新會員資料
  * PUT /api/members/me/password：變更密碼

6-3. **TrainingSessions**

  * GET /api/training-sessions?Page=1&PageSize=20`：查詢訓練紀錄列表
  * GET /api/training-sessions/{id}：查詢單筆（若已完成）
  * PUT /api/training-sessions/{id}/complete`：完成訓練（若已完成）

6-4. **Admin**

  * GET /api/admin/members：成員清單（管理）
  * GET /api/admin/dashboard：後台儀表（若有）

> 若目前以資料表說明為主，也可在 `/docs/schema.md` 補上各表重點（主鍵、索引、計算欄位如 `EmailNormalized` 等）。

---

7. 截圖

請將以下檔案放到：


/docs/screenshots/login.png
/docs/screenshots/member-center.png
/docs/screenshots/admin.png


README 會這樣引用：
![Login](/docs/screenshots/login.png)
![Member Center](/docs/screenshots/member-center.png)
![Admin](/docs/screenshots/admin.png)

---

8. Roadmap（結訓前完成）

8-1. 預計可輸入關鍵字或選取標籤（Tag）搜尋訓練課程。　

8-2. 計時器功能：訓練時啟動倒數計時，完成後自動記錄到 Session。

8-3. 葉面視覺優化：使用前端框（Bootstrap）提升介面一致性與美觀度。

---

9. 開發小抄

* API 與 Web 連線異常（HTTPS）時，先執行：

  ```bash
  dotnet dev-certs https --clean
  dotnet dev-certs https --trust
  ```
* 連線被拒絕時，確認 `BackendApi:BaseUrl` 是否指向 **API 的實際埠號**。
* 若 DB 在另一台機器，請把 `ConnectionStrings:Default` 改成對應伺服器位址，並勾選 `TrustServerCertificate=True`（開發用）。
