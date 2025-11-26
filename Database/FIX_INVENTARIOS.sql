-- Script URGENTE para corregir la tabla Inventarios
-- Ejecuta este script en SQL Server Management Studio

USE smartbook;
GO

-- Verificar si la columna existe
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Inventarios]') 
    AND name = 'FechaActualizacion'
)
BEGIN
    PRINT 'Agregando columna FechaActualizacion...';
    ALTER TABLE Inventarios
    ADD FechaActualizacion DATETIME NULL;
    PRINT 'Columna FechaActualizacion agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna FechaActualizacion ya existe.';
END
GO

-- Verificar la estructura de la tabla
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Inventarios'
ORDER BY ORDINAL_POSITION;
GO

