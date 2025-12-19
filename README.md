## Buenas Prácticas Aplicadas (Módulo Category)

Este módulo ha sido refactorizado aplicando principios SOLID y patrones de diseño para mejorar su mantenibilidad, escalabilidad y limpieza de código.

### Principios SOLID Aplicados

#### Open/Closed Principle (OCP)
- **Descripción:** Las clases deben estar abiertas para extensión, pero cerradas para modificación.
- **Aplicación:** 
  - La lógica de validación fue extraída del `CategoryService` y delegada a la clase `CategoryValidator`, que implementa la interfaz `ICategoryValidator`.
  - Esto permite agregar nuevas validaciones sin modificar el servicio.

#### Liskov Substitution Principle (LSP)
- **Descripción:** Una clase derivada debe poder sustituir a su clase base sin alterar el comportamiento.
- **Aplicación:** 
  - Tanto `ICategoryService` como `ICategoryValidator` permiten intercambiar sus implementaciones sin romper el código que los consume.

### Patrones de Diseño Aplicados

#### Factory Pattern
- **Descripción:** Centraliza la creación de objetos complejos en una clase dedicada.
- **Aplicación:** 
  - Se creó la clase `CategoryFactory`, encargada de construir instancias de `Category`, encapsulando la lógica de creación y actualización de entidades.
  - Esto mejora la cohesión y permite reutilizar la lógica en otros contextos.

#### Strategy Pattern
- **Descripción:** Permite definir una familia de algoritmos (estrategias) y hacerlos intercambiables.
- **Aplicación:**
  - `ICategoryValidator` define una estrategia de validación que puede ser reemplazada por otras implementaciones sin afectar al servicio.
  - Esto es útil, por ejemplo, si se quiere cambiar la lógica de validación según el entorno o reglas de negocio.

### Archivos Relacionados

- `Application/CategoryService.cs`: Servicio refactorizado para depender de validación externa y factoría.
- `Application/CategoryValidator.cs`: Implementación de validaciones según la estrategia definida.
- `Application/CategoryFactory.cs`: Factoría para crear y actualizar objetos `Category`.
- `Program.cs`: Registro de dependencias con inyección de servicios:

```csharp
builder.Services.AddScoped<ICategoryValidator, CategoryValidator>();
builder.Services.AddScoped<ICategoryService, CategoryService>();





