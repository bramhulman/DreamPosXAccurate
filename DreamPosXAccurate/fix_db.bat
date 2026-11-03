@echo off
set MYSQL="C:\Program Files\MySQL\MySQL Server 5.7.18\bin\mysql.exe"
set HOST=localhost
set PORT=3307
set USER=root
set PASS=t4v14n4ul1
set DB=cldmemberfongsheng2

%MYSQL% -h %HOST% -P %PORT% -u %USER% -p%PASS% -D %DB% -e "ALTER TABLE cld_log ADD COLUMN IF NOT EXISTS `type` VARCHAR(50) NULL AFTER `name`;"
echo Step 1 done: cld_log type column

%MYSQL% -h %HOST% -P %PORT% -u %USER% -p%PASS% -D %DB% -e "ALTER TABLE cld_syncsales MODIFY COLUMN TransNo VARCHAR(100) NULL;"
echo Step 2 done: cld_syncsales TransNo

%MYSQL% -h %HOST% -P %PORT% -u %USER% -p%PASS% -D %DB% -e "SELECT itemNo, itemname, flag, brandid, majorgroup FROM cld_itemmaster WHERE itemNo IN ('KMS03','KMS08','KMS09');"
echo Step 3 done: check KMS items

%MYSQL% -h %HOST% -P %PORT% -u %USER% -p%PASS% -D %DB% -e "SELECT busidate, olid, Type, TransNo FROM cld_syncsales WHERE custid=19 AND olid IN (119,120) AND busidate >= DATE_SUB(CURDATE(), INTERVAL 7 DAY) ORDER BY busidate, olid;"
echo Step 4 done: 7-day sync status

pause
