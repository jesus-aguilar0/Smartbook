using System.Text.RegularExpressions;

namespace Smartbook.LogicaDeNegocio.Validators;

public static class ValidationHelper
{
    /// <summary>
    /// Valida un número de teléfono colombiano (celular)
    /// Debe tener exactamente 10 dígitos y comenzar con 3
    /// </summary>
    public static bool IsValidColombianPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remover espacios, guiones y paréntesis
        var cleanPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");

        // Debe tener exactamente 10 dígitos
        if (cleanPhone.Length != 10)
            return false;

        // Debe contener solo números
        if (!cleanPhone.All(char.IsDigit))
            return false;

        // Los celulares colombianos comienzan con 3
        if (cleanPhone[0] != '3')
            return false;

        return true;
    }

    /// <summary>
    /// Valida una cédula colombiana
    /// Las cédulas colombianas son números de 6 a 10 dígitos sin algoritmo de verificación
    /// </summary>
    public static bool IsValidColombianId(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            return false;

        // Remover espacios, guiones y puntos
        var cleanId = Regex.Replace(identificacion, @"[\s\-\.]", "");

        // Debe tener entre 6 y 10 dígitos (rango estándar de cédulas colombianas)
        if (cleanId.Length < 6 || cleanId.Length > 10)
            return false;

        // Debe contener solo números
        if (!cleanId.All(char.IsDigit))
            return false;

        // Las cédulas colombianas no tienen dígito verificador, solo validamos formato
        // No debe ser todos ceros
        if (cleanId.All(c => c == '0'))
            return false;

        return true;
    }

    /// <summary>
    /// Valida un correo electrónico
    /// </summary>
    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email && email.Length <= 100;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida un correo electrónico institucional
    /// </summary>
    public static bool IsValidInstitutionalEmail(string email)
    {
        if (!IsValidEmail(email))
            return false;

        // Debe ser un correo institucional de CECAR
        return email.Contains("@cecar.edu.co", StringComparison.OrdinalIgnoreCase) ||
               email.Contains("@cecar", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Valida que un nombre solo contenga letras, espacios y caracteres especiales permitidos
    /// </summary>
    public static bool IsValidName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Permitir letras, espacios, guiones, apóstrofes y puntos
        var pattern = @"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s\-'\.]+$";
        return Regex.IsMatch(name, pattern) && name.Trim().Length >= 2 && name.Trim().Length <= 200;
    }

    /// <summary>
    /// Valida una contraseña
    /// Debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial
    /// </summary>
    public static bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (password.Length < 8)
            return false;

        // Debe tener al menos una mayúscula
        if (!password.Any(char.IsUpper))
            return false;

        // Debe tener al menos una minúscula
        if (!password.Any(char.IsLower))
            return false;

        // Debe tener al menos un número
        if (!password.Any(char.IsDigit))
            return false;

        // Debe tener al menos un carácter especial
        var specialChars = @"!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?";
        if (!password.Any(c => specialChars.Contains(c)))
            return false;

        return true;
    }

    /// <summary>
    /// Valida una fecha de nacimiento
    /// Debe ser una fecha válida y no puede ser futura
    /// </summary>
    public static bool IsValidBirthDate(DateTime fechaNacimiento)
    {
        if (fechaNacimiento > DateTime.Now)
            return false;

        // No puede ser anterior a 1900
        if (fechaNacimiento.Year < 1900)
            return false;

        return true;
    }

    /// <summary>
    /// Calcula la edad basada en la fecha de nacimiento
    /// </summary>
    public static int CalculateAge(DateTime fechaNacimiento)
    {
        var today = DateTime.Today;
        var age = today.Year - fechaNacimiento.Year;
        if (fechaNacimiento.Date > today.AddYears(-age))
            age--;
        return age;
    }

    /// <summary>
    /// Limpia un número de teléfono removiendo caracteres especiales
    /// </summary>
    public static string CleanPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return Regex.Replace(phone, @"[\s\-\(\)]", "");
    }

    /// <summary>
    /// Limpia una identificación removiendo caracteres especiales
    /// </summary>
    public static string CleanIdentification(string identificacion)
    {
        if (string.IsNullOrWhiteSpace(identificacion))
            return string.Empty;

        return Regex.Replace(identificacion, @"[\s\-\.]", "");
    }
}

