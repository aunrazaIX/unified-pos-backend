IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditEvent] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NULL,
        [ActorId] uniqueidentifier NOT NULL,
        [Action] nvarchar(1000) NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [Reason] nvarchar(1000) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_AuditEvent] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Operation] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [ActorId] uniqueidentifier NOT NULL,
        [Key] nvarchar(180) NOT NULL,
        [PayloadHash] nvarchar(1000) NOT NULL,
        [ResponseJson] nvarchar(max) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Operation] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Organization] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(1000) NOT NULL,
        [Currency] nvarchar(1000) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Organization] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [OutboxMessage] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [Type] nvarchar(1000) NOT NULL,
        [AggregateId] uniqueidentifier NOT NULL,
        [Payload] nvarchar(max) NOT NULL,
        [ProcessedOn] datetime2 NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_OutboxMessage] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Category] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [Name] nvarchar(1000) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Category] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Category_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Customer] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [Name] nvarchar(1000) NOT NULL,
        [Phone] nvarchar(1000) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Customer] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Customer_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Location] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [Name] nvarchar(1000) NOT NULL,
        [Address] nvarchar(1000) NOT NULL,
        [TimeZoneId] nvarchar(1000) NOT NULL,
        [ReceiptHeader] nvarchar(1000) NOT NULL,
        [Hours] nvarchar(1000) NOT NULL,
        [TaxRate] decimal(19,4) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Location] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Location_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [User] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [UserName] nvarchar(1000) NOT NULL,
        [NormalizedUserName] nvarchar(1000) NOT NULL,
        [PasswordHash] nvarchar(1000) NOT NULL,
        [PinHash] nvarchar(1000) NULL,
        [Role] int NOT NULL,
        [IsActive] bit NOT NULL,
        [FailedAttempts] int NOT NULL,
        [LockedUntil] datetime2 NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_User] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_User_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Product] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [ParentProductId] uniqueidentifier NULL,
        [Name] nvarchar(1000) NOT NULL,
        [Sku] nvarchar(100) NOT NULL,
        [Barcode] nvarchar(100) NULL,
        [Price] decimal(19,4) NOT NULL,
        [IsTaxable] bit NOT NULL,
        [MinimumAge] int NOT NULL,
        [IsActive] bit NOT NULL,
        [MergedIntoId] uniqueidentifier NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Product] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Product_Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Category] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Product_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Product_Product_MergedIntoId] FOREIGN KEY ([MergedIntoId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Product_Product_ParentProductId] FOREIGN KEY ([ParentProductId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [DayClose] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [BusinessDate] date NOT NULL,
        [ClosedBy] uniqueidentifier NOT NULL,
        [SalesTotal] decimal(19,4) NOT NULL,
        [RefundTotal] decimal(19,4) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_DayClose] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DayClose_Location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Location] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Register] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [Name] nvarchar(1000) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Register] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Register_Location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Location] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [UserLocation] (
        [UserId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserLocation] PRIMARY KEY ([UserId], [LocationId]),
        CONSTRAINT [FK_UserLocation_Location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Location] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserLocation_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [StoreStock] (
        [Id] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] decimal(19,4) NOT NULL,
        [IsBlocked] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_StoreStock] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StoreStock_Location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Location] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_StoreStock_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Session] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RegisterId] uniqueidentifier NULL,
        [RefreshTokenHash] nvarchar(64) NOT NULL,
        [ExpiresOn] datetime2 NOT NULL,
        [Revoked] bit NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Session] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Session_Register_RegisterId] FOREIGN KEY ([RegisterId]) REFERENCES [Register] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Session_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Shift] (
        [Id] uniqueidentifier NOT NULL,
        [RegisterId] uniqueidentifier NOT NULL,
        [CashierId] uniqueidentifier NOT NULL,
        [StartingCash] decimal(19,4) NOT NULL,
        [ExpectedCash] decimal(19,4) NULL,
        [CountedCash] decimal(19,4) NULL,
        [ClosedOn] datetime2 NULL,
        [ReviewedBy] uniqueidentifier NULL,
        [ReviewReason] nvarchar(1000) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Shift] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Shift_Register_RegisterId] FOREIGN KEY ([RegisterId]) REFERENCES [Register] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Shift_User_CashierId] FOREIGN KEY ([CashierId]) REFERENCES [User] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Cart] (
        [Id] uniqueidentifier NOT NULL,
        [ShiftId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [DiscountAmount] decimal(19,4) NOT NULL,
        [DiscountApprovedBy] uniqueidentifier NULL,
        [DiscountReason] nvarchar(1000) NULL,
        [VerifiedBirthDate] date NULL,
        [AgeVerifiedBy] uniqueidentifier NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Cart] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cart_Customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Cart_Shift_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shift] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [CartLine] (
        [Id] uniqueidentifier NOT NULL,
        [CartId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [Quantity] decimal(19,4) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_CartLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CartLine_Cart_CartId] FOREIGN KEY ([CartId]) REFERENCES [Cart] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CartLine_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Sale] (
        [Id] uniqueidentifier NOT NULL,
        [OrganizationId] uniqueidentifier NOT NULL,
        [LocationId] uniqueidentifier NOT NULL,
        [RegisterId] uniqueidentifier NOT NULL,
        [CashierId] uniqueidentifier NOT NULL,
        [ShiftId] uniqueidentifier NOT NULL,
        [CartId] uniqueidentifier NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        [Currency] nvarchar(1000) NOT NULL,
        [BusinessDate] date NOT NULL,
        [Status] int NOT NULL,
        [Subtotal] decimal(19,4) NOT NULL,
        [Discount] decimal(19,4) NOT NULL,
        [Tax] decimal(19,4) NOT NULL,
        [Total] decimal(19,4) NOT NULL,
        [ReceiptNumber] nvarchar(1000) NULL,
        [CompletedOn] datetime2 NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Sale] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Sale_Cart_CartId] FOREIGN KEY ([CartId]) REFERENCES [Cart] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sale_Customer_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customer] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sale_Location_LocationId] FOREIGN KEY ([LocationId]) REFERENCES [Location] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sale_Organization_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organization] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sale_Register_RegisterId] FOREIGN KEY ([RegisterId]) REFERENCES [Register] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Sale_Shift_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shift] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Payment] (
        [Id] uniqueidentifier NOT NULL,
        [SaleId] uniqueidentifier NOT NULL,
        [Tender] int NOT NULL,
        [Amount] decimal(19,4) NOT NULL,
        [Received] decimal(19,4) NOT NULL,
        [Change] decimal(19,4) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Payment] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Payment_Sale_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sale] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [Refund] (
        [Id] uniqueidentifier NOT NULL,
        [SaleId] uniqueidentifier NOT NULL,
        [ShiftId] uniqueidentifier NOT NULL,
        [ApprovedBy] uniqueidentifier NOT NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [Amount] decimal(19,4) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_Refund] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Refund_Sale_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sale] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Refund_Shift_ShiftId] FOREIGN KEY ([ShiftId]) REFERENCES [Shift] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [SaleLine] (
        [Id] uniqueidentifier NOT NULL,
        [SaleId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(1000) NOT NULL,
        [Sku] nvarchar(1000) NOT NULL,
        [Quantity] decimal(19,4) NOT NULL,
        [UnitPrice] decimal(19,4) NOT NULL,
        [Discount] decimal(19,4) NOT NULL,
        [TaxRate] decimal(19,4) NOT NULL,
        [Tax] decimal(19,4) NOT NULL,
        [Total] decimal(19,4) NOT NULL,
        [RefundedQuantity] decimal(19,4) NOT NULL,
        [RefundedAmount] decimal(19,4) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_SaleLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SaleLine_Category_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Category] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SaleLine_Product_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Product] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_SaleLine_Sale_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sale] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE TABLE [RefundLine] (
        [Id] uniqueidentifier NOT NULL,
        [RefundId] uniqueidentifier NOT NULL,
        [SaleLineId] uniqueidentifier NOT NULL,
        [Quantity] decimal(19,4) NOT NULL,
        [Amount] decimal(19,4) NOT NULL,
        [CreatedOn] datetime2 NOT NULL,
        [ModifiedOn] datetime2 NOT NULL,
        [Version] rowversion NOT NULL,
        CONSTRAINT [PK_RefundLine] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefundLine_Refund_RefundId] FOREIGN KEY ([RefundId]) REFERENCES [Refund] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RefundLine_SaleLine_SaleLineId] FOREIGN KEY ([SaleLineId]) REFERENCES [SaleLine] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditEvent_OrganizationId_LocationId_CreatedOn] ON [AuditEvent] ([OrganizationId], [LocationId], [CreatedOn]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cart_CustomerId] ON [Cart] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Cart_ShiftId] ON [Cart] ([ShiftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CartLine_CartId_ProductId] ON [CartLine] ([CartId], [ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_CartLine_ProductId] ON [CartLine] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Category_OrganizationId_Name] ON [Category] ([OrganizationId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Customer_OrganizationId_Phone] ON [Customer] ([OrganizationId], [Phone]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DayClose_LocationId_BusinessDate] ON [DayClose] ([LocationId], [BusinessDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Location_OrganizationId_Name] ON [Location] ([OrganizationId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Operation_OrganizationId_ActorId_Key] ON [Operation] ([OrganizationId], [ActorId], [Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OutboxMessage_Type_AggregateId] ON [OutboxMessage] ([Type], [AggregateId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Payment_SaleId] ON [Payment] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Product_CategoryId] ON [Product] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Product_MergedIntoId] ON [Product] ([MergedIntoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Product_OrganizationId_Barcode] ON [Product] ([OrganizationId], [Barcode]) WHERE [Barcode] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Product_OrganizationId_Sku] ON [Product] ([OrganizationId], [Sku]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Product_ParentProductId] ON [Product] ([ParentProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Refund_SaleId] ON [Refund] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Refund_ShiftId] ON [Refund] ([ShiftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefundLine_RefundId] ON [RefundLine] ([RefundId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefundLine_SaleLineId] ON [RefundLine] ([SaleLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Register_LocationId_Name] ON [Register] ([LocationId], [Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Sale_CartId] ON [Sale] ([CartId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sale_CustomerId] ON [Sale] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sale_LocationId] ON [Sale] ([LocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sale_OrganizationId_LocationId_BusinessDate] ON [Sale] ([OrganizationId], [LocationId], [BusinessDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Sale_ReceiptNumber] ON [Sale] ([ReceiptNumber]) WHERE [ReceiptNumber] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sale_RegisterId] ON [Sale] ([RegisterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Sale_ShiftId] ON [Sale] ([ShiftId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleLine_CategoryId] ON [SaleLine] ([CategoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleLine_ProductId] ON [SaleLine] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SaleLine_SaleId] ON [SaleLine] ([SaleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Session_RefreshTokenHash] ON [Session] ([RefreshTokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Session_RegisterId] ON [Session] ([RegisterId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Session_UserId] ON [Session] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Shift_CashierId] ON [Shift] ([CashierId]) WHERE [ClosedOn] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Shift_RegisterId] ON [Shift] ([RegisterId]) WHERE [ClosedOn] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StoreStock_LocationId_ProductId] ON [StoreStock] ([LocationId], [ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StoreStock_ProductId] ON [StoreStock] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_User_OrganizationId_NormalizedUserName] ON [User] ([OrganizationId], [NormalizedUserName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserLocation_LocationId] ON [UserLocation] ([LocationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261124131154_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261124131154_InitialCreate', N'10.0.14');
END;

COMMIT;
GO

