using System.Linq;
using System.Net.Http;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Smartbook.LogicaDeNegocio.Extensions;
using Smartbook.LogicaDeNegocio.Dtos;
using Smartbook.Entidades;
using Smartbook.LogicaDeNegocio.Exceptions;
using Smartbook.Persistencia.Data;
using Smartbook.Persistencia.Repositories;

namespace Smartbook.LogicaDeNegocio.Services;

public class VentaService : IVentaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SmartbookDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<VentaService> _logger;

    public VentaService(IUnitOfWork unitOfWork, SmartbookDbContext context, IEmailService emailService, ILogger<VentaService> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<VentaDto> CreateAsync(VentaCreateDto dto, int usuarioId, string baseUrl)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Validar clienteId
            if (dto.ClienteId <= 0)
            {
                throw new BusinessException("El ClienteId debe ser mayor que 0. Por favor, proporcione un ID de cliente válido.");
            }

            // Validar cliente
            var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId);
            if (cliente == null)
            {
                // Obtener lista de clientes disponibles para sugerencia
                var clientesDisponibles = await _unitOfWork.Clientes.GetAllAsync();
                var idsDisponibles = clientesDisponibles.Select(c => c.Id).ToList();
                
                var mensaje = $"Cliente con ID {dto.ClienteId} no encontrado. ";
                if (idsDisponibles.Any())
                {
                    mensaje += $"IDs de clientes disponibles: {string.Join(", ", idsDisponibles)}. ";
                    mensaje += "Use GET /api/clientes para ver todos los clientes o GET /api/clientes/id/{id} para obtener un cliente por ID.";
                }
                else
                {
                    mensaje += "No hay clientes registrados en el sistema. Por favor, cree un cliente primero usando POST /api/clientes.";
                }
                
                throw new BusinessException(mensaje);
            }

            // Validar usuario
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new BusinessException("Usuario no encontrado.");
            }

            if (dto.Detalles == null || !dto.Detalles.Any())
            {
                throw new BusinessException("La venta debe tener al menos un detalle.");
            }

            // Validar que los detalles tengan datos válidos
            foreach (var detalle in dto.Detalles)
            {
                if (detalle.LibroId <= 0)
                {
                    throw new BusinessException($"El LibroId debe ser mayor que 0. LibroId proporcionado: {detalle.LibroId}");
                }
                if (detalle.Unidades <= 0)
                {
                    throw new BusinessException("Las unidades deben ser mayores que 0.");
                }
            }

            var venta = new Venta
            {
                NumeroReciboPago = dto.NumeroReciboPago.Sanitize(),
                Fecha = DateTime.UtcNow,
                ClienteId = dto.ClienteId,
                UsuarioId = usuarioId,
                Observaciones = dto.Observaciones?.Sanitize(),
                Total = 0
            };

            var detallesVenta = new List<DetalleVenta>();
            decimal totalVenta = 0;

            // Validar stock y crear detalles
            foreach (var detalleDto in dto.Detalles)
            {
                // Las unidades ya se validaron arriba, pero verificamos nuevamente por seguridad
                if (detalleDto.Unidades <= 0)
                {
                    throw new BusinessException("Las unidades deben ser mayores que 0.");
                }

                if (detalleDto.LibroId <= 0)
                {
                    throw new BusinessException($"El LibroId debe ser mayor que 0. LibroId proporcionado: {detalleDto.LibroId}");
                }

                var libro = await _unitOfWork.Libros.GetByIdAsync(detalleDto.LibroId);
                if (libro == null)
                {
                    throw new BusinessException($"Libro con ID {detalleDto.LibroId} no encontrado.");
                }

                // Validar stock disponible en el lote
                Inventario? inventario = null;
                string loteUsado = detalleDto.Lote;

                // Si se especifica un lote, buscar ese lote específico
                if (!string.IsNullOrWhiteSpace(detalleDto.Lote) && detalleDto.Lote != "string")
                {
                    inventario = await _unitOfWork.Inventarios.GetByLibroAndLoteAsync(detalleDto.LibroId, detalleDto.Lote);
                }

                // Si no se encontró el lote específico o no se especificó, buscar cualquier lote disponible
                if (inventario == null || inventario.UnidadesDisponibles < detalleDto.Unidades)
                {
                    inventario = await _unitOfWork.Inventarios.GetAvailableLoteForBookAsync(detalleDto.LibroId, detalleDto.Unidades);
                    
                    if (inventario != null)
                    {
                        loteUsado = inventario.Lote;
                        _logger.LogInformation("Lote automático seleccionado para libro {LibroId}: {Lote}", detalleDto.LibroId, loteUsado);
                    }
                }

                // Validar stock total del libro como respaldo
                if (inventario == null || inventario.UnidadesDisponibles < detalleDto.Unidades)
                {
                    var inventariosDisponibles = await _unitOfWork.Inventarios.GetByLibroIdWithStockAsync(detalleDto.LibroId);
                    var stockDisponibleTotal = inventariosDisponibles.Sum(i => i.UnidadesDisponibles);

                    // Si no hay inventarios pero el libro tiene stock, crear un inventario automáticamente
                    if (!inventariosDisponibles.Any() && libro.Stock > 0)
                    {
                        _logger.LogWarning("No hay registros de inventario para el libro {LibroId} ({Nombre}), pero tiene stock de {Stock}. Creando inventario automáticamente.", 
                            detalleDto.LibroId, libro.Nombre, libro.Stock);
                        
                        // Buscar el último ingreso para obtener el lote y precio
                        var ultimoIngreso = (await _unitOfWork.Ingresos.SearchAsync(null, null, null, detalleDto.LibroId))
                            .OrderByDescending(i => i.Fecha)
                            .FirstOrDefault();
                        
                        string loteGenerado;
                        if (ultimoIngreso != null)
                        {
                            loteUsado = ultimoIngreso.Lote;
                            loteGenerado = ultimoIngreso.Lote;
                        }
                        else
                        {
                            // Generar un lote genérico basado en el año actual
                            var year = DateTime.Now.Year;
                            loteGenerado = $"{year}-SINCRONIZADO";
                            loteUsado = loteGenerado;
                        }
                        
                        // Crear inventario con el stock disponible del libro
                        inventario = new Smartbook.Entidades.Inventario
                        {
                            LibroId = detalleDto.LibroId,
                            Lote = loteGenerado,
                            UnidadesDisponibles = libro.Stock,
                            UnidadesVendidas = 0
                        };
                        
                        await _unitOfWork.Inventarios.AddAsync(inventario);
                        await _unitOfWork.SaveChangesAsync();
                        
                        _logger.LogInformation("Inventario creado automáticamente para libro {LibroId} con lote {Lote} y {Unidades} unidades", 
                            detalleDto.LibroId, loteGenerado, libro.Stock);
                    }
                    else if (stockDisponibleTotal < detalleDto.Unidades)
                    {
                        var lotesInfo = inventariosDisponibles.Any() 
                            ? string.Join(", ", inventariosDisponibles.Select(i => $"{i.Lote} ({i.UnidadesDisponibles} unidades)"))
                            : "No hay lotes disponibles";
                        
                        throw new BusinessException($"Stock insuficiente para el libro {libro.Nombre}. Stock total disponible: {stockDisponibleTotal}, Solicitado: {detalleDto.Unidades}. " +
                            $"Lotes disponibles: {lotesInfo}");
                    }
                    else
                    {
                        // Buscar un lote que tenga suficientes unidades
                        inventario = inventariosDisponibles
                            .Where(i => i.UnidadesDisponibles >= detalleDto.Unidades)
                            .OrderByDescending(i => i.UnidadesDisponibles)
                            .FirstOrDefault();
                        
                        // Si ningún lote tiene suficientes unidades, usar el lote con más unidades disponibles
                        if (inventario == null)
                        {
                            inventario = inventariosDisponibles
                                .OrderByDescending(i => i.UnidadesDisponibles)
                                .FirstOrDefault();
                            
                            if (inventario != null)
                            {
                                _logger.LogWarning("Stock distribuido en múltiples lotes. Usando lote {Lote} con {Unidades} unidades disponibles (se requieren {Requeridas})", 
                                    inventario.Lote, inventario.UnidadesDisponibles, detalleDto.Unidades);
                                loteUsado = inventario.Lote;
                            }
                        }
                        else
                        {
                            loteUsado = inventario.Lote;
                        }
                    }
                }

                if (inventario == null)
                {
                    throw new BusinessException($"No se encontró inventario disponible para el libro {libro.Nombre}.");
                }

                if (inventario.UnidadesDisponibles < detalleDto.Unidades)
                {
                    throw new BusinessException($"Stock insuficiente para el libro {libro.Nombre} en el lote {loteUsado}. Disponible: {inventario.UnidadesDisponibles}, Solicitado: {detalleDto.Unidades}");
                }

                // Obtener valor de venta del último ingreso de este libro y lote
                var ingreso = (await _unitOfWork.Ingresos.SearchAsync(null, null, loteUsado, detalleDto.LibroId))
                    .OrderByDescending(i => i.Fecha)
                    .FirstOrDefault();

                // Si no hay ingreso para este lote específico, buscar cualquier ingreso del libro
                if (ingreso == null)
                {
                    ingreso = (await _unitOfWork.Ingresos.SearchAsync(null, null, null, detalleDto.LibroId))
                        .OrderByDescending(i => i.Fecha)
                        .FirstOrDefault();
                }

                if (ingreso == null)
                {
                    throw new BusinessException($"No se encontró información de precio para el libro {libro.Nombre}. Por favor, registre un ingreso con precio de venta antes de realizar la venta.");
                }

                var valorUnitario = ingreso.ValorVentaPublico;
                var subtotal = valorUnitario * detalleDto.Unidades;
                totalVenta += subtotal;

                var detalle = new DetalleVenta
                {
                    LibroId = detalleDto.LibroId,
                    Lote = loteUsado, // Usar el lote encontrado/seleccionado
                    Unidades = detalleDto.Unidades,
                    ValorUnitario = valorUnitario,
                    Subtotal = subtotal
                };

                detallesVenta.Add(detalle);

                // Actualizar inventario
                inventario.UnidadesDisponibles -= detalleDto.Unidades;
                inventario.UnidadesVendidas += detalleDto.Unidades;
                await _unitOfWork.Inventarios.UpdateAsync(inventario);

                // Actualizar stock del libro
                libro.Stock -= detalleDto.Unidades;
                await _unitOfWork.Libros.UpdateAsync(libro);
            }

            venta.Total = totalVenta;
            
            await _unitOfWork.Ventas.AddAsync(venta);
            await _unitOfWork.SaveChangesAsync();

            // Agregar detalles de venta después de que la venta tenga ID
            foreach (var detalle in detallesVenta)
            {
                detalle.VentaId = venta.Id;
                _context.DetallesVenta.Add(detalle);
            }
            
            await _unitOfWork.SaveChangesAsync();
            
            // Sincronizar stock de todos los libros afectados después de la venta
            var librosAfectados = dto.Detalles.Select(d => d.LibroId).Distinct();
            foreach (var libroId in librosAfectados)
            {
                var libroAfectado = await _unitOfWork.Libros.GetByIdAsync(libroId);
                if (libroAfectado != null)
                {
                    var stockReal = await _unitOfWork.Inventarios.GetStockTotalByLibroIdAsync(libroId);
                    if (libroAfectado.Stock != stockReal)
                    {
                        _logger.LogInformation("Sincronizando stock después de venta. Libro {LibroId}, Stock anterior: {StockAnterior}, Stock real: {StockReal}", 
                            libroId, libroAfectado.Stock, stockReal);
                        libroAfectado.Stock = stockReal;
                        await _unitOfWork.Libros.UpdateAsync(libroAfectado);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Generar PDF y enviar correo
            var ventaCompleta = await _unitOfWork.Ventas.GetWithDetailsAsync(venta.Id);
            if (ventaCompleta != null)
            {
                var pdfContent = GeneratePdf(ventaCompleta);
                await _emailService.SendVentaNotificationAsync(
                    cliente.Email,
                    cliente.Nombres,
                    pdfContent,
                    venta.NumeroReciboPago
                );
            }

            _logger.LogInformation("Venta creada: {Id}, Cliente: {ClienteId}, Total: {Total}", venta.Id, dto.ClienteId, totalVenta);

            // Asegurar que ventaCompleta tenga todas las relaciones cargadas
            if (ventaCompleta == null)
            {
                ventaCompleta = await _unitOfWork.Ventas.GetWithDetailsAsync(venta.Id);
            }

            var ventaDto = ventaCompleta?.Adapt<VentaDto>() ?? venta.Adapt<VentaDto>();
            
            // Mapear manualmente los campos que dependen de relaciones
            if (ventaCompleta != null)
            {
                ventaDto.ClienteNombre = ventaCompleta.Cliente?.Nombres ?? string.Empty;
                ventaDto.ClienteIdentificacion = ventaCompleta.Cliente?.Identificacion ?? string.Empty;
                ventaDto.UsuarioNombre = ventaCompleta.Usuario?.Nombres ?? string.Empty;
                
                // Mapear detalles manualmente para asegurar LibroNombre
                var detallesDto = new List<DetalleVentaDto>();
                foreach (var detalle in ventaCompleta.DetallesVenta)
                {
                    var detalleDto = detalle.Adapt<DetalleVentaDto>();
                    if (detalle.Libro != null)
                    {
                        detalleDto.LibroNombre = detalle.Libro.Nombre;
                    }
                    detallesDto.Add(detalleDto);
                }
                ventaDto.Detalles = detallesDto;
            }
            else
            {
                // Si no se pudo cargar ventaCompleta, usar los datos que tenemos
                ventaDto.ClienteNombre = cliente.Nombres;
                ventaDto.ClienteIdentificacion = cliente.Identificacion;
                ventaDto.UsuarioNombre = usuario.Nombres;
                ventaDto.Detalles = new List<DetalleVentaDto>();
            }

            return ventaDto;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<VentaDto?> GetByIdAsync(int id)
    {
        var venta = await _unitOfWork.Ventas.GetWithDetailsAsync(id);
        if (venta == null) return null;

        var ventaDto = venta.Adapt<VentaDto>();
        
        // Mapear manualmente los campos que dependen de relaciones
        if (venta.Cliente != null)
        {
            ventaDto.ClienteNombre = venta.Cliente.Nombres;
            ventaDto.ClienteIdentificacion = venta.Cliente.Identificacion;
        }
        
        if (venta.Usuario != null)
        {
            ventaDto.UsuarioNombre = venta.Usuario.Nombres;
        }
        
        // Mapear detalles manualmente para asegurar LibroNombre
        var detallesDto = new List<DetalleVentaDto>();
        foreach (var detalle in venta.DetallesVenta)
        {
            var detalleDto = detalle.Adapt<DetalleVentaDto>();
            if (detalle.Libro != null)
            {
                detalleDto.LibroNombre = detalle.Libro.Nombre;
            }
            detallesDto.Add(detalleDto);
        }
        ventaDto.Detalles = detallesDto;

        return ventaDto;
    }

    public async Task<IEnumerable<VentaResumenDto>> SearchAsync(DateTime? desde, DateTime? hasta, string? clienteIdentificacion, int? libroId)
    {
        var ventas = await _unitOfWork.Ventas.SearchAsync(desde, hasta, clienteIdentificacion, libroId);
        var dtos = new List<VentaResumenDto>();
        
        foreach (var venta in ventas)
        {
            var dto = venta.Adapt<VentaResumenDto>();
            // Mapear manualmente ClienteNombre
            if (venta.Cliente != null)
            {
                dto.ClienteNombre = venta.Cliente.Nombres;
            }
            dtos.Add(dto);
        }
        
        return dtos;
    }

    private byte[] GeneratePdf(Venta venta)
    {
        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4, 50, 50, 25, 25);
        var writer = PdfWriter.GetInstance(doc, ms);

        doc.Open();

        // Agregar logo de CECAR
        try
        {
            var logoUrl = "https://credito.cecar.edu.co/assets/logo/logo.png";
            using var httpClient = new HttpClient();
            var logoBytes = httpClient.GetByteArrayAsync(logoUrl).GetAwaiter().GetResult();
            using var logoStream = new MemoryStream(logoBytes);
            var logo = Image.GetInstance(logoBytes);
            logo.Alignment = Element.ALIGN_CENTER;
            logo.ScaleToFit(150f, 80f);
            doc.Add(logo);
            doc.Add(new Paragraph(" "));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar el logo del PDF. Continuando sin logo.");
        }

        // Encabezado
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        doc.Add(new Paragraph("RECIBO DE PAGO", titleFont) { Alignment = Element.ALIGN_CENTER });
        doc.Add(new Paragraph("Centro de Idiomas - CECAR", normalFont) { Alignment = Element.ALIGN_CENTER });
        doc.Add(new Paragraph(" "));

        // Información de la venta
        doc.Add(new Paragraph($"Número de Recibo: {venta.NumeroReciboPago}", normalFont));
        doc.Add(new Paragraph($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}", normalFont));
        doc.Add(new Paragraph($"Cliente: {venta.Cliente?.Nombres}", normalFont));
        doc.Add(new Paragraph($"Identificación: {venta.Cliente?.Identificacion}", normalFont));
        doc.Add(new Paragraph(" "));

        // Tabla de detalles
        var table = new PdfPTable(5) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 3, 2, 1, 2, 2 });

        table.AddCell(new PdfPCell(new Phrase("Libro", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Lote", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Cant.", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Valor Unit.", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Subtotal", normalFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

        foreach (var detalle in venta.DetallesVenta)
        {
            table.AddCell(new Phrase(detalle.Libro?.Nombre ?? "", normalFont));
            table.AddCell(new Phrase(detalle.Lote, normalFont));
            table.AddCell(new Phrase(detalle.Unidades.ToString(), normalFont));
            table.AddCell(new Phrase($"${detalle.ValorUnitario:N2}", normalFont));
            table.AddCell(new Phrase($"${detalle.Subtotal:N2}", normalFont));
        }

        doc.Add(table);
        doc.Add(new Paragraph(" "));

        // Total
        var totalFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        doc.Add(new Paragraph($"TOTAL: ${venta.Total:N2}", totalFont) { Alignment = Element.ALIGN_RIGHT });

        if (!string.IsNullOrWhiteSpace(venta.Observaciones))
        {
            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph($"Observaciones: {venta.Observaciones}", normalFont));
        }

        doc.Close();

        return ms.ToArray();
    }
}

