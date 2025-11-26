-- Script simplificado para crear la tabla Clientes en MySQL
-- Ejecuta este script si la tabla Ventas aún no existe o si prefieres una versión más simple

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

-- Si la tabla Ventas ya existe, agregar la clave foránea
-- (Ejecuta esto solo si la tabla Ventas ya existe)
-- ALTER TABLE Ventas 
-- ADD CONSTRAINT FK_Ventas_Clientes_ClienteId 
-- FOREIGN KEY (ClienteId) 
-- REFERENCES Clientes(Id) 
-- ON DELETE RESTRICT 
-- ON UPDATE CASCADE;

SELECT 'Tabla Clientes creada exitosamente' AS Mensaje;

