 ALTER TABLE [ServerUserJoin] DROP CONSTRAINT [ServerUserJoinServerPk]
 ALTER TABLE [ServerUserJoin] DROP CONSTRAINT [ServerUserJoinDiscordUserPk]
 ALTER TABLE [ServerPresenceJoin] DROP CONSTRAINT [ServerPresenceJoinServerPk]
 ALTER TABLE [ServerPresenceJoin] DROP CONSTRAINT [ServerPresenceJoinPresencePk]
 ALTER TABLE [ServerChannelJoin] DROP CONSTRAINT [ServerChannelJoinServerPk]
 ALTER TABLE [ServerChannelJoin] DROP CONSTRAINT [ServerChannelJoinChannelPk]

 DROP TABLE [Server];
 DROP TABLE [ServerUserJoin];
 DROP TABLE [DiscordUser];
 DROP TABLE [Presence];
 DROP TABLE [ServerPresenceJoin];
 DROP TABLE [Channel];
 DROP TABLE [ServerChannelJoin];


