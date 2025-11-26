-- Script de creación de base de datos SmartBook
-- SQL Server

USE master;
GO

-- Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'smartbook')
BEGIN
    CREATE DATABASE smartbook;
END
GO

USE smartbook;
GO

-- Tabla Clientes
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clientes')
BEGIN
    CREATE TABLE Clientes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Identificacion VARCHAR(20) NOT NULL UNIQUE,
        Nombres VARCHAR(200) NOT NULL,
        Email VARCHAR(100) NOT NULL UNIQUE,
        Celular VARCHAR(10) NOT NULL UNIQUE,
        FechaNacimiento DATETIME NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL
    );
    
    CREATE INDEX IX_Clientes_Identificacion ON Clientes(Identificacion);
    CREATE INDEX IX_Clientes_Email ON Clientes(Email);
    CREATE INDEX IX_Clientes_Celular ON Clientes(Celular);
END
GO

-- Tabla Usuarios
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
BEGIN
    CREATE TABLE Usuarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Identificacion VARCHAR(20) NOT NULL UNIQUE,
        ContrasenaHash VARCHAR(500) NOT NULL,
        Nombres VARCHAR(200) NOT NULL,
        Email VARCHAR(100) NOT NULL UNIQUE,
        Rol INT NOT NULL,
        EmailConfirmado BIT NOT NULL DEFAULT 0,
        Activo BIT NOT NULL DEFAULT 1,
        TokenConfirmacion VARCHAR(500) NULL,
        TokenConfirmacionExpiracion DATETIME NULL,
        TokenResetPassword VARCHAR(500) NULL,
        TokenResetPasswordExpiracion DATETIME NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL
    );
    
    CREATE INDEX IX_Usuarios_Identificacion ON Usuarios(Identificacion);
    CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
END
GO

-- Tabla Libros
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Libros')
BEGIN
    CREATE TABLE Libros (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre VARCHAR(200) NOT NULL,
        Nivel VARCHAR(50) NOT NULL,
        Stock INT NOT NULL DEFAULT 0,
        Tipo INT NOT NULL,
        Editorial VARCHAR(100) NOT NULL,
        Edicion VARCHAR(20) NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        CONSTRAINT UK_Libros_Unique UNIQUE (Nombre, Nivel, Tipo, Edicion)
    );
END
GO

-- Tabla Ingresos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ingresos')
BEGIN
    CREATE TABLE Ingresos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Fecha DATETIME NOT NULL,
        LibroId INT NOT NULL,
        Lote VARCHAR(20) NOT NULL,
        Unidades INT NOT NULL,
        ValorCompra DECIMAL(18,2) NOT NULL,
        ValorVentaPublico DECIMAL(18,2) NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (LibroId) REFERENCES Libros(Id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_Ingresos_Lote_LibroId ON Ingresos(Lote, LibroId);
END
GO

-- Tabla Ventas
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Ventas')
BEGIN
    CREATE TABLE Ventas (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NumeroReciboPago VARCHAR(50) NOT NULL,
        Fecha DATETIME NOT NULL,
        ClienteId INT NOT NULL,
        UsuarioId INT NOT NULL,
        Observaciones VARCHAR(500) NULL,
        Total DECIMAL(18,2) NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (ClienteId) REFERENCES Clientes(Id) ON DELETE NO ACTION,
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE NO ACTION
    );
    
    CREATE INDEX IX_Ventas_NumeroReciboPago ON Ventas(NumeroReciboPago);
END
GO

-- Tabla DetallesVenta
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DetallesVenta')
BEGIN
    CREATE TABLE DetallesVenta (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VentaId INT NOT NULL,
        LibroId INT NOT NULL,
        Lote VARCHAR(20) NOT NULL,
        Unidades INT NOT NULL,
        ValorUnitario DECIMAL(18,2) NOT NULL,
        Subtotal DECIMAL(18,2) NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (VentaId) REFERENCES Ventas(Id) ON DELETE CASCADE,
        FOREIGN KEY (LibroId) REFERENCES Libros(Id) ON DELETE NO ACTION
    );
END
GO

-- Tabla Inventarios
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventarios')
BEGIN
    CREATE TABLE Inventarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        LibroId INT NOT NULL,
        Lote VARCHAR(20) NOT NULL,
        UnidadesDisponibles INT NOT NULL DEFAULT 0,
        UnidadesVendidas INT NOT NULL DEFAULT 0,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        FOREIGN KEY (LibroId) REFERENCES Libros(Id) ON DELETE CASCADE,
        CONSTRAINT UK_Inventarios_LibroId_Lote UNIQUE (LibroId, Lote)
    );
END
GO

-- Insertar usuario administrador por defecto
-- NOTA: La contraseña por defecto es "AdminCDI123!"
-- El hash BCrypt debe generarse en la aplicación al ejecutarse por primera vez
-- Este usuario se crea automáticamente en Program.cs si no existe
