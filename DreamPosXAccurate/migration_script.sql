-- ==============================================================================
-- SKRIP MIGRASI & PERUBAHAN DATABASE DREAMPOS X ACCURATE
-- Database Target: cldmemberfongsheng2
-- Tanggal: 11 September 2026
-- ==============================================================================

USE cldmemberfongsheng2;

-- ------------------------------------------------------------------------------
-- 1. PERUBAHAN STRUKTUR TABEL (DDL)
-- ------------------------------------------------------------------------------

-- A. Menambahkan kolom-kolom log yang dibutuhkan modul sinkronisasi Accurate ke tabel cld_log
--    Mengatasi error: Unknown column 'type' in 'field list' dan Unknown column 'datetime' in 'field list'
ALTER TABLE cld_log 
    ADD COLUMN `type` VARCHAR(50) NULL AFTER `json_payload`,
    ADD COLUMN `name` VARCHAR(500) NULL AFTER `type`,
    ADD COLUMN `status` VARCHAR(50) NULL AFTER `name`,
    ADD COLUMN `remarks` VARCHAR(100) NULL AFTER `status`,
    ADD COLUMN `olid` VARCHAR(50) NULL AFTER `remarks`,
    ADD COLUMN `datetime` DATETIME NULL AFTER `olid`;

-- B. Memperbesar panjang kolom TransNo pada tabel cld_syncsales
--    Mengatasi error: Data too long for column 'TransNo'
ALTER TABLE cld_syncsales 
    MODIFY COLUMN `TransNo` VARCHAR(100) NULL;


-- ------------------------------------------------------------------------------
-- 2. PENYESUAIAN DATA ITEM MASTER (DML)
-- ------------------------------------------------------------------------------

-- A. Pendaftaran Item Khusus '100001' (Service Charge) untuk Brand 2 (Hua Fong)
--    Agar faktur penjualan yang memiliki biaya layanan (Service) tidak ditolak Accurate
INSERT INTO cld_itemmaster (ItemNo, ItemName, Price, Cost, CustID, itemgroup, majorgroup, flag, brandid)
VALUES ('100001', 'Service Charge', 0, 0, 19, 1, 3, 0, 2)
ON DUPLICATE KEY UPDATE flag = 0, brandid = 2, majorgroup = 3;

-- B. Koreksi brandid item menu Hua Fong yang sebelumnya keliru tercatat sebagai brandid=1 (Fongsheng)
--    Menyebabkan item BPT09, RBT02, RBT15 tidak ikut tersinkronisasi ke Accurate Brand 2
UPDATE cld_itemmaster 
SET brandid = 2, flag = 0 
WHERE itemNo IN ('BPT09', 'RBT02', 'RBT15');

-- C. Reset flag item Brand 2 yang belum ada di Accurate Online agar dikirim ulang (SaveItem)
UPDATE cld_itemmaster 
SET flag = 0 
WHERE brandid = 2 AND itemNo IN (
    'KM01','KM02','KM03','KM04','KM05','KM06','KM07','KM09','KM10',
    'KMS01','KMS02','KMS03','KMS04','KMS05','KMS08','KMS09','KMS10',
    'KMST02','KMST04','KMST05','KMST10',
    'KMT02','KMT04','KMT08','KMT10',
    'BM01','BM02','BM03','BM04','BM05','BM06','BM07','BM08','BM09','BM10','BM11','BM12','BM14'
);
