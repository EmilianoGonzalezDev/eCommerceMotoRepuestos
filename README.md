# eCommerce MotoRepuestos

Aplicación web ASP.NET Core MVC para la gestión y venta de moto repuestos. El proyecto combina un catálogo público con un backoffice administrativo para productos, categorías, pedidos, configuración y backup.

## Stack técnico

- .NET 10
- ASP.NET Core MVC
- Entity Framework Core 10
- SQLite
- Autenticación por cookies
- Bootstrap, jQuery, Summernote, SweetAlert2 y Font Awesome

## Qué resuelve el proyecto

La aplicación permite:

- Publicar un catálogo de productos activos con detalle, búsqueda y filtrado por categoría.
- Registrar clientes, administradores e iniciar sesión.
- Administrar carrito para usuarios autenticados y visitantes.
- Generar pedidos, consultar su estado o modificarlo.
- Gestionar categorías y productos desde un panel administrativo.
- Configurar umbral de stock bajo.
- Crear y restaurar backups de la base de datos.

## Módulos funcionales

### 1. Catálogo público

Responsable de la navegación principal de la tienda.

- `HomeController`: listado de productos, filtro por categoría, búsqueda, sugerencias de búsqueda y detalle.
- `ProductService.GetCatalogAsync(...)`: devuelve solo productos activos, con stock y dentro de categorías activas.
- La búsqueda se normaliza para tolerar diferencias de mayúsculas, acentos y espacios.

### 2. Cuentas y autenticación

Gestiona acceso, registro y perfil de usuarios.

- `AccountController`: login, registro, edición de perfil, logout y creación de administradores.
- `UserService`: validación de credenciales, hash de contraseñas con `PasswordHasher`, alta de usuarios y actualización de perfil.
- Roles utilizados:
  - `Client`: cliente final.
  - `Admin`: administración del negocio.
  - `SuperAdmin`: puede crear nuevos administradores.

### 3. Carrito de compras

Administra productos seleccionados antes de confirmar la compra.

- `CartController`: agregar, quitar, actualizar cantidades, visualizar carrito y finalizar compra.
- `CartService` y `CartRepository`: persistencia del carrito para usuarios autenticados.
- Para visitantes, el carrito se guarda en sesión.
- Al iniciar sesión, el carrito de sesión se fusiona con el carrito persistido del usuario.
- Se valida stock y disponibilidad antes de agregar productos al carrito o de cerrar la compra.

### 4. Pedidos

Encargado de la generación y seguimiento de compras (pedidos).

- `OrderController`:
  - `MyOrders`: historial de compras del cliente autenticado.
  - `Index`: vista administrativa de pedidos. Lista general de compras de todos los clientes.
  - `UpdateStatus`: cambio de estado de pedidos para administración.
- `OrderService`: crea pedidos a partir del carrito y proyecta la información a view models.
- `OrderRepository`: obtiene pedidos con detalle de usuario, productos e items.
- Estados de pedido definidos en `Enums/OrderStatus.cs`.

### 5. Gestión de productos

Módulo de backoffice para alta, edición, listado y activación/desactivación.

- `ProductController`: ABM y listado con paginación, ordenamiento, búsqueda y filtro por stock bajo.
- `ProductService`:
  - carga categorias activas,
  - guarda imágenes de productos en `wwwroot/images`,
  - actualiza imágenes existentes,
  - alterna estado del producto activo/inactivo.
- El filtro de stock bajo usa la configuración almacenada en `AppSetting`.

### 6. Gestión de categorías

Permite administrar la clasificación del catálogo.

- `CategoryController`: listado, alta, edición, validación de nombre y cambio de estado.
- `CategoryService`: evita duplicados y expone categorías activas para el catálogo y formularios.
- Las categorías inactivas no aparecen en el catálogo público.

### 7. Configuración de stock

Parámetros simples del sistema persistidos en base de datos.

- `SettingsController`: edición del umbral de stock bajo.
- `AppSettingService`: lectura y escritura de configuraciones.
- Las claves se centralizan en `Utilities/AppSettingsKeys.cs`.

### 8. Backups de base de datos

Módulo para resguardar y restaurar la información.

- `BackupController`: creación y restauración de backups.
- `DatabaseBackupService`: copia y restaura la base de datos SQLite usando `SqliteConnection.BackupDatabase`.
- Los backups se almacenan en la carpeta `Backups/`.

## Arquitectura del proyecto

La solución sigue una separación por capas simple y clara:

- `Controllers/`: endpoints MVC y orquestación de casos de uso.
- `Services/`: lógica de negocio.
- `Repositories/`: acceso a datos con EF Core.
- `Context/`: `AppDbContext` y configuración del modelo.
- `Entities/`: entidades persistidas en SQLite.
- `Models/`: view models, filtros, paginación y modelos de formularios.
- `Enums/`: estados y tipos del dominio.
- `Utilities/`: helpers de sesión, paginación y claves de configuración.
- `Views/`: interfaz Razor.
- `wwwroot/`: assets estáticos, scripts, estilos e imágenes.
- `Migrations/`: historial de migraciones de Entity Framework Core.

## Persistencia y datos

- La base principal es `app.db`.
- Al iniciar la aplicación se ejecuta `Database.Migrate()`, por lo que las migraciones pendientes se aplican automáticamente.
- `AppDbContext` define relaciones entre usuarios, productos, categorías, pedidos, items de pedido, carrito y configuraciones.
- Se siembra un registro inicial de configuración para stock bajo y un usuario `SuperAdmin`.

## Ejecución local

### Requisitos

- SDK de .NET 10 (solo para framework-dependent)

### Pasos

1. Extraer Zip

2. Ejecutar eCommerceMotoRepuestos.exe

3. Abrir la URL indicada por ASP.NET Core en la consola.


## Estructura resumida

```text
Context/         DbContext y configuración EF Core
Controllers/     Controladores MVC
Entities/        Entidades de dominio
Enums/           Estados y tipos
Migrations/      Migraciones de base de datos
Models/          ViewModels y paginación
Repositories/    Acceso a datos
Services/        Lógica de negocio
Utilities/       Helpers y constantes
Views/           Vistas Razor
wwwroot/         Archivos estáticos
app.db           Base SQLite
Program.cs       Configuración de la aplicación
```

## Observaciones

- La cultura configurada por defecto es `es-AR`.
- La sesión expira a los 30 minutos.
- La autenticación se implementa con cookies.
- El catálogo público solo muestra productos vendibles: activos, con stock y dentro de categorías activas.
