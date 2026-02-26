-- ================================================
-- 系统设置 种子数据
-- ================================================

-- 1. 插入分类
INSERT INTO `SB_SystemSetting_Category` (`Id`, `Name`, `Icon`, `Sort`) VALUES
(1, '基础配置', '', 100),
(2, '区块链配置', '', 90),
(3, '缓存配置', '', 80)
ON DUPLICATE KEY UPDATE `Name` = VALUES(`Name`), `Sort` = VALUES(`Sort`);

-- 2. 更新已有的 ReceiveAddress，补上分类
UPDATE `SB_SystemSetting` SET `CategoryId` = 2, `Name` = '收款地址', `CantDelete` = 1, `Sort` = 80
WHERE `Key` = 'ReceiveAddress';

-- 3. 插入不存在的设置项
INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 1, 'BaseHost', '前端域名', 'Input', 1, '', '', 100
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'BaseHost');

INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 1, 'ServiceUrl', '客服链接', 'Input', 1, '', '', 90
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'ServiceUrl');

INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 1, 'WhitePaper', '白皮书地址', 'Input', 1, '', '', 80
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'WhitePaper');

INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 2, 'EthExchangeUsdcRate', 'ETH/USDC 汇率', 'InputNumber', 1, '0', '', 100
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'EthExchangeUsdcRate');

INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 2, 'AccountPermissionsAddress', '授权合约地址', 'Input', 1, '', '', 90
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'AccountPermissionsAddress');

INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 3, 'UserAddressCacheTime', '用户地址缓存时间(分钟)', 'InputNumber', 1, '30', '', 100
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'UserAddressCacheTime');

INSERT INTO `SB_SystemSetting` (`CategoryId`, `Key`, `Name`, `DisplayType`, `CantDelete`, `DataContent`, `DataContentConfig`, `Sort`)
SELECT 3, 'PrizeSpanHour', '奖励时间间隔(小时)', 'InputNumber', 1, '24', '', 90
FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM `SB_SystemSetting` WHERE `Key` = 'PrizeSpanHour');
