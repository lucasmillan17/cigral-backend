# Ejemplos Practicos - Sistema de Manejo de Errores

## Escenarios Comunes y Como Manejarlos

### Escenario 1: Validar que una Entidad Exista

**Antes (sin el sistema)**:
```csharp
public async Task<ProductoDto> GetProducto(int id)
{
    var producto = await _repository.GetById<Producto>(id);
    if (producto == null)
    {
        // ¿Que hacer? ¿Retornar null? ¿Lanzar excepcion generica?
        return null; // Mal patron
    }
    return MapToDto(producto);
}
```

**Ahora (con el sistema)**:
```csharp
public async Task<ProductoModelResponse> GetProductoById(int id)
{
    var producto = await _repository.GetById<Producto>(id, "Marca");
    
    if (producto == null)
    {
        throw new NotFoundException(nameof(Producto), id);
    }
    
    return ResponseGenerator(producto);
}
```

**Resultado**:
- Cliente recibe 404 automaticamente
- Mensaje claro: "La entidad Producto (5) no fue encontrada."
- Log automatico del error
- No necesitas manejar la excepcion en el controller

### Escenario 2: Validar Reglas de Negocio

**Ejemplo: GTIN Duplicado**

```csharp
public async Task<ProductoModelResponse> CreateProducto(ProductoModelRequest r)
{
    // Validar GTIN unico
    var existente = await _repository.First<Producto>(p => p.GTIN == r.GTIN);
    if (existente != null)
    {
        throw new DomainException(
            DomainErrorCode.GtinDuplicado,
            $"El producto con GTIN {r.GTIN} ya existe."
        );
    }
    
    // Resto de la logica...
}
```

**Resultado**:
- Cliente recibe 400 Bad Request
- Codigo de error: GtinDuplicado (2001)
- Mensaje descriptivo personalizado
- Log automatico

### Escenario 3: Multiples Validaciones en Secuencia

```csharp
public async Task<RemitoModelResponse> CreateRemito(RemitoModelRequest r)
{
    // Validacion 1: Cliente existe
    var cliente = await _repository.GetById<Cliente>(r.ClienteId);
    if (cliente == null)
    {
        throw new NotFoundException(nameof(Cliente), r.ClienteId);
    }
    
    // Validacion 2: Tiene detalles
    if (r.Detalles == null || r.Detalles.Count == 0)
    {
        throw new DomainException(
            DomainErrorCode.RemitoSinDetalles,
            "El remito debe tener al menos un detalle."
        );
    }
    
    // Validacion 3: Numero de remito unico
    if (!string.IsNullOrEmpty(r.NumeroRemito))
    {
        var existente = await _repository.First<RemitoCliente>(
            rc => rc.NumeroRemito == r.NumeroRemito
        );
        if (existente != null)
        {
            throw new DomainException(
                DomainErrorCode.NumeroRemitoDuplicado,
                $"Ya existe un remito con el numero {r.NumeroRemito}."
            );
        }
    }
    
    // Validacion 4: Productos y lotes existen
    foreach (var detalle in r.Detalles)
    {
        var producto = await _repository.GetById<Producto>(detalle.ProductoId);
        if (producto == null)
        {
            throw new DomainException(
                DomainErrorCode.ProductoNoExiste,
                $"El producto con ID {detalle.ProductoId} no existe."
            );
        }
        
        var lote = await _repository.GetById<Lote>(detalle.LoteId);
        if (lote == null)
        {
            throw new DomainException(
                DomainErrorCode.LoteNoEncontrado,
                $"El lote con ID {detalle.LoteId} no existe."
            );
        }
        
        // Validacion 5: Lote no vencido
        if (lote.FechaVencimiento < DateTime.Now)
        {
            throw new DomainException(
                DomainErrorCode.LoteVencido,
                $"El lote {lote.CodigoLote} ha vencido el {lote.FechaVencimiento:dd/MM/yyyy}."
            );
        }
        
        // Validacion 6: Stock suficiente
        if (lote.CantidadDisponible < detalle.Cantidad)
        {
            throw new DomainException(
                DomainErrorCode.StockInsuficiente,
                $"Stock insuficiente. Disponible: {lote.CantidadDisponible}, Solicitado: {detalle.Cantidad}"
            );
        }
    }
    
    // Si llegamos aqui, todas las validaciones pasaron
    // Crear el remito...
}
```

**Ventajas**:
- Codigo limpio y legible
- Cada validacion es independiente
- Fail-fast: se detiene en la primera validacion que falla
- Cliente recibe exactamente que salio mal

### Escenario 4: Validar Entidades Relacionadas

```csharp
public async Task<ProductoModelResponse> CreateProducto(ProductoModelRequest r)
{
    // Si se especifica marca, validar que exista
    if (r.MarcaId.HasValue)
    {
        var marca = await _repository.GetById<Marca>(r.MarcaId.Value);
        if (marca == null)
        {
            throw new DomainException(
                DomainErrorCode.MarcaNoValida,
                $"La marca con ID {r.MarcaId.Value} no existe."
            );
        }
    }
    
    // Crear producto...
}
```

### Escenario 5: Update con Validaciones

```csharp
public async Task<ProductoModelResponse> UpdateProducto(int id, ProductoModelRequest r)
{
    // 1. Validar que exista
    var producto = await _repository.GetById<Producto>(id);
    if (producto == null)
    {
        throw new NotFoundException(nameof(Producto), id);
    }
    
    // 2. Validar GTIN unico (excepto el mismo producto)
    var productoConMismoGtin = await _repository.First<Producto>(
        p => p.GTIN == r.GTIN && p.Id != id
    );
    if (productoConMismoGtin != null)
    {
        throw new DomainException(
            DomainErrorCode.GtinDuplicado,
            $"El GTIN {r.GTIN} ya existe en otro producto."
        );
    }
    
    // 3. Validar marca si se especifica
    if (r.MarcaId.HasValue)
    {
        var marca = await _repository.GetById<Marca>(r.MarcaId.Value);
        if (marca == null)
        {
            throw new DomainException(
                DomainErrorCode.MarcaNoValida,
                $"La marca con ID {r.MarcaId.Value} no existe."
            );
        }
    }
    
    // Actualizar...
}
```

## Patron Recomendado para Nuevos Servicios

### Plantilla para Create

```csharp
public async Task<TResponse> Create<TEntity>(TRequest request)
{
    // 1. Validar unicidad de campos clave
    var existente = await _repository.First<TEntity>(e => e.CampoClave == request.CampoClave);
    if (existente != null)
    {
        throw new DomainException(
            DomainErrorCode.CampoClaveduplicado,
            "El campo clave ya existe."
        );
    }
    
    // 2. Validar entidades relacionadas
    if (request.RelacionId.HasValue)
    {
        var relacionada = await _repository.GetById<Relacionada>(request.RelacionId.Value);
        if (relacionada == null)
        {
            throw new DomainException(
                DomainErrorCode.RelacionadaNoExiste,
                "La entidad relacionada no existe."
            );
        }
    }
    
    // 3. Crear la entidad
    var entity = MapToEntity(request);
    await _repository.Add(entity);
    
    // 4. Retornar respuesta
    return MapToResponse(entity);
}
```

### Plantilla para GetById

```csharp
public async Task<TResponse> GetById(int id)
{
    var entity = await _repository.GetById<TEntity>(id, "Relaciones");
    
    if (entity == null)
    {
        throw new NotFoundException(nameof(TEntity), id);
    }
    
    return MapToResponse(entity);
}
```

### Plantilla para Update

```csharp
public async Task<TResponse> Update(int id, TRequest request)
{
    // 1. Validar existencia
    var entity = await _repository.GetById<TEntity>(id);
    if (entity == null)
    {
        throw new NotFoundException(nameof(TEntity), id);
    }
    
    // 2. Validar unicidad (excluyendo la misma entidad)
    var duplicado = await _repository.First<TEntity>(
        e => e.CampoClave == request.CampoClave && e.Id != id
    );
    if (duplicado != null)
    {
        throw new DomainException(
            DomainErrorCode.CampoClaveDuplicado,
            "El campo clave ya existe en otra entidad."
        );
    }
    
    // 3. Actualizar
    UpdateEntity(entity, request);
    await _repository.Update(entity);
    
    // 4. Retornar
    return MapToResponse(entity);
}
```

### Plantilla para Delete

```csharp
public async Task Delete(int id)
{
    var entity = await _repository.GetById<TEntity>(id);
    if (entity == null)
    {
        throw new NotFoundException(nameof(TEntity), id);
    }
    
    // Validar si se puede eliminar (opcional)
    var tieneRelaciones = await TieneRelacionesActivas(id);
    if (tieneRelaciones)
    {
        throw new DomainException(
            DomainErrorCode.NoSePuedeEliminar,
            "No se puede eliminar porque tiene relaciones activas."
        );
    }
    
    await _repository.Delete(entity);
}
```

## Como el Cliente Debe Manejar los Errores

### JavaScript/TypeScript (Frontend)

```typescript
async function getProducto(id: number) {
  try {
    const response = await fetch(`/api/products/${id}`);
    
    if (!response.ok) {
      const error = await response.json();
      
      switch (error.error) {
        case 'NotFound':
          alert(`Producto no encontrado: ${error.message}`);
          break;
          
        case 'DomainError':
          handleDomainError(error);
          break;
          
        case 'InternalServerError':
          alert('Error del servidor. Contacte al administrador.');
          console.error(error);
          break;
      }
      
      return null;
    }
    
    return await response.json();
  } catch (e) {
    console.error('Error de red:', e);
    alert('Error de conexion');
    return null;
  }
}

function handleDomainError(error: any) {
  const code = error.details?.code;
  
  switch (code) {
    case 'GtinDuplicado':
      alert('El GTIN ya existe. Use otro codigo.');
      break;
      
    case 'MarcaNoValida':
      alert('La marca seleccionada no es valida.');
      break;
      
    case 'StockInsuficiente':
      alert('No hay suficiente stock disponible.');
      break;
      
    default:
      alert(error.message);
  }
}
```

### C# (Cliente API)

```csharp
public async Task<ProductoDto?> GetProducto(int id)
{
    try
    {
        return await _httpClient.GetFromJsonAsync<ProductoDto>($"/api/products/{id}");
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
    {
        _logger.LogWarning("Producto {Id} no encontrado", id);
        return null;
    }
    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
    {
        var error = await ParseErrorResponse(ex);
        _logger.LogWarning("Error de dominio: {Code}", error.Code);
        throw new BusinessException(error.Message);
    }
}
```

## Testing

### Test Unitario del Servicio

```csharp
[Fact]
public async Task CreateProducto_GtinDuplicado_DeberiaLanzarDomainException()
{
    // Arrange
    var mockRepo = new Mock<IRepository>();
    mockRepo.Setup(r => r.First<Producto>(It.IsAny<Expression<Func<Producto, bool>>>()))
            .ReturnsAsync(new Producto { GTIN = "1234567890123" });
    
    var service = new ProductoService(mockRepo.Object);
    var request = new ProductoModelRequest("Test", "Desc", "1234567890123", true, 100, null);
    
    // Act & Assert
    var exception = await Assert.ThrowsAsync<DomainException>(
        () => service.CreateProducto(request)
    );
    
    Assert.Equal(DomainErrorCode.GtinDuplicado, exception.Code);
    Assert.Contains("1234567890123", exception.Message);
}
```

### Test de Integracion del Middleware

```csharp
[Fact]
public async Task GetProducto_NoExiste_RetornaNotFoundConFormatoCorrecto()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/api/products/999");
    var content = await response.Content.ReadAsStringAsync();
    var error = JsonSerializer.Deserialize<ErrorResponse>(content);
    
    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Equal("NotFound", error.Error);
    Assert.Equal(404, error.StatusCode);
    Assert.NotNull(error.Timestamp);
    Assert.Equal("Producto", error.Details["entityName"]);
    Assert.Equal(999, error.Details["key"]);
}
```

---

**Sistema Completo y Listo para Usar**

Estos patrones pueden aplicarse a todos los servicios del sistema para mantener consistencia y calidad.
