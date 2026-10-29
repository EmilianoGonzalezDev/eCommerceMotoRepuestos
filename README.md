# eCommerce MotoRepuestos

Tienda online de moto repuestos desarrollada con ASP.NET Core MVC. Combina un catálogo público, donde los clientes buscan productos y compran, con un panel de administración para gestionar el catálogo, el stock, los pedidos y los respaldos de la base de datos.

Fue pensada para un negocio de repuestos, pero la base sirve para cualquier comercio que venda productos con stock.

## Demo online

**[motorepuestos-mvc.azurewebsites.net](https://motorepuestos-mvc.azurewebsites.net)**

Los datos son ficticios. Podés ingresar con cualquiera de estas cuentas, o usar los botones de acceso rápido de la pantalla de login:

| Rol | Email | Contraseña |
| --- | --- | --- |
| Cliente (con pedidos) | `juanperez@gmail.com` | `Demo1234` |
| Admin | `admin1@admin` | `Demo1234` |
| SuperAdmin | `superadmin@admin` | `Demo1234` |

- **Podés modificar todo:** los datos vuelven a su estado inicial todos los días a las 03:00 (hora de Argentina). Los administradores también pueden restablecerlos desde el banner superior.
- **Las cuentas demo no pueden editar su perfil,** para que nadie le cambie la contraseña al resto. Para probar esa función, registrá una cuenta propia; se elimina en el próximo reinicio.
- **El pago con tarjeta es simulado:** no se cobra nada y los datos de la tarjeta no salen del navegador. Hay un botón para completarlos con una tarjeta de prueba.
- **La primera carga puede tardar entre 10 y 30 segundos:** la demo está en un plan gratuito que suspende la app cuando no hay visitas.

### Qué probar

- **Como visitante:** buscar productos (la búsqueda tolera mayúsculas, acentos y espacios), filtrar por categoría y agregar al carrito sin estar logueado.
- **Como cliente:** iniciar sesión (el carrito de visitante se suma al del usuario), finalizar una compra y ver el historial en *Mis Compras*.
- **Como admin:**
  - crear y editar productos con imagen y descripción enriquecida;
  - dar de baja productos y categorías;
  - cambiar el estado de los pedidos;
  - filtrar productos con stock bajo y ajustar ese umbral en *Configuración*;
  - generar y restaurar un backup.
- **Como SuperAdmin:** crear nuevos administradores.

## Funcionalidades

### Para los clientes de la tienda

- Catálogo de productos disponibles, con búsqueda, filtro por categoría y paginación.
- Detalle de cada producto.
- Registro e inicio de sesión.
- Carrito de compras: para visitantes se guarda en la sesión; al iniciar sesión se fusiona con el carrito guardado del usuario.
- Compra con pago en tienda o con tarjeta, validando el stock disponible.
- Historial de pedidos propios.

### Para la administración

- **Productos:** alta, edición, baja lógica y reactivación. Cada producto tiene nombre, descripción, precio, stock e imagen. El listado tiene búsqueda, ordenamiento y filtro de stock bajo.
- **Categorías:** alta, edición, baja y reactivación. Las categorías inactivas dejan de mostrarse en la tienda.
- **Pedidos:** listado de todos los pedidos con filtro por estado, y cambio de estado según avanza la preparación o entrega.
- **Configuración:** umbral de stock bajo.
- **Backups:** generación y restauración de un respaldo de la base de datos.

La tienda solo muestra productos vendibles: activos, con stock y dentro de categorías activas.

### Roles

| Rol | Puede |
| --- | --- |
| Cliente | Comprar y consultar sus pedidos |
| Admin | Gestionar productos, categorías, pedidos, configuración y backups |
| SuperAdmin | Crear nuevos administradores |

## Stack técnico

- .NET 10 y ASP.NET Core MVC con vistas Razor
- Entity Framework Core 10 con SQLite
- Autenticación por cookies con roles, contraseñas hasheadas con `PasswordHasher`
- Bootstrap, jQuery, Summernote, SweetAlert2 y Font Awesome
- GitHub Actions para el despliegue continuo a Azure App Service (autenticación OIDC, sin credenciales guardadas) y el reinicio diario de la demo

## Arquitectura

La solución sigue una separación por capas:

| Carpeta | Contenido |
| --- | --- |
| `Controllers/` | Endpoints MVC y orquestación de casos de uso |
| `Services/` | Lógica de negocio |
| `Repositories/` | Acceso a datos con EF Core (repositorio genérico y repositorios específicos) |
| `Context/` | `AppDbContext`, inicialización de la base y semilla de datos |
| `Entities/` | Entidades persistidas |
| `Models/` | View models, paginación y configuración |
| `Enums/` | Estados y tipos del dominio |
| `Utilities/` | Validaciones, rate limiting, sesión y helpers |
| `Views/` | Interfaz Razor |
| `Migrations/` | Historial de migraciones de EF Core |
| `SeedData/` | Base e imágenes de demostración |

### Decisiones destacadas

- **Consultas en la base:** filtros, ordenamiento y paginación del catálogo, productos y pedidos se resuelven en SQL, no en memoria.
- **Búsqueda normalizada:** la columna `Product.SearchText` guarda nombre y descripción sin acentos ni mayúsculas. Se calcula al guardar un producto.
- **Migraciones automáticas:** se aplican al iniciar la app, también después de restaurar un backup.
- **Datos iniciales:** si la base no existe, se crea a partir de la semilla versionada en `SeedData/`.

## Seguridad

La demo publica credenciales de administrador, así que la aplicación asume que cualquier visitante puede serlo:

- **Imágenes subidas:** solo JPG, PNG o WEBP de hasta 2 MB, verificando la firma real del archivo y no solo la extensión.
- **Descripciones de producto:** el HTML del editor se sanitiza con `HtmlSanitizer` antes de guardarse, para evitar XSS.
- **CSRF:** token antiforgery obligatorio en todos los POST; ninguna acción que modifica datos acepta GET.
- **Rate limiting por IP:** un límite general, uno estricto para login y registro, otro para las operaciones de escritura y otro para el reinicio de la demo.
- **Reinicio de la demo:**
  - el endpoint programado se autentica con un token secreto, comparado en tiempo constante;
  - las sesiones emitidas antes de un reinicio se invalidan, porque sus usuarios pueden haber dejado de existir.

## Ejecución local

Requisito: [SDK de .NET 10](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/EmilianoGonzalezDev/eCommerceMotoRepuestos.git
cd eCommerceMotoRepuestos
dotnet run
```

La app se abre en la URL que indica la consola. En el entorno de desarrollo:
- la base se crea a partir de la semilla de demostración;
- el modo demo queda activo, con las mismas cuentas de la tabla de arriba.
