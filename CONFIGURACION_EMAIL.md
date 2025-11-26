# 📧 Configuración de Correo Electrónico - SmartBook

Esta guía te ayudará a configurar el servicio de correo electrónico para que funcione correctamente con SmartBook.

## 📋 Ubicación de Archivos

La configuración se realiza en dos archivos:
- `Smartbook/appsettings.json` - Configuración para producción
- `Smartbook/appsettings.Development.json` - Configuración para desarrollo

## 🔧 Configuración Básica

Agrega o actualiza la sección `"Email"` en ambos archivos:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-app-password",
    "FromEmail": "noreply@cecar.edu.co",
    "FromName": "SmartBook - CDI CECAR",
    "LogoUrl": "https://www.cecar.edu.co/wp-content/uploads/logo-cecar.png"
  }
}
```

## 📮 Configuración por Proveedor

### Gmail (Recomendado para desarrollo)

**Pasos para obtener App Password:**

1. Ve a tu cuenta de Google: https://myaccount.google.com/
2. Activa la verificación en 2 pasos si no la tienes activada
3. Ve a "Seguridad" → "Contraseñas de aplicaciones"
4. Genera una nueva contraseña de aplicación
5. Copia la contraseña generada (16 caracteres sin espacios)

**Configuración:**
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "abcd efgh ijkl mnop",  // App Password de 16 caracteres
    "FromEmail": "tu-email@gmail.com",
    "FromName": "SmartBook - CDI CECAR",
    "LogoUrl": "https://www.cecar.edu.co/wp-content/uploads/logo-cecar.png"
  }
}
```

### Outlook/Hotmail

```json
{
  "Email": {
    "SmtpHost": "smtp-mail.outlook.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@outlook.com",
    "SmtpPassword": "tu-contraseña",
    "FromEmail": "tu-email@outlook.com",
    "FromName": "SmartBook - CDI CECAR",
    "LogoUrl": "https://www.cecar.edu.co/wp-content/uploads/logo-cecar.png"
  }
}
```

### Office 365

```json
{
  "Email": {
    "SmtpHost": "smtp.office365.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@cecar.edu.co",
    "SmtpPassword": "tu-contraseña",
    "FromEmail": "tu-email@cecar.edu.co",
    "FromName": "SmartBook - CDI CECAR",
    "LogoUrl": "https://www.cecar.edu.co/wp-content/uploads/logo-cecar.png"
  }
}
```

### Servidor SMTP Personalizado

Si tienes un servidor SMTP propio:

```json
{
  "Email": {
    "SmtpHost": "smtp.tu-servidor.com",
    "SmtpPort": "587",  // o 465 para SSL
    "SmtpUser": "usuario@tu-servidor.com",
    "SmtpPassword": "tu-contraseña",
    "FromEmail": "noreply@cecar.edu.co",
    "FromName": "SmartBook - CDI CECAR",
    "LogoUrl": "https://www.cecar.edu.co/wp-content/uploads/logo-cecar.png"
  }
}
```

## 🔐 Seguridad

**⚠️ IMPORTANTE:** Nunca subas el archivo `appsettings.json` con contraseñas reales a un repositorio público.

### Opción 1: Usar Variables de Entorno (Recomendado)

En lugar de poner la contraseña directamente en el archivo, usa variables de entorno:

```json
{
  "Email": {
    "SmtpPassword": "%EMAIL_PASSWORD%"
  }
}
```

Luego configura la variable de entorno:
- Windows: `set EMAIL_PASSWORD=tu-password`
- Linux/Mac: `export EMAIL_PASSWORD=tu-password`

### Opción 2: Usar User Secrets (Solo desarrollo)

```bash
dotnet user-secrets set "Email:SmtpPassword" "tu-password"
```

## ✅ Verificación

Después de configurar, prueba el envío de correos:

1. Ejecuta la aplicación
2. Crea un nuevo usuario (requiere token Admin)
3. Verifica que llegue el correo de confirmación
4. Confirma el correo haciendo clic en el enlace
5. Verifica que llegue el correo de bienvenida

## 🐛 Solución de Problemas

### Error: "Authentication failed"

- Verifica que la contraseña sea correcta
- Si usas Gmail, asegúrate de usar una App Password, no tu contraseña normal
- Verifica que la verificación en 2 pasos esté activada (Gmail)

### Error: "Connection timeout"

- Verifica que el puerto sea correcto (587 para TLS, 465 para SSL)
- Verifica que el firewall no bloquee la conexión
- Prueba con otro proveedor de correo

### Los correos no llegan

- Revisa la carpeta de spam
- Verifica que `FromEmail` sea válido
- Verifica los logs de la aplicación en `logs/smartbook-*.txt`

## 📝 Notas

- El puerto 587 usa TLS (StartTls)
- El puerto 465 usa SSL directo
- Gmail requiere App Passwords si tienes 2FA activado
- Office 365 puede requerir autenticación moderna

