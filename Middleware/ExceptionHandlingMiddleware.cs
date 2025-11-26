using System.Net;
using System.Text.Json;
using MySqlConnector;
using Smartbook.LogicaDeNegocio.Exceptions;

namespace Smartbook.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        // Mensaje genérico para el usuario final (nunca expone detalles técnicos)
        var errorResponse = new
        {
            message = "Ha ocurrido un error al procesar su solicitud. Por favor, intente nuevamente más tarde.",
            statusCode = (int)HttpStatusCode.InternalServerError
        };

        switch (exception)
        {
            case BusinessException businessEx:
                // Excepción de negocio: mensaje claro y comprensible para el usuario
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    message = businessEx.Message, // Mensaje de negocio ya es claro y comprensible
                    statusCode = (int)HttpStatusCode.BadRequest
                };
                _logger.LogWarning(businessEx, "Business exception: {Message}", businessEx.Message);
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new
                {
                    message = "El recurso solicitado no fue encontrado.",
                    statusCode = (int)HttpStatusCode.NotFound
                };
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;

            case MySqlException mySqlEx:
                response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                // Mensajes claros y comprensibles sin detalles técnicos
                var sqlMessage = mySqlEx.ErrorCode switch
                {
                    MySqlErrorCode.AccessDenied => "Error de autenticación con la base de datos. Por favor, contacte al administrador del sistema.",
                    MySqlErrorCode.UnknownDatabase => "Error al acceder a la base de datos. Por favor, contacte al administrador del sistema.",
                    MySqlErrorCode.UnableToConnectToHost => "No se puede conectar al servidor de base de datos. Por favor, intente nuevamente más tarde.",
                    _ => "Error de conexión a la base de datos. Por favor, intente nuevamente más tarde."
                };
                errorResponse = new
                {
                    message = sqlMessage,
                    statusCode = (int)HttpStatusCode.ServiceUnavailable
                };
                // Log técnico solo en servidor, nunca expuesto al usuario
                _logger.LogError(mySqlEx, "MySQL Exception: {ErrorCode} - {Message} - StackTrace: {StackTrace}", 
                    mySqlEx.ErrorCode, mySqlEx.Message, mySqlEx.StackTrace);
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    message = "Los datos proporcionados no son válidos. Por favor, verifique la información enviada.",
                    statusCode = (int)HttpStatusCode.BadRequest
                };
                _logger.LogWarning(argEx, "Argument exception: {Message}", argEx.Message);
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Forbidden;
                errorResponse = new
                {
                    message = "No tiene permisos para realizar esta operación.",
                    statusCode = (int)HttpStatusCode.Forbidden
                };
                _logger.LogWarning(exception, "Unauthorized access attempt");
                break;

            default:
                // Para cualquier excepción no manejada, mensaje genérico sin detalles técnicos
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new
                {
                    message = "Ha ocurrido un error inesperado. Por favor, intente nuevamente más tarde o contacte al administrador del sistema.",
                    statusCode = (int)HttpStatusCode.InternalServerError
                };
                // Log completo solo en servidor (nunca expuesto al usuario)
                _logger.LogError(exception, 
                    "Unhandled exception: {Message} | Type: {Type} | StackTrace: {StackTrace}", 
                    exception.Message, 
                    exception.GetType().Name, 
                    exception.StackTrace);
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await response.WriteAsync(jsonResponse);
    }
}

