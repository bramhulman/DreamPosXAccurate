# Panduan Integrasi Accurate Online API

Sumber ringkasan:

- `D:\Downloads\API Example - Accurate Online.html`
- `D:\Downloads\OAuth - Accurate Online.html`
- `D:\Downloads\Area Developer_ Dokumentasi API.html`
- `D:\Downloads\accurate-online-api-token-1.0.3.pdf`
- `accurate-online-api-token-1.0.3.txt` sebagai hasil ekstraksi PDF yang sudah ada di repo

Catatan path: file ketiga yang tersedia di mesin ini adalah `D:\Downloads\Area Developer_ Dokumentasi API.html`, bukan `D:\Downloads\Area Developer\_ Dokumentasi API.html`.

## 1. Dua Model Otorisasi

Accurate Online mendukung dua pola otorisasi yang perlu dibedakan dengan jelas.

### OAuth 2.0

Dipakai oleh project ini saat ini. Bukti di kode:

- `DreamPosXAccurate\Class\modul.vb` memakai `sOAuthResponseType = "token"` dan scope OAuth.
- `DreamPosXAccurate\Class\clsToken.vb` mengambil `access_token` dari redirect OAuth.
- `DreamPosXAccurate\Class\clsStore.vb` memanggil `db-list.do` dan `open-db.do`.

Header untuk API Accurate setelah database dibuka:

```http
Authorization: Bearer {access_token}
X-Session-ID: {session_dari_open_db}
```

OAuth flow yang terlihat dari dokumentasi:

1. Daftarkan aplikasi di `https://account.accurate.id/developer`.
2. Arahkan user ke:

```text
https://account.accurate.id/oauth/authorize
```

Parameter penting:

- `client_id`: Client ID aplikasi.
- `response_type`: `token` untuk implicit flow, atau `code` untuk authorization code flow.
- `redirect_uri`: harus sama dengan URL OAuth Callback yang didaftarkan.
- `scope`: daftar scope dipisah spasi, contoh `glaccount_view glaccount_save journal_voucher_view journal_voucher_save`.

Untuk authorization code flow, tukar `code` ke token:

```http
POST https://account.accurate.id/oauth/token
Authorization: Basic base64(client_id:client_secret)
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code&code={code}&redirect_uri={redirect_uri}
```

Response berisi `access_token`, `refresh_token`, `expires_in`, dan `scope`. Refresh token:

```http
POST https://account.accurate.id/oauth/token
Authorization: Basic base64(client_id:client_secret)
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token&refresh_token={refresh_token}
```

### API Token

Model ini dijelaskan di PDF `accurate-online-api-token-1.0.3.pdf`. Ini bukan flow yang sama dengan OAuth session.

API Token dibuat oleh user di Accurate Store setelah aplikasi developer diinstall ke Data Usaha. API Token tidak expire selama user masih punya hak akses dan token tidak dicabut. Batas pemanggilan: maksimal 8 request per detik dan 8 proses API paralel.

Header wajib untuk API Token:

```http
Authorization: Bearer {api_token}
X-Api-Timestamp: {timestamp}
X-Api-Signature: {signature}
```

`X-Api-Signature` adalah HMAC-SHA256 dari nilai `X-Api-Timestamp`, memakai `Signature Secret` sebagai key, lalu hasilnya di-encode Base64 atau Hex. Timestamp diterima dalam beberapa format, termasuk `dd/mm/yyyy hh:nn:ss`, ISO 8601, Unix seconds, dan Unix milliseconds. Selisih timestamp dengan server maksimal 600 detik.

Untuk membaca host pada API Token:

```http
POST https://account.accurate.id/api/api-token.do
Authorization: Bearer {api_token}
X-Api-Timestamp: {timestamp}
X-Api-Signature: {signature}
```

Response berisi informasi Data Usaha, user, aplikasi, dan `host`. Host itu dipakai sebagai prefix API Accurate, contoh:

```text
{host}/accurate/api/journal-voucher/list.do
```

## 2. Alur Akses Database Dengan OAuth

Setelah punya `access_token`, aplikasi harus tahu database Accurate mana yang dipakai.

### Ambil daftar database

```http
GET https://account.accurate.id/api/db-list.do
Authorization: Bearer {access_token}
```

Simpan minimal:

- `id` database.
- `alias` database.

### Open database

```http
GET https://account.accurate.id/api/open-db.do?id={database_id}
Authorization: Bearer {access_token}
```

Response penting:

```json
{
  "session": "session-id",
  "host": "https://public.accurate.id"
}
```

Simpan `session` dan `host`. Semua API data transaksi memakai:

```text
{host}/accurate/api/{module}/{action}.do
```

Contoh:

```text
https://public.accurate.id/accurate/api/journal-voucher/bulk-save.do
```

Nilai `host` dapat berubah. Jika mendapat HTTP 308 Permanent Redirect, HTTP client harus mengaktifkan:

- follow redirect,
- tetap memakai HTTP method asli, terutama POST,
- tetap mengirim Authorization header.

Tetap update host tersimpan dari response redirect atau cek ulang host secara berkala.

## 3. Scope Yang Relevan Untuk Jurnal Umum

Scope minimal untuk fitur jurnal umum dan akun:

```text
glaccount_view glaccount_save journal_voucher_view journal_voucher_save
```

Tambahan sesuai kebutuhan:

- `journal_voucher_delete` jika aplikasi perlu hapus jurnal.
- `glaccount_delete` jika aplikasi perlu hapus akun perkiraan.
- `branch_view` jika perlu mencari cabang.
- Scope lain tetap disesuaikan modul yang dipakai.

Scope project saat ini di `modul.vb` sudah mencakup:

```text
company_data branch_view glaccount_view glaccount_save item_view item_save item_category_view item_category_save tax_view tax_save sales_invoice_view sales_invoice_save sales_receipt_view sales_receipt_save journal_voucher_view journal_voucher_save customer_view customer_save
```

## 4. Endpoint Jurnal Umum

Module:

```text
/api/journal-voucher
```

Endpoint:

- `POST {host}/accurate/api/journal-voucher/save.do`
- `POST {host}/accurate/api/journal-voucher/bulk-save.do`
- `GET {host}/accurate/api/journal-voucher/list.do`
- `GET {host}/accurate/api/journal-voucher/detail.do`
- `DELETE {host}/accurate/api/journal-voucher/delete.do`

Header OAuth:

```http
Authorization: Bearer {access_token}
X-Session-ID: {session}
```

### Save Jurnal

Parameter utama:

- `transDate` wajib, format tanggal contoh `31/03/2016`.
- `detailJournalVoucher` wajib.
- `number` opsional, isi jika tidak memakai penomoran otomatis.
- `description` opsional.
- `branchId` atau `branchName` opsional.
- `typeAutoNumber` opsional.
- `id` opsional untuk edit data existing.

Detail jurnal:

- `accountNo` wajib.
- `amount` wajib.
- `amountType` wajib: `DEBIT` atau `CREDIT`.
- `memo` opsional.
- `customerNo`, `vendorNo`, `employeeNo` opsional jika ada lawan transaksi.
- `subsidiaryType` opsional: `CUSTOMER`, `VENDOR`, atau `EMPLOYEE`.
- `departmentName`, `projectNo`, dan `dataClassification1Name` sampai `dataClassification10Name` opsional.
- `rate` dan `primeAmount` opsional untuk mata uang asing.
- `id` dan `_status=delete` dipakai saat edit/hapus baris detail.

Contoh body `save.do`:

```json
{
  "transDate": "11/09/2026",
  "number": "JU-2026-09-0001",
  "description": "Jurnal penjualan harian POS",
  "branchName": "Cabang Utama",
  "detailJournalVoucher": [
    {
      "accountNo": "1101",
      "amount": 1500000,
      "amountType": "DEBIT",
      "memo": "Kas penjualan"
    },
    {
      "accountNo": "4101",
      "amount": 1500000,
      "amountType": "CREDIT",
      "memo": "Pendapatan penjualan"
    }
  ]
}
```

Validasi wajib sebelum kirim:

- Total `DEBIT` harus sama dengan total `CREDIT`.
- Setiap detail wajib punya `accountNo`, `amount`, dan `amountType`.
- `amount` sebaiknya lebih besar dari 0.
- Gunakan akun leaf/detail, bukan akun induk.

### Bulk Save Jurnal

Endpoint:

```http
POST {host}/accurate/api/journal-voucher/bulk-save.do
```

Scope:

```text
journal_voucher_save
```

Maksimal 100 data per request. Dokumentasi menyebut parameter memakai index mulai dari nol, contoh `data[0]`, `data[1]`, dan seterusnya. Jika memakai JSON serializer, pastikan bentuk request sesuai implementasi yang diterima Accurate. Kode project saat ini memakai `bulk-save.do` di `clsTransactions.vb`.

Contoh konseptual:

```json
{
  "data": [
    {
      "transDate": "11/09/2026",
      "number": "JU-2026-09-0001",
      "detailJournalVoucher": [
        { "accountNo": "1101", "amount": 1500000, "amountType": "DEBIT" },
        { "accountNo": "4101", "amount": 1500000, "amountType": "CREDIT" }
      ]
    }
  ]
}
```

### List Jurnal

Endpoint:

```http
GET {host}/accurate/api/journal-voucher/list.do
```

Parameter yang sering dipakai:

- `fields`: contoh `id,number,transDate,description`.
- `sp.page`: mulai dari 1.
- `sp.pageSize`: default 20, bisa dinaikkan sesuai kebutuhan.
- `sp.sort`: contoh `transDate|desc;number|asc`.
- `keywords`: pencarian umum.
- `transDateFilter`: filter tanggal transaksi.
- `lastUpdateFilter`: filter waktu perubahan data.
- `approvalStatusFilter`: `DRAFT`, `UNAPPROVED`, `APPROVED`, `REJECTED`, atau `NEXTUSER_TOAPPROVED`.
- `branchFilter`: filter cabang.

Contoh:

```text
{host}/accurate/api/journal-voucher/list.do?sp.page=1&sp.pageSize=100&fields=id,number,transDate
```

### Detail Jurnal

Endpoint:

```http
GET {host}/accurate/api/journal-voucher/detail.do?id={id}
```

Alternatif identifikasi:

```http
GET {host}/accurate/api/journal-voucher/detail.do?number={number}
```

Dokumentasi menandai `id` wajib, tetapi `number` disebut sebagai alternatif dari `id`. Dalam implementasi, lebih aman pakai `id` jika sudah tersedia.

## 5. Endpoint Akun Perkiraan

Module:

```text
/api/glaccount
```

Endpoint:

- `POST {host}/accurate/api/glaccount/save.do`
- `POST {host}/accurate/api/glaccount/bulk-save.do`
- `GET {host}/accurate/api/glaccount/list.do`
- `GET {host}/accurate/api/glaccount/detail.do`
- `DELETE {host}/accurate/api/glaccount/delete.do`
- `GET {host}/accurate/api/glaccount/get-balance.do`
- `GET {host}/accurate/api/glaccount/get-bs-account-amount.do`
- `GET {host}/accurate/api/glaccount/get-pl-account-amount.do`

Scope:

- `glaccount_view`
- `glaccount_save`
- `glaccount_delete`

### Save Akun

Parameter wajib:

- `accountType`
- `asOf`
- `currencyCode`
- `name`
- `no`

Parameter opsional:

- `parentNo` untuk sub-akun.
- `openBalance` untuk saldo awal.
- `memo`.
- `bankAccount`, `bankAccountName`, `bankCode` untuk akun bank.
- `rate` untuk mata uang asing.
- `id` untuk edit data existing.
- `useUserRoleAccessListId`.

Nilai `accountType`:

```text
ACCOUNT_PAYABLE
ACCOUNT_RECEIVABLE
ACCUMULATED_DEPRECIATION
CASH_BANK
COGS
EQUITY
EXPENSE
FIXED_ASSET
INVENTORY
LONG_TERM_LIABILITY
OTHER_ASSET
OTHER_CURRENT_ASSET
OTHER_CURRENT_LIABILITY
OTHER_EXPENSE
OTHER_INCOME
REVENUE
```

Contoh body:

```json
{
  "no": "4101",
  "name": "Pendapatan Penjualan POS",
  "accountType": "REVENUE",
  "currencyCode": "IDR",
  "asOf": "11/09/2026",
  "memo": "Akun pendapatan dari integrasi POS"
}
```

### List Akun

Endpoint:

```http
GET {host}/accurate/api/glaccount/list.do
```

Parameter umum:

- `fields`: contoh `id,name,no,accountType`.
- `keywords`: pencarian.
- `accountType`: filter jenis akun.
- `leafOnly`: `true` agar tidak menampilkan akun induk.
- `sp.page`, `sp.pageSize`, `sp.sort`.

Contoh:

```text
{host}/accurate/api/glaccount/list.do?sp.page=1&sp.pageSize=100&fields=id,name,no&leafOnly=true
```

### Detail Akun

```http
GET {host}/accurate/api/glaccount/detail.do?id={id}
```

Alternatif:

```http
GET {host}/accurate/api/glaccount/detail.do?no={account_no}
```

## 6. Format Response

Response sukses Accurate umumnya memakai:

```json
{
  "s": true,
  "d": [],
  "r": {}
}
```

Untuk list biasanya ada paging:

```json
{
  "s": true,
  "d": [],
  "sp": {
    "page": 1,
    "pageSize": 20,
    "pageCount": 1,
    "rowCount": 3
  }
}
```

Praktik parsing:

- Cek HTTP status terlebih dahulu.
- Jika body diawali `<`, kemungkinan error HTML/XML seperti `InvalidTokenException`, jangan langsung parse JSON.
- Cek `s`. Jika `false`, log seluruh body response.
- Simpan `d` atau `r` sesuai endpoint.

## 7. Error Yang Perlu Ditangani

Error OAuth/API umum:

- `invalid_token`: token salah, kosong, expired, atau dicabut.
- `Unauthorized`: header Authorization tidak ada atau tidak valid.
- HTTP 308: host berubah, update host dan ulangi request dengan redirect policy benar.
- Response HTML/XML saat kode mengharapkan JSON: tangani sebagai error mentah dan log body.

Error API Token:

- `Invalid or Revoked API Token`
- `Header X-Api-Signature is required`
- `Header X-Api-Signature invalid`
- `Header X-Api-Timestamp is required`
- `Header X-Api-Timestamp invalid`
- `Header X-Api Timestamp difference more than 600 seconds with Server Time`

Untuk project ini, log lama menunjukkan kasus `InvalidTokenException` dan parsing gagal karena response dimulai dengan `<`. Handler request sebaiknya membedakan response JSON dan non-JSON sebelum deserialize.

## 8. Catatan Implementasi Untuk Project Ini

Project saat ini lebih cocok diteruskan dengan OAuth, karena:

- Kode sudah punya flow `access_token -> db-list -> open-db -> session + host`.
- Semua request transaksi sudah mengirim `Authorization: Bearer` dan `X-Session-ID`.
- Scope `journal_voucher_view` dan `journal_voucher_save` sudah ada.

Checklist sebelum kirim jurnal umum:

1. Pastikan `accessToken` tidak kosong.
2. Jalankan `db-list.do`, pilih database, lalu `open-db.do`.
3. Simpan `session` dan `host` terbaru.
4. Ambil daftar akun dengan `glaccount/list.do?leafOnly=true&fields=id,name,no`.
5. Mapping akun lokal ke `accountNo` Accurate.
6. Bentuk `detailJournalVoucher`.
7. Validasi debit sama dengan kredit.
8. Kirim ke `journal-voucher/save.do` atau `bulk-save.do`.
9. Jika gagal, log HTTP status dan body mentah sebelum parse JSON.
10. Jika status 308, update host lalu retry.

Untuk API Token, jangan hanya mengganti token OAuth dengan API Token pada kode lama. API Token perlu tambahan `X-Api-Timestamp` dan `X-Api-Signature`, serta host diambil dari `POST https://account.accurate.id/api/api-token.do`, bukan dari `open-db.do`.


## Rekonsiliasi faktur harian setelah sinkronisasi

`SyncSalesInvoiceDailyRange` memanggil `ReconcileSalesDaily` setelah proses faktur/penerimaan, termasuk saat faktur sudah lengkap sehingga pengiriman dilewati. Pemeriksaan ini hanya membaca Accurate dan tidak mengubah status sukses, menghapus, atau mengirim ulang faktur akibat selisih nilai.

- Sumber lokal: SUM(subtotal), SUM(tax) dari cld_sumsalesdaily, dikelompokkan per olnumber untuk olid dan seluruh rentang hari terkait. Scope mengikuti sumber pengiriman faktur.
- Detail Accurate: GET /accurate/api/sales-invoice/detail.do?number={kode-yyyyMMdd-olnumber}, memakai OAuth Bearer dan X-Session-ID yang sudah tersedia. Nomor respons harus sama persis (abaikan huruf besar/kecil). Parameter number sebagai alternatif id tercantum di HTML dokumentasi lokal.
- Subtotal dibandingkan langsung dengan field subTotal (nama field dibaca tanpa membedakan kapital); tax dibandingkan dengan tax1Amount + tax2Amount bila tax2Amount tersedia. Tidak ada pengurangan service/diskon, atau perhitungan pajak ulang. Service yang dikirim sebagai item bisa menyebabkan selisih subtotal dan tetap dicatat.
- Selisih diuji per faktur sampai dua desimal (AwayFromZero), sehingga selisih antar faktur yang saling menutup tetap terdeteksi. Kolom selisih adalah Accurate dikurangi lokal.
- Field subtotal/tax wajib yang tidak tersedia, faktur tidak ditemukan, dan kegagalan HTTP/DB menghasilkan GAGAL_CEK. Nilai Accurate dan selisih dikosongkan jika pemeriksaan tidak lengkap, bukan dianggap nol.
- Log tambahan: SalesReconciliation.csv di folder yang sama dengan setting ErrLog; fallback folder aplikasi jika folder setting kosong. Format UTF-8, separator titik koma, seluruh nilai diapit kutip, satu baris per tanggal + outlet ID. Sementara untuk pengujian server, semua hasil dicatat: COCOK, SELISIH, GAGAL_CEK, atau TIDAK_ADA_DATA bila tidak ada sumber lokal. Pemeriksaan ulang mengganti baris lama dan hasil cocok tetap disimpan. Riwayat perubahan tidak ditambahkan. Kegagalan penulisan masuk ErrLog.

Validasi: build Release berhasil dengan MSBuild .NET Framework. Masih ada warning referensi OpenQA.Selenium.Winium tidak ditemukan dan komentar XML SyncForm yang tidak valid. Belum menjalankan sinkronisasi live ke database/Accurate. HTML panduan menjelaskan parameter request tetapi tidak menyediakan skema nilai response subtotal/pajak; field response di atas perlu dikonfirmasi pada respons live pertama. Jika berbeda, sistem mencatat GAGAL_CEK, tidak melaporkan cocok.
