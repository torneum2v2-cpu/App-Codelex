# Módulo de cotizaciones y facturas

## Propósito

Crear cotizaciones por servicio y actividades (el precio sale de la fórmula de costeo guardada en la base), facturar las órdenes completadas, ver facturas, imprimirlas o guardarlas como PDF, anularlas con un motivo, saldar cuotas y registrar abonos. La factura no se puede modificar: todo viene de la orden y su cotización; solo se elige tipo de pago, método y cuotas.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `cotizacion.xaml(.cs)` | Cotización: cliente, servicio, actividades con su cantidad, descuento en porcentaje (opcional) y totales. El descuento se guarda como una línea negativa, así la orden y la factura lo heredan; el ISV se calcula después del descuento. |
| `factura.xaml(.cs)` | Factura nueva de una orden completada; solo se elige la forma de pago. |
| `factura-detalle.xaml(.cs)` | Ver factura: estado, cuotas, saldar cuota, registrar abono, imprimir/PDF y anular (con motivo y confirmación; no se anula una factura a crédito con abonos). |
| `registrar-abono.xaml(.cs)` | Abono libre con vista previa de cómo quedan las cuotas. |
| `linea-actividad.cs` | Línea de actividad de la cotización (medida, personas y precio). |
| `estilos-documento.xaml` | Estilos compartidos por las pantallas de este módulo. |

## Dependencias

- Clase `Facturacion` de `sistema-AccesoDatos`.
- Procedimientos: `sp_CotizarLineas`, `sp_InsertarCotizacion`, `sp_ListarActividades`, `sp_InsertarFactura`, `sp_ObtenerFactura`, `sp_DetalleFactura`, `sp_CuotasFactura`, `sp_SaldarCuota`, `sp_RegistrarAbono`, `sp_SimularAbono`, `sp_ListarAbonosFactura`, `sp_AnularFactura`.
- Tablas: `cotizaciones`, `detalles_cotizacion`, `facturas`, `detalles_factura`, `cuentas_por_cobrar`, `cuotas`, `abonos`, `parametros_costeo`.
- Módulos: `sistema-Clientes` (lupa de clientes), `sistema-Ordenes` (se abre desde ahí).

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Órdenes → Ingresar orden → Agregar cotización**, y **Órdenes → Gestión de órdenes** para facturar.
