-- Script para crear la tabla Clientes en MySQL
-- Alineado con la entidad Cliente y SmartbookDbContext

USE smartbook;

-- Crear tabla Clientes
CREATE TABLE IF NOT EXISTS Clientes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Identificacion VARCHAR(20) NOT NULL,
    Nombres VARCHAR(200) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Celular VARCHAR(10) NOT NULL,
    FechaNacimiento DATETIME NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion DATETIME NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP,
    
    -- Índices únicos
    UNIQUE KEY UK_Clientes_Identificacion (Identificacion),
    UNIQUE KEY UK_Clientes_Email (Email),
    UNIQUE KEY UK_Clientes_Celular (Celular),
    
    -- Índice para búsquedas por nombres
    KEY IX_Clientes_Nombres (Nombres)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Verificar si la tabla Ventas existe y agregar la clave foránea si no existe
-- (Solo si la tabla Ventas ya existe pero no tiene la FK)
SET @fk_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
    WHERE TABLE_SCHEMA = 'smartbook' 
    AND TABLE_NAME = 'Ventas' 
    AND CONSTRAINT_NAME = 'FK_Ventas_Clientes_ClienteId'
);

SET @sql = IF(@fk_exists = 0,
    'ALTER TABLE Ventas 
     ADD CONSTRAINT FK_Ventas_Clientes_ClienteId 
     FOREIGN KEY (ClienteId) 
     REFERENCES Clientes(Id) 
     ON DELETE RESTRICT 
     ON UPDATE CASCADE',
    'SELECT "La clave foránea FK_Ventas_Clientes_ClienteId ya existe" AS Mensaje'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verificar la estructura de la tabla
DESCRIBE Clientes;

-- Mostrar mensaje de confirmación
SELECT 'Tabla Clientes creada exitosamente' AS Mensaje;

