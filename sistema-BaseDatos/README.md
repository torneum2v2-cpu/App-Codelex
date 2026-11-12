# Base de datos

## Propósito

Scripts de SQL Server de Codelex: tablas, tipos, datos fijos (servicios, actividades, roles, formas de pago, parámetros de costeo), procedimientos almacenados y disparadores de bitácora.

## Archivos

| Archivo | Contenido | Cuándo usarlo |
|---|---|---|
| `codelex-tablas.sql` | Crea la base, las tablas, los tipos y los datos iniciales | Para crear la base desde cero |
| `codelex-procedimientos.sql` | Solo funciones, procedimientos y disparadores | Después de `codelex-tablas.sql`, o cuando cambie un procedimiento |
| `codelex-base-completa.sql` | Todo junto, más los cambios para bases de versiones anteriores (incluye el cambio de nombres de tablas y campos) | Para crear la base o para poner al día una que ya existe |

Las facturas anuladas quedan con estado `Anulada` y la orden se puede facturar otra vez. Los ajustes de inventario se guardan en `ajustes_inventario`. Las tablas tienen índices en las llaves que más se consultan (órdenes, abonos, asistencias, bitácora). Cada cargo tiene un rol (`cargos.rol_id`) y el usuario de un empleado toma siempre el rol de su cargo. Cada abono guarda a qué cuotas se aplicó (`abono_cuotas`) para poder deshacer el último abono sin borrarlo.

Los tres se pueden ejecutar varias veces sin errores y sin duplicar datos. Cada cambio en la base se agrega en los tres archivos.

## Nombres

- Tablas en plural y minúscula (`clientes`, `detalles_factura`).
- Campos en singular y con guion bajo (`fecha_registro`, `nombre_cliente`).
- Los RTN e identidades se guardan siempre sin guiones.

## Dependencias

- SQL Server 2019 o superior.

## Cómo ejecutarlo

1. Abrir `codelex-base-completa.sql` en SQL Server Management Studio.
2. Ejecutarlo completo (F5).
3. Cada cambio posterior se aplica volviendo a ejecutar el mismo archivo.
