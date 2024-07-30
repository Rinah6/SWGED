USE SoftGED;

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentsSenders')
	BEGIN
		CREATE TABLE DocumentsSenders (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Type INT NOT NULL
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'Projects')
	BEGIN
		CREATE TABLE Projects (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Name NVARCHAR(255) NOT NULL,
			Storage NVARCHAR(255) NOT NULL,
			HasAccessToInternalUsersHandling BIT NOT NULL DEFAULT 0,
			HasAccessToSuppliersHandling BIT NOT NULL DEFAULT 0,
			HasAccessToProcessingCircuitsHandling BIT NOT NULL DEFAULT 0,
			HasAccessToSignMySelfFeature BIT NOT NULL DEFAULT 0,
			HasAccessToArchiveImmediatelyFeature BIT NOT NULL DEFAULT 0,
			HasAccessToGlobalDynamicFieldsHandling BIT NOT NULL DEFAULT 0,
			HasAccessToPhysicalLocationHandling BIT NOT NULL DEFAULT 0,
			HasAccessToTomProLinking BIT NOT NULL DEFAULT 0,
			HasAccessToUsersConnectionsInformation BIT NOT NULL DEFAULT 0,
			HasAccessToNumericLibrary BIT NOT NULL DEFAULT 0,
			HasAccessToDocumentTypesHandling BIT NOT NULL DEFAULT 0,
			HasAccessToDocumentsAccessesHandling BIT NOT NULL DEFAULT 0,
			HasAccessToRSF BIT NOT NULL DEFAULT 0,
			ServerName VARCHAR(255),
			Login VARCHAR(255),
			Password VARCHAR(255),
			DatabaseId INT,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			DeletionDate DATETIME
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'UsersRoles')
	BEGIN
		CREATE TABLE UsersRoles (
			Id INT PRIMARY KEY,
			Title NVARCHAR(255) NOT NULL
		);

		INSERT INTO UsersRoles (Id, Title) VALUES 
		(0, 'Super Admin'),
		(1, 'Admin'),
		(2, 'Utilisateur normal');
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'Users')
	BEGIN
		CREATE TABLE Users (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Username NVARCHAR(255) NOT NULL,
			Email NVARCHAR(255) NOT NULL,
			Password NVARCHAR(255) NOT NULL,
			FirstName NVARCHAR(255) NOT NULL,
			LastName NVARCHAR(255) NOT NULL,
			RoleId INT NOT NULL,
			ProjectId UNIQUEIDENTIFIER,
			IsADocumentsReceiver BIT NOT NULL DEFAULT 0,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (Id) REFERENCES DocumentsSenders(Id),
			FOREIGN KEY (RoleId) REFERENCES UsersRoles(Id),
			FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
			-- FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);

		DECLARE @superAdminId AS UNIQUEIDENTIFIER = NEWID();

		INSERT INTO DocumentsSenders (Id, Type)
		VALUES (@superAdminId, 0);

		INSERT INTO Users 
		(Id, Username, Email, Password, FirstName, LastName, RoleId)
		VALUES (@superAdminId, 'Super Admin', 'super.admin@gmail.com', '$2a$11$vllrJJAZIo9WR9KP2D1h8e2nU8AzQUDCxsNdM.A1MWtTZ5oxVxTkG', 'Super', 'Admin', 0);

		-- SuperAdmin 0
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'UserSignatures')
	BEGIN
		CREATE TABLE UserSignatures (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			UserId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			DeletionDate DATETIME,
			FOREIGN KEY (UserId) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'FieldTypes')
	BEGIN
		CREATE TABLE FieldTypes (
			Id INT PRIMARY KEY,
			Title NVARCHAR(255) NOT NULL
		);

		INSERT INTO FieldTypes (Id, Title) VALUES 
		(0, 'Signature'), 
		(1, 'Paraphe');
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentTypes')
	BEGIN
		CREATE TABLE DocumentTypes (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Title NVARCHAR(255) NOT NULL,
			ProjectId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentTypesSteps')
	BEGIN
		CREATE TABLE DocumentTypesSteps (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			StepNumber INT NOT NULL DEFAULT 0,
			ProcessingDuration FLOAT NOT NULL DEFAULT 0,
			DocumentTypeId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (DocumentTypeId) REFERENCES DocumentTypes(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentTypesUsersSteps')
	BEGIN
		CREATE TABLE DocumentTypesUsersSteps (
			Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
			UserId UNIQUEIDENTIFIER NOT NULL,
			StepId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (UserId) REFERENCES Users(Id),
			FOREIGN KEY (StepId) REFERENCES DocumentTypesSteps(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'Suppliers')
	BEGIN
		CREATE TABLE Suppliers (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			NIF VARCHAR(255),
			STAT VARCHAR(255),
            MAIL VARCHAR(255),
            CIN VARCHAR(255),
            CONTACT VARCHAR(255),
			Name NVARCHAR(255) NOT NULL,
			ProjectId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (Id) REFERENCES DocumentsSenders(Id),
			FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'SuppliersEmails')
	BEGIN
		CREATE TABLE SuppliersEmails (
			Id INT IDENTITY(1, 1) PRIMARY KEY,
			Email NVARCHAR(255) NOT NULL,
			SupplierId UNIQUEIDENTIFIER NOT NULL,
			FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'Documents')
	BEGIN
		CREATE TABLE Documents (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Filename NVARCHAR(255) NOT NULL,
			OriginalFilename NVARCHAR(255) NOT NULL,
			Url NVARCHAR(255) NOT NULL,
			Title NVARCHAR(255) NOT NULL,
			Object NVARCHAR(255) NOT NULL,
			Message NVARCHAR(500) NOT NULL,
			Status INT NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			SenderId UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			CanBeAccessedByAnyone BIT NOT NULL DEFAULT 1,
			PhysicalLocation NVARCHAR(255),
			RSF BIT NOT NULL DEFAULT 0,
			FOREIGN KEY (SenderId) REFERENCES DocumentsSenders(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'SuppliersDocumentsAcknowledgements')
	BEGIN
		CREATE TABLE SuppliersDocumentsAcknowledgements (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			InitiatorId UNIQUEIDENTIFIER NOT NULL,
			FOREIGN KEY (Id) REFERENCES Documents(Id),
			FOREIGN KEY (InitiatorId) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'SuppliersDocumentsSendings')
	BEGIN
		CREATE TABLE SuppliersDocumentsSendings (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			InitiatorId UNIQUEIDENTIFIER NOT NULL,
			FOREIGN KEY (Id) REFERENCES Documents(Id),
			FOREIGN KEY (InitiatorId) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DynamicFieldTypes')
	BEGIN
		CREATE TABLE DynamicFieldTypes (
			Id INT PRIMARY KEY,
			Title NVARCHAR(255) NOT NULL
		);

		INSERT INTO DynamicFieldTypes (Id, Title) VALUES 
		(0, 'Texte'),
		(1, 'Date'),
		(2, 'Case à cocher'),
		(3, 'Liste déroulante'),
		(4, 'Bouton radio'),
		(5, 'Fichier (upload)');
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DynamicFields')
	BEGIN
		CREATE TABLE DynamicFields (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Label NVARCHAR(255) NOT NULL,
			IsForUsersProject BIT NOT NULL,
			IsForSuppliers BIT NOT NULL,
			IsRequired BIT NOT NULL,
			Type INT NOT NULL,
			IsGlobal BIT NOT NULL,
			ProjectId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
			FOREIGN KEY (Type) REFERENCES DynamicFieldTypes(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DynamicFieldItems')
	BEGIN
		CREATE TABLE DynamicFieldItems (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Value NVARCHAR(255),
			DynamicFieldId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (DynamicFieldId) REFERENCES DynamicFields(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentDynamicFields')
	BEGIN
		CREATE TABLE DocumentDynamicFields (
			Id INT IDENTITY(1, 1) PRIMARY KEY,
			Value NVARCHAR(255) NOT NULL,
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			DynamicFieldId UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
			FOREIGN KEY (DynamicFieldId) REFERENCES DynamicFields(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentsDynamicAttachements')
	BEGIN
		CREATE TABLE DocumentsDynamicAttachements (
			Id INT IDENTITY(1, 1) PRIMARY KEY,
			Filename NVARCHAR(255) NOT NULL,
			FilePath NVARCHAR(255) NOT NULL,
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			DynamicFieldId UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
			FOREIGN KEY (DynamicFieldId) REFERENCES DynamicFields(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'Attachements')
	BEGIN
		CREATE TABLE Attachements (
			Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
			FileName NVARCHAR(255) NOT NULL,
			Url NVARCHAR(255) NOT NULL,
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'UserDocumentRoles')
	BEGIN
		CREATE TABLE UserDocumentRoles (
			Id INT PRIMARY KEY,
			Title NVARCHAR(255) NOT NULL
		);

		INSERT INTO UserDocumentRoles (Id, Title) VALUES 
		(0, 'Lecteur'),
		(1, 'Validateur'),
		(2, 'Signataire');
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentsReceptions')
	BEGIN
		CREATE TABLE DocumentsReceptions (
			Id INT IDENTITY(1, 1) PRIMARY KEY,
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			UserId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
			FOREIGN KEY (UserId) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentSteps')
	BEGIN
		CREATE TABLE DocumentSteps (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			StepNumber INT NOT NULL DEFAULT 0,
			ProcessingDuration FLOAT NOT NULL DEFAULT 0,
			Role INT NOT NULL,
			Color NVARCHAR(255),
			Message NVARCHAR(500),
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			DeletionDate DATETIME,
			FOREIGN KEY (Role) REFERENCES UserDocumentRoles(Id),
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id)
		);
	END	
	
IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'UsersSteps')
	BEGIN
		CREATE TABLE UsersSteps (
			Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
			UserId UNIQUEIDENTIFIER NOT NULL,
			DocumentStepId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			ProcessingDate DATETIME,
			DeletionDate DATETIME,
			FOREIGN KEY (UserId) REFERENCES Users(Id),
			FOREIGN KEY (DocumentStepId) REFERENCES DocumentSteps(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DocumentsFields')
	BEGIN
		CREATE TABLE DocumentsFields (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Variable NVARCHAR(255) NOT NULL,
			X FLOAT NOT NULL,
			Y FLOAT NOT NULL,
			Width FLOAT NOT NULL,
			Height FLOAT NOT NULL,
			FirstPage INT NOT NULL,
			LastPage INT NOT NULL,
			FieldTypeId INT NOT NULL,
			DocumentStepId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			DeletionDate DATETIME,
			FOREIGN KEY (FieldTypeId) REFERENCES FieldTypes(Id),
			FOREIGN KEY (DocumentStepId) REFERENCES DocumentSteps(Id),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'VerificationTokens')
	BEGIN
		CREATE TABLE VerificationTokens (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			Content VARCHAR(142) NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'VerificationTokensHistory')
	BEGIN
		CREATE TABLE VerificationTokensHistory (
			Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
			Content VARCHAR(142) NOT NULL,
			SignatureId UNIQUEIDENTIFIER NOT NULL,
			VerificationTokenId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			FOREIGN KEY (SignatureId) REFERENCES UserSignatures(Id),
			FOREIGN KEY (VerificationTokenId) REFERENCES VerificationTokens(Id),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'DigitalSignatures')
	BEGIN
		CREATE TABLE DigitalSignatures (
			Id VARCHAR(41) PRIMARY KEY,
			EncryptedSignature VARCHAR(800) NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'ValidationsHistory')
	BEGIN
		CREATE TABLE ValidationsHistory (
			Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
			FromUserId UNIQUEIDENTIFIER NOT NULL,
			ToDocumentStepId UNIQUEIDENTIFIER,
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			Comment NVARCHAR(255),
			ActionType INT NOT NULL,
			CreationDate DATETIME NOT NULL,
			FOREIGN KEY (FromUserId) REFERENCES Users(Id),
			FOREIGN KEY (ToDocumentStepId) REFERENCES DocumentSteps(Id),
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'TomProConnections')
	BEGIN
		CREATE TABLE TomProConnections (
			Id UNIQUEIDENTIFIER PRIMARY KEY,
			ServerName VARCHAR(255) NOT NULL,
			Login VARCHAR(255),
			Password VARCHAR(255),
			ProjectId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'TomProDatabases')
	BEGIN
		CREATE TABLE TomProDatabases (
			Id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
			DatabaseName VARCHAR(255) NOT NULL,
			DatabaseId INT NOT NULL,
			TomProConnectionId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER NOT NULL,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (TomProConnectionId) REFERENCES TomProConnections(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'UsersDocumentsAccesses')
	BEGIN
		CREATE TABLE UsersDocumentsAccesses (
			Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
			UserId UNIQUEIDENTIFIER NOT NULL,
			DocumentId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			CreatedBy UNIQUEIDENTIFIER,
			DeletionDate DATETIME,
			DeletedBy UNIQUEIDENTIFIER,
			FOREIGN KEY (UserId) REFERENCES Users(Id),
			FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
			FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
			FOREIGN KEY (DeletedBy) REFERENCES Users(Id)
		);
	END

IF NOT EXISTS (SELECT name FROM sys.tables WHERE name = 'UsersConnections')
	BEGIN
		CREATE TABLE UsersConnections (
			Id BIGINT IDENTITY(1, 1) PRIMARY KEY,
			UserId UNIQUEIDENTIFIER NOT NULL,
			CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
			EndDate DATETIME,
			FOREIGN KEY (UserId) REFERENCES Users(Id)
		);
	END
