create table [dbo].[User]
(
[Id] int not null primary key Identity(1,1),
[FirstName] Varchar(640) null,
[LastName] Varchar(640) null,
[Email] Varchar(640) not null,
[Password] Varchar(640) not null,
[PhoneNumber] Varchar(640) null
);