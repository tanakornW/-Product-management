# Product Code Manager

เว็บแอปสำหรับจัดการรหัสสินค้า 30 หลัก (ตัวอักษร A–Z และตัวเลขเท่านั้น) พร้อมสร้าง QR Code และยืนยันก่อนลบ พัฒนาแบบ full-stack: Backend ASP.NET Core 9 + EF Core, Frontend Vue 3 + Vite

## โครงสร้างโปรเจ็กต์

```
- backend/          ASP.NET Core Web API + EF Core (SQLite default, Postgres optional)
- frontend/         Vue 3 + Vite single-page app
- tools/            Utility console apps (ไม่จำเป็นต่อการรันหลัก)
```

## ข้อกำหนด

- .NET 9 SDK
- Node.js 18+ และ Yarn

## การตั้งค่า Backend

1. เข้าโฟลเดอร์ `backend`
2. (ครั้งแรก) กำหนด provider ถ้าต้องการ PostgreSQL:

   ```json
   // appsettings.Development.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=...;Port=...;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true"
     },
     "DatabaseProvider": "Postgres"
   }
   ```

   หากต้องการ SQLite (default) ให้ตั้ง `"DatabaseProvider": "Sqlite"` และใช้ `"Data Source=products.db"`

3. รัน API:

   ```bash
   cd backend
   dotnet run
   ```

4. API จะอยู่ที่ `http://localhost:5271`
5. ระบบจะ seed ตัวอย่าง 3 รายการให้โดยอัตโนมัติเมื่อฐานข้อมูลยังว่าง

## การตั้งค่า Frontend

1. เข้า `frontend`
2. ติดตั้ง dependency:

   ```bash
   yarn install
   ```

3. ตั้งค่า API base (ถ้าต้องการปรับ):

   - สร้างไฟล์ `.env` แล้วระบุ `VITE_API_BASE_URL=http://localhost:5271`
   - หากไม่ตั้งจะใช้ค่า default ดังกล่าวอยู่แล้ว

4. รัน dev server:

   ```bash
   yarn dev --port 5175
   ```

5. เปิดเว็บที่ `http://localhost:5175`

## ฟีเจอร์หลัก

- แสดงรายการรหัสสินค้า (ค้นหาได้แบบ real-time)
- ปุ่ม `Add` เปิด dialog เพิ่มรหัส (บังคับรูปแบบ 30 ตัวอักษร A–Z/0–9, ใส่ `-` อัตโนมัติ)
- ปุ่ม `QR` แสดง modal พร้อม QR Code PNG ดาวน์โหลดได้จาก API
- ปุ่ม `Delete` แสดงยืนยันก่อนลบ
- ปุ่ม `รีเฟรช` ดึงข้อมูลใหม่จาก API
- state, error, loading แสดงบนหน้าให้รู้สถานะทันที

## การ deploy / ปรับฐานข้อมูล

- ถ้าจะใช้งาน PostgreSQL ให้ติดตั้งแพ็กเกจ `Npgsql` แล้วตั้งค่า `DatabaseProvider` เป็น `Postgres` ตามตัวอย่าง
- สามารถสร้างตาราง `products` ด้วยโค้ดใน `ProductDbContext` (`EnsureCreated`) หรือรัน SQL:

  ```sql
  CREATE TABLE IF NOT EXISTS products (
      id SERIAL PRIMARY KEY,
      code VARCHAR(35) NOT NULL UNIQUE,
      created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
  );
  ```

## Testing

- ปัจจุบันไม่มี unit test (โดนถอดออกตามคำขอล่าสุด) หากต้องการเพิ่มสามารถสร้างโปรเจ็กต์ใน `backend.tests` แล้วอ้างอิง service ได้

## Scripts/คำสั่งที่ใช้บ่อย

| คำสั่ง | ใช้ทำอะไร |
| ------ | --------- |
| `dotnet run` (ใน `backend`) | รัน API |
| `yarn dev --port 5175` (ใน `frontend`) | รันเว็บ UI |
| `yarn build` | สร้างไฟล์ static ของ frontend |

## หมายเหตุ

- หาก frontend รันบนพอร์ตอื่น (5173/5174/5175) backend เปิด CORS ไว้แล้ว (ดู `Program.cs`)
- ตั้งค่าปุ่ม Add ให้ block ตัวอักษรที่ไม่อนุญาตและไม่ให้พิมพ์เกิน 30 ตัวตั้งแต่ต้น

พร้อมใช้งานทั้ง backend และ frontend รีสตาร์ตด้วยคำสั่งด้านบนได้เลย ✨

