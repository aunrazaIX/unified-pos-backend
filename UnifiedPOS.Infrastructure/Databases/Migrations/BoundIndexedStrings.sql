BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'UserName');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [User] ALTER COLUMN [UserName] nvarchar(100) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_User_OrganizationId_NormalizedUserName] ON [User];
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[User]') AND [c].[name] = N'NormalizedUserName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [User] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [User] ALTER COLUMN [NormalizedUserName] nvarchar(100) NOT NULL;
    CREATE UNIQUE INDEX [IX_User_OrganizationId_NormalizedUserName] ON [User] ([OrganizationId], [NormalizedUserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_Sale_ReceiptNumber] ON [Sale];
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sale]') AND [c].[name] = N'ReceiptNumber');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Sale] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Sale] ALTER COLUMN [ReceiptNumber] nvarchar(100) NULL;
    EXEC(N'CREATE UNIQUE INDEX [IX_Sale_ReceiptNumber] ON [Sale] ([ReceiptNumber]) WHERE [ReceiptNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_Register_LocationId_Name] ON [Register];
    DECLARE @var3 nvarchar(max);
    SELECT @var3 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Register]') AND [c].[name] = N'Name');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Register] DROP CONSTRAINT ' + @var3 + ';');
    ALTER TABLE [Register] ALTER COLUMN [Name] nvarchar(100) NOT NULL;
    CREATE UNIQUE INDEX [IX_Register_LocationId_Name] ON [Register] ([LocationId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_OutboxMessage_Type_AggregateId] ON [OutboxMessage];
    DECLARE @var4 nvarchar(max);
    SELECT @var4 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OutboxMessage]') AND [c].[name] = N'Type');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [OutboxMessage] DROP CONSTRAINT ' + @var4 + ';');
    ALTER TABLE [OutboxMessage] ALTER COLUMN [Type] nvarchar(180) NOT NULL;
    CREATE UNIQUE INDEX [IX_OutboxMessage_Type_AggregateId] ON [OutboxMessage] ([Type], [AggregateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_Location_OrganizationId_Name] ON [Location];
    DECLARE @var5 nvarchar(max);
    SELECT @var5 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Location]') AND [c].[name] = N'Name');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Location] DROP CONSTRAINT ' + @var5 + ';');
    ALTER TABLE [Location] ALTER COLUMN [Name] nvarchar(100) NOT NULL;
    CREATE UNIQUE INDEX [IX_Location_OrganizationId_Name] ON [Location] ([OrganizationId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_Customer_OrganizationId_Phone] ON [Customer];
    DECLARE @var6 nvarchar(max);
    SELECT @var6 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Customer]') AND [c].[name] = N'Phone');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Customer] DROP CONSTRAINT ' + @var6 + ';');
    ALTER TABLE [Customer] ALTER COLUMN [Phone] nvarchar(20) NULL;
    CREATE INDEX [IX_Customer_OrganizationId_Phone] ON [Customer] ([OrganizationId], [Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    DROP INDEX [IX_Category_OrganizationId_Name] ON [Category];
    DECLARE @var7 nvarchar(max);
    SELECT @var7 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Category]') AND [c].[name] = N'Name');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Category] DROP CONSTRAINT ' + @var7 + ';');
    ALTER TABLE [Category] ALTER COLUMN [Name] nvarchar(100) NOT NULL;
    CREATE UNIQUE INDEX [IX_Category_OrganizationId_Name] ON [Category] ([OrganizationId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124135101_BoundIndexedStrings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124135101_BoundIndexedStrings', N'10.0.14');
END;

COMMIT;
GO

