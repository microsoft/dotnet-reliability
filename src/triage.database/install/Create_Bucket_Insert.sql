-- Licensed to the .NET Foundation under one or more agreements.
-- The .NET Foundation licenses this file to you under the MIT license.
-- See the LICENSE file in the project root for more information.


CREATE PROCEDURE [dbo].[Bucket_Insert]
    @name nvarchar(256)
AS
BEGIN
    DECLARE @BID as int 
	DECLARE @bugUrl as nvarchar(MAX)
    SELECT @BID = [B].[Id]
    FROM [Buckets] AS [B] 
    WHERE [B].[Name] = @name 

    IF @BID IS NULL -- if the module doesn't exist create it
    BEGIN TRY
        INSERT INTO [Buckets] ([Name], [BugUrl]) 
        VALUES ( @name, NULL )
        SELECT @BID = SCOPE_IDENTITY(), @bugUrl = NULL
    END TRY
    BEGIN CATCH    
        SELECT @BID = [B].[Id], @bugUrl = [B].[BugUrl]
        FROM [Buckets] AS [B] 
        WHERE [B].[Name] = @name 
    END CATCH
SELECT @BID, @name, @bugUrl
END