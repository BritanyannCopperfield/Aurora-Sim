ALTER TABLE `users` 
	ADD COLUMN `webLoginKey` varchar(36) default '00000000-0000-0000-0000-000000000000',
COMMENT='Rev. 2';