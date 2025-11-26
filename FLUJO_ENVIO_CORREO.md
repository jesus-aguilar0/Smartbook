# 📧 Flujo de Envío de Token por Correo Electrónico

## 🔄 Proceso Completo

### 1. Creación de Usuario (POST /api/usuarios)

Cuando un administrador crea un nuevo usuario, sucede lo siguiente:

```csharp
// 1. Se generan los datos del usuario
var usuario = new Usuario {
    Email = "usuario@cecar.edu.co",
    EmailConfirmado = false  // ← Aún no confirmado
};

// 2. Se genera un token aleatorio de confirmación
usuario.TokenConfirmacion = GenerateToken();  // Ejemplo: "ABC123XYZ789..."
usuario.TokenConfirmacionExpiracion = DateTime.UtcNow.AddHours(1);  // Válido por 1 hora

// 3. Se guarda en la base de datos
await _unitOfWork.Usuarios.AddAsync(usuario);
await _unitOfWork.SaveChangesAsync();

// 4. Se envía el correo con el token
await _emailService.SendEmailConfirmationAsync(
    usuario.Email,                    // Para: usuario@cecar.edu.co
    usuario.TokenConfirmacion,        // Token: "ABC123XYZ789..."
    baseUrl                          // URL base: "http://localhost:5235"
);
```

### 2. Generación del Token

El token se genera usando criptografía segura:

```csharp
private static string GenerateToken()
{
    var randomBytes = new byte[32];  // 32 bytes = 256 bits
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
    // Resultado: "ABC123XYZ789..." (44 caracteres en Base64)
}
```

**Características del token:**
- ✅ Aleatorio y seguro (256 bits)
- ✅ Único para cada usuario
- ✅ Válido por 1 hora
- ✅ Se guarda encriptado en la base de datos

### 3. Construcción del Enlace

El servicio de correo construye el enlace completo:

```csharp
public async Task SendEmailConfirmationAsync(string to, string token, string baseUrl)
{
    // Construye la URL completa con el token
    var confirmUrl = $"{baseUrl}/api/usuarios/confirmar-email?token={Uri.EscapeDataString(token)}";
    
    // Ejemplo resultante:
    // http://localhost:5235/api/usuarios/confirmar-email?token=ABC123XYZ789...
}
```

### 4. Envío del Correo

El correo se envía usando SMTP:

```csharp
public async Task SendEmailAsync(string to, string subject, string body)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress("SmartBook - CDI CECAR", "noreply@cecar.edu.co"));
    message.To.Add(new MailboxAddress("", to));  // usuario@cecar.edu.co
    message.Subject = "Confirmación de Correo Electrónico - SmartBook";
    message.Body = body;  // HTML con el enlace

    using var client = new SmtpClient();
    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
    await client.AuthenticateAsync("smtp-user", "smtp-password");
    await client.SendAsync(message);
    await client.DisconnectAsync(true);
}
```

### 5. Contenido del Correo

El correo incluye:

```html
<!DOCTYPE html>
<html>
<body>
    <h1>Confirmación de Correo Electrónico</h1>
    <p>Para confirmar su correo, haga clic en el siguiente enlace:</p>
    
    <!-- Botón con el enlace -->
    <a href="http://localhost:5235/api/usuarios/confirmar-email?token=ABC123XYZ789...">
        Confirmar Correo Electrónico
    </a>
    
    <!-- URL completa para copiar/pegar -->
    <p>O copie esta URL: 
       http://localhost:5235/api/usuarios/confirmar-email?token=ABC123XYZ789...
    </p>
    
    <p><strong>Este enlace expirará en 1 hora.</strong></p>
</body>
</html>
```

## 📋 Flujo Visual

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Admin crea usuario (POST /api/usuarios)                  │
│    - Email: usuario@cecar.edu.co                             │
│    - Token generado: "ABC123XYZ789..."                       │
│    - Expiración: +1 hora                                    │
└─────────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. Token guardado en BD                                      │
│    Tabla: Usuarios                                           │
│    - TokenConfirmacion: "ABC123XYZ789..."                    │
│    - TokenConfirmacionExpiracion: 2025-11-20 10:00:00       │
└─────────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Correo enviado vía SMTP                                   │
│    De: noreply@cecar.edu.co                                  │
│    Para: usuario@cecar.edu.co                                │
│    Asunto: Confirmación de Correo Electrónico                │
│    Contenido: Enlace con token                               │
└─────────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. Usuario recibe correo                                     │
│    - Ve el botón "Confirmar Correo Electrónico"              │
│    - O copia la URL completa                                 │
└─────────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Usuario hace clic en el enlace                            │
│    GET /api/usuarios/confirmar-email?token=ABC123XYZ789...   │
└─────────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. Sistema valida el token                                   │
│    - Busca en BD: TokenConfirmacion = "ABC123XYZ789..."      │
│    - Verifica expiración: ¿Es < 1 hora?                      │
│    - Si es válido: confirma el email                         │
│    - Envía correo de bienvenida                             │
└─────────────────────────────────────────────────────────────┘
```

## 🔧 Configuración Requerida

Para que funcione, debes tener configurado en `appsettings.json`:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-app-password",
    "FromEmail": "noreply@cecar.edu.co",
    "FromName": "SmartBook - CDI CECAR"
  }
}
```

## ✅ Verificación

Para verificar que funciona:

1. **Crea un usuario:**
   ```bash
   POST /api/usuarios
   Authorization: Bearer [TOKEN_ADMIN]
   {
     "identificacion": "1234567890",
     "contrasena": "Password123!",
     "nombres": "Usuario Test",
     "email": "test@cecar.edu.co",
     "rol": 2
   }
   ```

2. **Revisa el correo del usuario creado**

3. **Deberías ver:**
   - ✅ Correo recibido
   - ✅ Enlace con el token
   - ✅ Botón para confirmar

4. **Haz clic en el enlace o copia la URL**

5. **El sistema confirmará el email automáticamente**

## 🐛 Solución de Problemas

### El correo no llega

1. **Verifica la configuración SMTP:**
   - ¿Las credenciales son correctas?
   - ¿El servidor SMTP está accesible?

2. **Revisa los logs:**
   - Archivo: `logs/smartbook-*.txt`
   - Busca errores de SMTP

3. **Verifica la carpeta de spam**

4. **Prueba con otro proveedor de correo**

### El token no funciona

1. **Verifica que el token sea el del correo** (no el JWT)
2. **Verifica que no haya expirado** (1 hora)
3. **Verifica que el usuario exista en la BD**

## 📝 Notas Importantes

- ⏰ El token expira en **1 hora**
- 🔒 El token es **único** para cada usuario
- 📧 El correo se envía **automáticamente** al crear el usuario
- ✅ Después de confirmar, se envía otro correo de bienvenida

