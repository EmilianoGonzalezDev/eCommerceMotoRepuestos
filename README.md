# eCommerce MotoRepuestos

Aplicación web ASP.NET Core MVC para la gestión y venta de moto repuestos. El proyecto combina un catálogo público con un backoffice administrativo para productos, categorías, pedidos, configuración y backup.

## Demo online

La aplicación está publicada como demo en Azure App Service, con datos ficticios.

| Rol | Email | Contraseña |
| --- | --- | --- |
| Cliente (con pedidos) | `juanperez@gmail.com` | `Demo1234` |
| Admin | `admin1@admin` | `Demo1234` |
| SuperAdmin | `superadmin@admin` | `Demo1234` |

- En la pantalla de login hay botones para ingresar con cada cuenta en un clic.
- Los datos se restablecen todos los días a las 03:00 (hora de Argentina). Los admins también pueden restablecerlos desde el banner superior.
- Las cuentas demo no pueden editar su perfil. Para probar esa función, registrá una cuenta propia (se elimina en el próximo reinicio).
- El pago con tarjeta es simulado: no se cobra nada y los datos de la tarjeta no se envían al servidor.
- La app corre en el plan gratuito, que la suspende cuando no hay tráfico: la primera carga puede tardar entre 10 y 30 segundos.

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
- `ProductService.GetCatalogPagedAsync(...)`: devuelve solo productos activos, con stock y dentro de categorías activas; filtra y pagina en la base de datos.
- La búsqueda se normaliza para tolerar diferencias de mayúsculas, acentos y espacios. Se hace sobre la columna `Product.SearchText` (nombre y descripción normalizados), que se calcula al crear o editar un producto y se completa al iniciar la app para los productos que no la tengan.

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

### 8. Modo demo

Pensado para publicar la aplicación con cuentas compartidas. Se configura en la sección `Demo` de `appsettings.json` (`Enabled`, `ResetToken`, `Accounts`) y está desactivado por defecto.

- `DemoResetService`: restaura `SeedData/seed.db` sobre la base, aplica migraciones, fija las contraseñas de las cuentas demo, vuelve las imágenes de `wwwroot/images` a las de la semilla y registra la fecha del reinicio.
- `DemoController`:
  - `POST /demo/reset`: reinicio programado, autenticado con el header `X-Demo-Reset-Token`.
  - `POST /demo/reset-now`: botón del banner, solo para Admin y SuperAdmin.
- Las cookies de sesión llevan la fecha del último reinicio (`Utilities/DemoSession.cs`); las emitidas antes de un reinicio se invalidan, porque sus usuarios pueden no existir más.
- Las cuentas demo no pueden modificar su perfil.

### Medidas de seguridad

Como las credenciales de admin son públicas, la aplicación asume que cualquier visitante puede ser administrador:

- Validación de imágenes subidas: solo JPG, PNG o WEBP, hasta 2 MB, verificando la firma del archivo.
- Sanitización del HTML de las descripciones de producto (`HtmlSanitizer`), que se muestran con `Html.Raw`.
- Token antiforgery obligatorio en todos los POST; las acciones que modifican datos no aceptan GET.
- Rate limiting por IP (`Utilities/RateLimitPolicies.cs`): límite general, uno estricto para login y registro, otro para operaciones de escritura y otro para el reinicio de la demo.

### 9. Backups de base de datos

Módulo para resguardar y restaurar la información.

- `BackupController`: creación y restauración de backups.
- `DatabaseBackupService`: copia y restaura la base de datos SQLite usando `SqliteConnection.BackupDatabase`.
- Los backups se almacenan en la carpeta `Backups/`, junto al archivo de la base.

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

- La base principal es `app.db`. Su ubicación se define con la cadena de conexión `SqlString` (relativa a la carpeta de la app o absoluta).
- Si al iniciar la base no existe, `DemoSeeder` copia la semilla versionada `SeedData/seed.db` y restaura en `wwwroot/images` las imágenes de `SeedData/images` que falten.
- Al iniciar la aplicación se ejecuta `Database.Migrate()`, por lo que las migraciones pendientes se aplican automáticamente (también sobre la semilla).
- `AppDbContext` define relaciones entre usuarios, productos, categorías, pedidos, items de pedido, carrito y configuraciones.
- Se siembra un registro inicial de configuración para stock bajo y un usuario `SuperAdmin`.

## Ejecución local

### Requisitos

- SDK de .NET 10 (solo para framework-dependent)

### Pasos

1. Extraer Zip

2. Ejecutar eCommerceMotoRepuestos.exe

3. Abrir la URL indicada por ASP.NET Core en la consola.

## Despliegue en Azure App Service

El workflow [`.github/workflows/deploy-azure.yml`](.github/workflows/deploy-azure.yml) publica la app en un App Service Linux en cada push a `main` (o manualmente con *Run workflow*), autenticándose por OIDC.

La base SQLite vive en `/home/data`, que es almacenamiento persistente de App Service: sobrevive a redeploys y reinicios. En el primer arranque se crea a partir de la semilla.

### Configuración inicial (una sola vez)

1. **Web App**: Publish *Code*, runtime *.NET 10 (LTS)*, sistema operativo *Linux*, plan *Free F1*. Dejar el despliegue continuo deshabilitado.
2. **Web App → Settings → Environment variables**: agregar `ConnectionStrings__SqlString` = `Data Source=/home/data/app.db`.
3. **Web App → Configuration → General settings**: *HTTPS Only* = On.
4. **Managed Identity** (user-assigned), por ejemplo `id-motorepuestos-github`:
   - *Federated credentials* → *GitHub Actions deploying Azure resources*: organización `EmilianoGonzalezDev`, repositorio `eCommerceMotoRepuestos`, entidad *Branch*, rama `main`.
   - En la Web App → *Access control (IAM)* → asignar el rol **Website Contributor** a la identidad.
5. **GitHub → Settings → Secrets and variables → Actions**:
   - Secrets: `AZURE_CLIENT_ID` (client ID de la identidad), `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`.
   - Variable: `AZURE_WEBAPP_NAME` (nombre de la Web App).
6. **Modo demo**:
   - Generar un token aleatorio, por ejemplo con `openssl rand -hex 32`.
   - En la Web App → *Environment variables*: `Demo__Enabled` = `true` y `Demo__ResetToken` = el token.
   - En GitHub: secret `DEMO_RESET_TOKEN` con el mismo token y variable `DEMO_APP_URL` con la URL de la app (`https://<app>.azurewebsites.net`).

### Reinicio de la demo

El workflow [`.github/workflows/demo-reset.yml`](.github/workflows/demo-reset.yml) llama a `POST /demo/reset` todas las noches a las 03:00 (hora de Argentina) y también se puede ejecutar a mano con *Run workflow*. Tener en cuenta que:

- los workflows programados solo corren desde la rama por defecto del repositorio;
- GitHub los desactiva tras 60 días sin actividad en el repositorio, y hay que volver a habilitarlos desde la pestaña *Actions*.

### Evitar costos en Azure

El plan *Free F1* no tiene costo y no escala solo: si se agota su cuota diaria, la app se detiene hasta el día siguiente. Los costos suelen venir de recursos creados alrededor:

- **Application Insights / Log Analytics**: el portal los propone al crear la Web App. Desactivarlos, o eliminarlos si ya se crearon.
- **Presupuesto**: en *Cost Management → Budgets*, crear uno de USD 1 con alertas al 50% y al 100% (real y pronosticado). Solo avisa, no detiene nada.
- **Azure Policy** (opcional): restringir los tipos de recursos y SKUs permitidos en el resource group.
- No crear deployment slots, Storage Accounts, Azure SQL ni dominios con certificado propio; la base SQLite en `/home/data` alcanza.

### Regenerar la semilla

`SeedData/seed.db` es una copia compacta (`VACUUM INTO`) de una base local curada, con `journal_mode=DELETE` (WAL no funciona bien sobre el almacenamiento de red de `/home`). `SeedData/images` contiene solo las imágenes referenciadas por sus productos. Para que Azure tome una semilla nueva, después del deploy alcanza con ejecutar el workflow de reinicio de la demo (o, con el modo demo apagado, borrar `/home/data/app.db` desde Kudu/SSH y reiniciar la app).


## Estructura resumida

```text
.github/         Workflows de despliegue a Azure y reinicio de la demo
Context/         DbContext, inicialización y semilla demo
Controllers/     Controladores MVC
Entities/        Entidades de dominio
Enums/           Estados y tipos
Migrations/      Migraciones de base de datos
Models/          ViewModels y paginación
Repositories/    Acceso a datos
SeedData/        Base e imágenes demo para el primer arranque
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
