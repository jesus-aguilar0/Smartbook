# SmartBook - Sistema de Gestión de Inventario

Sistema de inventario para el Centro de Idiomas (CDI) de la Corporación Universitaria del Caribe – CECAR.

## Arquitectura

El proyecto sigue una arquitectura en N-Capas:

- **Capa de Presentación (Smartbook)**: Controllers, Middleware
- **Capa de Aplicación (Smartbook.Aplicacion)**: Services, Extensions, Mapping
- **Capa de Dominio (Smartbook.Dominio)**: Entities, Enums, Exceptions, DTOs
- **Capa de Persistencia (Smartbook.Persistencia)**: Repositories, DbContext, Data

## Tecnologías

- ASP.NET Core 8.0
- SQL Server (Microsoft.EntityFrameworkCore.SqlServer)
- Entity Framework Core 8
- JWT Authentication
- Serilog para logging
- MailKit para envío de correos
- iTextSharp para generación de PDFs
- Mapster para mapeo de objetos

## Configuración

### Base de Datos

1. Crear la base de datos SQL Server ejecutando el script `Database/smartbook.sql`
2. Actualizar la cadena de conexión en `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=smartbook;User Id=sa;Password=tu_password;TrustServerCertificate=True;"
}
```

### JWT

Configurar la clave JWT en `appsettings.json`:

```json
"Jwt": {
  "Key": "TuClaveSecretaDeAlMenos32Caracteres!",
  "Issuer": "SmartBook",
  "Audience": "SmartBook"
}
```

### Email

Configurar el servicio de correo en `appsettings.json`:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "tu-email@gmail.com",
  "SmtpPassword": "tu-app-password",
  "FromEmail": "noreply@cecar.edu.co",
  "FromName": "SmartBook - CDI CECAR"
}
```

## Usuario por Defecto

El sistema crea automáticamente un usuario administrador al iniciar:

- **Email**: admin@cecar.edu.co
- **Contraseña**: AdminCDI123!
- **Rol**: Admin

## Endpoints Principales

### Autenticación
- `POST /api/usuarios/login` - Iniciar sesión
- `POST /api/usuarios` - Crear usuario (Admin)
- `GET /api/usuarios/confirmar-email?token=xxx` - Confirmar email

### Clientes
- `POST /api/clientes` - Crear cliente
- `GET /api/clientes/{identificacion}` - Obtener cliente
- `GET /api/clientes?nombres=xxx` - Buscar clientes
- `PUT /api/clientes/{identificacion}` - Actualizar cliente

### Libros
- `POST /api/libros` - Crear libro (Admin)
- `GET /api/libros/{id}` - Obtener libro
- `GET /api/libros?nombre=xxx&nivel=xxx` - Buscar libros
- `PUT /api/libros/{id}` - Actualizar libro (Admin)

### Ingresos
- `POST /api/ingresos` - Registrar ingreso (Admin)
- `GET /api/ingresos/{id}` - Obtener ingreso
- `GET /api/ingresos?desde=xxx&hasta=xxx` - Buscar ingresos

### Ventas
- `POST /api/ventas` - Registrar venta
- `GET /api/ventas/{id}` - Obtener venta
- `GET /api/ventas?desde=xxx&hasta=xxx` - Buscar ventas

### Inventario
- `GET /api/inventarios/{lote}` - Consultar inventario por lote

## Características Implementadas

✅ Gestión de clientes y usuarios
✅ Autenticación JWT con roles (Admin/Vendedor)
✅ Confirmación de email
✅ Restablecimiento de contraseña
✅ Gestión de libros con validaciones
✅ Control de inventario por lote
✅ Registro de ingresos con generación automática de lotes
✅ Registro de ventas con validación de stock
✅ Generación de PDF para ventas
✅ Notificaciones por correo electrónico
✅ Logging estructurado con Serilog
✅ Manejo centralizado de excepciones
✅ Sanitización de entrada de datos
✅ Transacciones para operaciones críticas
✅ Auditoría (fechas de creación/actualización)

## Validaciones de Negocio

- Clientes deben ser mayores de 14 años
- Emails únicos y formato válido
- Celulares únicos de 10 dígitos
- Solo correos institucionales para usuarios
- Contraseñas mínimo 8 caracteres
- No se permiten libros duplicados (mismo nombre, nivel, tipo, edición)
- Validación de stock antes de ventas
- Generación automática de códigos de lote (AÑO-NÚMERO)

## Despliegue

El sistema está listo para desplegarse en http://runasp.net. Asegúrese de:

1. Configurar la cadena de conexión a SQL Server
2. Configurar el servicio de correo
3. Configurar la clave JWT
4. Habilitar Swagger para pruebas

## Notas

- Todos los strings de entrada son sanitizados automáticamente
- Las contraseñas se almacenan con hash BCrypt
- Los tokens JWT expiran en 1 hora
- Los tokens de confirmación/reset expiran en 1 hora
- El sistema genera automáticamente el usuario admin si no existe

