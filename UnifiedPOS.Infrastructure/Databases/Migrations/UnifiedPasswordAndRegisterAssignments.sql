BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125113415_UnifiedPasswordAndRegisterAssignments'
)
BEGIN
    DROP INDEX [IX_User_OrganizationId_NormalizedUserName] ON [User];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125113415_UnifiedPasswordAndRegisterAssignments'
)
BEGIN
    CREATE TABLE [UserRegister] (
        [UserId] uniqueidentifier NOT NULL,
        [RegisterId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserRegister] PRIMARY KEY ([UserId], [RegisterId]),
        CONSTRAINT [FK_UserRegister_Register_RegisterId] FOREIGN KEY ([RegisterId]) REFERENCES [Register] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserRegister_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125113415_UnifiedPasswordAndRegisterAssignments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_User_NormalizedUserName] ON [User] ([NormalizedUserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125113415_UnifiedPasswordAndRegisterAssignments'
)
BEGIN
    CREATE INDEX [IX_User_OrganizationId] ON [User] ([OrganizationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125113415_UnifiedPasswordAndRegisterAssignments'
)
BEGIN
    CREATE INDEX [IX_UserRegister_RegisterId] ON [UserRegister] ([RegisterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261125113415_UnifiedPasswordAndRegisterAssignments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261125113415_UnifiedPasswordAndRegisterAssignments', N'10.0.14');
END;

COMMIT;
GO

