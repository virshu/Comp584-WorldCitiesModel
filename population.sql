BEGIN TRANSACTION;
GO

ALTER TABLE [Cities] ADD [Population] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221004034702_Population', N'6.0.9');
GO

COMMIT;
GO

