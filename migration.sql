-- ========================================
-- Migration: Rename Pinyin to English
-- Date: 2024
-- IMPORTANT: Backup database before executing!
-- ========================================

-- 1. Rename Tables
RENAME TABLE `SB_Guanliyuan` TO `SB_Admin`;
RENAME TABLE `SB_Guanliyuan_Juese` TO `SB_AdminRole`;
RENAME TABLE `SB_Guanliyuan_Juese_Menu` TO `SB_AdminRoleMenu`;
RENAME TABLE `SB_Yonghu` TO `SB_User`;

-- 2. Rename Columns in SB_Admin (was SB_Guanliyuan)
ALTER TABLE `SB_Admin`
  CHANGE COLUMN `Yonghuming` `Username` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `Mima` `Password` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `Mingcheng` `Name` VARCHAR(255) NULL,
  CHANGE COLUMN `Touxiang` `Avatar` VARCHAR(255) NULL,
  CHANGE COLUMN `Zhuangtai` `Status` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `ChuangjianShijian` `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CHANGE COLUMN `ZaixianShijian` `LastOnlineTime` TIMESTAMP NULL,
  CHANGE COLUMN `Weishanchu` `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0,
  CHANGE COLUMN `JueseId` `RoleId` INT NULL;

-- 3. Rename Columns in SB_AdminRole (was SB_Guanliyuan_Juese)
ALTER TABLE `SB_AdminRole`
  CHANGE COLUMN `Mingcheng` `Name` VARCHAR(255) NULL,
  CHANGE COLUMN `JueseKey` `RoleKey` VARCHAR(255) NULL,
  CHANGE COLUMN `Weishanchu` `IsDeleted` TINYINT(1) NOT NULL DEFAULT 0;

-- 4. Rename Columns in SB_AdminRoleMenu (was SB_Guanliyuan_Juese_Menu)
ALTER TABLE `SB_AdminRoleMenu`
  CHANGE COLUMN `JueseId` `RoleId` INT NOT NULL;

-- 5. Rename Columns in SB_User (was SB_Yonghu)
ALTER TABLE `SB_User`
  CHANGE COLUMN `Yonghuming` `Username` VARCHAR(255) NULL,
  CHANGE COLUMN `Mima` `Password` VARCHAR(255) NULL,
  CHANGE COLUMN `Touxiang` `Avatar` VARCHAR(255) NULL,
  CHANGE COLUMN `Shoujihao` `Phone` VARCHAR(255) NULL,
  CHANGE COLUMN `Mingcheng` `Name` VARCHAR(255) NULL,
  CHANGE COLUMN `Zhuangtai` `Status` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `ChuangjianShijian` `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CHANGE COLUMN `ZaixianShijian` `LastOnlineTime` TIMESTAMP NULL;

-- 6. Rename Columns in SB_UserRecord
ALTER TABLE `SB_UserRecord`
  CHANGE COLUMN `Shoujihao` `Phone` VARCHAR(255) NULL,
  CHANGE COLUMN `Guojia` `Country` VARCHAR(255) NULL,
  CHANGE COLUMN `Xieyi` `Agreement` TEXT NULL,
  CHANGE COLUMN `XieyiImagePath` `AgreementImagePath` VARCHAR(255) NULL,
  CHANGE COLUMN `Jiluzhuangtai` `RecordStatus` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `ChuangjianShijian` `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CHANGE COLUMN `YongHuXingMing` `FullName` VARCHAR(255) NULL,
  CHANGE COLUMN `InteractZhangHu` `InteractAccount` VARCHAR(255) NULL,
  CHANGE COLUMN `WeiXinYongHuMing` `WechatUsername` VARCHAR(255) NULL,
  CHANGE COLUMN `ShengXiaoRiQi` `EffectiveDate` TIMESTAMP NULL,
  CHANGE COLUMN `KouKuanRiQi` `DeductionDate` TIMESTAMP NULL,
  CHANGE COLUMN `ZhangDanDiZhi` `BillingAddress` VARCHAR(255) NULL,
  CHANGE COLUMN `SheBeiXingHao` `DeviceModel` VARCHAR(255) NULL,
  CHANGE COLUMN `YunYingShang` `Carrier` VARCHAR(255) NULL,
  CHANGE COLUMN `DianZiYouJian` `Email` VARCHAR(255) NULL,
  CHANGE COLUMN `KouKuanDanWei` `DeductionUnit` VARCHAR(255) NULL;
