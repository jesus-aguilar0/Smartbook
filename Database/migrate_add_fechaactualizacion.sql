-- Script de migración para agregar FechaActualizacion a la tabla Inventarios
-- Ejecutar este script si la tabla Inventarios no tiene la columna FechaActualizacion

USE smartbook;
GO

-- Agregar columna FechaActualizacion a Inventarios si no existe
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Inventarios]') 
    AND name = 'FechaActualizacion'
)
BEGIN
    ALTER TABLE Inventarios
    ADD FechaActualizacion DATETIME NULL;
    
    PRINT 'Columna FechaActualizacion agregada a la tabla Inventarios.';
END
ELSE
BEGIN
    PRINT 'La columna FechaActualizacion ya existe en la tabla Inventarios.';
END
GO

