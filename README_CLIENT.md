# eCommerce MotoRepuestos - Guía para el cliente

## Descripción general

Esta aplicación fue desarrollada para administrar una tienda online de moto repuestos. Permite publicar productos, organizar el catálogo, recibir pedidos, gestionar clientes y operar tareas administrativas desde un único sistema.

El objetivo es que el negocio pueda vender por internet y, al mismo tiempo, mantener control sobre stock, pedidos y datos principales de la tienda.

## Qué incluye la aplicación

La solución contempla dos grandes áreas de uso:

- Un sector público para los clientes que visitan la tienda.
- Un sector interno de administración para operar el negocio.

## Funciones para los clientes de la tienda

Los usuarios que ingresan al sitio pueden:

- Ver el catálogo de productos disponibles.
- Filtrar productos por categoría.
- Buscar productos por nombre o descripción.
- Consultar el detalle de cada producto.
- Registrarse como clientes.
- Iniciar sesión.
- Agregar productos al carrito.
- Modificar cantidades en el carrito.
- Finalizar una compra.
- Consultar sus pedidos realizados.

## Funciones para la administración

El panel administrativo permite:

- Crear, editar, activar o desactivar productos.
- Crear, editar, activar o desactivar categorías.
- Visualizar pedidos recibidos.
- Cambiar el estado de los pedidos.
- Definir un valor de referencia para stock bajo.
- Generar un respaldo de la base de datos.
- Restaurar la información desde un respaldo existente.

## Gestión de productos

La aplicación permite mantener actualizado el catálogo comercial:

- Cada producto puede tener nombre, descripción, precio, stock e imágen.
- Los productos pueden darse de alta o de baja (sin ser eliminados).
- Solo los productos disponibles y activos aparecen para los clientes.
- El sistema ayuda a identificar productos con stock bajo.

## Gestión de categorías

Las categorías permiten filtrar el catálogo para que el cliente encuentre más rápido lo que busca.

- Se pueden crear nuevas categorías.
- Se pueden editar categorías existentes.
- Se pueden desactivar categorías que ya no se utilicen, así como volver a activarse.
- Las categorías inactivas dejan de mostrarse en la tienda.

## Gestión de clientes y accesos

La aplicación maneja distintos tipos de usuario:

- Cliente: compra productos y consulta sus pedidos.
- Administrador: gestiona la operación diaria.
- Super administrador: puede crear nuevos administradores.

Cada usuario accede con email y contraseña.

## Carrito y proceso de compra

La compra sigue un flujo simple:

1. El cliente selecciona productos.
2. Los agrega al carrito.
3. Revisa cantidades y disponibilidad.
4. Confirma la compra.
5. El sistema genera el pedido.

El sistema controla que no se vendan unidades por encima del stock disponible.

## Gestión de pedidos

Cada compra queda registrada como pedido y puede ser seguida dentro del sistema.

Esto permite:

- Cliente: consultar su historial de pedidos.
- Administrador: Ver todos los pedidos y cambiar su estado según avance la preparación o entrega.

## Resguardo de información

La aplicación incluye herramientas para proteger los datos del negocio:

- Generación de backup.
- Restauración del backup.

Esto facilita recuperar información ante errores operativos o necesidad de resguardo.

## Beneficios para el negocio

Entre los beneficios principales se encuentran:

- Centralización de la operación en un único sistema.
- Mejor control del catálogo y del stock.
- Seguimiento claro de pedidos.
- Mayor autonomía para administrar productos y categorías.
- Base preparada para ventas online con gestión interna.

## Alcance de la entrega

La aplicación entrega una base funcional para operar una tienda online de moto repuestos (adaptable a otros negocios), incluyendo:

- catálogo público,
- registro e inicio de sesión,
- carrito de compras,
- gestión de pedidos,
- administración de productos,
- administración de categorías,
- configuración de stock,
- backup y restauración.

## Recomendaciones de uso

Para un uso ordenado del sistema se recomienda:

- mantener actualizado el stock,
- revisar periódicamente el estado de los pedidos,
- usar imágenes claras y nombres precisos en los productos,
- generar backups de forma regular,
- limitar el acceso administrativo solo a personal autorizado y resguardar correctamente las contraseñas.

## Nota final

Este documento está pensado para explicar la solución desde el punto de vista del negocio. Para detalles de instalación, estructura interna o aspectos técnicos, debe consultarse el archivo `README.md`.
