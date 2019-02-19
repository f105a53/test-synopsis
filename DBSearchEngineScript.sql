USE master

IF DB_ID('DB_SearchEngine') IS NOT NULL
	DROP DATABASE DB_SearchEngine
CREATE DATABASE DB_SearchEngine
GO

USE DB_SearchEngine
GO

IF Object_ID('tblDocument') IS NOT NULL
	DROP TABLE tblDocument
CREATE TABLE tblDocument (fldUrl VARCHAR(256) PRIMARY KEY,
						  fldModifiedDate DATETIME NOT NULL)

IF Object_ID('tblTerm') IS NOT NULL
	DROP TABLE tblTerm
CREATE TABLE tblTerm (fldVal NVARCHAR(256) PRIMARY KEY)

IF Object_ID('tblTermDoc') IS NOT NULL
	DROP TABLE tblTermDoc
CREATE TABLE tblTermDoc (fldId UNIQUEIDENTIFIER PRIMARY KEY,
						 fldDocUrl VARCHAR(256) FOREIGN KEY REFERENCES tblDocument(fldUrl) NOT NULL,
						 fldTermId NVARCHAR(256) FOREIGN KEY REFERENCES tblTerm(fldVal) NOT NULL,
						 fldLinePos INTEGER NOT NULL,
						 fldLineNo INTEGER NOT NULL)