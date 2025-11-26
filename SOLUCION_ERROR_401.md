# 🔧 Solución: Error 401 "invalid_token"

## ❌ Problema

Al intentar crear un usuario, recibes:
```
401 Unauthorized
Bearer error="invalid_token"
```

## 🔍 Causas Comunes

### 1. **Token Expirado** (Más Común)
Los tokens JWT expiran después de **1 hora**. Si pasó más tiempo desde que hiciste login, el token ya no es válido.

**Solución:**
1. Vuelve a hacer login: `POST /api/usuarios/login`
2. Copia el nuevo token
3. Actualiza el token en Swagger (botón "Authorize")

### 2. **Token Truncado o Mal Copiado**
Si el token está incompleto o tiene caracteres faltantes, no funcionará.

**Solución:**
- Asegúrate de copiar el token completo
- No debe tener espacios al inicio o final
- Debe tener 3 partes separadas por puntos (ejemplo: `xxx.yyy.zzz`)

### 3. **Token de Otra Sesión**
Si reiniciaste la aplicación o cambiaste la clave JWT, los tokens antiguos no funcionarán.

**Solución:**
- Haz login nuevamente para obtener un token válido

### 4. **Usuario No Tiene Rol Admin**
El endpoint requiere rol `Admin`. Si el usuario no tiene ese rol, recibirás 401.

**Solución:**
- Verifica que el usuario tenga rol `Admin` en la base de datos
- Usa el usuario por defecto: `admin@cecar.edu.co`

## ✅ Pasos para Solucionar

### Paso 1: Verificar que el Token Sea Válido

1. **Haz login nuevamente:**
   ```
   POST /api/usuarios/login
   {
     "email": "admin@cecar.edu.co",
     "contrasena": "AdminCDI123!"
   }
   ```

2. **Copia el token completo** de la respuesta:
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "expiracion": "2025-11-20T03:52:14Z",
     "usuario": { ... }
   }
   ```

3. **Verifica la expiración:**
   - El token debe ser reciente (menos de 1 hora)
   - Si `expiracion` ya pasó, haz login de nuevo

### Paso 2: Actualizar el Token en Swagger

1. **Haz clic en el botón "Authorize" 🔓** (arriba a la derecha en Swagger)

2. **Pega el token** (sin escribir "Bearer", solo el token)

3. **Haz clic en "Authorize"** y luego "Close"

4. **Intenta crear el usuario nuevamente**

### Paso 3: Verificar el Rol del Usuario

Si aún no funciona, verifica que el usuario tenga rol Admin:

1. **Consulta el usuario en la BD:**
   ```sql
   SELECT Id, Email, Rol, Activo, EmailConfirmado 
   FROM Usuarios 
   WHERE Email = 'admin@cecar.edu.co'
   ```

2. **Verifica:**
   - `Rol` debe ser `0` (Admin)
   - `Activo` debe ser `1` (true)
   - `EmailConfirmado` debe ser `1` (true)

## 🧪 Prueba Rápida

### Opción 1: Desde Swagger

1. **Login:**
   - Endpoint: `POST /api/usuarios/login`
   - Body: `{ "email": "admin@cecar.edu.co", "contrasena": "AdminCDI123!" }`
   - Copia el `token` de la respuesta

2. **Authorize:**
   - Haz clic en "Authorize" 🔓
   - Pega el token
   - Clic en "Authorize" y "Close"

3. **Crear Usuario:**
   - Endpoint: `POST /api/usuarios`
   - Body: `{ "identificacion": "...", "email": "...", ... }`
   - Debe funcionar ahora

### Opción 2: Desde el Archivo HTTP

1. **Actualiza el token en `Smartbook.http`:**
   ```http
   @Token = TU_TOKEN_AQUI
   ```

2. **Ejecuta el endpoint de crear usuario**

## 📋 Checklist

Antes de intentar crear un usuario, verifica:

- [ ] ¿Hice login hace menos de 1 hora?
- [ ] ¿Copié el token completo (sin truncar)?
- [ ] ¿Actualicé el token en Swagger (botón Authorize)?
- [ ] ¿El usuario tiene rol Admin?
- [ ] ¿El usuario está activo?
- [ ] ¿El email está confirmado?

## 🔍 Verificación en Logs

Revisa los logs para ver qué está pasando:

**Ubicación:** `logs/smartbook-*.txt`

**Busca mensajes como:**
- `"Error de autenticación JWT: ..."`
- `"Token JWT validado - UserId: ..., Rol: ..."`
- `"Challenge de autenticación: ..."`

Estos mensajes te dirán exactamente qué está fallando.

## 💡 Consejos

1. **Siempre haz login antes de usar endpoints protegidos**
2. **El token expira en 1 hora** - si pasó tiempo, vuelve a hacer login
3. **Usa el botón "Authorize" en Swagger** para actualizar el token fácilmente
4. **Verifica los logs** si el problema persiste

