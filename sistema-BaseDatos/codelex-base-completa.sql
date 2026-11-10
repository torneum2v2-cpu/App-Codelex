IF DB_ID('Codelex') IS NULL
	CREATE DATABASE Codelex;
GO

USE Codelex
GO

IF DB_NAME() <> 'Codelex'
BEGIN
	RAISERROR('No se pudo usar la base Codelex.', 16, 1);
	SET NOEXEC ON;
END
GO

-- Nombres estandar: tablas en plural y minuscula, campos en singular con guion bajo.
-- Solo cambia las bases creadas con los nombres anteriores; si la base ya esta al dia, no hace nada.

-- Los disparadores de bitacora y el tipo de ingreso de equipos usan los nombres anteriores: se borran y se vuelven a crear mas abajo.

DROP TRIGGER IF EXISTS trg_bitacora_CLIENTES;
GO

DROP TRIGGER IF EXISTS trg_bitacora_COMPRAS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_CUENTAS_POR_COBRAR;
GO

DROP TRIGGER IF EXISTS trg_bitacora_CUOTAS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_EMPLEADOS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_EQUIPOS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_FACTURAS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_LOCACIONES;
GO

DROP TRIGGER IF EXISTS trg_bitacora_ORDENES;
GO

DROP TRIGGER IF EXISTS trg_bitacora_PRODUCTOS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_PROVEEDORES;
GO

DROP TRIGGER IF EXISTS trg_bitacora_USUARIOS;
GO

DROP TRIGGER IF EXISTS trg_bitacora_VEHICULOS;
GO

IF EXISTS (SELECT 1 FROM sys.table_types tt
		   INNER JOIN sys.columns c ON c.object_id = tt.type_table_object_id
		   WHERE tt.name = 'tipo_ingreso_equipos' AND c.name = 'nombreEquipo')
BEGIN
	DROP PROCEDURE IF EXISTS sp_RegistrarIngresoInventario;
	DROP TYPE dbo.tipo_ingreso_equipos;
END
GO

IF COL_LENGTH('CLIENTES', 'tipoIdentificacion') IS NOT NULL AND OBJECT_ID('CK_CLIENTES_tipoIdentificacion') IS NOT NULL
	ALTER TABLE CLIENTES DROP CONSTRAINT CK_CLIENTES_tipoIdentificacion;
GO

IF COL_LENGTH('ACTIVIDAD_EQUIPOS', 'nombreEquipo') IS NOT NULL
	EXEC sp_rename 'ACTIVIDAD_EQUIPOS.nombreEquipo', 'nombre_equipo', 'COLUMN';
GO

IF COL_LENGTH('ACTIVIDAD_RANGOS', 'personas') IS NOT NULL
	EXEC sp_rename 'ACTIVIDAD_RANGOS.personas', 'cantidad_personas', 'COLUMN';
GO

IF COL_LENGTH('ASISTENCIAS', 'fechaAsistencia') IS NOT NULL
	EXEC sp_rename 'ASISTENCIAS.fechaAsistencia', 'fecha_asistencia', 'COLUMN';
GO

IF COL_LENGTH('BITACORAS', 'equipoSistema') IS NOT NULL
	EXEC sp_rename 'BITACORAS.equipoSistema', 'equipo_sistema', 'COLUMN';
GO

IF COL_LENGTH('BITACORAS', 'fechaBitacora') IS NOT NULL
	EXEC sp_rename 'BITACORAS.fechaBitacora', 'fecha_bitacora', 'COLUMN';
GO

IF COL_LENGTH('CARGOS', 'nombreCargo') IS NOT NULL
	EXEC sp_rename 'CARGOS.nombreCargo', 'nombre_cargo', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'identificacionCliente') IS NOT NULL
	EXEC sp_rename 'CLIENTES.identificacionCliente', 'identificacion_cliente', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'tipoIdentificacion') IS NOT NULL
	EXEC sp_rename 'CLIENTES.tipoIdentificacion', 'tipo_identificacion', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'nombreEmpresa') IS NOT NULL
	EXEC sp_rename 'CLIENTES.nombreEmpresa', 'nombre_empresa', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'nombreCliente') IS NOT NULL
	EXEC sp_rename 'CLIENTES.nombreCliente', 'nombre_cliente', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'apellidoCliente') IS NOT NULL
	EXEC sp_rename 'CLIENTES.apellidoCliente', 'apellido_cliente', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'telefonoCliente') IS NOT NULL
	EXEC sp_rename 'CLIENTES.telefonoCliente', 'telefono_cliente', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'correoCliente') IS NOT NULL
	EXEC sp_rename 'CLIENTES.correoCliente', 'correo_cliente', 'COLUMN';
GO

IF COL_LENGTH('CLIENTES', 'estadoCliente') IS NOT NULL
	EXEC sp_rename 'CLIENTES.estadoCliente', 'estado_cliente', 'COLUMN';
GO

IF COL_LENGTH('COMPRAS', 'fechaCompra') IS NOT NULL
	EXEC sp_rename 'COMPRAS.fechaCompra', 'fecha_compra', 'COLUMN';
GO

IF COL_LENGTH('COTIZACION_DETALLE', 'personas') IS NOT NULL
	EXEC sp_rename 'COTIZACION_DETALLE.personas', 'cantidad_personas', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'identificacionCliente') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.identificacionCliente', 'identificacion_cliente', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'nombreEmpresa') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.nombreEmpresa', 'nombre_empresa', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'nombreCliente') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.nombreCliente', 'nombre_cliente', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'apellidoCliente') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.apellidoCliente', 'apellido_cliente', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'telefonoCliente') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.telefonoCliente', 'telefono_cliente', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'correoCliente') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.correoCliente', 'correo_cliente', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'fechaCotizacion') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.fechaCotizacion', 'fecha_cotizacion', 'COLUMN';
GO

IF COL_LENGTH('COTIZACIONES', 'observaciones') IS NOT NULL
	EXEC sp_rename 'COTIZACIONES.observaciones', 'observacion', 'COLUMN';
GO

IF COL_LENGTH('EMPLEADOS', 'nombreEmpleado') IS NOT NULL
	EXEC sp_rename 'EMPLEADOS.nombreEmpleado', 'nombre_empleado', 'COLUMN';
GO

IF COL_LENGTH('EMPLEADOS', 'apellidoEmpleado') IS NOT NULL
	EXEC sp_rename 'EMPLEADOS.apellidoEmpleado', 'apellido_empleado', 'COLUMN';
GO

IF COL_LENGTH('EMPLEADOS', 'telefonoEmpleado') IS NOT NULL
	EXEC sp_rename 'EMPLEADOS.telefonoEmpleado', 'telefono_empleado', 'COLUMN';
GO

IF COL_LENGTH('EMPLEADOS', 'correoEmpleado') IS NOT NULL
	EXEC sp_rename 'EMPLEADOS.correoEmpleado', 'correo_empleado', 'COLUMN';
GO

IF COL_LENGTH('EMPLEADOS', 'estadoEmpleado') IS NOT NULL
	EXEC sp_rename 'EMPLEADOS.estadoEmpleado', 'estado_empleado', 'COLUMN';
GO

IF COL_LENGTH('EQUIPOS', 'nombreEquipo') IS NOT NULL
	EXEC sp_rename 'EQUIPOS.nombreEquipo', 'nombre_equipo', 'COLUMN';
GO

IF COL_LENGTH('EQUIPOS', 'tipoEquipo') IS NOT NULL
	EXEC sp_rename 'EQUIPOS.tipoEquipo', 'tipo_equipo', 'COLUMN';
GO

IF COL_LENGTH('EQUIPOS', 'marcaEquipo') IS NOT NULL
	EXEC sp_rename 'EQUIPOS.marcaEquipo', 'marca_equipo', 'COLUMN';
GO

IF COL_LENGTH('EQUIPOS', 'modeloEquipo') IS NOT NULL
	EXEC sp_rename 'EQUIPOS.modeloEquipo', 'modelo_equipo', 'COLUMN';
GO

IF COL_LENGTH('ESTADO_EQUIPOS', 'nombreEstadoEquipo') IS NOT NULL
	EXEC sp_rename 'ESTADO_EQUIPOS.nombreEstadoEquipo', 'nombre_estado_equipo', 'COLUMN';
GO

IF COL_LENGTH('ESTADO_ORDENES', 'nombreEstadoOrden') IS NOT NULL
	EXEC sp_rename 'ESTADO_ORDENES.nombreEstadoOrden', 'nombre_estado_orden', 'COLUMN';
GO

IF COL_LENGTH('FACTURAS', 'fechaFactura') IS NOT NULL
	EXEC sp_rename 'FACTURAS.fechaFactura', 'fecha_factura', 'COLUMN';
GO

IF COL_LENGTH('FORMAS_PAGOS', 'nombreFormaPago') IS NOT NULL
	EXEC sp_rename 'FORMAS_PAGOS.nombreFormaPago', 'nombre_forma_pago', 'COLUMN';
GO

IF COL_LENGTH('LOCACIONES', 'nombreLocacion') IS NOT NULL
	EXEC sp_rename 'LOCACIONES.nombreLocacion', 'nombre_locacion', 'COLUMN';
GO

IF COL_LENGTH('LOCACIONES', 'tipoLocacion') IS NOT NULL
	EXEC sp_rename 'LOCACIONES.tipoLocacion', 'tipo_locacion', 'COLUMN';
GO

IF COL_LENGTH('LOCACIONES', 'identificacionCliente') IS NOT NULL
	EXEC sp_rename 'LOCACIONES.identificacionCliente', 'identificacion_cliente', 'COLUMN';
GO

IF COL_LENGTH('ORDENES', 'identificacionCliente') IS NOT NULL
	EXEC sp_rename 'ORDENES.identificacionCliente', 'identificacion_cliente', 'COLUMN';
GO

IF COL_LENGTH('ORDENES', 'observaciones') IS NOT NULL
	EXEC sp_rename 'ORDENES.observaciones', 'observacion', 'COLUMN';
GO

IF COL_LENGTH('PRODUCTOS', 'nombreProducto') IS NOT NULL
	EXEC sp_rename 'PRODUCTOS.nombreProducto', 'nombre_producto', 'COLUMN';
GO

IF COL_LENGTH('PRODUCTOS', 'tipoProducto') IS NOT NULL
	EXEC sp_rename 'PRODUCTOS.tipoProducto', 'tipo_producto', 'COLUMN';
GO

IF COL_LENGTH('PROVEEDORES', 'nombreProveedor') IS NOT NULL
	EXEC sp_rename 'PROVEEDORES.nombreProveedor', 'nombre_proveedor', 'COLUMN';
GO

IF COL_LENGTH('PROVEEDORES', 'telefonoProveedor') IS NOT NULL
	EXEC sp_rename 'PROVEEDORES.telefonoProveedor', 'telefono_proveedor', 'COLUMN';
GO

IF COL_LENGTH('PROVEEDORES', 'correoProveedor') IS NOT NULL
	EXEC sp_rename 'PROVEEDORES.correoProveedor', 'correo_proveedor', 'COLUMN';
GO

IF COL_LENGTH('ROLES', 'nombreRol') IS NOT NULL
	EXEC sp_rename 'ROLES.nombreRol', 'nombre_rol', 'COLUMN';
GO

IF COL_LENGTH('SERVICIOS', 'nombreServicio') IS NOT NULL
	EXEC sp_rename 'SERVICIOS.nombreServicio', 'nombre_servicio', 'COLUMN';
GO

IF COL_LENGTH('USUARIOS', 'estadoUsuario') IS NOT NULL
	EXEC sp_rename 'USUARIOS.estadoUsuario', 'estado_usuario', 'COLUMN';
GO

IF COL_LENGTH('VEHICULOS', 'marcaVehiculo') IS NOT NULL
	EXEC sp_rename 'VEHICULOS.marcaVehiculo', 'marca_vehiculo', 'COLUMN';
GO

IF COL_LENGTH('VEHICULOS', 'modeloVehiculo') IS NOT NULL
	EXEC sp_rename 'VEHICULOS.modeloVehiculo', 'modelo_vehiculo', 'COLUMN';
GO

IF COL_LENGTH('VEHICULOS', 'estadoVehiculo') IS NOT NULL
	EXEC sp_rename 'VEHICULOS.estadoVehiculo', 'estado_vehiculo', 'COLUMN';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'CARGOS')
	EXEC sp_rename 'CARGOS', 'cargos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'EMPLEADOS')
	EXEC sp_rename 'EMPLEADOS', 'empleados';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ROLES')
	EXEC sp_rename 'ROLES', 'roles';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'USUARIOS')
	EXEC sp_rename 'USUARIOS', 'usuarios';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'BITACORAS')
	EXEC sp_rename 'BITACORAS', 'bitacoras';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ASISTENCIAS')
	EXEC sp_rename 'ASISTENCIAS', 'asistencias';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'CLIENTES')
	EXEC sp_rename 'CLIENTES', 'clientes';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'LOCACIONES')
	EXEC sp_rename 'LOCACIONES', 'locaciones';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'SERVICIOS')
	EXEC sp_rename 'SERVICIOS', 'servicios';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'VEHICULOS')
	EXEC sp_rename 'VEHICULOS', 'vehiculos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ESTADO_ORDENES')
	EXEC sp_rename 'ESTADO_ORDENES', 'estados_orden';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ORDENES')
	EXEC sp_rename 'ORDENES', 'ordenes';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ACTIVIDADES')
	EXEC sp_rename 'ACTIVIDADES', 'actividades';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'PROVEEDORES')
	EXEC sp_rename 'PROVEEDORES', 'proveedores';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'PRODUCTOS')
	EXEC sp_rename 'PRODUCTOS', 'productos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'COMPRAS')
	EXEC sp_rename 'COMPRAS', 'compras';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'COMPRAS_PRODUCTOS')
	EXEC sp_rename 'COMPRAS_PRODUCTOS', 'compras_productos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ESTADO_EQUIPOS')
	EXEC sp_rename 'ESTADO_EQUIPOS', 'estados_equipo';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'EQUIPOS')
	EXEC sp_rename 'EQUIPOS', 'equipos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ORDEN_EMPLEADOS')
	EXEC sp_rename 'ORDEN_EMPLEADOS', 'orden_empleados';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ORDEN_PRODUCTOS')
	EXEC sp_rename 'ORDEN_PRODUCTOS', 'orden_productos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ORDEN_EQUIPOS')
	EXEC sp_rename 'ORDEN_EQUIPOS', 'orden_equipos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ORDEN_ACTIVIDADES')
	EXEC sp_rename 'ORDEN_ACTIVIDADES', 'orden_actividades';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'FORMAS_PAGOS')
	EXEC sp_rename 'FORMAS_PAGOS', 'formas_pago';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'FACTURAS')
	EXEC sp_rename 'FACTURAS', 'facturas';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'COTIZACIONES')
	EXEC sp_rename 'COTIZACIONES', 'cotizaciones';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'COTIZACION_DETALLE')
	EXEC sp_rename 'COTIZACION_DETALLE', 'detalles_cotizacion';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'FACTURA_DETALLE')
	EXEC sp_rename 'FACTURA_DETALLE', 'detalles_factura';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'COMPRAS_EQUIPOS')
	EXEC sp_rename 'COMPRAS_EQUIPOS', 'compras_equipos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'CUENTAS_POR_COBRAR')
	EXEC sp_rename 'CUENTAS_POR_COBRAR', 'cuentas_por_cobrar';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'CUOTAS')
	EXEC sp_rename 'CUOTAS', 'cuotas';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'PARAMETROS_COSTEO')
	EXEC sp_rename 'PARAMETROS_COSTEO', 'parametros_costeo';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ACTIVIDAD_RANGOS')
	EXEC sp_rename 'ACTIVIDAD_RANGOS', 'actividad_rangos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ACTIVIDAD_PRODUCTOS')
	EXEC sp_rename 'ACTIVIDAD_PRODUCTOS', 'actividad_productos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ACTIVIDAD_EQUIPOS')
	EXEC sp_rename 'ACTIVIDAD_EQUIPOS', 'actividad_equipos';
GO

IF EXISTS (SELECT 1 FROM sys.tables WHERE name COLLATE Latin1_General_CS_AS = 'ABONOS')
	EXEC sp_rename 'ABONOS', 'abonos';
GO


-- Tablas

IF OBJECT_ID('cargos') IS NULL
CREATE TABLE cargos
(
	cargo_id INT PRIMARY KEY IDENTITY,
	nombre_cargo VARCHAR(50) UNIQUE
)
GO

IF OBJECT_ID('empleados') IS NULL
CREATE TABLE empleados
(
	empleado_id INT PRIMARY KEY IDENTITY,
	cargo_id INT FOREIGN KEY REFERENCES cargos(cargo_id),
	nombre_empleado VARCHAR(30),
	apellido_empleado VARCHAR(30),
	identidad VARCHAR(30) UNIQUE NOT NULL,
	telefono_empleado Varchar(50),
	correo_empleado VARCHAR(50),
	estado_empleado BIT NOT NULL CONSTRAINT DF_EMPLEADOS_estadoEmpleado DEFAULT 1
)
GO

IF OBJECT_ID('roles') IS NULL
CREATE TABLE roles
(
	rol_id INT PRIMARY KEY IDENTITY,
	nombre_rol VARCHAR(50) UNIQUE NOT NULL
)
GO

IF OBJECT_ID('usuarios') IS NULL
CREATE TABLE usuarios
(
	usuario_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES empleados(empleado_id) NOT NULL UNIQUE,
	usuario VARCHAR(50) UNIQUE NOT NULL,
	contrasena_hash VARCHAR(255),
	passkey_id VARBINARY(1023) NULL,
	passkey_clave VARBINARY(1024) NULL,
	foto_perfil VARBINARY(MAX) NULL,
	rol_id INT NULL CONSTRAINT FK_USUARIOS_ROLES REFERENCES roles(rol_id),
	estado_usuario BIT NOT NULL CONSTRAINT DF_USUARIOS_estadoUsuario DEFAULT 1
)
GO

IF OBJECT_ID('bitacoras') IS NULL
CREATE TABLE bitacoras
(
	bitacora_id INT PRIMARY KEY IDENTITY,
	usuario_id INT FOREIGN KEY REFERENCES usuarios(usuario_id),
	accion VARCHAR(50),
	tabla Varchar(50),
	equipo_sistema VARCHAR(50),
	fecha_bitacora DATETIME
)
GO

IF OBJECT_ID('asistencias') IS NULL
CREATE TABLE asistencias
(
	asistencia_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES empleados(empleado_id),
	fecha_asistencia DATE,
	hora_entrada TIME,
	hora_salida TIME
)
GO

IF OBJECT_ID('clientes') IS NULL
CREATE TABLE clientes
(
	cliente_id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_CLIENTES PRIMARY KEY,
	identificacion_cliente VARCHAR(30) NOT NULL CONSTRAINT UQ_CLIENTES_identificacion UNIQUE,
	tipo_identificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN'
		CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipo_identificacion IN ('RTN', 'Identidad')),
	nombre_empresa VARCHAR(50),
	nombre_cliente VARCHAR(50),
	apellido_cliente VARCHAR(50),
	telefono_cliente VARCHAR(50),
	correo_cliente VARCHAR(50),
	estado_cliente BIT NOT NULL CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1
)
GO

IF OBJECT_ID('locaciones') IS NULL
CREATE TABLE locaciones
(
	locacion_id INT PRIMARY KEY IDENTITY,
	nombre_locacion VARCHAR(60),
	direccion VARCHAR(80),
	tipo_locacion VARCHAR(25),
	identificacion_cliente VARCHAR(30) CONSTRAINT FK_LOCACIONES_CLIENTES REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE
)
GO

IF OBJECT_ID('servicios') IS NULL
CREATE TABLE servicios
(
	servicio_id INT PRIMARY KEY IDENTITY,
	nombre_servicio VARCHAR(30)
)
GO

IF OBJECT_ID('vehiculos') IS NULL
CREATE TABLE vehiculos
(
	vehiculo_id INT PRIMARY KEY IDENTITY,
	placa VARCHAR(15) UNIQUE,
	marca_vehiculo varchar(20),
	modelo_vehiculo Varchar(25),
	estado_vehiculo BIT NOT NULL CONSTRAINT DF_VEHICULOS_estadoVehiculo DEFAULT 1
)
GO

IF OBJECT_ID('estados_orden') IS NULL
CREATE TABLE estados_orden
(
	estado_orden_id INT PRIMARY KEY IDENTITY,
	nombre_estado_orden VARCHAR(25) UNIQUE
)
GO

IF OBJECT_ID('ordenes') IS NULL
CREATE TABLE ordenes
(
	orden_id VARCHAR(50) PRIMARY KEY NOT NULL,
	identificacion_cliente VARCHAR(30) CONSTRAINT FK_ORDENES_CLIENTES REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE,
	locacion_id INT FOREIGN KEY REFERENCES locaciones(locacion_id),
	servicio_id INT FOREIGN KEY REFERENCES servicios(servicio_id),
	supervisor_id INT FOREIGN KEY REFERENCES empleados(empleado_id),
	vehiculo_id INT FOREIGN KEY REFERENCES vehiculos(vehiculo_id),
	fecha_solicitud DATE,
	fecha_servicio DATE,
	estado_orden_id INT FOREIGN KEY REFERENCES estados_orden(estado_orden_id),
	observacion VARCHAR(200),
	conformidad_cliente BIT
)
GO

IF OBJECT_ID('actividades') IS NULL
CREATE TABLE actividades
(
	actividad_id INT PRIMARY KEY IDENTITY,
	servicio_id INT FOREIGN KEY REFERENCES servicios(servicio_id),
	descripcion VARCHAR(50),
	frecuencia VARCHAR(50),
	precio MONEY
)
GO

IF OBJECT_ID('proveedores') IS NULL
CREATE TABLE proveedores
(
	proveedor_id INT PRIMARY KEY IDENTITY,
	nombre_proveedor VARCHAR(50),
	telefono_proveedor VARCHAR(50),
	correo_proveedor VARCHAR(50)
)
GO

IF OBJECT_ID('productos') IS NULL
CREATE TABLE productos
(
	producto_id INT PRIMARY KEY IDENTITY,
	nombre_producto VARCHAR(50),
	tipo_producto VARCHAR(20),
	stock INT
)
GO

IF OBJECT_ID('compras') IS NULL
CREATE TABLE compras
(
	compra_id INT PRIMARY KEY IDENTITY,
	proveedor_id INT FOREIGN KEY REFERENCES proveedores(proveedor_id),
	firma_empleado_id INT FOREIGN KEY REFERENCES empleados(empleado_id),
	numero_factura VARCHAR(50) UNIQUE,
	fecha_compra DATE
)
GO

IF OBJECT_ID('compras_productos') IS NULL
CREATE TABLE compras_productos
(
	compra_producto_id INT IDENTITY CONSTRAINT PK_COMPRAS_PRODUCTOS PRIMARY KEY,
	compra_id INT NOT NULL FOREIGN KEY REFERENCES compras(compra_id),
	producto_id INT FOREIGN KEY REFERENCES productos(producto_id),
	cantidad INT,
	precio_unitario MONEY
)
GO

IF OBJECT_ID('estados_equipo') IS NULL
CREATE TABLE estados_equipo
(
	estado_equipo_id INT PRIMARY KEY IDENTITY,
	nombre_estado_equipo VARCHAR(30) UNIQUE
)
GO

IF OBJECT_ID('equipos') IS NULL
CREATE TABLE equipos
(
	equipo_id INT PRIMARY KEY IDENTITY,
	nombre_equipo VARCHAR(50),
	tipo_equipo VARCHAR(50),
	marca_equipo VARCHAR(50),
	modelo_equipo VARCHAR(50),
	estado_equipo_id INT FOREIGN KEY REFERENCES estados_equipo(estado_equipo_id)
)
GO

IF OBJECT_ID('orden_empleados') IS NULL
CREATE TABLE orden_empleados
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	empleado_id INT NOT NULL FOREIGN KEY REFERENCES empleados(empleado_id),
	turno VARCHAR(20),
	piso_area VARCHAR(20),
	CONSTRAINT PK_ORDEN_EMPLEADOS PRIMARY KEY (orden_id, empleado_id)
)
GO

IF OBJECT_ID('orden_productos') IS NULL
CREATE TABLE orden_productos
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	producto_id INT NOT NULL FOREIGN KEY REFERENCES productos(producto_id),
	cantidad INT,
	CONSTRAINT PK_ORDEN_PRODUCTOS PRIMARY KEY (orden_id, producto_id)
)
GO

IF OBJECT_ID('orden_equipos') IS NULL
CREATE TABLE orden_equipos
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	equipo_id INT NOT NULL FOREIGN KEY REFERENCES equipos(equipo_id),
	fecha_asignacion DATE,
	devuelto BIT,
	CONSTRAINT PK_ORDEN_EQUIPOS PRIMARY KEY (orden_id, equipo_id)
)
GO

IF OBJECT_ID('orden_actividades') IS NULL
CREATE TABLE orden_actividades
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	estado BIT,
	duracion_min INT,
	CONSTRAINT PK_ORDEN_ACTIVIDADES PRIMARY KEY (orden_id, actividad_id)
)
GO

IF OBJECT_ID('formas_pago') IS NULL
CREATE TABLE formas_pago
(
	forma_pago_id INT PRIMARY KEY IDENTITY,
	nombre_forma_pago VARCHAR(30) UNIQUE
)
GO

IF OBJECT_ID('facturas') IS NULL
CREATE TABLE facturas
(
	factura_id INT PRIMARY KEY IDENTITY,
	orden_id VARCHAR(50) FOREIGN KEY REFERENCES ordenes(orden_id) UNIQUE,
	fecha_factura DATE,
	tipo_pago VARCHAR(10) NOT NULL DEFAULT 'Contado'
		CHECK (tipo_pago IN ('Contado', 'Credito')),
	estado_pago VARCHAR(50) DEFAULT 'Pendiente'
		CHECK (estado_pago IN ('Pendiente', 'Pagada')),
	fecha_pago DATE,
	forma_pago_id INT FOREIGN KEY REFERENCES formas_pago(forma_pago_id),
	precio_facturado MONEY
)
GO

IF OBJECT_ID('cotizaciones') IS NULL
CREATE TABLE cotizaciones
(
	cotizacion_id VARCHAR(50) PRIMARY KEY NOT NULL,
	identificacion_cliente VARCHAR(30),
	nombre_empresa VARCHAR(50),
	nombre_cliente VARCHAR(50),
	apellido_cliente VARCHAR(50),
	telefono_cliente VARCHAR(50),
	correo_cliente VARCHAR(50),
	locacion_id INT NULL FOREIGN KEY REFERENCES locaciones(locacion_id),
	direccion VARCHAR(80),
	fecha_cotizacion DATE,
	valida_hasta DATE,
	observacion VARCHAR(200),
	precio_cotizado MONEY
)
GO

IF OBJECT_ID('detalles_cotizacion') IS NULL
CREATE TABLE detalles_cotizacion
(
	cotizacion_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES cotizaciones(cotizacion_id),
	linea INT NOT NULL,
	concepto VARCHAR(150) NOT NULL,
	cantidad INT NOT NULL,
	precio_unitario MONEY NOT NULL,
	CONSTRAINT PK_COTIZACION_DETALLE PRIMARY KEY (cotizacion_id, linea)
)
GO

IF COL_LENGTH('ordenes', 'cotizacion_id') IS NULL
	ALTER TABLE ordenes ADD cotizacion_id VARCHAR(50) NULL;
GO

IF OBJECT_ID('FK_ORDENES_COTIZACIONES') IS NULL
	ALTER TABLE ordenes
	ADD CONSTRAINT FK_ORDENES_COTIZACIONES FOREIGN KEY (cotizacion_id) REFERENCES cotizaciones(cotizacion_id);
GO

IF OBJECT_ID('detalles_factura') IS NULL
CREATE TABLE detalles_factura
(
	factura_id INT NOT NULL FOREIGN KEY REFERENCES facturas(factura_id),
	linea INT NOT NULL,
	concepto VARCHAR(150) NOT NULL,
	cantidad INT NOT NULL,
	precio_unitario MONEY NOT NULL,
	adicional BIT NOT NULL DEFAULT 0,
	CONSTRAINT PK_FACTURA_DETALLE PRIMARY KEY (factura_id, linea)
)
GO

IF OBJECT_ID('compras_equipos') IS NULL
CREATE TABLE compras_equipos
(
	compra_equipo_id INT IDENTITY CONSTRAINT PK_COMPRAS_EQUIPOS PRIMARY KEY,
	compra_id INT NOT NULL FOREIGN KEY REFERENCES compras(compra_id),
	equipo_id INT FOREIGN KEY REFERENCES equipos(equipo_id),
	precio_unitario MONEY
)
GO

IF OBJECT_ID('cuentas_por_cobrar') IS NULL
CREATE TABLE cuentas_por_cobrar
(
	cuenta_id INT PRIMARY KEY IDENTITY,
	factura_id INT FOREIGN KEY REFERENCES facturas(factura_id) NOT NULL UNIQUE,
	monto_total MONEY NOT NULL,
	prima MONEY,
	numero_cuotas INT NOT NULL,
	frecuencia VARCHAR(20),
	fecha_inicio DATE,
	estado_cuenta VARCHAR(20) DEFAULT 'Vigente'
		CHECK (estado_cuenta IN ('Vigente', 'Pagada', 'Vencida'))
)
GO

IF OBJECT_ID('cuotas') IS NULL
CREATE TABLE cuotas
(
	cuota_id INT PRIMARY KEY IDENTITY,
	cuenta_id INT FOREIGN KEY REFERENCES cuentas_por_cobrar(cuenta_id) NOT NULL,
	numero_cuota INT NOT NULL,
	fecha_vencimiento DATE NOT NULL,
	monto_cuota MONEY NOT NULL,
	monto_pagado MONEY DEFAULT 0,
	fecha_pago DATE,
	forma_pago_id INT FOREIGN KEY REFERENCES formas_pago(forma_pago_id),
	estado_cuota VARCHAR(20) DEFAULT 'Pendiente'
		CHECK (estado_cuota IN ('Pendiente', 'Parcial', 'Pagada', 'Vencida')),
	CONSTRAINT UQ_CUOTAS_cuenta_numero UNIQUE (cuenta_id, numero_cuota)
)
GO

IF COL_LENGTH('actividades', 'unidad') IS NULL
	ALTER TABLE actividades ADD unidad VARCHAR(20) NULL;
GO

IF COL_LENGTH('actividades', 'rendimiento') IS NULL
	ALTER TABLE actividades ADD rendimiento DECIMAL(10, 2) NULL;
GO

IF OBJECT_ID('parametros_costeo') IS NULL
CREATE TABLE parametros_costeo
(
	clave VARCHAR(40) PRIMARY KEY,
	valor DECIMAL(14, 4) NOT NULL,
	descripcion VARCHAR(150) NOT NULL
)
GO

IF OBJECT_ID('actividad_rangos') IS NULL
CREATE TABLE actividad_rangos
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	desde DECIMAL(12, 2) NOT NULL,
	hasta DECIMAL(12, 2) NULL,
	cantidad_personas INT NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_RANGOS PRIMARY KEY (actividad_id, desde)
)
GO

IF OBJECT_ID('actividad_productos') IS NULL
CREATE TABLE actividad_productos
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	producto_id INT NOT NULL FOREIGN KEY REFERENCES productos(producto_id),
	cantidad_por_unidad DECIMAL(10, 4) NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_PRODUCTOS PRIMARY KEY (actividad_id, producto_id)
)
GO

IF OBJECT_ID('actividad_equipos') IS NULL
CREATE TABLE actividad_equipos
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	nombre_equipo VARCHAR(50) NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_EQUIPOS PRIMARY KEY (actividad_id, nombre_equipo)
)
GO

IF COL_LENGTH('cotizaciones', 'servicio_id') IS NULL
	ALTER TABLE cotizaciones ADD servicio_id INT NULL;
GO

IF OBJECT_ID('FK_COTIZACIONES_SERVICIOS') IS NULL
	ALTER TABLE cotizaciones
	ADD CONSTRAINT FK_COTIZACIONES_SERVICIOS FOREIGN KEY (servicio_id) REFERENCES servicios(servicio_id);
GO

IF COL_LENGTH('detalles_cotizacion', 'actividad_id') IS NULL
	ALTER TABLE detalles_cotizacion ADD actividad_id INT NULL;
GO

IF COL_LENGTH('detalles_cotizacion', 'medida') IS NULL
	ALTER TABLE detalles_cotizacion ADD medida DECIMAL(12, 2) NULL;
GO

IF COL_LENGTH('detalles_cotizacion', 'cantidad_personas') IS NULL
	ALTER TABLE detalles_cotizacion ADD cantidad_personas INT NULL;
GO

IF OBJECT_ID('FK_COTIZACION_DETALLE_ACTIVIDADES') IS NULL
	ALTER TABLE detalles_cotizacion
	ADD CONSTRAINT FK_COTIZACION_DETALLE_ACTIVIDADES FOREIGN KEY (actividad_id) REFERENCES actividades(actividad_id);
GO

IF OBJECT_ID('abonos') IS NULL
CREATE TABLE abonos
(
	abono_id INT PRIMARY KEY IDENTITY,
	factura_id INT NOT NULL FOREIGN KEY REFERENCES facturas(factura_id),
	fecha_abono DATE NOT NULL,
	monto MONEY NOT NULL,
	forma_pago_id INT FOREIGN KEY REFERENCES formas_pago(forma_pago_id),
	saldo_despues MONEY NOT NULL
)
GO

IF COL_LENGTH('abonos', 'concepto') IS NULL
	ALTER TABLE abonos
	ADD concepto VARCHAR(20) NOT NULL
		CONSTRAINT DF_ABONOS_concepto DEFAULT 'Abono';
GO

IF COL_LENGTH('clientes', 'cliente_id') IS NULL AND COL_LENGTH('clientes', 'identificacion_cliente') IS NOT NULL
BEGIN
	DECLARE @sql NVARCHAR(MAX) = N'';

	SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id))
				+ N' DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.foreign_keys
	WHERE referenced_object_id = OBJECT_ID('clientes');

	SELECT @sql = @sql + N'ALTER TABLE clientes DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.key_constraints
	WHERE parent_object_id = OBJECT_ID('clientes') AND type = 'PK';

	SET @sql = @sql + N'ALTER TABLE clientes ADD cliente_id INT IDENTITY(1, 1) NOT NULL; '
			 + N'ALTER TABLE clientes ADD CONSTRAINT PK_CLIENTES PRIMARY KEY (cliente_id); '
			 + N'ALTER TABLE clientes ADD CONSTRAINT UQ_CLIENTES_identificacion UNIQUE (identificacion_cliente); '
			 + N'ALTER TABLE locaciones ADD CONSTRAINT FK_LOCACIONES_CLIENTES '
			 + N'FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE; '
			 + N'ALTER TABLE ordenes ADD CONSTRAINT FK_ORDENES_CLIENTES '
			 + N'FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE; ';

	EXEC sp_executesql @sql;
END
GO

IF COL_LENGTH('clientes', 'tipo_identificacion') IS NULL
	ALTER TABLE clientes
	ADD tipo_identificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN';
GO

IF OBJECT_ID('CK_CLIENTES_tipoIdentificacion') IS NULL
	ALTER TABLE clientes
	ADD CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipo_identificacion IN ('RTN', 'Identidad'));
GO

-- Los numeros de identidad y RTN se guardan sin guiones ni espacios (corrige datos viejos)
UPDATE cotizaciones
SET identificacion_cliente = REPLACE(REPLACE(identificacion_cliente, '-', ''), ' ', '')
WHERE identificacion_cliente LIKE '%[- ]%';
GO

UPDATE clientes
SET identificacion_cliente = REPLACE(REPLACE(identificacion_cliente, '-', ''), ' ', '')
WHERE identificacion_cliente LIKE '%[- ]%'
  AND NOT EXISTS (SELECT 1 FROM clientes x
				  WHERE x.identificacion_cliente = REPLACE(REPLACE(clientes.identificacion_cliente, '-', ''), ' ', '')
					AND x.cliente_id <> clientes.cliente_id);
GO

UPDATE empleados
SET identidad = REPLACE(REPLACE(identidad, '-', ''), ' ', '')
WHERE identidad LIKE '%[- ]%'
  AND NOT EXISTS (SELECT 1 FROM empleados x
				  WHERE x.identidad = REPLACE(REPLACE(empleados.identidad, '-', ''), ' ', '')
					AND x.empleado_id <> empleados.empleado_id);
GO

UPDATE clientes
SET tipo_identificacion = 'Identidad'
WHERE tipo_identificacion = 'RTN'
  AND LEN(identificacion_cliente) = 13
  AND identificacion_cliente NOT LIKE '%[^0-9]%';
GO

IF COL_LENGTH('empleados', 'huella_id') IS NOT NULL
BEGIN
	DECLARE @quitar NVARCHAR(MAX) = N'';

	SELECT @quitar = @quitar + N'ALTER TABLE empleados DROP CONSTRAINT ' + QUOTENAME(kc.name) + N'; '
	FROM sys.key_constraints kc
	INNER JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
	INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
	WHERE kc.parent_object_id = OBJECT_ID('empleados') AND c.name = 'huella_id';

	SET @quitar = @quitar + N'ALTER TABLE empleados DROP COLUMN huella_id; ';

	EXEC sp_executesql @quitar;
END
GO

-- Tipos de tabla que usan los procedimientos

IF TYPE_ID('dbo.tipo_lista_empleados') IS NULL
	CREATE TYPE dbo.tipo_lista_empleados AS TABLE (empleado_id INT NOT NULL PRIMARY KEY);
GO

IF TYPE_ID('dbo.tipo_ingreso_productos') IS NULL
	CREATE TYPE dbo.tipo_ingreso_productos AS TABLE
	(
		producto_id INT,
		cantidad INT,
		precio_unitario MONEY
	);
GO

IF TYPE_ID('dbo.tipo_ingreso_equipos') IS NULL
	CREATE TYPE dbo.tipo_ingreso_equipos AS TABLE
	(
		nombre_equipo VARCHAR(50),
		tipo_equipo VARCHAR(50),
		marca_equipo VARCHAR(50),
		modelo_equipo VARCHAR(50),
		cantidad INT,
		precio_unitario MONEY
	);
GO

IF TYPE_ID('dbo.tipo_lista_medidas') IS NULL
	CREATE TYPE dbo.tipo_lista_medidas AS TABLE
	(
		actividad_id INT NOT NULL,
		medida DECIMAL(12, 2) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_rangos') IS NULL
	CREATE TYPE dbo.tipo_actividad_rangos AS TABLE
	(
		desde DECIMAL(12, 2) NOT NULL,
		hasta DECIMAL(12, 2) NULL,
		cantidad_personas INT NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_productos') IS NULL
	CREATE TYPE dbo.tipo_actividad_productos AS TABLE
	(
		producto_id INT NOT NULL PRIMARY KEY,
		cantidad_por_unidad DECIMAL(10, 4) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_productos_nuevos') IS NULL
	CREATE TYPE dbo.tipo_actividad_productos_nuevos AS TABLE
	(
		nombre_producto VARCHAR(50) NOT NULL,
		tipo_producto VARCHAR(20) NOT NULL,
		cantidad_por_unidad DECIMAL(10, 4) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_equipos') IS NULL
	CREATE TYPE dbo.tipo_actividad_equipos AS TABLE
	(
		nombre_equipo VARCHAR(50) NOT NULL PRIMARY KEY
	);
GO

IF TYPE_ID('dbo.tipo_producto_actividades') IS NULL
	CREATE TYPE dbo.tipo_producto_actividades AS TABLE
	(
		actividad_id INT NOT NULL PRIMARY KEY,
		cantidad_por_unidad DECIMAL(10, 4) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_equipo_actividades') IS NULL
	CREATE TYPE dbo.tipo_equipo_actividades AS TABLE
	(
		actividad_id INT NOT NULL PRIMARY KEY
	);
GO

-- Datos iniciales

INSERT INTO formas_pago (nombre_forma_pago)
SELECT v.nombre
FROM (VALUES ('Efectivo'), ('Tarjeta'), ('Transferencia'), ('Cheque')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM formas_pago f WHERE f.nombre_forma_pago = v.nombre);
GO

INSERT INTO roles (nombre_rol)
SELECT v.nombre
FROM (VALUES ('Usuario Básico'), ('Usuario Medio'), ('Usuario Avanzado')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM roles r WHERE r.nombre_rol = v.nombre);
GO

IF NOT EXISTS (SELECT 1 FROM cargos)
	INSERT INTO cargos (nombre_cargo) VALUES ('Administrador'), ('Operario'), ('Supervisor');
GO

INSERT INTO estados_orden (nombre_estado_orden)
SELECT v.nombre
FROM (VALUES ('Pendiente'), ('En proceso'), ('Completada'), ('Cancelada')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM estados_orden e WHERE e.nombre_estado_orden = v.nombre);
GO

IF NOT EXISTS (SELECT 1 FROM estados_equipo WHERE nombre_estado_equipo = 'Disponible')
	INSERT INTO estados_equipo (nombre_estado_equipo) VALUES ('Disponible');
GO

INSERT INTO parametros_costeo (clave, valor, descripcion)
SELECT v.clave, v.valor, v.descripcion
FROM (VALUES
	('SALARIO_MINIMO', 12500, 'Salario minimo mensual en Honduras (L). Ajustar al vigente'),
	('CARGAS_SOCIALES', 0.3033, 'IHSS patronal 7%, RAP 1.5%, INFOP 1%, decimo tercer y cuarto mes, vacaciones'),
	('HORAS_MES', 190, 'Horas de trabajo al mes (jornada de 44 horas semanales)'),
	('FACTOR_SUPERVISOR', 1.5, 'El supervisor gana este multiplo del salario minimo'),
	('CUADRILLAS_POR_SUPERVISOR', 5, 'Cuadrillas que atiende el supervisor al mismo tiempo'),
	('LITROS_GASOLINA_VIAJE', 6, 'Litros de gasolina estandar para ir y volver del servicio'),
	('PRECIO_LITRO_GASOLINA', 100, 'Precio del litro de gasolina (L)'),
	('VIDA_UTIL_EQUIPO_MESES', 36, 'Meses de vida util del equipo'),
	('MANTENIMIENTO_EQUIPO_MENSUAL', 0.01, 'Mantenimiento mensual del equipo (fraccion de su precio)'),
	('GASTOS_ADMINISTRATIVOS', 0.10, 'Gastos administrativos sobre el costo directo'),
	('MARGEN_UTILIDAD', 0.50, 'Margen de utilidad sobre el precio de venta'),
	('ISV', 0.15, 'Impuesto sobre ventas de Honduras')
) v(clave, valor, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM parametros_costeo p WHERE p.clave = v.clave);
GO

DECLARE @servicios TABLE (nombre VARCHAR(30));
DECLARE @act TABLE (servicio VARCHAR(30), descripcion VARCHAR(50), unidad VARCHAR(20), rendimiento DECIMAL(10, 2), perfil VARCHAR(5));
DECLARE @rango TABLE (perfil VARCHAR(5), desde DECIMAL(12, 2), hasta DECIMAL(12, 2), cantidad_personas INT);
DECLARE @prod TABLE (nombre VARCHAR(50), tipo VARCHAR(20), stock INT, costo MONEY);
DECLARE @eq TABLE (nombre VARCHAR(50), tipo VARCHAR(50), marca VARCHAR(50), modelo VARCHAR(50), unidades INT, costo MONEY);
DECLARE @ap TABLE (actividad VARCHAR(50), producto VARCHAR(50), cantidad DECIMAL(10, 4));
DECLARE @ae TABLE (actividad VARCHAR(50), equipo VARCHAR(50));

INSERT INTO @act VALUES
	('Limpieza general', 'Barrido y trapeado de pisos', 'm²', 25, 'M2'),
	('Limpieza general', 'Limpieza de ventanas y vidrios', 'm²', 15, 'VID'),
	('Limpieza general', 'Limpieza de paredes y zócalos', 'm²', 20, 'M2'),
	('Limpieza general', 'Desempolvado de techos y esquinas', 'm²', 30, 'M2'),
	('Limpieza general', 'Recolección y retiro de basura', 'm²', 80, 'M2'),
	('Limpieza general', 'Limpieza y desinfección de baños', 'baños', 1.5, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de escritorios', 'escritorios', 8, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de equipo de cómputo', 'equipos', 6, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de sillas y mobiliario', 'muebles', 10, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de salas de reuniones', 'salas', 1, 'UNI'),
	('Limpieza de oficinas', 'Vaciado de papeleras', 'papeleras', 40, 'UNI'),
	('Limpieza de tanques', 'Limpieza de tanque de 1,000 litros', 'tanques', 0.5, 'TNQ'),
	('Limpieza de tanques', 'Limpieza de tanque de 5,000 litros', 'tanques', 0.2, 'TNQ'),
	('Limpieza de tanques', 'Limpieza de tanque de 10,000 litros', 'tanques', 0.1, 'TNQ'),
	('Pisos y alfombras', 'Pulido y encerado de pisos', 'm²', 12, 'M2'),
	('Pisos y alfombras', 'Lavado de alfombras', 'm²', 10, 'M2'),
	('Pisos y alfombras', 'Lavado a presión de áreas exteriores', 'm²', 20, 'M2');

INSERT INTO @rango VALUES
	('M2', 0.01, 50, 1), ('M2', 51, 150, 2), ('M2', 151, 400, 3), ('M2', 401, 1000, 4), ('M2', 1001, NULL, 6),
	('VID', 0.01, 30, 1), ('VID', 31, 100, 2), ('VID', 101, 300, 3), ('VID', 301, 800, 4), ('VID', 801, NULL, 6),
	('UNI', 0.01, 10, 1), ('UNI', 11, 30, 2), ('UNI', 31, 100, 3), ('UNI', 101, NULL, 4),
	('TNQ', 0.01, 2, 2), ('TNQ', 3, 6, 3), ('TNQ', 7, NULL, 4);

INSERT INTO @prod VALUES
	('Desinfectante multiusos (litro)', 'Químico', 200, 45),
	('Detergente líquido (litro)', 'Químico', 200, 38),
	('Limpiavidrios (litro)', 'Químico', 150, 55),
	('Desengrasante (litro)', 'Químico', 100, 60),
	('Cloro (litro)', 'Químico', 300, 20),
	('Cera para piso (litro)', 'Químico', 100, 120),
	('Shampoo para alfombras (litro)', 'Químico', 100, 90),
	('Alcohol isopropílico (litro)', 'Químico', 100, 70),
	('Bolsas para basura (unidad)', 'Consumible', 1000, 3),
	('Paños de microfibra (unidad)', 'Consumible', 500, 25),
	('Esponja de limpieza (unidad)', 'Consumible', 500, 12);

INSERT INTO @eq VALUES
	('Aspiradora industrial', 'Eléctrico', 'Karcher', 'NT 70', 3, 9500),
	('Trapeador industrial con balde', 'Manual', 'Rubbermaid', 'Pro', 3, 1800),
	('Carro de limpieza', 'Manual', 'Rubbermaid', 'Pro', 3, 4500),
	('Escalera telescópica', 'Manual', 'Werner', 'Telescópica', 2, 3200),
	('Kit rasqueta para vidrios', 'Manual', 'Unger', 'Profesional', 3, 900),
	('Hidrolavadora', 'Eléctrico', 'Karcher', 'K5', 2, 7500),
	('Pulidora de pisos', 'Eléctrico', 'Hako', 'Industrial', 2, 18000),
	('Extractor de alfombras', 'Eléctrico', 'Karcher', 'Puzzi', 2, 14000),
	('Bomba de agua sumergible', 'Eléctrico', 'Truper', '1 HP', 2, 3500);

INSERT INTO @ap VALUES
	('Barrido y trapeado de pisos', 'Desinfectante multiusos (litro)', 0.01),
	('Barrido y trapeado de pisos', 'Detergente líquido (litro)', 0.005),
	('Barrido y trapeado de pisos', 'Paños de microfibra (unidad)', 0.002),
	('Limpieza de ventanas y vidrios', 'Limpiavidrios (litro)', 0.008),
	('Limpieza de ventanas y vidrios', 'Paños de microfibra (unidad)', 0.01),
	('Limpieza de paredes y zócalos', 'Desinfectante multiusos (litro)', 0.006),
	('Limpieza de paredes y zócalos', 'Esponja de limpieza (unidad)', 0.004),
	('Desempolvado de techos y esquinas', 'Paños de microfibra (unidad)', 0.005),
	('Recolección y retiro de basura', 'Bolsas para basura (unidad)', 0.02),
	('Limpieza y desinfección de baños', 'Cloro (litro)', 0.15),
	('Limpieza y desinfección de baños', 'Desinfectante multiusos (litro)', 0.2),
	('Limpieza y desinfección de baños', 'Esponja de limpieza (unidad)', 0.5),
	('Limpieza y desinfección de baños', 'Bolsas para basura (unidad)', 1),
	('Limpieza de escritorios', 'Desinfectante multiusos (litro)', 0.02),
	('Limpieza de escritorios', 'Paños de microfibra (unidad)', 0.1),
	('Limpieza de equipo de cómputo', 'Alcohol isopropílico (litro)', 0.03),
	('Limpieza de equipo de cómputo', 'Paños de microfibra (unidad)', 0.1),
	('Limpieza de sillas y mobiliario', 'Desinfectante multiusos (litro)', 0.03),
	('Limpieza de sillas y mobiliario', 'Paños de microfibra (unidad)', 0.1),
	('Limpieza de salas de reuniones', 'Desinfectante multiusos (litro)', 0.15),
	('Limpieza de salas de reuniones', 'Paños de microfibra (unidad)', 1),
	('Limpieza de salas de reuniones', 'Bolsas para basura (unidad)', 1),
	('Vaciado de papeleras', 'Bolsas para basura (unidad)', 1),
	('Limpieza de tanque de 1,000 litros', 'Cloro (litro)', 1.5),
	('Limpieza de tanque de 1,000 litros', 'Detergente líquido (litro)', 2),
	('Limpieza de tanque de 1,000 litros', 'Esponja de limpieza (unidad)', 3),
	('Limpieza de tanque de 5,000 litros', 'Cloro (litro)', 6),
	('Limpieza de tanque de 5,000 litros', 'Detergente líquido (litro)', 5),
	('Limpieza de tanque de 5,000 litros', 'Esponja de limpieza (unidad)', 5),
	('Limpieza de tanque de 10,000 litros', 'Cloro (litro)', 12),
	('Limpieza de tanque de 10,000 litros', 'Detergente líquido (litro)', 10),
	('Limpieza de tanque de 10,000 litros', 'Esponja de limpieza (unidad)', 8),
	('Pulido y encerado de pisos', 'Cera para piso (litro)', 0.02),
	('Pulido y encerado de pisos', 'Detergente líquido (litro)', 0.005),
	('Pulido y encerado de pisos', 'Paños de microfibra (unidad)', 0.005),
	('Lavado de alfombras', 'Shampoo para alfombras (litro)', 0.02),
	('Lavado de alfombras', 'Paños de microfibra (unidad)', 0.004),
	('Lavado a presión de áreas exteriores', 'Detergente líquido (litro)', 0.01),
	('Lavado a presión de áreas exteriores', 'Desengrasante (litro)', 0.005);

INSERT INTO @ae VALUES
	('Barrido y trapeado de pisos', 'Trapeador industrial con balde'),
	('Barrido y trapeado de pisos', 'Carro de limpieza'),
	('Barrido y trapeado de pisos', 'Aspiradora industrial'),
	('Limpieza de ventanas y vidrios', 'Escalera telescópica'),
	('Limpieza de ventanas y vidrios', 'Kit rasqueta para vidrios'),
	('Limpieza de paredes y zócalos', 'Escalera telescópica'),
	('Limpieza de paredes y zócalos', 'Carro de limpieza'),
	('Desempolvado de techos y esquinas', 'Escalera telescópica'),
	('Desempolvado de techos y esquinas', 'Aspiradora industrial'),
	('Recolección y retiro de basura', 'Carro de limpieza'),
	('Limpieza y desinfección de baños', 'Carro de limpieza'),
	('Limpieza de escritorios', 'Carro de limpieza'),
	('Limpieza de sillas y mobiliario', 'Carro de limpieza'),
	('Limpieza de salas de reuniones', 'Carro de limpieza'),
	('Limpieza de salas de reuniones', 'Aspiradora industrial'),
	('Vaciado de papeleras', 'Carro de limpieza'),
	('Limpieza de tanque de 1,000 litros', 'Bomba de agua sumergible'),
	('Limpieza de tanque de 1,000 litros', 'Hidrolavadora'),
	('Limpieza de tanque de 5,000 litros', 'Bomba de agua sumergible'),
	('Limpieza de tanque de 5,000 litros', 'Hidrolavadora'),
	('Limpieza de tanque de 10,000 litros', 'Bomba de agua sumergible'),
	('Limpieza de tanque de 10,000 litros', 'Hidrolavadora'),
	('Pulido y encerado de pisos', 'Pulidora de pisos'),
	('Pulido y encerado de pisos', 'Aspiradora industrial'),
	('Lavado de alfombras', 'Extractor de alfombras'),
	('Lavado a presión de áreas exteriores', 'Hidrolavadora');

INSERT INTO @servicios SELECT DISTINCT servicio FROM @act;

INSERT INTO servicios (nombre_servicio)
SELECT s.nombre FROM @servicios s
WHERE NOT EXISTS (SELECT 1 FROM servicios x WHERE x.nombre_servicio = s.nombre);

INSERT INTO actividades (servicio_id, descripcion, frecuencia, precio, unidad, rendimiento)
SELECT sv.servicio_id, a.descripcion, NULL, NULL, a.unidad, a.rendimiento
FROM @act a
INNER JOIN servicios sv ON sv.nombre_servicio = a.servicio
WHERE NOT EXISTS (SELECT 1 FROM actividades x WHERE x.descripcion = a.descripcion);

UPDATE x
SET x.unidad = a.unidad, x.rendimiento = a.rendimiento
FROM actividades x
INNER JOIN @act a ON a.descripcion = x.descripcion
WHERE x.rendimiento IS NULL;

INSERT INTO actividad_rangos (actividad_id, desde, hasta, cantidad_personas)
SELECT x.actividad_id, r.desde, r.hasta, r.cantidad_personas
FROM @act a
INNER JOIN actividades x ON x.descripcion = a.descripcion
INNER JOIN @rango r ON r.perfil = a.perfil
WHERE NOT EXISTS (SELECT 1 FROM actividad_rangos y WHERE y.actividad_id = x.actividad_id);

IF NOT EXISTS (SELECT 1 FROM compras WHERE numero_factura = 'INVENTARIO-INICIAL')
BEGIN
	DECLARE @nuevos_p TABLE (producto_id INT, nombre VARCHAR(50));
	DECLARE @nuevos_e TABLE (equipo_id INT, nombre VARCHAR(50));
	DECLARE @proveedor INT;
	DECLARE @compra INT;
	DECLARE @disponible INT = (SELECT TOP 1 estado_equipo_id FROM estados_equipo WHERE nombre_estado_equipo = 'Disponible');

	IF NOT EXISTS (SELECT 1 FROM proveedores WHERE nombre_proveedor = 'Proveedor inicial')
		INSERT INTO proveedores (nombre_proveedor, telefono_proveedor, correo_proveedor) VALUES ('Proveedor inicial', '', '');

	SELECT TOP 1 @proveedor = proveedor_id FROM proveedores WHERE nombre_proveedor = 'Proveedor inicial';

	INSERT INTO compras (proveedor_id, firma_empleado_id, numero_factura, fecha_compra)
	VALUES (@proveedor, NULL, 'INVENTARIO-INICIAL', CAST(GETDATE() AS DATE));

	SET @compra = CAST(SCOPE_IDENTITY() AS INT);

	INSERT INTO productos (nombre_producto, tipo_producto, stock)
	OUTPUT inserted.producto_id, inserted.nombre_producto INTO @nuevos_p
	SELECT p.nombre, p.tipo, p.stock FROM @prod p
	WHERE NOT EXISTS (SELECT 1 FROM productos x WHERE x.nombre_producto = p.nombre);

	INSERT INTO compras_productos (compra_id, producto_id, cantidad, precio_unitario)
	SELECT @compra, n.producto_id, p.stock, p.costo
	FROM @nuevos_p n INNER JOIN @prod p ON p.nombre = n.nombre;

	INSERT INTO equipos (nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo, estado_equipo_id)
	OUTPUT inserted.equipo_id, inserted.nombre_equipo INTO @nuevos_e
	SELECT e.nombre, e.tipo, e.marca, e.modelo, @disponible
	FROM @eq e
	INNER JOIN (VALUES (1), (2), (3), (4)) n(numero) ON n.numero <= e.unidades
	WHERE NOT EXISTS (SELECT 1 FROM equipos x WHERE x.nombre_equipo = e.nombre);

	INSERT INTO compras_equipos (compra_id, equipo_id, precio_unitario)
	SELECT @compra, n.equipo_id, e.costo
	FROM @nuevos_e n INNER JOIN @eq e ON e.nombre = n.nombre;
END

INSERT INTO actividad_productos (actividad_id, producto_id, cantidad_por_unidad)
SELECT x.actividad_id, p.producto_id, a.cantidad
FROM @ap a
INNER JOIN actividades x ON x.descripcion = a.actividad
INNER JOIN productos p ON p.nombre_producto = a.producto
WHERE NOT EXISTS (SELECT 1 FROM actividad_productos y WHERE y.actividad_id = x.actividad_id);

INSERT INTO actividad_equipos (actividad_id, nombre_equipo)
SELECT x.actividad_id, a.equipo
FROM @ae a
INNER JOIN actividades x ON x.descripcion = a.actividad
WHERE NOT EXISTS (SELECT 1 FROM actividad_equipos y WHERE y.actividad_id = x.actividad_id);
GO

-- Actualización de bases creadas con versiones anteriores (si la base ya está al día, no cambia nada)

IF COL_LENGTH('clientes', 'estado_cliente') IS NULL
	ALTER TABLE clientes
	ADD estado_cliente BIT NOT NULL
		CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1;
GO

IF COL_LENGTH('clientes', 'locacion_id') IS NOT NULL
BEGIN
	IF EXISTS (SELECT 1 FROM clientes
			   WHERE identificacion_cliente IS NULL OR LTRIM(RTRIM(identificacion_cliente)) = '')
	BEGIN
		RAISERROR('Hay clientes sin RTN / identidad. Póngales uno antes de ejecutar este script.', 16, 1);
		SET NOEXEC ON;
	END
END
GO

IF COL_LENGTH('clientes', 'locacion_id') IS NOT NULL
BEGIN
	SET XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRANSACTION;

		-- Nuevas columnas
		EXEC('ALTER TABLE locaciones ADD identificacion_cliente VARCHAR(30) NULL');
		EXEC('ALTER TABLE ordenes ADD identificacion_cliente VARCHAR(30) NULL');

		-- Pasar los datos que ya existen
		EXEC('UPDATE l SET l.identificacion_cliente = c.identificacion_cliente
			  FROM locaciones l
			  INNER JOIN clientes c ON c.locacion_id = l.locacion_id');

		EXEC('UPDATE o SET o.identificacion_cliente = c.identificacion_cliente
			  FROM ordenes o
			  INNER JOIN clientes c ON c.cliente_id = o.cliente_id');

		-- Quitar las llaves viejas de CLIENTES y las que apuntan a CLIENTES
		DECLARE @sql NVARCHAR(MAX) = N'';

		SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id))
					+ N' DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
		FROM sys.foreign_keys
		WHERE referenced_object_id = OBJECT_ID('clientes')
		   OR parent_object_id = OBJECT_ID('clientes');

		SELECT @sql = @sql + N'ALTER TABLE clientes DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
		FROM sys.key_constraints
		WHERE parent_object_id = OBJECT_ID('clientes');

		EXEC sp_executesql @sql;

		-- Borrar las columnas que ya no se usan
		EXEC('ALTER TABLE ordenes DROP COLUMN cliente_id');
		EXEC('ALTER TABLE clientes DROP COLUMN locacion_id');
		EXEC('ALTER TABLE clientes DROP COLUMN cliente_id');

		-- Nueva llave primaria y llaves foráneas
		EXEC('ALTER TABLE clientes ALTER COLUMN identificacion_cliente VARCHAR(30) NOT NULL');
		EXEC('ALTER TABLE clientes ADD CONSTRAINT PK_CLIENTES PRIMARY KEY (identificacion_cliente)');
		EXEC('ALTER TABLE locaciones ADD CONSTRAINT FK_LOCACIONES_CLIENTES
			  FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente)');
		EXEC('ALTER TABLE ordenes ADD CONSTRAINT FK_ORDENES_CLIENTES
			  FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente)');

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END
GO

IF COL_LENGTH('empleados', 'estado_empleado') IS NULL
	ALTER TABLE empleados
	ADD estado_empleado BIT NOT NULL
		CONSTRAINT DF_EMPLEADOS_estadoEmpleado DEFAULT 1;
GO

IF COL_LENGTH('vehiculos', 'estado_vehiculo') IS NULL
	ALTER TABLE vehiculos
	ADD estado_vehiculo BIT NOT NULL
		CONSTRAINT DF_VEHICULOS_estadoVehiculo DEFAULT 1;
GO

IF COL_LENGTH('ordenes', 'observacion') < 200
	ALTER TABLE ordenes ALTER COLUMN observacion VARCHAR(200) NULL;
GO

DECLARE @detalle TABLE (tabla SYSNAME, columna SYSNAME);
INSERT INTO @detalle VALUES ('orden_actividades', 'actividad_id'),
							('orden_empleados', 'empleado_id'),
							('orden_productos', 'producto_id'),
							('orden_equipos', 'equipo_id');

DECLARE @tabla SYSNAME, @columna SYSNAME, @pk SYSNAME, @sql NVARCHAR(MAX);

DECLARE cursor_detalle CURSOR LOCAL FAST_FORWARD FOR
	SELECT tabla, columna FROM @detalle;

OPEN cursor_detalle;
FETCH NEXT FROM cursor_detalle INTO @tabla, @columna;

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @pk = NULL;

	SELECT @pk = kc.name
	FROM sys.key_constraints kc
	WHERE kc.parent_object_id = OBJECT_ID(@tabla)
	  AND kc.type = 'PK'
	  AND (SELECT COUNT(1) FROM sys.index_columns ic
		   WHERE ic.object_id = kc.parent_object_id
			 AND ic.index_id = kc.unique_index_id) = 1;

	IF @pk IS NOT NULL
	BEGIN
		SET @sql = 'ALTER TABLE ' + @tabla + ' DROP CONSTRAINT ' + QUOTENAME(@pk);
		EXEC (@sql);

		SET @sql = 'ALTER TABLE ' + @tabla + ' ALTER COLUMN ' + @columna + ' INT NOT NULL';
		EXEC (@sql);

		SET @sql = 'ALTER TABLE ' + @tabla + ' ADD CONSTRAINT ' + QUOTENAME('PK_' + @tabla)
				 + ' PRIMARY KEY (orden_id, ' + @columna + ')';
		EXEC (@sql);
	END

	FETCH NEXT FROM cursor_detalle INTO @tabla, @columna;
END

CLOSE cursor_detalle;
DEALLOCATE cursor_detalle;
GO

IF COL_LENGTH('orden_actividades', 'orden_actividad_id') IS NOT NULL
	ALTER TABLE orden_actividades DROP COLUMN orden_actividad_id;
IF COL_LENGTH('orden_empleados', 'orden_empleado_id') IS NOT NULL
	ALTER TABLE orden_empleados DROP COLUMN orden_empleado_id;
IF COL_LENGTH('orden_productos', 'orden_producto_id') IS NOT NULL
	ALTER TABLE orden_productos DROP COLUMN orden_producto_id;
IF COL_LENGTH('orden_equipos', 'orden_equipo_id') IS NOT NULL
	ALTER TABLE orden_equipos DROP COLUMN orden_equipo_id;
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints k
		   INNER JOIN sys.index_columns ic ON ic.object_id = k.parent_object_id AND ic.index_id = k.unique_index_id
		   INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
		   WHERE k.parent_object_id = OBJECT_ID('compras_productos') AND k.type = 'PK' AND c.name = 'compra_id')
   AND COL_LENGTH('compras_productos', 'compra_producto_id') IS NULL
BEGIN
	DECLARE @pk_productos SYSNAME = (SELECT name FROM sys.key_constraints
									WHERE parent_object_id = OBJECT_ID('compras_productos') AND type = 'PK');
	EXEC('ALTER TABLE compras_productos DROP CONSTRAINT ' + @pk_productos);
	EXEC('ALTER TABLE compras_productos ADD compra_producto_id INT IDENTITY(1,1)
		  CONSTRAINT PK_COMPRAS_PRODUCTOS PRIMARY KEY');
END
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints k
		   INNER JOIN sys.index_columns ic ON ic.object_id = k.parent_object_id AND ic.index_id = k.unique_index_id
		   INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
		   WHERE k.parent_object_id = OBJECT_ID('compras_equipos') AND k.type = 'PK' AND c.name = 'compra_id')
   AND COL_LENGTH('compras_equipos', 'compra_equipo_id') IS NULL
BEGIN
	DECLARE @pk_equipos SYSNAME = (SELECT name FROM sys.key_constraints
								  WHERE parent_object_id = OBJECT_ID('compras_equipos') AND type = 'PK');
	EXEC('ALTER TABLE compras_equipos DROP CONSTRAINT ' + @pk_equipos);
	EXEC('ALTER TABLE compras_equipos ADD compra_equipo_id INT IDENTITY(1,1)
		  CONSTRAINT PK_COMPRAS_EQUIPOS PRIMARY KEY');
END
GO

IF COL_LENGTH('usuarios', 'passkey_id') IS NULL
	ALTER TABLE usuarios ADD passkey_id VARBINARY(1023) NULL;
GO

IF COL_LENGTH('usuarios', 'passkey_clave') IS NULL
	ALTER TABLE usuarios ADD passkey_clave VARBINARY(1024) NULL;
GO

IF COL_LENGTH('usuarios', 'rol_id') IS NULL
	ALTER TABLE usuarios
	ADD rol_id INT NULL
		CONSTRAINT FK_USUARIOS_ROLES REFERENCES roles(rol_id);
GO

IF COL_LENGTH('usuarios', 'estado_usuario') IS NULL
	ALTER TABLE usuarios
	ADD estado_usuario BIT NOT NULL
		CONSTRAINT DF_USUARIOS_estadoUsuario DEFAULT 1;
GO

IF COL_LENGTH('usuarios', 'foto_perfil') IS NULL
	ALTER TABLE usuarios ADD foto_perfil VARBINARY(MAX) NULL;
GO

DROP PROCEDURE IF EXISTS sp_ListarProductosEquipos;
GO

-- Clientes con llave propia y huella_id eliminada (otra vez, por si la conversión anterior se ejecutó arriba)

IF COL_LENGTH('clientes', 'cliente_id') IS NULL AND COL_LENGTH('clientes', 'identificacion_cliente') IS NOT NULL
BEGIN
	DECLARE @sql NVARCHAR(MAX) = N'';

	SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id))
				+ N' DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.foreign_keys
	WHERE referenced_object_id = OBJECT_ID('clientes');

	SELECT @sql = @sql + N'ALTER TABLE clientes DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.key_constraints
	WHERE parent_object_id = OBJECT_ID('clientes') AND type = 'PK';

	SET @sql = @sql + N'ALTER TABLE clientes ADD cliente_id INT IDENTITY(1, 1) NOT NULL; '
			 + N'ALTER TABLE clientes ADD CONSTRAINT PK_CLIENTES PRIMARY KEY (cliente_id); '
			 + N'ALTER TABLE clientes ADD CONSTRAINT UQ_CLIENTES_identificacion UNIQUE (identificacion_cliente); '
			 + N'ALTER TABLE locaciones ADD CONSTRAINT FK_LOCACIONES_CLIENTES '
			 + N'FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE; '
			 + N'ALTER TABLE ordenes ADD CONSTRAINT FK_ORDENES_CLIENTES '
			 + N'FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE; ';

	EXEC sp_executesql @sql;
END
GO

IF COL_LENGTH('clientes', 'tipo_identificacion') IS NULL
	ALTER TABLE clientes
	ADD tipo_identificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN';
GO

IF OBJECT_ID('CK_CLIENTES_tipoIdentificacion') IS NULL
	ALTER TABLE clientes
	ADD CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipo_identificacion IN ('RTN', 'Identidad'));
GO

-- Los numeros de identidad y RTN se guardan sin guiones ni espacios (corrige datos viejos)
UPDATE cotizaciones
SET identificacion_cliente = REPLACE(REPLACE(identificacion_cliente, '-', ''), ' ', '')
WHERE identificacion_cliente LIKE '%[- ]%';
GO

UPDATE clientes
SET identificacion_cliente = REPLACE(REPLACE(identificacion_cliente, '-', ''), ' ', '')
WHERE identificacion_cliente LIKE '%[- ]%'
  AND NOT EXISTS (SELECT 1 FROM clientes x
				  WHERE x.identificacion_cliente = REPLACE(REPLACE(clientes.identificacion_cliente, '-', ''), ' ', '')
					AND x.cliente_id <> clientes.cliente_id);
GO

UPDATE empleados
SET identidad = REPLACE(REPLACE(identidad, '-', ''), ' ', '')
WHERE identidad LIKE '%[- ]%'
  AND NOT EXISTS (SELECT 1 FROM empleados x
				  WHERE x.identidad = REPLACE(REPLACE(empleados.identidad, '-', ''), ' ', '')
					AND x.empleado_id <> empleados.empleado_id);
GO

UPDATE clientes
SET tipo_identificacion = 'Identidad'
WHERE tipo_identificacion = 'RTN'
  AND LEN(identificacion_cliente) = 13
  AND identificacion_cliente NOT LIKE '%[^0-9]%';
GO

IF COL_LENGTH('empleados', 'huella_id') IS NOT NULL
BEGIN
	DECLARE @quitar NVARCHAR(MAX) = N'';

	SELECT @quitar = @quitar + N'ALTER TABLE empleados DROP CONSTRAINT ' + QUOTENAME(kc.name) + N'; '
	FROM sys.key_constraints kc
	INNER JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
	INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
	WHERE kc.parent_object_id = OBJECT_ID('empleados') AND c.name = 'huella_id';

	SET @quitar = @quitar + N'ALTER TABLE empleados DROP COLUMN huella_id; ';

	EXEC sp_executesql @quitar;
END
GO

-- Login y recuperación de contraseña

CREATE OR ALTER PROCEDURE sp_ObtenerUsuarioLogin
	@usuario VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT u.usuario_id,
		   u.empleado_id,
		   u.usuario,
		   u.contrasena_hash,
		   u.passkey_id,
		   u.passkey_clave,
		   e.nombre_empleado,
		   e.apellido_empleado,
		   c.nombre_cargo,
		   ISNULL(r.nombre_rol, '') AS rol
	FROM usuarios u
	INNER JOIN empleados e ON e.empleado_id = u.empleado_id
	LEFT JOIN cargos c ON c.cargo_id = e.cargo_id
	LEFT JOIN roles r ON r.rol_id = u.rol_id
	WHERE u.usuario = @usuario;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarUsuarioRecuperacion
	@usuario VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT u.usuario_id,
		   e.nombre_empleado,
		   e.correo_empleado
	FROM usuarios u
	INNER JOIN empleados e ON e.empleado_id = u.empleado_id
	WHERE u.usuario = @usuario;
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarContrasena
	@usuario_id INT,
	@nuevo_hash VARCHAR(255),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	IF @nuevo_hash IS NULL OR LTRIM(RTRIM(@nuevo_hash)) = ''
	BEGIN
		SET @mensaje = 'La contrasena no puede estar vacia.';
		RETURN;
	END

	UPDATE usuarios
	SET contrasena_hash = @nuevo_hash
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = 'La contrasena se cambio correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarContrasenaPorCorreo
	@correo VARCHAR(50),
	@contrasena_hash VARCHAR(255),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @correo = LTRIM(RTRIM(ISNULL(@correo, '')));

	DECLARE @cantidad INT;

	SELECT @cantidad = COUNT(1)
	FROM usuarios u
	INNER JOIN empleados e ON e.empleado_id = u.empleado_id
	WHERE e.correo_empleado = @correo;

	IF @cantidad = 0
	BEGIN
		SET @mensaje = 'No existe un usuario asociado a ese correo.';
		RETURN;
	END

	IF @cantidad > 1
	BEGIN
		SET @mensaje = 'Hay mas de un usuario con ese correo. Contacta a soporte.';
		RETURN;
	END

	UPDATE u
	SET u.contrasena_hash = @contrasena_hash
	FROM usuarios u
	INNER JOIN empleados e ON e.empleado_id = u.empleado_id
	WHERE e.correo_empleado = @correo;

	SET @resultado = 1;
	SET @mensaje = 'Contrasena cambiada correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_GuardarPasskey
	@usuario_id INT,
	@passkey_id VARBINARY(1023),
	@passkey_clave VARBINARY(1024),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM usuarios WHERE usuario_id = @usuario_id AND passkey_id IS NOT NULL)
	BEGIN
		SET @mensaje = 'Este usuario ya tiene un teléfono registrado.';
		RETURN;
	END

	UPDATE usuarios
	SET passkey_id = @passkey_id,
		passkey_clave = @passkey_clave
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = 'Teléfono registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarPasskey
	@usuario VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario = @usuario)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	UPDATE usuarios
	SET passkey_id = NULL,
		passkey_clave = NULL
	WHERE usuario = @usuario;

	SET @resultado = 1;
	SET @mensaje = 'Teléfono eliminado. Al entrar deberá registrar uno nuevo.';
END
GO

CREATE OR ALTER PROCEDURE sp_ExisteCorreoEmpleado
	@correo VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT(1) AS cantidad
	FROM empleados
	WHERE correo_empleado = LTRIM(RTRIM(@correo));
END
GO

-- Usuarios

CREATE OR ALTER PROCEDURE sp_ListarRoles
AS
BEGIN
	SET NOCOUNT ON;

	SELECT rol_id, nombre_rol
	FROM roles
	ORDER BY rol_id;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarUsuarios
	@texto VARCHAR(50) = NULL,
	@buscar_por VARCHAR(10) = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscar_por = UPPER(LTRIM(RTRIM(ISNULL(@buscar_por, ''))));

	SELECT u.usuario_id, u.empleado_id, u.usuario, u.rol_id, u.estado_usuario,
		   e.nombre_empleado, e.apellido_empleado, e.correo_empleado, e.telefono_empleado,
		   r.nombre_rol AS rol,
		   CASE WHEN u.estado_usuario = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM usuarios u
	INNER JOIN empleados e ON e.empleado_id = u.empleado_id
	LEFT JOIN roles r ON r.rol_id = u.rol_id
	WHERE (@estado IS NULL OR u.estado_usuario = @estado)
	  AND (@texto = ''
		   OR (@buscar_por = 'NOMBRE'
			   AND (e.nombre_empleado LIKE '%' + @texto + '%'
					OR e.apellido_empleado LIKE '%' + @texto + '%'
					OR ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '') LIKE '%' + @texto + '%'))
		   OR (@buscar_por = 'USUARIO'
			   AND u.usuario LIKE '%' + @texto + '%')
		   OR (@buscar_por = 'CORREO'
			   AND e.correo_empleado LIKE '%' + @texto + '%')
		   OR (@buscar_por = 'ROL'
			   AND r.nombre_rol LIKE '%' + @texto + '%')
		   OR (@buscar_por NOT IN ('NOMBRE', 'USUARIO', 'CORREO', 'ROL')
			   AND (u.usuario LIKE '%' + @texto + '%'
					OR e.nombre_empleado LIKE '%' + @texto + '%'
					OR e.apellido_empleado LIKE '%' + @texto + '%'
					OR e.correo_empleado LIKE '%' + @texto + '%'
					OR r.nombre_rol LIKE '%' + @texto + '%')))
	ORDER BY e.nombre_empleado, e.apellido_empleado;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarEmpleadosSinUsuario
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT e.empleado_id, e.nombre_empleado, e.apellido_empleado, e.identidad,
		   e.telefono_empleado, e.correo_empleado, c.nombre_cargo
	FROM empleados e
	LEFT JOIN cargos c ON c.cargo_id = e.cargo_id
	WHERE NOT EXISTS (SELECT 1 FROM usuarios u WHERE u.empleado_id = e.empleado_id)
	  AND (@texto = ''
		   OR e.identidad LIKE '%' + @texto + '%'
		   OR e.nombre_empleado LIKE '%' + @texto + '%'
		   OR e.apellido_empleado LIKE '%' + @texto + '%'
		   OR ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '') LIKE '%' + @texto + '%')
	ORDER BY e.nombre_empleado, e.apellido_empleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ExisteUsuario
	@usuario VARCHAR(50),
	@excluir_usuario_id INT = 0
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT(1) AS cantidad
	FROM usuarios
	WHERE usuario = LTRIM(RTRIM(@usuario))
	  AND usuario_id <> @excluir_usuario_id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarUsuario
	@empleado_id INT,
	@usuario VARCHAR(50),
	@contrasena_hash VARCHAR(255),
	@rol_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @usuario = LTRIM(RTRIM(ISNULL(@usuario, '')));

	IF @usuario = ''
	BEGIN
		SET @mensaje = 'El usuario es obligatorio.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM usuarios WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'Ese empleado ya tiene un usuario.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM usuarios WHERE usuario = @usuario)
	BEGIN
		SET @mensaje = 'Ya existe un usuario con ese nombre.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM roles WHERE rol_id = @rol_id)
	BEGIN
		SET @mensaje = 'El rol seleccionado no existe.';
		RETURN;
	END

	INSERT INTO usuarios (empleado_id, usuario, contrasena_hash, rol_id)
	VALUES (@empleado_id, @usuario, @contrasena_hash, @rol_id);

	SET @resultado = 1;
	SET @mensaje = 'Usuario creado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarUsuario
	@usuario_id INT,
	@usuario VARCHAR(50),
	@contrasena_hash VARCHAR(255) = NULL,
	@rol_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @usuario = LTRIM(RTRIM(ISNULL(@usuario, '')));

	IF NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	IF @usuario = ''
	BEGIN
		SET @mensaje = 'El usuario es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM usuarios
			   WHERE usuario = @usuario
				 AND usuario_id <> @usuario_id)
	BEGIN
		SET @mensaje = 'Ya existe otro usuario con ese nombre.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM roles WHERE rol_id = @rol_id)
	BEGIN
		SET @mensaje = 'El rol seleccionado no existe.';
		RETURN;
	END

	-- Si la contraseña viene vacia, se deja la que ya tenia
	UPDATE usuarios
	SET usuario = @usuario,
		rol_id = @rol_id,
		contrasena_hash = CASE WHEN ISNULL(@contrasena_hash, '') = ''
							   THEN contrasena_hash
							   ELSE @contrasena_hash END
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN ISNULL(@contrasena_hash, '') = ''
						THEN 'Usuario actualizado correctamente.'
						ELSE 'Usuario y contraseña actualizados correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoUsuario
	@usuario_id INT,
	@estado_usuario BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	UPDATE usuarios
	SET estado_usuario = @estado_usuario
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estado_usuario = 1
						THEN 'Usuario activado correctamente.'
						ELSE 'Usuario desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarUsuario
	@usuario_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM bitacoras WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: tiene registros en la bitácora. Desactívelo en su lugar.';
		RETURN;
	END

	BEGIN TRY
		DELETE FROM usuarios
		WHERE usuario_id = @usuario_id;

		SET @resultado = 1;
		SET @mensaje = 'Usuario eliminado correctamente.';
	END TRY
	BEGIN CATCH
		SET @resultado = 0;
		SET @mensaje = 'No se puede eliminar: tiene registros asociados. Desactívelo en su lugar.';
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerFotoUsuario
	@usuario_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT foto_perfil
	FROM usuarios
	WHERE usuario_id = @usuario_id;
END
GO

CREATE OR ALTER PROCEDURE sp_GuardarFotoUsuario
	@usuario_id INT,
	@foto VARBINARY(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE usuarios
	SET foto_perfil = @foto
	WHERE usuario_id = @usuario_id;
END
GO

-- Clientes

CREATE OR ALTER FUNCTION dbo.fn_ValidarIdentificacion (@tipo VARCHAR(10), @numero VARCHAR(30))
RETURNS VARCHAR(100)
AS
BEGIN
	DECLARE @n VARCHAR(30) = LTRIM(RTRIM(ISNULL(@numero, '')));

	IF @n = ''
		RETURN 'El RTN / identidad es obligatorio.';
	IF ISNULL(@tipo, '') NOT IN ('RTN', 'Identidad')
		RETURN 'Seleccione si es RTN o identidad.';
	IF @n LIKE '%[^0-9]%'
		RETURN 'Solo puede tener numeros, sin guiones ni espacios.';
	IF @n NOT LIKE '%[^0]%'
		RETURN 'El numero no puede ser todo ceros.';
	IF @tipo = 'Identidad' AND LEN(@n) <> 13
		RETURN 'La identidad debe tener 13 digitos.';
	IF @tipo = 'RTN' AND LEN(@n) <> 14
		RETURN 'El RTN debe tener 14 digitos.';

	RETURN '';
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarCliente
	@identificacion_cliente VARCHAR(30),
	@tipo_identificacion VARCHAR(10),
	@nombre_empresa VARCHAR(50),
	@nombre_cliente VARCHAR(50),
	@apellido_cliente VARCHAR(50),
	@telefono_cliente VARCHAR(50),
	@correo_cliente VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @identificacion_cliente = LTRIM(RTRIM(ISNULL(@identificacion_cliente, '')));
	SET @mensaje = dbo.fn_ValidarIdentificacion(@tipo_identificacion, @identificacion_cliente);

	IF @mensaje <> ''
		RETURN;

	IF EXISTS (SELECT 1 FROM clientes WHERE identificacion_cliente = @identificacion_cliente)
	BEGIN
		SET @mensaje = 'Ya existe un cliente con esa identificacion.';
		RETURN;
	END

	INSERT INTO clientes (identificacion_cliente, tipo_identificacion, nombre_empresa, nombre_cliente,
						  apellido_cliente, telefono_cliente, correo_cliente)
	VALUES (@identificacion_cliente, @tipo_identificacion, @nombre_empresa, @nombre_cliente,
			@apellido_cliente, @telefono_cliente, @correo_cliente);

	SET @resultado = 1;
	SET @mensaje = 'Cliente registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarCliente
	@cliente_id INT,
	@identificacion_cliente VARCHAR(30),
	@tipo_identificacion VARCHAR(10),
	@nombre_empresa VARCHAR(50),
	@nombre_cliente VARCHAR(50),
	@apellido_cliente VARCHAR(50),
	@telefono_cliente VARCHAR(50),
	@correo_cliente VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @anterior VARCHAR(30);
	SELECT @anterior = identificacion_cliente FROM clientes WHERE cliente_id = @cliente_id;

	IF @anterior IS NULL
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	SET @identificacion_cliente = LTRIM(RTRIM(ISNULL(@identificacion_cliente, '')));
	SET @mensaje = dbo.fn_ValidarIdentificacion(@tipo_identificacion, @identificacion_cliente);

	IF @mensaje <> ''
		RETURN;

	IF EXISTS (SELECT 1 FROM clientes
			   WHERE identificacion_cliente = @identificacion_cliente AND cliente_id <> @cliente_id)
	BEGIN
		SET @mensaje = 'Ya existe otro cliente con esa identificacion.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Las locaciones y las ordenes se actualizan solas (ON UPDATE CASCADE)
		UPDATE clientes
		SET identificacion_cliente = @identificacion_cliente,
			tipo_identificacion = @tipo_identificacion,
			nombre_empresa = @nombre_empresa,
			nombre_cliente = @nombre_cliente,
			apellido_cliente = @apellido_cliente,
			telefono_cliente = @telefono_cliente,
			correo_cliente = @correo_cliente
		WHERE cliente_id = @cliente_id;

		-- Las cotizaciones guardan la identificacion como texto
		IF @anterior <> @identificacion_cliente
			UPDATE cotizaciones
			SET identificacion_cliente = @identificacion_cliente
			WHERE identificacion_cliente = @anterior;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Cliente actualizado correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoCliente
	@identificacion_cliente VARCHAR(30),
	@estado_cliente BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM clientes WHERE identificacion_cliente = @identificacion_cliente)
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	UPDATE clientes
	SET estado_cliente = @estado_cliente
	WHERE identificacion_cliente = @identificacion_cliente;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estado_cliente = 1
						THEN 'Cliente activado correctamente.'
						ELSE 'Cliente desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarClientes
	@texto VARCHAR(50) = NULL,
	@buscar_por VARCHAR(10) = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscar_por = UPPER(LTRIM(RTRIM(ISNULL(@buscar_por, ''))));

	SELECT c.cliente_id, c.identificacion_cliente, c.tipo_identificacion, c.nombre_empresa, c.nombre_cliente,
		   c.apellido_cliente, c.telefono_cliente, c.correo_cliente, c.estado_cliente,
		   CASE WHEN c.estado_cliente = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM clientes c
	WHERE (@estado IS NULL OR c.estado_cliente = @estado)
	  AND (@texto = ''
		   OR (@buscar_por = 'RTN'
			   AND c.identificacion_cliente LIKE '%' + @texto + '%')
		   OR (@buscar_por = 'NOMBRE'
			   AND (c.nombre_empresa LIKE '%' + @texto + '%'
					OR c.nombre_cliente LIKE '%' + @texto + '%'
					OR c.apellido_cliente LIKE '%' + @texto + '%'
					OR ISNULL(c.nombre_cliente, '') + ' ' + ISNULL(c.apellido_cliente, '') LIKE '%' + @texto + '%'))
		   OR (@buscar_por NOT IN ('RTN', 'NOMBRE')
			   AND (c.identificacion_cliente LIKE '%' + @texto + '%'
					OR c.nombre_empresa LIKE '%' + @texto + '%'
					OR c.nombre_cliente LIKE '%' + @texto + '%'
					OR c.apellido_cliente LIKE '%' + @texto + '%')))
	ORDER BY c.nombre_empresa, c.nombre_cliente, c.apellido_cliente;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerCliente
	@identificacion_cliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.cliente_id, c.identificacion_cliente, c.tipo_identificacion, c.nombre_empresa, c.nombre_cliente,
		   c.apellido_cliente, c.telefono_cliente, c.correo_cliente, c.estado_cliente,
		   CASE WHEN c.estado_cliente = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM clientes c
	WHERE c.identificacion_cliente = @identificacion_cliente;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarOrdenesPorCliente
	@identificacion_cliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   s.nombre_servicio AS servicio,
		   ISNULL(o.fecha_servicio, o.fecha_solicitud) AS fecha,
		   e.nombre_estado_orden AS estado
	FROM ordenes o
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN estados_orden e ON e.estado_orden_id = o.estado_orden_id
	WHERE o.identificacion_cliente = @identificacion_cliente
	ORDER BY fecha DESC, o.orden_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarFacturasPorCliente
	@identificacion_cliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.factura_id,
		   f.fecha_factura AS fecha,
		   f.estado_pago,
		   f.precio_facturado AS monto
	FROM facturas f
	INNER JOIN ordenes o ON o.orden_id = f.orden_id
	WHERE o.identificacion_cliente = @identificacion_cliente
	ORDER BY f.fecha_factura DESC, f.factura_id DESC;
END
GO

-- Locaciones

CREATE OR ALTER PROCEDURE sp_ListarLocacionesPorCliente
	@identificacion_cliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT locacion_id, nombre_locacion, direccion, tipo_locacion, identificacion_cliente
	FROM locaciones
	WHERE identificacion_cliente = @identificacion_cliente
	ORDER BY nombre_locacion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarLocaciones
AS
BEGIN
	SET NOCOUNT ON;

	SELECT locacion_id, nombre_locacion, direccion, tipo_locacion, identificacion_cliente
	FROM locaciones
	ORDER BY nombre_locacion;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarLocacion
	@identificacion_cliente VARCHAR(30),
	@nombre_locacion VARCHAR(60),
	@direccion VARCHAR(80),
	@tipo_locacion VARCHAR(25) = NULL,
	@locacion_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @locacion_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM clientes WHERE identificacion_cliente = @identificacion_cliente)
	BEGIN
		SET @mensaje = 'El cliente de la locacion no existe.';
		RETURN;
	END

	IF @nombre_locacion IS NULL OR LTRIM(RTRIM(@nombre_locacion)) = ''
	BEGIN
		SET @mensaje = 'El nombre de la locacion es obligatorio.';
		RETURN;
	END

	INSERT INTO locaciones (identificacion_cliente, nombre_locacion, direccion, tipo_locacion)
	VALUES (@identificacion_cliente, @nombre_locacion, @direccion, @tipo_locacion);

	SET @locacion_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Locacion registrada correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarLocacion
	@locacion_id INT,
	@identificacion_cliente VARCHAR(30),
	@nombre_locacion VARCHAR(60),
	@direccion VARCHAR(80),
	@tipo_locacion VARCHAR(25) = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM locaciones WHERE locacion_id = @locacion_id)
	BEGIN
		SET @mensaje = 'La locacion no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM clientes WHERE identificacion_cliente = @identificacion_cliente)
	BEGIN
		SET @mensaje = 'El cliente de la locacion no existe.';
		RETURN;
	END

	UPDATE locaciones
	SET identificacion_cliente = @identificacion_cliente,
		nombre_locacion = @nombre_locacion,
		direccion = @direccion,
		tipo_locacion = @tipo_locacion
	WHERE locacion_id = @locacion_id;

	SET @resultado = 1;
	SET @mensaje = 'Locacion actualizada correctamente.';
END
GO

-- Empleados

CREATE OR ALTER PROCEDURE sp_ListarCargos
AS
BEGIN
	SET NOCOUNT ON;
	SELECT cargo_id, nombre_cargo FROM cargos ORDER BY nombre_cargo;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarEmpleados
	@texto VARCHAR(50) = '',
	@buscar_por VARCHAR(20) = '',
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscar_por = UPPER(LTRIM(RTRIM(ISNULL(@buscar_por, ''))));

	SELECT e.empleado_id, e.cargo_id, c.nombre_cargo,
		   e.nombre_empleado, e.apellido_empleado,
		   e.nombre_empleado + ' ' + e.apellido_empleado AS nombre_completo,
		   e.identidad, e.telefono_empleado, e.correo_empleado,
		   u.usuario, e.estado_empleado,
		   CASE WHEN e.estado_empleado = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM empleados e
	LEFT JOIN cargos c ON c.cargo_id = e.cargo_id
	LEFT JOIN usuarios u ON u.empleado_id = e.empleado_id
	WHERE (@estado IS NULL OR e.estado_empleado = @estado)
	  AND (@texto = ''
		   OR (@buscar_por = 'NOMBRE' AND (e.nombre_empleado + ' ' + e.apellido_empleado) LIKE '%' + @texto + '%')
		   OR (@buscar_por = 'IDENTIDAD' AND e.identidad LIKE '%' + @texto + '%')
		   OR (@buscar_por = 'CARGO' AND c.nombre_cargo LIKE '%' + @texto + '%')
		   OR (@buscar_por = ''
			   AND ((e.nombre_empleado + ' ' + e.apellido_empleado) LIKE '%' + @texto + '%'
				 OR e.identidad LIKE '%' + @texto + '%'
				 OR c.nombre_cargo LIKE '%' + @texto + '%'
				 OR e.correo_empleado LIKE '%' + @texto + '%'
				 OR u.usuario LIKE '%' + @texto + '%')))
	ORDER BY e.nombre_empleado, e.apellido_empleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerEmpleado
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id, e.cargo_id, c.nombre_cargo,
		   e.nombre_empleado, e.apellido_empleado, e.identidad,
		   e.telefono_empleado, e.correo_empleado,
		   u.usuario, e.estado_empleado,
		   CASE WHEN e.estado_empleado = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM empleados e
	LEFT JOIN cargos c ON c.cargo_id = e.cargo_id
	LEFT JOIN usuarios u ON u.empleado_id = e.empleado_id
	WHERE e.empleado_id = @empleado_id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarEmpleado
	@cargo_id INT,
	@nombre_empleado VARCHAR(30),
	@apellido_empleado VARCHAR(30),
	@identidad VARCHAR(30),
	@telefono_empleado VARCHAR(50) = NULL,
	@correo_empleado VARCHAR(50) = NULL,
	@empleado_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @empleado_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM cargos WHERE cargo_id = @cargo_id)
	BEGIN
		SET @mensaje = 'El cargo seleccionado no existe.';
		RETURN;
	END

	SET @identidad = REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@identidad, ''))), '-', ''), ' ', '');

	IF @identidad LIKE '%[^0-9]%' OR LEN(@identidad) <> 13
	BEGIN
		SET @mensaje = 'La identidad debe tener 13 digitos, sin guiones.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM empleados WHERE identidad = @identidad)
	BEGIN
		SET @mensaje = 'Ya existe un empleado con esa identidad.';
		RETURN;
	END

	INSERT INTO empleados (cargo_id, nombre_empleado, apellido_empleado, identidad,
						   telefono_empleado, correo_empleado)
	VALUES (@cargo_id, @nombre_empleado, @apellido_empleado, @identidad,
			NULLIF(@telefono_empleado, ''), NULLIF(@correo_empleado, ''));

	SET @empleado_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Empleado registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarEmpleado
	@empleado_id INT,
	@cargo_id INT,
	@nombre_empleado VARCHAR(30),
	@apellido_empleado VARCHAR(30),
	@identidad VARCHAR(30),
	@telefono_empleado VARCHAR(50) = NULL,
	@correo_empleado VARCHAR(50) = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM cargos WHERE cargo_id = @cargo_id)
	BEGIN
		SET @mensaje = 'El cargo seleccionado no existe.';
		RETURN;
	END

	SET @identidad = REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@identidad, ''))), '-', ''), ' ', '');

	IF @identidad LIKE '%[^0-9]%' OR LEN(@identidad) <> 13
	BEGIN
		SET @mensaje = 'La identidad debe tener 13 digitos, sin guiones.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM empleados WHERE identidad = @identidad AND empleado_id <> @empleado_id)
	BEGIN
		SET @mensaje = 'Ya existe otro empleado con esa identidad.';
		RETURN;
	END

	UPDATE empleados
	SET cargo_id = @cargo_id,
		nombre_empleado = @nombre_empleado,
		apellido_empleado = @apellido_empleado,
		identidad = @identidad,
		telefono_empleado = NULLIF(@telefono_empleado, ''),
		correo_empleado = NULLIF(@correo_empleado, '')
	WHERE empleado_id = @empleado_id;

	SET @resultado = 1;
	SET @mensaje = 'Empleado actualizado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoEmpleado
	@empleado_id INT,
	@estado_empleado BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	UPDATE empleados SET estado_empleado = @estado_empleado WHERE empleado_id = @empleado_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estado_empleado = 1 THEN 'Empleado activado correctamente.'
						ELSE 'Empleado desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_AsistenciasPorEmpleado
	@empleado_id INT,
	@desde DATE,
	@hasta DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT CONVERT(VARCHAR(10), a.fecha_asistencia, 103) AS fecha_asistencia,
		   CONVERT(VARCHAR(5), a.hora_entrada, 108) AS hora_entrada,
		   CONVERT(VARCHAR(5), a.hora_salida, 108) AS hora_salida,
		   CASE WHEN a.hora_entrada IS NOT NULL AND a.hora_salida IS NOT NULL
				THEN CAST(DATEDIFF(MINUTE, a.hora_entrada, a.hora_salida) / 60.0 AS DECIMAL(6, 2))
				ELSE 0 END AS horas
	FROM asistencias a
	WHERE a.empleado_id = @empleado_id
	  AND a.fecha_asistencia BETWEEN @desde AND @hasta
	ORDER BY a.fecha_asistencia;
END
GO

CREATE OR ALTER PROCEDURE sp_OrdenesPorEmpleado
	@empleado_id INT,
	@desde DATE,
	@hasta DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   ISNULL(c.nombre_empresa, o.identificacion_cliente) AS cliente,
		   s.nombre_servicio,
		   CONVERT(VARCHAR(10), o.fecha_servicio, 103) AS fecha_servicio,
		   oe.turno, oe.piso_area
	FROM orden_empleados oe
	INNER JOIN ordenes o ON o.orden_id = oe.orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	WHERE oe.empleado_id = @empleado_id
	  AND o.fecha_servicio BETWEEN @desde AND @hasta
	ORDER BY o.fecha_servicio, o.orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarEmpleado
	@empleado_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM orden_empleados WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: esta asignado a ordenes. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM ordenes WHERE supervisor_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: es supervisor de ordenes. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM asistencias WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: tiene asistencias registradas. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM compras WHERE firma_empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: firmo ingresos de inventario. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM usuarios WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: tiene un usuario del sistema. Desactivelo.';
		RETURN;
	END

	BEGIN TRY
		DELETE FROM empleados WHERE empleado_id = @empleado_id;

		SET @resultado = 1;
		SET @mensaje = 'Empleado eliminado correctamente.';
	END TRY
	BEGIN CATCH
		-- Cualquier otra tabla ligada al empleado: la llave foranea lo impide
		SET @resultado = 0;
		SET @mensaje = 'No se puede eliminar: tiene registros asociados. Desactivelo.';
	END CATCH
END
GO

-- Asistencia

CREATE OR ALTER PROCEDURE sp_RegistrarAsistencia
	@identidad VARCHAR(30),
	@tipo VARCHAR(10) OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @tipo = '';
	SET @resultado = 0;
	SET @mensaje = '';

	SET @identidad = REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@identidad, ''))), '-', ''), ' ', '');

	DECLARE @empleado_id INT, @activo BIT;

	SELECT @empleado_id = empleado_id, @activo = estado_empleado
	FROM empleados
	WHERE identidad = @identidad;

	IF @empleado_id IS NULL
	BEGIN
		SET @mensaje = 'La identidad no esta registrada.';
		RETURN;
	END

	IF @activo = 0
	BEGIN
		SET @mensaje = 'El empleado esta inactivo.';
		RETURN;
	END

	DECLARE @hoy DATE = CAST(GETDATE() AS DATE),
			@ahora TIME(0) = CAST(GETDATE() AS TIME(0)),
			@asistencia_id INT,
			@salida TIME;

	SELECT TOP 1 @asistencia_id = asistencia_id, @salida = hora_salida
	FROM asistencias
	WHERE empleado_id = @empleado_id AND fecha_asistencia = @hoy
	ORDER BY asistencia_id DESC;

	IF @asistencia_id IS NULL
	BEGIN
		INSERT INTO asistencias (empleado_id, fecha_asistencia, hora_entrada, hora_salida)
		VALUES (@empleado_id, @hoy, @ahora, NULL);

		SET @tipo = 'ENTRADA';
		SET @resultado = 1;
		SET @mensaje = 'Entrada registrada.';
	END
	ELSE IF @salida IS NULL
	BEGIN
		UPDATE asistencias SET hora_salida = @ahora WHERE asistencia_id = @asistencia_id;

		SET @tipo = 'SALIDA';
		SET @resultado = 1;
		SET @mensaje = 'Salida registrada.';
	END
	ELSE
	BEGIN
		SET @mensaje = 'Ya registro entrada y salida hoy.';
	END
END
GO

-- Mi trabajo (órdenes del empleado)

CREATE OR ALTER PROCEDURE sp_MisOrdenes
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   COALESCE(NULLIF(c.nombre_empresa, ''),
					NULLIF(LTRIM(ISNULL(c.nombre_cliente, '') + ' ' + ISNULL(c.apellido_cliente, '')), ''),
					o.identificacion_cliente) AS cliente,
		   ISNULL(s.nombre_servicio, 'Sin servicio') AS nombre_servicio,
		   COALESCE(o.fecha_servicio, o.fecha_solicitud) AS fecha,
		   ISNULL(eo.nombre_estado_orden, 'Sin estado') AS estado
	FROM orden_empleados oe
	INNER JOIN ordenes o ON o.orden_id = oe.orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE oe.empleado_id = @empleado_id
	ORDER BY COALESCE(o.fecha_servicio, o.fecha_solicitud) DESC, o.orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleMiOrden
	@orden_id VARCHAR(50),
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   COALESCE(NULLIF(c.nombre_empresa, ''),
					NULLIF(LTRIM(ISNULL(c.nombre_cliente, '') + ' ' + ISNULL(c.apellido_cliente, '')), ''),
					o.identificacion_cliente) AS cliente,
		   ISNULL(s.nombre_servicio, '') AS servicio,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   ISNULL(eo.nombre_estado_orden, 'Sin estado') AS estado,
		   ISNULL(l.nombre_locacion, '') AS locacion,
		   ISNULL(l.direccion, '') AS direccion,
		   CASE WHEN v.vehiculo_id IS NULL THEN ''
				ELSE ISNULL(v.placa, '') + ' - ' + ISNULL(v.marca_vehiculo, '') + ' ' + ISNULL(v.modelo_vehiculo, '')
		   END AS vehiculo,
		   LTRIM(ISNULL(sup.nombre_empleado, '') + ' ' + ISNULL(sup.apellido_empleado, '')) AS supervisor,
		   ISNULL(oe.turno, '') AS turno,
		   ISNULL(oe.piso_area, '') AS piso_area,
		   ISNULL(o.observacion, '') AS observacion
	FROM orden_empleados oe
	INNER JOIN ordenes o ON o.orden_id = oe.orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	LEFT JOIN locaciones l ON l.locacion_id = o.locacion_id
	LEFT JOIN vehiculos v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN empleados sup ON sup.empleado_id = o.supervisor_id
	WHERE oe.orden_id = @orden_id
	  AND oe.empleado_id = @empleado_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ListasMiOrden
	@orden_id VARCHAR(50),
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM orden_empleados WHERE orden_id = @orden_id AND empleado_id = @empleado_id)
		RETURN;

	SELECT 1 AS orden,
		   'Equipo de trabajo' AS seccion,
		   LTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')) AS titulo,
		   ISNULL(cg.nombre_cargo, '')
			 + CASE WHEN ISNULL(oe.turno, '') = '' THEN '' ELSE ' - Turno: ' + oe.turno END
			 + CASE WHEN ISNULL(oe.piso_area, '') = '' THEN '' ELSE ' - ' + oe.piso_area END AS detalle
	FROM orden_empleados oe
	INNER JOIN empleados e ON e.empleado_id = oe.empleado_id
	LEFT JOIN cargos cg ON cg.cargo_id = e.cargo_id
	WHERE oe.orden_id = @orden_id
	  AND oe.empleado_id <> @empleado_id

	UNION ALL

	SELECT 2,
		   'Actividades',
		   ISNULL(a.descripcion, ''),
		   CASE WHEN ISNULL(a.frecuencia, '') = '' THEN '' ELSE a.frecuencia + ' - ' END
			 + CASE WHEN oa.estado = 1 THEN 'Realizada' ELSE 'Pendiente' END
	FROM orden_actividades oa
	INNER JOIN actividades a ON a.actividad_id = oa.actividad_id
	WHERE oa.orden_id = @orden_id

	UNION ALL

	SELECT 3,
		   'Equipos asignados',
		   ISNULL(q.nombre_equipo, ''),
		   LTRIM(ISNULL(q.marca_equipo, '') + ' ' + ISNULL(q.modelo_equipo, ''))
			 + CASE WHEN oq.devuelto = 1 THEN ' - Devuelto' ELSE ' - En uso' END
	FROM orden_equipos oq
	INNER JOIN equipos q ON q.equipo_id = oq.equipo_id
	WHERE oq.orden_id = @orden_id

	UNION ALL

	SELECT 4,
		   'Productos',
		   ISNULL(p.nombre_producto, ''),
		   'Cantidad: ' + CAST(ISNULL(op.cantidad, 0) AS VARCHAR(10))
	FROM orden_productos op
	INNER JOIN productos p ON p.producto_id = op.producto_id
	WHERE op.orden_id = @orden_id

	ORDER BY orden, titulo;
END
GO

-- Vehículos

CREATE OR ALTER PROCEDURE sp_InsertarVehiculo
	@placa VARCHAR(15),
	@marca_vehiculo VARCHAR(20),
	@modelo_vehiculo VARCHAR(25),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @placa = UPPER(LTRIM(RTRIM(ISNULL(@placa, ''))));

	IF @placa = ''
	BEGIN
		SET @mensaje = 'La placa es obligatoria.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM vehiculos WHERE placa = @placa)
	BEGIN
		SET @mensaje = 'Ya existe un vehiculo con esa placa.';
		RETURN;
	END

	INSERT INTO vehiculos (placa, marca_vehiculo, modelo_vehiculo)
	VALUES (@placa, @marca_vehiculo, @modelo_vehiculo);

	SET @resultado = 1;
	SET @mensaje = 'Vehiculo registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarVehiculo
	@vehiculo_id INT,
	@placa VARCHAR(15),
	@marca_vehiculo VARCHAR(20),
	@modelo_vehiculo VARCHAR(25),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @placa = UPPER(LTRIM(RTRIM(ISNULL(@placa, ''))));

	IF NOT EXISTS (SELECT 1 FROM vehiculos WHERE vehiculo_id = @vehiculo_id)
	BEGIN
		SET @mensaje = 'El vehiculo no existe.';
		RETURN;
	END

	IF @placa = ''
	BEGIN
		SET @mensaje = 'La placa es obligatoria.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM vehiculos WHERE placa = @placa AND vehiculo_id <> @vehiculo_id)
	BEGIN
		SET @mensaje = 'Ya existe otro vehiculo con esa placa.';
		RETURN;
	END

	UPDATE vehiculos
	SET placa = @placa,
		marca_vehiculo = @marca_vehiculo,
		modelo_vehiculo = @modelo_vehiculo
	WHERE vehiculo_id = @vehiculo_id;

	SET @resultado = 1;
	SET @mensaje = 'Vehiculo actualizado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoVehiculo
	@vehiculo_id INT,
	@estado_vehiculo BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM vehiculos WHERE vehiculo_id = @vehiculo_id)
	BEGIN
		SET @mensaje = 'El vehiculo no existe.';
		RETURN;
	END

	UPDATE vehiculos
	SET estado_vehiculo = @estado_vehiculo
	WHERE vehiculo_id = @vehiculo_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estado_vehiculo = 1
						THEN 'Vehiculo activado correctamente.'
						ELSE 'Vehiculo desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarVehiculos
	@texto VARCHAR(50) = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT v.vehiculo_id, v.placa, v.marca_vehiculo, v.modelo_vehiculo, v.estado_vehiculo,
		   CASE WHEN v.estado_vehiculo = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM vehiculos v
	WHERE (@estado IS NULL OR v.estado_vehiculo = @estado)
	  AND (@texto = ''
		   OR v.placa LIKE '%' + @texto + '%'
		   OR v.marca_vehiculo LIKE '%' + @texto + '%'
		   OR v.modelo_vehiculo LIKE '%' + @texto + '%')
	ORDER BY v.placa;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerVehiculo
	@vehiculo_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT v.vehiculo_id, v.placa, v.marca_vehiculo, v.modelo_vehiculo, v.estado_vehiculo,
		   CASE WHEN v.estado_vehiculo = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM vehiculos v
	WHERE v.vehiculo_id = @vehiculo_id;
END
GO

-- Órdenes

CREATE OR ALTER PROCEDURE sp_ListarEstadosOrden
AS
BEGIN
	SET NOCOUNT ON;

	SELECT estado_orden_id, nombre_estado_orden
	FROM estados_orden
	ORDER BY estado_orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarServicios
AS
BEGIN
	SET NOCOUNT ON;

	SELECT servicio_id, nombre_servicio
	FROM servicios
	ORDER BY nombre_servicio;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarActividades
AS
BEGIN
	SET NOCOUNT ON;

	SELECT actividad_id,
		   ISNULL(servicio_id, 0) AS servicio_id,
		   ISNULL(descripcion, '') +
		   CASE WHEN ISNULL(frecuencia, '') = '' THEN '' ELSE ' (' + frecuencia + ')' END AS texto,
		   ISNULL(descripcion, '') AS nombre,
		   ISNULL(unidad, 'unidad') AS unidad
	FROM actividades
	ORDER BY descripcion;
END
GO

-- Un empleado (o supervisor) que esta en una orden "En proceso" no se puede asignar a otra.
CREATE OR ALTER FUNCTION dbo.fn_EmpleadoEnProceso (@empleado_id INT, @orden_excluir VARCHAR(50))
RETURNS BIT
AS
BEGIN
	IF EXISTS (SELECT 1
			   FROM ordenes o
			   INNER JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
			   WHERE eo.nombre_estado_orden = 'En proceso'
				 AND o.orden_id <> ISNULL(@orden_excluir, '')
				 AND (o.supervisor_id = @empleado_id
					  OR EXISTS (SELECT 1 FROM orden_empleados oe
								 WHERE oe.orden_id = o.orden_id AND oe.empleado_id = @empleado_id)))
		RETURN 1;

	RETURN 0;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosOrden
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id,
		   LTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')) AS nombre_completo
	FROM empleados e
	WHERE e.estado_empleado = 1
	  AND dbo.fn_EmpleadoEnProceso(e.empleado_id, '') = 0
	ORDER BY e.nombre_empleado, e.apellido_empleado;
END
GO

-- Costeo de actividades
-- Precio = (mano de obra + insumos + equipo + transporte + supervision) + gastos administrativos,
-- dividido entre (1 - margen). El ISV se suma al total del documento.
-- Los valores (salario minimo, cargas, margen, etc.) estan en la tabla PARAMETROS_COSTEO.

CREATE OR ALTER FUNCTION dbo.fn_CotizarLineas (@lineas dbo.tipo_lista_medidas READONLY)
RETURNS TABLE
AS
RETURN
(
	WITH par AS
	(
		SELECT CAST(MAX(CASE WHEN clave = 'SALARIO_MINIMO' THEN valor END) AS FLOAT) AS salario,
			   CAST(MAX(CASE WHEN clave = 'CARGAS_SOCIALES' THEN valor END) AS FLOAT) AS cargas,
			   CAST(MAX(CASE WHEN clave = 'HORAS_MES' THEN valor END) AS FLOAT) AS horas_mes,
			   CAST(MAX(CASE WHEN clave = 'FACTOR_SUPERVISOR' THEN valor END) AS FLOAT) AS factor_sup,
			   CAST(MAX(CASE WHEN clave = 'CUADRILLAS_POR_SUPERVISOR' THEN valor END) AS FLOAT) AS cuadrillas,
			   CAST(MAX(CASE WHEN clave = 'LITROS_GASOLINA_VIAJE' THEN valor END) AS FLOAT)
			   * CAST(MAX(CASE WHEN clave = 'PRECIO_LITRO_GASOLINA' THEN valor END) AS FLOAT) AS transporte,
			   CAST(MAX(CASE WHEN clave = 'VIDA_UTIL_EQUIPO_MESES' THEN valor END) AS FLOAT) AS vida_util,
			   CAST(MAX(CASE WHEN clave = 'MANTENIMIENTO_EQUIPO_MENSUAL' THEN valor END) AS FLOAT) AS mantenimiento,
			   CAST(MAX(CASE WHEN clave = 'GASTOS_ADMINISTRATIVOS' THEN valor END) AS FLOAT) AS admin,
			   CAST(MAX(CASE WHEN clave = 'MARGEN_UTILIDAD' THEN valor END) AS FLOAT) AS margen
		FROM parametros_costeo
	),
	pedido AS
	(
		SELECT actividad_id, SUM(medida) AS medida
		FROM @lineas
		GROUP BY actividad_id
	),
	base AS
	(
		SELECT a.actividad_id,
			   a.descripcion,
			   ISNULL(a.unidad, 'unidad') AS unidad,
			   CAST(l.medida AS FLOAT) AS medida,
			   CAST(a.rendimiento AS FLOAT) AS rendimiento,
			   CAST(ISNULL(a.precio, 0) AS FLOAT) AS precio_fijo,
			   ISNULL(r.cantidad_personas, 1) AS cantidad_personas
		FROM pedido l
		INNER JOIN actividades a ON a.actividad_id = l.actividad_id
		OUTER APPLY (SELECT TOP 1 ar.cantidad_personas
					 FROM actividad_rangos ar
					 WHERE ar.actividad_id = a.actividad_id AND ar.desde <= l.medida
					 ORDER BY ar.desde DESC) r
	),
	horas AS
	(
		SELECT b.*,
			   CASE WHEN ISNULL(b.rendimiento, 0) > 0
					THEN b.medida / (b.rendimiento * b.cantidad_personas) ELSE 0 END AS horas_trabajo
		FROM base b
	),
	costos AS
	(
		SELECT h.*,
			   CASE WHEN ISNULL(h.rendimiento, 0) > 0
					THEN h.medida / h.rendimiento * (p.salario * (1 + p.cargas) / p.horas_mes)
					ELSE 0 END AS mano_obra,
			   h.medida * CAST(ISNULL((SELECT SUM(ap.cantidad_por_unidad * ISNULL(pr.precio, 0))
									   FROM actividad_productos ap
									   OUTER APPLY (SELECT TOP 1 cp.precio_unitario AS precio
													FROM compras_productos cp
													WHERE cp.producto_id = ap.producto_id
													ORDER BY cp.compra_producto_id DESC) pr
									   WHERE ap.actividad_id = h.actividad_id), 0) AS FLOAT) AS insumos,
			   h.horas_trabajo * CAST(ISNULL((SELECT SUM(eq.precio)
											 FROM actividad_equipos ae
											 CROSS APPLY (SELECT AVG(ce.precio_unitario) AS precio
														  FROM compras_equipos ce
														  INNER JOIN equipos e ON e.equipo_id = ce.equipo_id
														  WHERE e.nombre_equipo = ae.nombre_equipo) eq
											 WHERE ae.actividad_id = h.actividad_id), 0) AS FLOAT)
			   * (1 / p.vida_util + p.mantenimiento) / p.horas_mes AS equipo,
			   h.horas_trabajo * (p.salario * p.factor_sup * (1 + p.cargas) / p.horas_mes) / p.cuadrillas AS supervision,
			   CASE WHEN SUM(h.horas_trabajo) OVER () > 0
					THEN p.transporte * h.horas_trabajo / SUM(h.horas_trabajo) OVER ()
					ELSE p.transporte / COUNT(*) OVER () END AS transporte,
			   p.admin, p.margen
		FROM horas h
		CROSS JOIN par p
	),
	total AS
	(
		SELECT c.*,
			   c.mano_obra + c.insumos + c.equipo + c.transporte + c.supervision AS directo
		FROM costos c
	)
	SELECT t.actividad_id,
		   LEFT(t.descripcion + ' (' +
				CASE WHEN t.medida = FLOOR(t.medida)
					 THEN CAST(CAST(t.medida AS BIGINT) AS VARCHAR(20))
					 ELSE CAST(CAST(t.medida AS DECIMAL(12, 2)) AS VARCHAR(20)) END +
				' ' + t.unidad + ')', 150) AS concepto,
		   t.descripcion,
		   t.unidad,
		   CAST(t.medida AS DECIMAL(12, 2)) AS medida,
		   t.cantidad_personas,
		   CAST(t.horas_trabajo AS DECIMAL(12, 2)) AS horas,
		   CAST(t.mano_obra AS DECIMAL(12, 2)) AS mano_obra,
		   CAST(t.insumos AS DECIMAL(12, 2)) AS insumos,
		   CAST(t.equipo AS DECIMAL(12, 2)) AS equipo,
		   CAST(t.transporte AS DECIMAL(12, 2)) AS transporte,
		   CAST(t.supervision AS DECIMAL(12, 2)) AS supervision,
		   CAST(t.directo * t.admin AS DECIMAL(12, 2)) AS gastos_admin,
		   CAST(t.directo * (1 + t.admin) AS DECIMAL(12, 2)) AS costo_total,
		   CAST(CASE WHEN ISNULL(t.rendimiento, 0) > 0
					 THEN t.directo * (1 + t.admin) / (1 - t.margen)
					 ELSE t.precio_fijo * t.medida END AS DECIMAL(12, 2)) AS precio
	FROM total t
);
GO

CREATE OR ALTER PROCEDURE sp_CotizarLineas
	@lineas dbo.tipo_lista_medidas READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT actividad_id, concepto, unidad, medida, cantidad_personas, horas,
		   mano_obra, insumos, equipo, transporte, supervision, gastos_admin, costo_total, precio
	FROM dbo.fn_CotizarLineas(@lineas)
	ORDER BY actividad_id;
END
GO

-- Lo que necesita una cotizacion: productos (cantidad redondeada hacia arriba) y un equipo de cada tipo.
CREATE OR ALTER FUNCTION dbo.fn_NecesidadesCotizacion (@cotizacion_id VARCHAR(50))
RETURNS TABLE
AS
RETURN
(
	SELECT 'P' AS tipo,
		   p.producto_id,
		   p.nombre_producto AS nombre,
		   CAST(CEILING(SUM(d.medida * ap.cantidad_por_unidad)) AS INT) AS cantidad
	FROM detalles_cotizacion d
	INNER JOIN actividad_productos ap ON ap.actividad_id = d.actividad_id
	INNER JOIN productos p ON p.producto_id = ap.producto_id
	WHERE d.cotizacion_id = @cotizacion_id
	GROUP BY p.producto_id, p.nombre_producto

	UNION ALL

	SELECT 'E', NULL, ae.nombre_equipo, 1
	FROM detalles_cotizacion d
	INNER JOIN actividad_equipos ae ON ae.actividad_id = d.actividad_id
	WHERE d.cotizacion_id = @cotizacion_id
	GROUP BY ae.nombre_equipo
);
GO

CREATE OR ALTER PROCEDURE sp_ResumenCotizacion
	@cotizacion_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 'Personas' AS tipo,
		   'Empleados necesarios' AS nombre,
		   ISNULL(MAX(d.cantidad_personas), 0) AS cantidad,
		   (SELECT COUNT(1) FROM empleados e
			WHERE e.estado_empleado = 1
			  AND dbo.fn_EmpleadoEnProceso(e.empleado_id, '') = 0) AS disponible
	FROM detalles_cotizacion d
	WHERE d.cotizacion_id = @cotizacion_id

	UNION ALL

	SELECT 'Producto', n.nombre, n.cantidad, ISNULL(p.stock, 0)
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	INNER JOIN productos p ON p.producto_id = n.producto_id
	WHERE n.tipo = 'P'

	UNION ALL

	SELECT 'Equipo', n.nombre, n.cantidad,
		   (SELECT COUNT(1) FROM equipos e
			INNER JOIN estados_equipo ee ON ee.estado_equipo_id = e.estado_equipo_id
			WHERE e.nombre_equipo = n.nombre AND ee.nombre_estado_equipo = 'Disponible'
			  AND NOT EXISTS (SELECT 1 FROM orden_equipos oe
							  WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0))
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	WHERE n.tipo = 'E';
END
GO

CREATE OR ALTER PROCEDURE sp_ListarCotizacionesOrden
	@texto VARCHAR(50) = '',
	@identificacion_cliente VARCHAR(30) = ''
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @identificacion_cliente = LTRIM(RTRIM(ISNULL(@identificacion_cliente, '')));

	SELECT q.cotizacion_id,
		   q.fecha_cotizacion,
		   q.valida_hasta,
		   ISNULL(q.identificacion_cliente, '') AS identificacion_cliente,
		   COALESCE(NULLIF(q.nombre_empresa, ''),
					LTRIM(ISNULL(q.nombre_cliente, '') + ' ' + ISNULL(q.apellido_cliente, ''))) AS cliente,
		   q.locacion_id,
		   ISNULL(q.observacion, '') AS observacion,
		   q.precio_cotizado,
		   CASE WHEN c.identificacion_cliente IS NULL THEN 'Cliente no registrado'
				WHEN c.estado_cliente = 0 THEN 'Cliente inactivo'
				WHEN q.valida_hasta < CAST(GETDATE() AS DATE) THEN 'Vencida'
				WHEN q.servicio_id IS NULL OR NOT EXISTS (SELECT 1 FROM detalles_cotizacion d
														  WHERE d.cotizacion_id = q.cotizacion_id
															AND d.actividad_id IS NOT NULL) THEN 'Sin actividades'
				ELSE 'Disponible' END AS estado,
		   CASE WHEN c.identificacion_cliente IS NULL
				THEN 'El cliente de esta cotización no está registrado. Regístrelo en Clientes antes de crear la orden.'
				WHEN c.estado_cliente = 0
				THEN 'El cliente de esta cotización está inactivo.'
				WHEN q.valida_hasta < CAST(GETDATE() AS DATE)
				THEN 'Esta cotización ya venció. Haga una cotización nueva.'
				WHEN q.servicio_id IS NULL OR NOT EXISTS (SELECT 1 FROM detalles_cotizacion d
														  WHERE d.cotizacion_id = q.cotizacion_id
															AND d.actividad_id IS NOT NULL)
				THEN 'Esta cotización es antigua y no tiene actividades. Haga una cotización nueva.'
				ELSE '' END AS motivo,
		   ISNULL(q.servicio_id, 0) AS servicio_id,
		   ISNULL((SELECT MAX(d.cantidad_personas) FROM detalles_cotizacion d
				   WHERE d.cotizacion_id = q.cotizacion_id), 0) AS cantidad_personas
	FROM cotizaciones q
	LEFT JOIN clientes c ON c.identificacion_cliente = q.identificacion_cliente
	WHERE NOT EXISTS (SELECT 1 FROM ordenes o WHERE o.cotizacion_id = q.cotizacion_id)
	  AND (@identificacion_cliente = '' OR q.identificacion_cliente = @identificacion_cliente)
	  AND (@texto = ''
		   OR q.cotizacion_id LIKE '%' + @texto + '%'
		   OR q.identificacion_cliente LIKE '%' + @texto + '%'
		   OR q.nombre_empresa LIKE '%' + @texto + '%'
		   OR ISNULL(q.nombre_cliente, '') + ' ' + ISNULL(q.apellido_cliente, '') LIKE '%' + @texto + '%')
	ORDER BY q.fecha_cotizacion DESC, q.cotizacion_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleCotizacion
	@cotizacion_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT linea, concepto, cantidad, precio_unitario,
		   CAST(cantidad * precio_unitario AS MONEY) AS subtotal,
		   ISNULL(actividad_id, 0) AS actividad_id,
		   ISNULL(medida, 0) AS medida,
		   ISNULL(cantidad_personas, 0) AS cantidad_personas
	FROM detalles_cotizacion
	WHERE cotizacion_id = @cotizacion_id
	ORDER BY linea;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarOrden
	@identificacion_cliente VARCHAR(30),
	@locacion_id INT,
	@supervisor_id INT,
	@vehiculo_id INT,
	@fecha_solicitud DATE,
	@fecha_servicio DATE,
	@estado_orden_id INT,
	@observacion VARCHAR(200),
	@turno VARCHAR(20),
	@piso_area VARCHAR(20),
	@empleados dbo.tipo_lista_empleados READONLY,
	@cotizacion_id VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT,
	@orden_id VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @orden_id = '';

	SET @identificacion_cliente = LTRIM(RTRIM(ISNULL(@identificacion_cliente, '')));
	SET @cotizacion_id = LTRIM(RTRIM(ISNULL(@cotizacion_id, '')));

	DECLARE @servicio_id INT, @cantidad_personas INT, @nombre VARCHAR(100);

	IF @cotizacion_id = ''
	BEGIN
		SET @mensaje = 'Seleccione una cotizacion.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM cotizaciones WHERE cotizacion_id = @cotizacion_id)
	BEGIN
		SET @mensaje = 'La cotizacion no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM ordenes WHERE cotizacion_id = @cotizacion_id)
	BEGIN
		SET @mensaje = 'Esa cotizacion ya tiene una orden.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM cotizaciones
			   WHERE cotizacion_id = @cotizacion_id AND valida_hasta < CAST(GETDATE() AS DATE))
	BEGIN
		SET @mensaje = 'La cotizacion ya vencio.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM cotizaciones
				   WHERE cotizacion_id = @cotizacion_id AND identificacion_cliente = @identificacion_cliente)
	BEGIN
		SET @mensaje = 'El cliente no coincide con el de la cotizacion.';
		RETURN;
	END

	SELECT @servicio_id = servicio_id FROM cotizaciones WHERE cotizacion_id = @cotizacion_id;

	IF @servicio_id IS NULL
	   OR NOT EXISTS (SELECT 1 FROM detalles_cotizacion WHERE cotizacion_id = @cotizacion_id AND actividad_id IS NOT NULL)
	BEGIN
		SET @mensaje = 'La cotizacion no tiene actividades. Haga una nueva.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM clientes
				   WHERE identificacion_cliente = @identificacion_cliente AND estado_cliente = 1)
	BEGIN
		SET @mensaje = 'El cliente no existe o esta inactivo.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM locaciones
				   WHERE locacion_id = @locacion_id AND identificacion_cliente = @identificacion_cliente)
	BEGIN
		SET @mensaje = 'La locacion no pertenece al cliente.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @supervisor_id AND estado_empleado = 1)
	BEGIN
		SET @mensaje = 'El supervisor no existe o esta inactivo.';
		RETURN;
	END

	IF dbo.fn_EmpleadoEnProceso(@supervisor_id, '') = 1
	BEGIN
		SET @mensaje = 'El supervisor esta en una orden en proceso.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @empleados WHERE empleado_id = @supervisor_id)
	BEGIN
		SET @mensaje = 'El supervisor no puede ser tambien empleado de la orden.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM vehiculos WHERE vehiculo_id = @vehiculo_id AND estado_vehiculo = 1)
	BEGIN
		SET @mensaje = 'El vehiculo no existe o esta inactivo.';
		RETURN;
	END

	SELECT @cantidad_personas = ISNULL(MAX(cantidad_personas), 0) FROM detalles_cotizacion WHERE cotizacion_id = @cotizacion_id;

	IF (SELECT COUNT(1) FROM @empleados) < @cantidad_personas
	BEGIN
		SET @mensaje = 'La cotizacion necesita ' + CAST(@cantidad_personas AS VARCHAR(10)) + ' empleados.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @empleados x
			   WHERE NOT EXISTS (SELECT 1 FROM empleados e WHERE e.empleado_id = x.empleado_id AND e.estado_empleado = 1))
	BEGIN
		SET @mensaje = 'Uno de los empleados no existe o esta inactivo.';
		RETURN;
	END

	SELECT TOP 1 @nombre = LTRIM(RTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')))
	FROM @empleados x
	INNER JOIN empleados e ON e.empleado_id = x.empleado_id
	WHERE dbo.fn_EmpleadoEnProceso(x.empleado_id, '') = 1;

	IF @nombre IS NOT NULL
	BEGIN
		SET @mensaje = LEFT(@nombre + ' esta en una orden en proceso.', 100);
		RETURN;
	END

	SET @nombre = NULL;

	SELECT TOP 1 @nombre = n.nombre
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	INNER JOIN productos p ON p.producto_id = n.producto_id
	WHERE n.tipo = 'P' AND n.cantidad > ISNULL(p.stock, 0);

	IF @nombre IS NOT NULL
	BEGIN
		SET @mensaje = LEFT('No hay stock suficiente de ' + @nombre + '.', 100);
		RETURN;
	END

	SELECT TOP 1 @nombre = n.nombre
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	WHERE n.tipo = 'E'
	  AND NOT EXISTS (SELECT 1 FROM equipos e
					  INNER JOIN estados_equipo ee ON ee.estado_equipo_id = e.estado_equipo_id
					  WHERE e.nombre_equipo = n.nombre AND ee.nombre_estado_equipo = 'Disponible'
						AND NOT EXISTS (SELECT 1 FROM orden_equipos oe
										WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0));

	IF @nombre IS NOT NULL
	BEGIN
		SET @mensaje = LEFT('No hay equipo disponible: ' + @nombre + '.', 100);
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @siguiente INT;

		SELECT @siguiente = ISNULL(MAX(TRY_CAST(SUBSTRING(orden_id, 5, 20) AS INT)), 0) + 1
		FROM ordenes WITH (UPDLOCK, HOLDLOCK)
		WHERE orden_id LIKE 'ORD-%';

		SET @orden_id = 'ORD-' + RIGHT('000000' + CAST(@siguiente AS VARCHAR(10)), 6);

		INSERT INTO ordenes (orden_id, locacion_id, servicio_id, supervisor_id, vehiculo_id,
							 fecha_solicitud, fecha_servicio, estado_orden_id, observacion,
							 conformidad_cliente, identificacion_cliente, cotizacion_id)
		VALUES (@orden_id, @locacion_id, @servicio_id, @supervisor_id, @vehiculo_id,
				@fecha_solicitud, @fecha_servicio, @estado_orden_id, @observacion,
				0, @identificacion_cliente, @cotizacion_id);

		INSERT INTO orden_actividades (orden_id, actividad_id, estado, duracion_min)
		SELECT DISTINCT @orden_id, actividad_id, 0, NULL
		FROM detalles_cotizacion
		WHERE cotizacion_id = @cotizacion_id AND actividad_id IS NOT NULL;

		INSERT INTO orden_empleados (orden_id, empleado_id, turno, piso_area)
		SELECT @orden_id, empleado_id, @turno, @piso_area FROM @empleados;

		INSERT INTO orden_productos (orden_id, producto_id, cantidad)
		SELECT @orden_id, producto_id, cantidad
		FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id)
		WHERE tipo = 'P';

		UPDATE p
		SET p.stock = ISNULL(p.stock, 0) - n.cantidad
		FROM productos p
		INNER JOIN dbo.fn_NecesidadesCotizacion(@cotizacion_id) n ON n.producto_id = p.producto_id
		WHERE n.tipo = 'P';

		INSERT INTO orden_equipos (orden_id, equipo_id, fecha_asignacion, devuelto)
		SELECT @orden_id,
			   (SELECT MIN(e.equipo_id) FROM equipos e
				INNER JOIN estados_equipo ee ON ee.estado_equipo_id = e.estado_equipo_id
				WHERE e.nombre_equipo = n.nombre AND ee.nombre_estado_equipo = 'Disponible'
				  AND NOT EXISTS (SELECT 1 FROM orden_equipos oe
								  WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0)),
			   @fecha_servicio, 0
		FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
		WHERE n.tipo = 'E';

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Orden registrada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @orden_id = '';
		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Gestión de órdenes

CREATE OR ALTER PROCEDURE sp_ListarOrdenesGestion
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.identificacion_cliente,
		   ISNULL(c.nombre_empresa, '') AS nombre_empresa,
		   LTRIM(RTRIM(ISNULL(c.nombre_cliente, '') + ' ' + ISNULL(c.apellido_cliente, ''))) AS contacto,
		   ISNULL(s.nombre_servicio, '') AS servicio,
		   ISNULL(o.supervisor_id, 0) AS supervisor_id,
		   LTRIM(RTRIM(ISNULL(sup.nombre_empleado, '') + ' ' + ISNULL(sup.apellido_empleado, ''))) AS supervisor,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   ISNULL(o.estado_orden_id, 0) AS estado_orden_id,
		   ISNULL(eo.nombre_estado_orden, '') AS estado
	FROM ordenes o
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN empleados sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	ORDER BY o.fecha_servicio DESC, o.orden_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleOrdenGestion
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.identificacion_cliente,
		   ISNULL(c.nombre_empresa, '') AS nombre_empresa,
		   LTRIM(RTRIM(ISNULL(c.nombre_cliente, '') + ' ' + ISNULL(c.apellido_cliente, ''))) AS contacto,
		   ISNULL(l.nombre_locacion, '') AS locacion,
		   ISNULL(l.tipo_locacion, '') AS tipo_locacion,
		   ISNULL(l.direccion, '') AS direccion,
		   ISNULL(oe.piso_area, '') AS piso_area,
		   ISNULL(oe.turno, '') AS turno,
		   CASE WHEN v.vehiculo_id IS NULL THEN ''
				ELSE LTRIM(RTRIM(ISNULL(v.placa, '') + ' (' + ISNULL(v.marca_vehiculo, '') + ' '
								 + ISNULL(v.modelo_vehiculo, '') + ')')) END AS vehiculo,
		   ISNULL(o.observacion, '') AS observacion,
		   ISNULL(eo.nombre_estado_orden, '') AS estado,
		   o.conformidad_cliente,
		   f.factura_id,
		   f.estado_pago,
		   f.precio_facturado,
		   f.fecha_factura
	FROM ordenes o
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN locaciones l ON l.locacion_id = o.locacion_id
	LEFT JOIN vehiculos v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	LEFT JOIN facturas f ON f.orden_id = o.orden_id
	OUTER APPLY (SELECT TOP 1 x.turno, x.piso_area
				 FROM orden_empleados x
				 WHERE x.orden_id = o.orden_id
				 ORDER BY x.empleado_id) oe
	WHERE o.orden_id = @orden_id;
END
GO

-- seccion: 1 actividades, 2 empleados, 3 productos, 4 equipos
CREATE OR ALTER PROCEDURE sp_ListasOrdenGestion
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT seccion, texto
	FROM
	(
		SELECT 1 AS seccion,
			   CAST(CASE WHEN ISNULL(oa.estado, 0) = 1 THEN NCHAR(10003) + N' ' ELSE NCHAR(8226) + N' ' END
					+ ISNULL(a.descripcion, '')
					+ CASE WHEN oa.duracion_min IS NOT NULL
						   THEN N' (' + CAST(oa.duracion_min AS NVARCHAR(10)) + N' min)' ELSE N'' END
					AS NVARCHAR(200)) AS texto
		FROM orden_actividades oa
		INNER JOIN actividades a ON a.actividad_id = oa.actividad_id
		WHERE oa.orden_id = @orden_id

		UNION ALL

		SELECT 2,
			   CAST(LTRIM(RTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')))
					+ CASE WHEN ca.nombre_cargo IS NOT NULL THEN N' (' + ca.nombre_cargo + N')' ELSE N'' END
					AS NVARCHAR(200))
		FROM orden_empleados oe
		INNER JOIN empleados e ON e.empleado_id = oe.empleado_id
		LEFT JOIN cargos ca ON ca.cargo_id = e.cargo_id
		WHERE oe.orden_id = @orden_id

		UNION ALL

		SELECT 3,
			   CAST(N'Producto: ' + ISNULL(p.nombre_producto, '') + N' ' + NCHAR(215) + N' '
					+ CAST(ISNULL(op.cantidad, 0) AS NVARCHAR(10)) AS NVARCHAR(200))
		FROM orden_productos op
		INNER JOIN productos p ON p.producto_id = op.producto_id
		WHERE op.orden_id = @orden_id

		UNION ALL

		SELECT 4,
			   CAST(N'Equipo: ' + LTRIM(RTRIM(ISNULL(q.nombre_equipo, '') + ' ' + ISNULL(q.marca_equipo, '')
											   + ' ' + ISNULL(q.modelo_equipo, '')))
					+ CASE WHEN ISNULL(oq.devuelto, 0) = 1 THEN N' (devuelto)' ELSE N' (en uso)' END
					AS NVARCHAR(200))
		FROM orden_equipos oq
		INNER JOIN equipos q ON q.equipo_id = oq.equipo_id
		WHERE oq.orden_id = @orden_id
	) listas
	ORDER BY seccion, texto;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarOrdenes
	@texto VARCHAR(50) = NULL,
	@estado_orden_id INT = NULL,
	@desde DATE = NULL,
	@hasta DATE = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT o.orden_id,
		   o.identificacion_cliente,
		   ISNULL(c.nombre_empresa, o.identificacion_cliente) AS cliente,
		   s.nombre_servicio,
		   l.nombre_locacion,
		   v.placa,
		   LTRIM(RTRIM(ISNULL(sup.nombre_empleado, '') + ' ' + ISNULL(sup.apellido_empleado, ''))) AS supervisor,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   o.estado_orden_id,
		   eo.nombre_estado_orden AS estado,
		   o.observacion,
		   o.conformidad_cliente,
		   c.nombre_empresa,
		   c.nombre_cliente,
		   c.apellido_cliente,
		   l.direccion,
		   v.marca_vehiculo,
		   v.modelo_vehiculo
	FROM ordenes o
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN locaciones l ON l.locacion_id = o.locacion_id
	LEFT JOIN vehiculos v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN empleados sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE (@texto = '' OR o.orden_id LIKE '%' + @texto + '%'
		   OR o.identificacion_cliente LIKE '%' + @texto + '%'
		   OR c.nombre_empresa LIKE '%' + @texto + '%')
	  AND (@estado_orden_id IS NULL OR o.estado_orden_id = @estado_orden_id)
	  AND (@desde IS NULL OR o.fecha_servicio >= @desde)
	  AND (@hasta IS NULL OR o.fecha_servicio <= @hasta)
	ORDER BY o.fecha_servicio DESC, o.orden_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.identificacion_cliente,
		   ISNULL(c.nombre_empresa, o.identificacion_cliente) AS cliente,
		   o.locacion_id,
		   l.nombre_locacion,
		   o.servicio_id,
		   s.nombre_servicio,
		   o.supervisor_id,
		   LTRIM(RTRIM(ISNULL(sup.nombre_empleado, '') + ' ' + ISNULL(sup.apellido_empleado, ''))) AS supervisor,
		   o.vehiculo_id,
		   v.placa,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   o.estado_orden_id,
		   eo.nombre_estado_orden AS estado,
		   o.observacion,
		   ISNULL(o.conformidad_cliente, 0) AS conformidad_cliente,
		   (SELECT TOP 1 oe.turno FROM orden_empleados oe WHERE oe.orden_id = o.orden_id) AS turno,
		   (SELECT TOP 1 oe.piso_area FROM orden_empleados oe WHERE oe.orden_id = o.orden_id) AS piso_area
	FROM ordenes o
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN locaciones l ON l.locacion_id = o.locacion_id
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN empleados sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN vehiculos v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarActividadesPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.actividad_id,
		   a.descripcion,
		   a.frecuencia,
		   a.precio,
		   ISNULL(oa.estado, 0) AS estado,
		   oa.duracion_min
	FROM orden_actividades oa
	INNER JOIN actividades a ON a.actividad_id = oa.actividad_id
	WHERE oa.orden_id = @orden_id
	ORDER BY a.descripcion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id,
		   LTRIM(RTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, ''))) AS nombre_completo,
		   ca.nombre_cargo,
		   oe.turno,
		   oe.piso_area
	FROM orden_empleados oe
	INNER JOIN empleados e ON e.empleado_id = oe.empleado_id
	LEFT JOIN cargos ca ON ca.cargo_id = e.cargo_id
	WHERE oe.orden_id = @orden_id
	ORDER BY e.nombre_empleado, e.apellido_empleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.producto_id,
		   p.nombre_producto,
		   p.tipo_producto,
		   op.cantidad
	FROM orden_productos op
	INNER JOIN productos p ON p.producto_id = op.producto_id
	WHERE op.orden_id = @orden_id
	ORDER BY p.nombre_producto;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEquiposPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.equipo_id,
		   e.nombre_equipo,
		   e.tipo_equipo,
		   e.marca_equipo,
		   e.modelo_equipo,
		   oe.fecha_asignacion,
		   ISNULL(oe.devuelto, 0) AS devuelto
	FROM orden_equipos oe
	INNER JOIN equipos e ON e.equipo_id = oe.equipo_id
	WHERE oe.orden_id = @orden_id
	ORDER BY e.nombre_equipo, e.equipo_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarOrden
	@orden_id VARCHAR(50),
	@locacion_id INT,
	@servicio_id INT,
	@supervisor_id INT,
	@vehiculo_id INT,
	@fecha_solicitud DATE,
	@fecha_servicio DATE,
	@observacion VARCHAR(200),
	@turno VARCHAR(20),
	@piso_area VARCHAR(20),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @cliente VARCHAR(30), @estado VARCHAR(25);

	SELECT @cliente = o.identificacion_cliente, @estado = eo.nombre_estado_orden
	FROM ordenes o
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;

	IF @cliente IS NULL
	BEGIN
		SET @mensaje = 'La orden no existe.';
		RETURN;
	END

	IF ISNULL(@estado, '') <> 'Pendiente'
	BEGIN
		SET @mensaje = 'Solo se pueden editar ordenes pendientes.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM locaciones WHERE locacion_id = @locacion_id AND identificacion_cliente = @cliente)
	BEGIN
		SET @mensaje = 'La locacion no pertenece al cliente.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM servicios WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'El servicio no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @supervisor_id AND estado_empleado = 1)
	BEGIN
		SET @mensaje = 'El supervisor no existe o esta inactivo.';
		RETURN;
	END

	IF dbo.fn_EmpleadoEnProceso(@supervisor_id, @orden_id) = 1
	BEGIN
		SET @mensaje = 'El supervisor esta en una orden en proceso.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM vehiculos WHERE vehiculo_id = @vehiculo_id AND estado_vehiculo = 1)
	BEGIN
		SET @mensaje = 'El vehiculo no existe o esta inactivo.';
		RETURN;
	END

	IF @fecha_servicio < @fecha_solicitud
	BEGIN
		SET @mensaje = 'La fecha de limpieza no puede ser anterior a la solicitud.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE ordenes
		SET locacion_id = @locacion_id,
			servicio_id = @servicio_id,
			supervisor_id = @supervisor_id,
			vehiculo_id = @vehiculo_id,
			fecha_solicitud = @fecha_solicitud,
			fecha_servicio = @fecha_servicio,
			observacion = LTRIM(RTRIM(ISNULL(@observacion, '')))
		WHERE orden_id = @orden_id;

		UPDATE orden_empleados
		SET turno = LTRIM(RTRIM(ISNULL(@turno, ''))),
			piso_area = LTRIM(RTRIM(ISNULL(@piso_area, '')))
		WHERE orden_id = @orden_id;

		UPDATE orden_equipos
		SET fecha_asignacion = @fecha_servicio
		WHERE orden_id = @orden_id AND ISNULL(devuelto, 0) = 0;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'La orden se actualizo correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoOrden
	@orden_id VARCHAR(50),
	@estado_orden_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @actual VARCHAR(25), @nuevo VARCHAR(25);

	SELECT @actual = eo.nombre_estado_orden
	FROM ordenes o
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;

	IF NOT EXISTS (SELECT 1 FROM ordenes WHERE orden_id = @orden_id)
	BEGIN
		SET @mensaje = 'La orden no existe.';
		RETURN;
	END

	SELECT @nuevo = nombre_estado_orden FROM estados_orden WHERE estado_orden_id = @estado_orden_id;

	IF @nuevo IS NULL
	BEGIN
		SET @mensaje = 'El estado no existe.';
		RETURN;
	END

	IF ISNULL(@actual, '') IN ('Completada', 'Cancelada')
	BEGIN
		SET @mensaje = 'La orden ya esta ' + LOWER(@actual) + ' y no se puede cambiar.';
		RETURN;
	END

	IF @nuevo = 'En proceso' AND ISNULL(@actual, '') <> 'En proceso'
	BEGIN
		DECLARE @ocupado VARCHAR(100);

		SELECT TOP 1 @ocupado = LTRIM(RTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')))
		FROM empleados e
		WHERE (e.empleado_id = (SELECT supervisor_id FROM ordenes WHERE orden_id = @orden_id)
			   OR e.empleado_id IN (SELECT empleado_id FROM orden_empleados WHERE orden_id = @orden_id))
		  AND dbo.fn_EmpleadoEnProceso(e.empleado_id, @orden_id) = 1;

		IF @ocupado IS NOT NULL
		BEGIN
			SET @mensaje = LEFT(@ocupado + ' ya esta en otra orden en proceso.', 100);
			RETURN;
		END
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE ordenes SET estado_orden_id = @estado_orden_id WHERE orden_id = @orden_id;

		-- Al cancelar se devuelve el stock de los productos
		IF @nuevo = 'Cancelada'
		BEGIN
			UPDATE p
			SET p.stock = ISNULL(p.stock, 0) + op.cantidad
			FROM productos p
			INNER JOIN orden_productos op ON op.producto_id = p.producto_id
			WHERE op.orden_id = @orden_id;
		END

		-- Al cerrar la orden (completada o cancelada) los equipos quedan libres
		IF @nuevo IN ('Completada', 'Cancelada')
		BEGIN
			UPDATE orden_equipos SET devuelto = 1 WHERE orden_id = @orden_id;
		END

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'La orden ahora esta ' + LOWER(@nuevo) + '.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_MarcarActividadOrden
	@orden_id VARCHAR(50),
	@actividad_id INT,
	@estado BIT,
	@duracion_min INT = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM orden_actividades WHERE orden_id = @orden_id AND actividad_id = @actividad_id)
	BEGIN
		SET @mensaje = 'La actividad no pertenece a la orden.';
		RETURN;
	END

	IF @duracion_min IS NOT NULL AND @duracion_min < 0
	BEGIN
		SET @mensaje = 'La duracion no puede ser negativa.';
		RETURN;
	END

	UPDATE orden_actividades
	SET estado = @estado,
		duracion_min = CASE WHEN @estado = 1 THEN @duracion_min ELSE NULL END
	WHERE orden_id = @orden_id AND actividad_id = @actividad_id;

	SET @resultado = 1;
	SET @mensaje = 'La actividad se actualizo correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_DevolverEquipoOrden
	@orden_id VARCHAR(50),
	@equipo_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM orden_equipos WHERE orden_id = @orden_id AND equipo_id = @equipo_id)
	BEGIN
		SET @mensaje = 'El equipo no esta asignado a la orden.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM orden_equipos
			   WHERE orden_id = @orden_id AND equipo_id = @equipo_id AND ISNULL(devuelto, 0) = 1)
	BEGIN
		SET @mensaje = 'El equipo ya fue devuelto.';
		RETURN;
	END

	UPDATE orden_equipos SET devuelto = 1 WHERE orden_id = @orden_id AND equipo_id = @equipo_id;

	SET @resultado = 1;
	SET @mensaje = 'El equipo se devolvio correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_RegistrarConformidadOrden
	@orden_id VARCHAR(50),
	@conformidad BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @estado VARCHAR(25);

	SELECT @estado = eo.nombre_estado_orden
	FROM ordenes o
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;

	IF NOT EXISTS (SELECT 1 FROM ordenes WHERE orden_id = @orden_id)
	BEGIN
		SET @mensaje = 'La orden no existe.';
		RETURN;
	END

	IF ISNULL(@estado, '') <> 'Completada'
	BEGIN
		SET @mensaje = 'La conformidad solo se registra en ordenes completadas.';
		RETURN;
	END

	UPDATE ordenes SET conformidad_cliente = @conformidad WHERE orden_id = @orden_id;

	SET @resultado = 1;
	SET @mensaje = 'La conformidad del cliente se registro correctamente.';
END
GO

-- Inventario

CREATE OR ALTER PROCEDURE sp_BuscarInventario
	@texto VARCHAR(50) = NULL,
	@tipo VARCHAR(10) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @tipo = UPPER(LTRIM(RTRIM(ISNULL(@tipo, ''))));

	WITH unidades AS
	(
		SELECT ISNULL(e.nombre_equipo, '') AS nombre_equipo,
			   ISNULL(e.tipo_equipo, '') AS tipo_equipo,
			   ISNULL(e.marca_equipo, '') AS marca_equipo,
			   ISNULL(e.modelo_equipo, '') AS modelo_equipo,
			   CASE WHEN EXISTS (SELECT 1 FROM orden_equipos oe
								 WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0)
					THEN 0 ELSE 1 END AS libre,
			   (SELECT TOP 1 ce.precio_unitario FROM compras_equipos ce
				WHERE ce.equipo_id = e.equipo_id) AS costo
		FROM equipos e
	),
	grupos AS
	(
		SELECT nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo,
			   COUNT(1) AS total,
			   SUM(libre) AS disponible,
			   SUM(costo) AS valor
		FROM unidades
		GROUP BY nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo
	)
	SELECT tipo, nombre, categoria, marca_modelo, disponible, total, proveedor, valor
	FROM
	(
		SELECT 'Producto' AS tipo,
			   p.nombre_producto AS nombre,
			   p.tipo_producto AS categoria,
			   N'—' AS marca_modelo,
			   N'—' AS disponible,
			   ISNULL(p.stock, 0) AS total,
			   (SELECT TOP 1 pr.nombre_proveedor
				FROM compras_productos cp
				INNER JOIN compras c ON c.compra_id = cp.compra_id
				INNER JOIN proveedores pr ON pr.proveedor_id = c.proveedor_id
				WHERE cp.producto_id = p.producto_id
				ORDER BY c.fecha_compra DESC, c.compra_id DESC) AS proveedor,
			   CAST(ISNULL(p.stock, 0) *
					(SELECT SUM(cp.cantidad * cp.precio_unitario) / NULLIF(SUM(cp.cantidad), 0)
					 FROM compras_productos cp
					 WHERE cp.producto_id = p.producto_id) AS MONEY) AS valor
		FROM productos p
		WHERE @tipo IN ('', 'PRODUCTO')
		  AND (@texto = '' OR p.nombre_producto LIKE '%' + @texto + '%' OR p.tipo_producto LIKE '%' + @texto + '%')

		UNION ALL

		SELECT 'Equipo' AS tipo,
			   g.nombre_equipo AS nombre,
			   g.tipo_equipo AS categoria,
			   LTRIM(RTRIM(g.marca_equipo + ' ' + g.modelo_equipo)) AS marca_modelo,
			   CAST(g.disponible AS NVARCHAR(10)) AS disponible,
			   g.total,
			   (SELECT TOP 1 pr.nombre_proveedor
				FROM equipos e
				INNER JOIN compras_equipos ce ON ce.equipo_id = e.equipo_id
				INNER JOIN compras c ON c.compra_id = ce.compra_id
				INNER JOIN proveedores pr ON pr.proveedor_id = c.proveedor_id
				WHERE ISNULL(e.nombre_equipo, '') = g.nombre_equipo
				  AND ISNULL(e.tipo_equipo, '') = g.tipo_equipo
				  AND ISNULL(e.marca_equipo, '') = g.marca_equipo
				  AND ISNULL(e.modelo_equipo, '') = g.modelo_equipo
				ORDER BY c.fecha_compra DESC, c.compra_id DESC) AS proveedor,
			   CAST(g.valor AS MONEY) AS valor
		FROM grupos g
		WHERE @tipo IN ('', 'EQUIPO')
		  AND (@texto = '' OR g.nombre_equipo LIKE '%' + @texto + '%' OR g.tipo_equipo LIKE '%' + @texto + '%'
			   OR g.marca_equipo LIKE '%' + @texto + '%' OR g.modelo_equipo LIKE '%' + @texto + '%')
	) inventario
	ORDER BY tipo DESC, nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosInventario
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT producto_id, nombre_producto, tipo_producto, ISNULL(stock, 0) AS stock
	FROM productos
	WHERE @texto = '' OR nombre_producto LIKE '%' + @texto + '%' OR tipo_producto LIKE '%' + @texto + '%'
	ORDER BY nombre_producto;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEquiposInventario
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT ISNULL(nombre_equipo, '') AS nombre_equipo,
		   ISNULL(tipo_equipo, '') AS tipo_equipo,
		   ISNULL(marca_equipo, '') AS marca_equipo,
		   ISNULL(modelo_equipo, '') AS modelo_equipo,
		   COUNT(1) AS total
	FROM equipos
	WHERE @texto = '' OR nombre_equipo LIKE '%' + @texto + '%' OR tipo_equipo LIKE '%' + @texto + '%'
	   OR marca_equipo LIKE '%' + @texto + '%' OR modelo_equipo LIKE '%' + @texto + '%'
	GROUP BY ISNULL(nombre_equipo, ''), ISNULL(tipo_equipo, ''), ISNULL(marca_equipo, ''), ISNULL(modelo_equipo, '')
	ORDER BY nombre_equipo, marca_equipo, modelo_equipo;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProveedores
AS
BEGIN
	SET NOCOUNT ON;

	SELECT proveedor_id, nombre_proveedor, telefono_proveedor, correo_proveedor
	FROM proveedores
	ORDER BY nombre_proveedor;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarProveedor
	@nombre_proveedor VARCHAR(50),
	@telefono_proveedor VARCHAR(50),
	@correo_proveedor VARCHAR(50),
	@proveedor_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @proveedor_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @nombre_proveedor = LTRIM(RTRIM(ISNULL(@nombre_proveedor, '')));

	IF @nombre_proveedor = ''
	BEGIN
		SET @mensaje = 'El nombre del proveedor es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM proveedores WHERE nombre_proveedor = @nombre_proveedor)
	BEGIN
		SET @mensaje = 'Ya existe un proveedor con ese nombre.';
		RETURN;
	END

	INSERT INTO proveedores (nombre_proveedor, telefono_proveedor, correo_proveedor)
	VALUES (@nombre_proveedor, @telefono_proveedor, @correo_proveedor);

	SET @proveedor_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Proveedor registrado correctamente.';
END
GO

-- Servicios y actividades
-- Un servicio agrupa actividades. Cada actividad necesita rangos de personas, al menos un producto y al menos un equipo.

CREATE OR ALTER PROCEDURE sp_ListarServiciosGestion
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.servicio_id,
		   ISNULL(s.nombre_servicio, '') AS nombre_servicio,
		   (SELECT COUNT(*) FROM actividades a WHERE a.servicio_id = s.servicio_id) AS cantidad_actividades
	FROM servicios s
	ORDER BY s.nombre_servicio;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarServiciosConActividades
AS
BEGIN
	SET NOCOUNT ON;

	SELECT s.servicio_id, s.nombre_servicio
	FROM servicios s
	WHERE EXISTS (SELECT 1 FROM actividades a WHERE a.servicio_id = s.servicio_id)
	ORDER BY s.nombre_servicio;
END
GO

CREATE OR ALTER PROCEDURE sp_GuardarServicio
	@servicio_id INT,
	@nombre_servicio VARCHAR(30),
	@id_guardado INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @id_guardado = @servicio_id;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @nombre_servicio = LTRIM(RTRIM(ISNULL(@nombre_servicio, '')));

	IF @nombre_servicio = ''
	BEGIN
		SET @mensaje = 'El nombre del servicio es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM servicios WHERE nombre_servicio = @nombre_servicio AND servicio_id <> ISNULL(@servicio_id, 0))
	BEGIN
		SET @mensaje = 'Ya existe un servicio con ese nombre.';
		RETURN;
	END

	IF ISNULL(@servicio_id, 0) = 0
	BEGIN
		INSERT INTO servicios (nombre_servicio) VALUES (@nombre_servicio);
		SET @id_guardado = CAST(SCOPE_IDENTITY() AS INT);
		SET @mensaje = 'Servicio registrado. Ahora agréguele sus actividades.';
	END
	ELSE
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM servicios WHERE servicio_id = @servicio_id)
		BEGIN
			SET @mensaje = 'El servicio no existe.';
			RETURN;
		END

		UPDATE servicios SET nombre_servicio = @nombre_servicio WHERE servicio_id = @servicio_id;
		SET @mensaje = 'Servicio actualizado correctamente.';
	END

	SET @resultado = 1;
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarServicio
	@servicio_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM servicios WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'El servicio no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM actividades WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'Primero elimine las actividades del servicio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM cotizaciones WHERE servicio_id = @servicio_id)
	   OR EXISTS (SELECT 1 FROM ordenes WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'El servicio ya se usó en cotizaciones u órdenes y no se puede eliminar.';
		RETURN;
	END

	DELETE FROM servicios WHERE servicio_id = @servicio_id;

	SET @resultado = 1;
	SET @mensaje = 'Servicio eliminado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ListarActividadesGestion
	@servicio_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.actividad_id,
		   ISNULL(a.servicio_id, 0) AS servicio_id,
		   ISNULL(s.nombre_servicio, '') AS nombre_servicio,
		   ISNULL(a.descripcion, '') AS descripcion,
		   ISNULL(a.unidad, '') AS unidad,
		   ISNULL(a.rendimiento, 0) AS rendimiento,
		   (SELECT COUNT(*) FROM actividad_rangos r WHERE r.actividad_id = a.actividad_id) AS cantidad_rangos,
		   (SELECT COUNT(*) FROM actividad_productos p WHERE p.actividad_id = a.actividad_id) AS cantidad_productos,
		   (SELECT COUNT(*) FROM actividad_equipos e WHERE e.actividad_id = a.actividad_id) AS cantidad_equipos
	FROM actividades a
	LEFT JOIN servicios s ON s.servicio_id = a.servicio_id
	WHERE ISNULL(@servicio_id, 0) = 0 OR a.servicio_id = @servicio_id
	ORDER BY s.nombre_servicio, a.descripcion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarRangosActividad
	@actividad_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT desde, hasta, cantidad_personas
	FROM actividad_rangos
	WHERE actividad_id = @actividad_id
	ORDER BY desde;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosActividad
	@actividad_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.producto_id,
		   ISNULL(p.nombre_producto, '') AS nombre_producto,
		   ISNULL(p.tipo_producto, '') AS tipo_producto,
		   ap.cantidad_por_unidad
	FROM actividad_productos ap
	INNER JOIN productos p ON p.producto_id = ap.producto_id
	WHERE ap.actividad_id = @actividad_id
	ORDER BY p.nombre_producto;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEquiposActividad
	@actividad_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT nombre_equipo
	FROM actividad_equipos
	WHERE actividad_id = @actividad_id
	ORDER BY nombre_equipo;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosCatalogo
AS
BEGIN
	SET NOCOUNT ON;

	SELECT producto_id,
		   ISNULL(nombre_producto, '') AS nombre_producto,
		   ISNULL(tipo_producto, '') AS tipo_producto
	FROM productos
	ORDER BY nombre_producto;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEquiposCatalogo
AS
BEGIN
	SET NOCOUNT ON;

	SELECT nombre_equipo
	FROM
	(
		SELECT nombre_equipo FROM equipos WHERE nombre_equipo IS NOT NULL
		UNION
		SELECT nombre_equipo FROM actividad_equipos
	) t
	ORDER BY nombre_equipo;
END
GO

CREATE OR ALTER PROCEDURE sp_GuardarActividad
	@actividad_id INT,
	@servicio_id INT,
	@descripcion VARCHAR(50),
	@unidad VARCHAR(20),
	@rendimiento DECIMAL(10, 2),
	@rangos dbo.tipo_actividad_rangos READONLY,
	@productos dbo.tipo_actividad_productos READONLY,
	@productos_nuevos dbo.tipo_actividad_productos_nuevos READONLY,
	@equipos dbo.tipo_actividad_equipos READONLY,
	@id_guardado INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @id_guardado = ISNULL(@actividad_id, 0);
	SET @resultado = 0;
	SET @mensaje = '';

	SET @descripcion = LTRIM(RTRIM(ISNULL(@descripcion, '')));
	SET @unidad = LTRIM(RTRIM(ISNULL(@unidad, '')));

	IF NOT EXISTS (SELECT 1 FROM servicios WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'Elija el servicio de la actividad.';
		RETURN;
	END

	IF @descripcion = ''
	BEGIN
		SET @mensaje = 'El nombre de la actividad es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM actividades WHERE descripcion = @descripcion AND actividad_id <> ISNULL(@actividad_id, 0))
	BEGIN
		SET @mensaje = 'Ya existe una actividad con ese nombre.';
		RETURN;
	END

	IF @unidad = ''
	BEGIN
		SET @mensaje = 'La unidad de medida es obligatoria.';
		RETURN;
	END

	IF ISNULL(@rendimiento, 0) <= 0
	BEGIN
		SET @mensaje = 'El rendimiento debe ser mayor que cero.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @rangos)
	BEGIN
		SET @mensaje = 'Agregue al menos un rango de personas.';
		RETURN;
	END

	IF (SELECT MIN(desde) FROM @rangos) <> 0
	BEGIN
		SET @mensaje = 'El primer rango de personas debe empezar en 0.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @rangos WHERE desde < 0 OR cantidad_personas < 1 OR (hasta IS NOT NULL AND hasta <= desde))
	BEGIN
		SET @mensaje = 'Hay un rango de personas con valores no válidos.';
		RETURN;
	END

	IF EXISTS (SELECT desde FROM @rangos GROUP BY desde HAVING COUNT(*) > 1)
	BEGIN
		SET @mensaje = 'Hay rangos de personas repetidos.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @productos) AND NOT EXISTS (SELECT 1 FROM @productos_nuevos)
	BEGIN
		SET @mensaje = 'La actividad necesita al menos un producto.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos WHERE cantidad_por_unidad <= 0)
	   OR EXISTS (SELECT 1 FROM @productos_nuevos WHERE cantidad_por_unidad <= 0)
	BEGIN
		SET @mensaje = 'La cantidad de cada producto debe ser mayor que cero.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos t WHERE NOT EXISTS (SELECT 1 FROM productos p WHERE p.producto_id = t.producto_id))
	BEGIN
		SET @mensaje = 'Hay un producto que no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos_nuevos WHERE LTRIM(RTRIM(nombre_producto)) = '')
	BEGIN
		SET @mensaje = 'El nombre de un producto nuevo está vacío.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos_nuevos n
			   WHERE EXISTS (SELECT 1 FROM productos p
							 WHERE p.nombre_producto = LTRIM(RTRIM(n.nombre_producto))
							   AND ISNULL(p.tipo_producto, '') = LTRIM(RTRIM(n.tipo_producto))))
	   OR EXISTS (SELECT LTRIM(RTRIM(nombre_producto)), LTRIM(RTRIM(tipo_producto)) FROM @productos_nuevos
				  GROUP BY LTRIM(RTRIM(nombre_producto)), LTRIM(RTRIM(tipo_producto)) HAVING COUNT(*) > 1)
	BEGIN
		SET @mensaje = 'Un producto nuevo ya está registrado.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @equipos)
	BEGIN
		SET @mensaje = 'La actividad necesita al menos un equipo.';
		RETURN;
	END

	IF ISNULL(@actividad_id, 0) <> 0 AND NOT EXISTS (SELECT 1 FROM actividades WHERE actividad_id = @actividad_id)
	BEGIN
		SET @mensaje = 'La actividad no existe.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		IF ISNULL(@actividad_id, 0) = 0
		BEGIN
			INSERT INTO actividades (servicio_id, descripcion, frecuencia, precio, unidad, rendimiento)
			VALUES (@servicio_id, @descripcion, NULL, NULL, @unidad, @rendimiento);

			SET @id_guardado = CAST(SCOPE_IDENTITY() AS INT);
			SET @mensaje = 'Actividad registrada correctamente.';
		END
		ELSE
		BEGIN
			UPDATE actividades
			SET servicio_id = @servicio_id, descripcion = @descripcion, unidad = @unidad, rendimiento = @rendimiento
			WHERE actividad_id = @actividad_id;

			DELETE FROM actividad_rangos WHERE actividad_id = @actividad_id;
			DELETE FROM actividad_productos WHERE actividad_id = @actividad_id;
			DELETE FROM actividad_equipos WHERE actividad_id = @actividad_id;

			SET @mensaje = 'Actividad actualizada correctamente.';
		END

		INSERT INTO actividad_rangos (actividad_id, desde, hasta, cantidad_personas)
		SELECT @id_guardado, desde, hasta, cantidad_personas FROM @rangos;

		INSERT INTO actividad_productos (actividad_id, producto_id, cantidad_por_unidad)
		SELECT @id_guardado, producto_id, cantidad_por_unidad FROM @productos;

		INSERT INTO productos (nombre_producto, tipo_producto, stock)
		SELECT LTRIM(RTRIM(nombre_producto)), LTRIM(RTRIM(tipo_producto)), 0 FROM @productos_nuevos;

		INSERT INTO actividad_productos (actividad_id, producto_id, cantidad_por_unidad)
		SELECT @id_guardado, p.producto_id, n.cantidad_por_unidad
		FROM @productos_nuevos n
		INNER JOIN productos p ON p.nombre_producto = LTRIM(RTRIM(n.nombre_producto))
							  AND ISNULL(p.tipo_producto, '') = LTRIM(RTRIM(n.tipo_producto));

		INSERT INTO actividad_equipos (actividad_id, nombre_equipo)
		SELECT @id_guardado, nombre_equipo FROM @equipos;

		COMMIT TRANSACTION;
		SET @resultado = 1;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @id_guardado = ISNULL(@actividad_id, 0);
		SET @mensaje = LEFT('No se pudo guardar la actividad: ' + ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarActividad
	@actividad_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM actividades WHERE actividad_id = @actividad_id)
	BEGIN
		SET @mensaje = 'La actividad no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM detalles_cotizacion WHERE actividad_id = @actividad_id)
	   OR EXISTS (SELECT 1 FROM orden_actividades WHERE actividad_id = @actividad_id)
	BEGIN
		SET @mensaje = 'La actividad ya se usó en cotizaciones u órdenes y no se puede eliminar.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DELETE FROM actividad_rangos WHERE actividad_id = @actividad_id;
		DELETE FROM actividad_productos WHERE actividad_id = @actividad_id;
		DELETE FROM actividad_equipos WHERE actividad_id = @actividad_id;
		DELETE FROM actividades WHERE actividad_id = @actividad_id;

		COMMIT TRANSACTION;
		SET @resultado = 1;
		SET @mensaje = 'Actividad eliminada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT('No se pudo eliminar la actividad: ' + ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Un producto nuevo nace ligado a las actividades donde se usa (como minimo una).
CREATE OR ALTER PROCEDURE sp_InsertarProductoConActividades
	@nombre_producto VARCHAR(50),
	@tipo_producto VARCHAR(20),
	@actividades dbo.tipo_producto_actividades READONLY,
	@producto_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @producto_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @nombre_producto = LTRIM(RTRIM(ISNULL(@nombre_producto, '')));
	SET @tipo_producto = LTRIM(RTRIM(ISNULL(@tipo_producto, '')));

	IF @nombre_producto = ''
	BEGIN
		SET @mensaje = 'El nombre del producto es obligatorio.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @actividades)
	BEGIN
		SET @mensaje = 'Elija al menos una actividad donde se usa el producto.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @actividades WHERE cantidad_por_unidad <= 0)
	BEGIN
		SET @mensaje = 'La cantidad por unidad debe ser mayor que cero.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @actividades t WHERE NOT EXISTS (SELECT 1 FROM actividades a WHERE a.actividad_id = t.actividad_id))
	BEGIN
		SET @mensaje = 'Hay una actividad que no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM productos WHERE nombre_producto = @nombre_producto AND ISNULL(tipo_producto, '') = @tipo_producto)
	BEGIN
		SET @mensaje = 'Ese producto ya está registrado.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		INSERT INTO productos (nombre_producto, tipo_producto, stock)
		VALUES (@nombre_producto, @tipo_producto, 0);

		SET @producto_id = CAST(SCOPE_IDENTITY() AS INT);

		INSERT INTO actividad_productos (actividad_id, producto_id, cantidad_por_unidad)
		SELECT actividad_id, @producto_id, cantidad_por_unidad FROM @actividades;

		COMMIT TRANSACTION;
		SET @resultado = 1;
		SET @mensaje = 'Producto registrado y ligado a sus actividades.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @producto_id = NULL;
		SET @mensaje = LEFT('No se pudo registrar el producto: ' + ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Un equipo nuevo se liga por nombre a las actividades que lo usan (como minimo una).
CREATE OR ALTER PROCEDURE sp_VincularEquipoActividades
	@nombre_equipo VARCHAR(50),
	@actividades dbo.tipo_equipo_actividades READONLY,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @nombre_equipo = LTRIM(RTRIM(ISNULL(@nombre_equipo, '')));

	IF @nombre_equipo = ''
	BEGIN
		SET @mensaje = 'El nombre del equipo es obligatorio.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @actividades)
	BEGIN
		SET @mensaje = 'Elija al menos una actividad donde se usa el equipo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @actividades t WHERE NOT EXISTS (SELECT 1 FROM actividades a WHERE a.actividad_id = t.actividad_id))
	BEGIN
		SET @mensaje = 'Hay una actividad que no existe.';
		RETURN;
	END

	INSERT INTO actividad_equipos (actividad_id, nombre_equipo)
	SELECT t.actividad_id, @nombre_equipo
	FROM @actividades t
	WHERE NOT EXISTS (SELECT 1 FROM actividad_equipos x WHERE x.actividad_id = t.actividad_id AND x.nombre_equipo = @nombre_equipo);

	SET @resultado = 1;
	SET @mensaje = 'Equipo ligado a sus actividades.';
END
GO

CREATE OR ALTER PROCEDURE sp_RegistrarIngresoInventario
	@proveedor_id INT,
	@firma_empleado_id INT,
	@numero_factura VARCHAR(50),
	@fecha_compra DATE,
	@productos dbo.tipo_ingreso_productos READONLY,
	@equipos dbo.tipo_ingreso_equipos READONLY,
	@compra_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @compra_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @numero_factura = LTRIM(RTRIM(ISNULL(@numero_factura, '')));

	IF NOT EXISTS (SELECT 1 FROM proveedores WHERE proveedor_id = @proveedor_id)
	BEGIN
		SET @mensaje = 'El proveedor no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM empleados WHERE empleado_id = @firma_empleado_id)
	BEGIN
		SET @mensaje = 'El empleado que recibe no existe.';
		RETURN;
	END

	IF @numero_factura = ''
	BEGIN
		SET @mensaje = 'El número de factura es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM compras WHERE numero_factura = @numero_factura)
	BEGIN
		SET @mensaje = 'Ya existe una compra con ese número de factura.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @productos) AND NOT EXISTS (SELECT 1 FROM @equipos)
	BEGIN
		SET @mensaje = 'La lista a ingresar está vacía.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos WHERE ISNULL(cantidad, 0) <= 0 OR ISNULL(precio_unitario, -1) < 0)
	   OR EXISTS (SELECT 1 FROM @equipos WHERE ISNULL(cantidad, 0) <= 0 OR ISNULL(precio_unitario, -1) < 0)
	BEGIN
		SET @mensaje = 'Hay cantidades o precios no válidos en la lista.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos x WHERE NOT EXISTS (SELECT 1 FROM productos p WHERE p.producto_id = x.producto_id))
	BEGIN
		SET @mensaje = 'Uno de los productos de la lista no existe.';
		RETURN;
	END

	DECLARE @estado_disponible INT = (SELECT TOP 1 estado_equipo_id FROM estados_equipo
									 WHERE nombre_estado_equipo = 'Disponible');

	DECLARE @lista TABLE
	(
		fila INT IDENTITY(1,1),
		nombre_equipo VARCHAR(50),
		tipo_equipo VARCHAR(50),
		marca_equipo VARCHAR(50),
		modelo_equipo VARCHAR(50),
		cantidad INT,
		precio_unitario MONEY
	);

	INSERT INTO @lista (nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo, cantidad, precio_unitario)
	SELECT nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo, cantidad, precio_unitario
	FROM @equipos;

	BEGIN TRY
		BEGIN TRANSACTION;

		INSERT INTO compras (proveedor_id, firma_empleado_id, numero_factura, fecha_compra)
		VALUES (@proveedor_id, @firma_empleado_id, @numero_factura, @fecha_compra);

		SET @compra_id = CAST(SCOPE_IDENTITY() AS INT);

		INSERT INTO compras_productos (compra_id, producto_id, cantidad, precio_unitario)
		SELECT @compra_id, producto_id, cantidad, precio_unitario
		FROM @productos;

		UPDATE p
		SET p.stock = ISNULL(p.stock, 0) + x.cantidad
		FROM productos p
		INNER JOIN (SELECT producto_id, SUM(cantidad) AS cantidad
					FROM @productos
					GROUP BY producto_id) x ON x.producto_id = p.producto_id;

		DECLARE @fila INT = 1;
		DECLARE @total_filas INT = (SELECT COUNT(1) FROM @lista);
		DECLARE @unidad INT;
		DECLARE @cantidad INT;
		DECLARE @precio MONEY;
		DECLARE @equipo_id INT;

		WHILE @fila <= @total_filas
		BEGIN
			SELECT @cantidad = cantidad, @precio = precio_unitario FROM @lista WHERE fila = @fila;
			SET @unidad = 1;

			WHILE @unidad <= @cantidad
			BEGIN
				INSERT INTO equipos (nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo, estado_equipo_id)
				SELECT nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo, @estado_disponible
				FROM @lista WHERE fila = @fila;

				SET @equipo_id = CAST(SCOPE_IDENTITY() AS INT);

				INSERT INTO compras_equipos (compra_id, equipo_id, precio_unitario)
				VALUES (@compra_id, @equipo_id, @precio);

				SET @unidad = @unidad + 1;
			END

			SET @fila = @fila + 1;
		END

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Ingreso registrado correctamente (factura ' + @numero_factura + ').';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @compra_id = NULL;
		SET @resultado = 0;
		SET @mensaje = LEFT('No se pudo registrar el ingreso: ' + ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosInventario
AS
BEGIN
	SET NOCOUNT ON;

	SELECT empleado_id,
		   LTRIM(RTRIM(ISNULL(nombre_empleado, '') + ' ' + ISNULL(apellido_empleado, ''))) AS nombre_completo
	FROM empleados
	WHERE estado_empleado = 1
	ORDER BY nombre_empleado, apellido_empleado;
END
GO

-- Facturación y cotizaciones

CREATE OR ALTER PROCEDURE sp_ListarFormasPago
AS
BEGIN
	SET NOCOUNT ON;

	SELECT forma_pago_id, nombre_forma_pago
	FROM formas_pago
	ORDER BY forma_pago_id;
END
GO

-- Las líneas de la orden salen de su cotización (una por actividad). Sin cotización, de las actividades.
CREATE OR ALTER PROCEDURE sp_DetalleOrdenFactura
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT 1 FROM ordenes o
			   INNER JOIN detalles_cotizacion d ON d.cotizacion_id = o.cotizacion_id
			   WHERE o.orden_id = @orden_id)
	BEGIN
		SELECT d.concepto,
			   d.cantidad,
			   CAST(d.precio_unitario AS DECIMAL(12, 2)) AS precio_unitario
		FROM ordenes o
		INNER JOIN detalles_cotizacion d ON d.cotizacion_id = o.cotizacion_id
		WHERE o.orden_id = @orden_id
		ORDER BY d.linea;
	END
	ELSE
	BEGIN
		SELECT ISNULL(a.descripcion, '') +
			   CASE WHEN ISNULL(a.frecuencia, '') = '' THEN '' ELSE ' (' + a.frecuencia + ')' END AS concepto,
			   1 AS cantidad,
			   CAST(ISNULL(a.precio, 0) AS DECIMAL(12, 2)) AS precio_unitario
		FROM orden_actividades oa
		INNER JOIN actividades a ON a.actividad_id = oa.actividad_id
		WHERE oa.orden_id = @orden_id
		ORDER BY a.descripcion;
	END
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarCotizacion
	@identificacion_cliente VARCHAR(30),
	@nombre_empresa VARCHAR(50),
	@nombre_cliente VARCHAR(50),
	@apellido_cliente VARCHAR(50),
	@telefono_cliente VARCHAR(50),
	@correo_cliente VARCHAR(50),
	@locacion_id INT = NULL,
	@direccion VARCHAR(80),
	@fecha_cotizacion DATE,
	@valida_hasta DATE,
	@observacion VARCHAR(200),
	@servicio_id INT,
	@lineas dbo.tipo_lista_medidas READONLY,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT,
	@cotizacion_id VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @cotizacion_id = '';

	SET @nombre_cliente = LTRIM(RTRIM(ISNULL(@nombre_cliente, '')));
	SET @identificacion_cliente = REPLACE(REPLACE(LTRIM(RTRIM(ISNULL(@identificacion_cliente, ''))), '-', ''), ' ', '');

	IF @nombre_cliente = ''
	BEGIN
		SET @mensaje = 'El nombre del contacto es obligatorio.';
		RETURN;
	END

	IF @locacion_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM locaciones WHERE locacion_id = @locacion_id)
	BEGIN
		SET @mensaje = 'La locacion no existe.';
		RETURN;
	END

	IF @valida_hasta < @fecha_cotizacion
	BEGIN
		SET @mensaje = 'La fecha de validez no puede ser anterior a la cotizacion.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM servicios WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'El servicio no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @lineas)
	BEGIN
		SET @mensaje = 'La cotizacion necesita al menos una actividad.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @lineas WHERE medida <= 0)
	BEGIN
		SET @mensaje = 'Todas las actividades necesitan una medida mayor que 0.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @lineas x WHERE NOT EXISTS (SELECT 1 FROM actividades a WHERE a.actividad_id = x.actividad_id))
	BEGIN
		SET @mensaje = 'Una de las actividades no existe.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @siguiente INT;
		DECLARE @tasa DECIMAL(6, 4) = ISNULL((SELECT valor FROM parametros_costeo WHERE clave = 'ISV'), 0.15);
		DECLARE @subtotal DECIMAL(12, 2) = (SELECT ISNULL(SUM(precio), 0) FROM dbo.fn_CotizarLineas(@lineas));
		DECLARE @total DECIMAL(12, 2) = @subtotal + ROUND(@subtotal * @tasa, 2);

		SELECT @siguiente = ISNULL(MAX(TRY_CAST(SUBSTRING(cotizacion_id, 5, 20) AS INT)), 0) + 1
		FROM cotizaciones WITH (UPDLOCK, HOLDLOCK)
		WHERE cotizacion_id LIKE 'COT-%';

		SET @cotizacion_id = 'COT-' + RIGHT('000000' + CAST(@siguiente AS VARCHAR(10)), 6);

		INSERT INTO cotizaciones (cotizacion_id, identificacion_cliente, nombre_empresa, nombre_cliente,
								  apellido_cliente, telefono_cliente, correo_cliente, locacion_id, direccion,
								  fecha_cotizacion, valida_hasta, observacion, precio_cotizado, servicio_id)
		VALUES (@cotizacion_id, NULLIF(LTRIM(RTRIM(@identificacion_cliente)), ''), NULLIF(@nombre_empresa, ''),
				@nombre_cliente, NULLIF(@apellido_cliente, ''), NULLIF(@telefono_cliente, ''),
				NULLIF(@correo_cliente, ''), @locacion_id, @direccion,
				@fecha_cotizacion, @valida_hasta, NULLIF(@observacion, ''), @total, @servicio_id);

		INSERT INTO detalles_cotizacion (cotizacion_id, linea, concepto, cantidad, precio_unitario,
										actividad_id, medida, cantidad_personas)
		SELECT @cotizacion_id, ROW_NUMBER() OVER (ORDER BY actividad_id), concepto, 1, precio,
			   actividad_id, medida, cantidad_personas
		FROM dbo.fn_CotizarLineas(@lineas);

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Cotizacion registrada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @cotizacion_id = '';
		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Contado (1 cuota): factura pagada. De 6 a 24 cuotas: crédito con cuenta por cobrar y cuotas mensuales.
CREATE OR ALTER PROCEDURE sp_InsertarFactura
	@orden_id VARCHAR(50),
	@fecha_factura DATE,
	@forma_pago_id INT,
	@numero_cuotas INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT,
	@factura_id INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @factura_id = NULL;

	IF NOT EXISTS (SELECT 1 FROM ordenes o
				   INNER JOIN estados_orden e ON e.estado_orden_id = o.estado_orden_id
				   WHERE o.orden_id = @orden_id AND e.nombre_estado_orden = 'Completada')
	BEGIN
		SET @mensaje = 'La orden no existe o no esta completada.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM facturas WHERE orden_id = @orden_id)
	BEGIN
		SET @mensaje = 'Esta orden ya fue facturada.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM formas_pago WHERE forma_pago_id = @forma_pago_id)
	BEGIN
		SET @mensaje = 'El metodo de pago no existe.';
		RETURN;
	END

	IF @numero_cuotas < 1 OR @numero_cuotas > 24
	BEGIN
		SET @mensaje = 'El numero de cuotas debe estar entre 1 y 24.';
		RETURN;
	END

	CREATE TABLE #lineas (concepto VARCHAR(150), cantidad INT, precio_unitario DECIMAL(12, 2));
	INSERT INTO #lineas EXEC sp_DetalleOrdenFactura @orden_id;

	IF NOT EXISTS (SELECT 1 FROM #lineas)
	BEGIN
		SET @mensaje = 'La orden no tiene servicios para facturar.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @tasa DECIMAL(6, 4) = ISNULL((SELECT valor FROM parametros_costeo WHERE clave = 'ISV'), 0.15);
		DECLARE @subtotal DECIMAL(12, 2) = (SELECT ISNULL(SUM(cantidad * precio_unitario), 0) FROM #lineas);
		DECLARE @total DECIMAL(12, 2) = @subtotal + ROUND(@subtotal * @tasa, 2);
		DECLARE @contado BIT = CASE WHEN @numero_cuotas = 1 THEN 1 ELSE 0 END;

		INSERT INTO facturas (orden_id, fecha_factura, tipo_pago, estado_pago, fecha_pago,
							  forma_pago_id, precio_facturado)
		VALUES (@orden_id, @fecha_factura,
				CASE WHEN @contado = 1 THEN 'Contado' ELSE 'Credito' END,
				CASE WHEN @contado = 1 THEN 'Pagada' ELSE 'Pendiente' END,
				CASE WHEN @contado = 1 THEN @fecha_factura ELSE NULL END,
				@forma_pago_id, @total);

		SET @factura_id = CAST(SCOPE_IDENTITY() AS INT);

		INSERT INTO detalles_factura (factura_id, linea, concepto, cantidad, precio_unitario, adicional)
		SELECT @factura_id, ROW_NUMBER() OVER (ORDER BY (SELECT 1)), concepto, cantidad, precio_unitario, 0
		FROM #lineas;

		IF @contado = 0
		BEGIN
			INSERT INTO cuentas_por_cobrar (factura_id, monto_total, prima, numero_cuotas,
											frecuencia, fecha_inicio, estado_cuenta)
			VALUES (@factura_id, @total, 0, @numero_cuotas, 'Mensual', @fecha_factura, 'Vigente');

			DECLARE @cuenta_id INT = CAST(SCOPE_IDENTITY() AS INT);
			DECLARE @cuota DECIMAL(12, 2) = ROUND(@total / @numero_cuotas, 2);

			-- La última cuota absorbe la diferencia por redondeo
			;WITH n AS
			(
				SELECT 1 AS num
				UNION ALL
				SELECT num + 1 FROM n WHERE num < @numero_cuotas
			)
			INSERT INTO cuotas (cuenta_id, numero_cuota, fecha_vencimiento, monto_cuota, monto_pagado, estado_cuota)
			SELECT @cuenta_id, num, DATEADD(MONTH, num, @fecha_factura),
				   CASE WHEN num = @numero_cuotas THEN @total - @cuota * (@numero_cuotas - 1) ELSE @cuota END,
				   0, 'Pendiente'
			FROM n;
		END

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Factura registrada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @factura_id = NULL;
		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarFacturas
	@texto VARCHAR(50) = NULL,
	@estado_pago VARCHAR(50) = NULL,
	@desde DATE = NULL,
	@hasta DATE = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @estado_pago = LTRIM(RTRIM(ISNULL(@estado_pago, '')));

	SELECT f.factura_id,
		   f.orden_id,
		   o.identificacion_cliente,
		   ISNULL(c.nombre_empresa, o.identificacion_cliente) AS cliente,
		   f.fecha_factura,
		   f.tipo_pago,
		   f.estado_pago,
		   f.fecha_pago,
		   fp.nombre_forma_pago,
		   f.precio_facturado
	FROM facturas f
	INNER JOIN ordenes o ON o.orden_id = f.orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN formas_pago fp ON fp.forma_pago_id = f.forma_pago_id
	WHERE (@texto = '' OR CAST(f.factura_id AS VARCHAR(10)) = @texto
		   OR f.orden_id LIKE '%' + @texto + '%'
		   OR o.identificacion_cliente LIKE '%' + @texto + '%'
		   OR c.nombre_empresa LIKE '%' + @texto + '%')
	  AND (@estado_pago = '' OR f.estado_pago = @estado_pago)
	  AND (@desde IS NULL OR f.fecha_factura >= @desde)
	  AND (@hasta IS NULL OR f.fecha_factura <= @hasta)
	ORDER BY f.fecha_factura DESC, f.factura_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.factura_id,
		   f.orden_id,
		   f.fecha_factura,
		   f.tipo_pago,
		   f.estado_pago,
		   f.fecha_pago,
		   ISNULL(fp.nombre_forma_pago, '') AS nombre_forma_pago,
		   ISNULL(f.precio_facturado, 0) AS precio_facturado,
		   ISNULL(o.identificacion_cliente, '') AS identificacion_cliente,
		   ISNULL(c.nombre_empresa, '') AS nombre_empresa,
		   ISNULL(c.nombre_cliente, '') AS nombre_cliente,
		   ISNULL(c.apellido_cliente, '') AS apellido_cliente,
		   ISNULL(c.telefono_cliente, '') AS telefono_cliente,
		   ISNULL(c.correo_cliente, '') AS correo_cliente,
		   ISNULL(l.nombre_locacion, '') AS nombre_locacion,
		   ISNULL(l.tipo_locacion, '') AS tipo_locacion,
		   ISNULL(l.direccion, '') AS direccion,
		   pg.pagado,
		   ISNULL(f.precio_facturado, 0) - pg.pagado AS saldo
	FROM facturas f
	INNER JOIN ordenes o ON o.orden_id = f.orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN locaciones l ON l.locacion_id = o.locacion_id
	LEFT JOIN formas_pago fp ON fp.forma_pago_id = f.forma_pago_id
	CROSS APPLY (SELECT CASE WHEN f.tipo_pago = 'Credito'
							 THEN ISNULL((SELECT SUM(ISNULL(q.monto_pagado, 0))
										  FROM cuentas_por_cobrar cc
										  INNER JOIN cuotas q ON q.cuenta_id = cc.cuenta_id
										  WHERE cc.factura_id = f.factura_id), 0)
							 WHEN f.estado_pago = 'Pagada' THEN ISNULL(f.precio_facturado, 0)
							 ELSE 0 END AS pagado) pg
	WHERE f.factura_id = @factura_id;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT linea,
		   CASE WHEN adicional = 1 THEN 'Adicional' ELSE 'Servicio' END AS tipo,
		   concepto,
		   cantidad,
		   precio_unitario,
		   CAST(cantidad * precio_unitario AS DECIMAL(12, 2)) AS subtotal
	FROM detalles_factura
	WHERE factura_id = @factura_id
	ORDER BY linea;
END
GO

CREATE OR ALTER PROCEDURE sp_CuotasFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT q.cuota_id,
		   q.numero_cuota,
		   q.fecha_vencimiento,
		   q.monto_cuota,
		   ISNULL(q.monto_pagado, 0) AS monto_pagado,
		   q.monto_cuota - ISNULL(q.monto_pagado, 0) AS saldo,
		   q.estado_cuota
	FROM cuentas_por_cobrar cpc
	INNER JOIN cuotas q ON q.cuenta_id = cpc.cuenta_id
	WHERE cpc.factura_id = @factura_id
	ORDER BY q.numero_cuota;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerOrdenFactura
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.fecha_servicio,
		   ISNULL(o.cotizacion_id, '') AS cotizacion_id,
		   ISNULL(o.identificacion_cliente, '') AS identificacion_cliente,
		   ISNULL(c.nombre_empresa, '') AS nombre_empresa,
		   ISNULL(c.nombre_cliente, '') AS nombre_cliente,
		   ISNULL(c.apellido_cliente, '') AS apellido_cliente,
		   ISNULL(c.telefono_cliente, '') AS telefono_cliente,
		   ISNULL(c.correo_cliente, '') AS correo_cliente,
		   ISNULL(l.nombre_locacion, '') AS nombre_locacion,
		   ISNULL(l.tipo_locacion, '') AS tipo_locacion,
		   ISNULL(l.direccion, '') AS direccion,
		   ISNULL(s.nombre_servicio, '') AS nombre_servicio,
		   CASE WHEN eo.nombre_estado_orden = 'Completada'
				 AND NOT EXISTS (SELECT 1 FROM facturas f WHERE f.orden_id = o.orden_id)
				THEN 1 ELSE 0 END AS facturable
	FROM ordenes o
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN locaciones l ON l.locacion_id = o.locacion_id
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	WHERE o.orden_id = @orden_id;
END
GO

-- Cómo quedan las cuotas si se abona un monto: el abono cubre primero las cuotas más antiguas.
CREATE OR ALTER FUNCTION dbo.fn_AplicarAbono (@factura_id INT, @monto MONEY)
RETURNS TABLE
AS
RETURN
(
	WITH c AS
	(
		SELECT q.cuota_id, q.numero_cuota, q.fecha_vencimiento, q.monto_cuota,
			   ISNULL(q.monto_pagado, 0) AS pagado,
			   q.monto_cuota - ISNULL(q.monto_pagado, 0) AS pendiente
		FROM cuotas q
		INNER JOIN cuentas_por_cobrar cc ON cc.cuenta_id = q.cuenta_id
		WHERE cc.factura_id = @factura_id
	),
	a AS
	(
		SELECT c.*,
			   ISNULL(SUM(c.pendiente) OVER (ORDER BY c.numero_cuota
											 ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS antes
		FROM c
	),
	b AS
	(
		SELECT a.*,
			   CASE WHEN @monto - a.antes <= 0 THEN 0
					WHEN @monto - a.antes >= a.pendiente THEN a.pendiente
					ELSE @monto - a.antes END AS aplicado
		FROM a
	)
	SELECT b.cuota_id, b.numero_cuota, b.fecha_vencimiento, b.monto_cuota, b.pagado,
		   b.aplicado,
		   b.pagado + b.aplicado AS nuevo_pagado,
		   b.monto_cuota - b.pagado - b.aplicado AS nuevo_saldo,
		   CASE WHEN b.pagado + b.aplicado >= b.monto_cuota THEN 'Pagada'
				WHEN b.fecha_vencimiento < CAST(GETDATE() AS DATE) THEN 'Vencida'
				WHEN b.pagado + b.aplicado > 0 THEN 'Parcial'
				ELSE 'Pendiente' END AS nuevo_estado
	FROM b
);
GO

CREATE OR ALTER PROCEDURE sp_SimularAbono
	@factura_id INT,
	@monto MONEY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT numero_cuota, fecha_vencimiento, monto_cuota, pagado, aplicado, nuevo_pagado, nuevo_saldo, nuevo_estado
	FROM dbo.fn_AplicarAbono(@factura_id, @monto)
	ORDER BY numero_cuota;
END
GO

-- Deja la cuenta y la factura en su estado correcto despues de un pago.
CREATE OR ALTER PROCEDURE sp_RecalcularEstadoFactura
	@factura_id INT,
	@fecha DATE
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @cuenta_id INT = (SELECT cuenta_id FROM cuentas_por_cobrar WHERE factura_id = @factura_id);

	IF NOT EXISTS (SELECT 1 FROM cuotas WHERE cuenta_id = @cuenta_id AND estado_cuota <> 'Pagada')
	BEGIN
		UPDATE cuentas_por_cobrar SET estado_cuenta = 'Pagada' WHERE cuenta_id = @cuenta_id;

		UPDATE facturas
		SET estado_pago = 'Pagada', fecha_pago = @fecha
		WHERE factura_id = @factura_id;
	END
	ELSE IF EXISTS (SELECT 1 FROM cuotas WHERE cuenta_id = @cuenta_id AND estado_cuota = 'Vencida')
		UPDATE cuentas_por_cobrar SET estado_cuenta = 'Vencida' WHERE cuenta_id = @cuenta_id;
	ELSE
		UPDATE cuentas_por_cobrar SET estado_cuenta = 'Vigente' WHERE cuenta_id = @cuenta_id;
END
GO

-- El abono usa el metodo de pago que se eligio al crear la factura.
CREATE OR ALTER PROCEDURE sp_RegistrarAbono
	@factura_id INT,
	@monto MONEY,
	@fecha_abono DATE = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @fecha_abono = ISNULL(@fecha_abono, CAST(GETDATE() AS DATE));

	DECLARE @tipo VARCHAR(10), @estado VARCHAR(50), @forma_pago_id INT, @saldo MONEY;

	SELECT @tipo = tipo_pago, @estado = estado_pago, @forma_pago_id = forma_pago_id
	FROM facturas WHERE factura_id = @factura_id;

	IF @tipo IS NULL
	BEGIN
		SET @mensaje = 'La factura no existe.';
		RETURN;
	END

	IF @tipo <> 'Credito'
	BEGIN
		SET @mensaje = 'Solo las facturas a credito reciben abonos.';
		RETURN;
	END

	IF @estado = 'Pagada'
	BEGIN
		SET @mensaje = 'La factura ya esta pagada.';
		RETURN;
	END

	SELECT @saldo = SUM(q.monto_cuota - ISNULL(q.monto_pagado, 0))
	FROM cuentas_por_cobrar cc
	INNER JOIN cuotas q ON q.cuenta_id = cc.cuenta_id
	WHERE cc.factura_id = @factura_id;

	IF ISNULL(@monto, 0) <= 0
	BEGIN
		SET @mensaje = 'El abono debe ser mayor que 0.';
		RETURN;
	END

	IF @monto > @saldo
	BEGIN
		SET @mensaje = 'El abono supera el saldo pendiente: ' + CAST(@saldo AS VARCHAR(20)) + '.';
		RETURN;
	END

	DECLARE @nuevas TABLE (cuota_id INT, aplicado MONEY, nuevo_pagado MONEY, nuevo_estado VARCHAR(20));

	INSERT INTO @nuevas
	SELECT cuota_id, aplicado, nuevo_pagado, nuevo_estado
	FROM dbo.fn_AplicarAbono(@factura_id, @monto);

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE q
		SET q.monto_pagado = n.nuevo_pagado,
			q.estado_cuota = n.nuevo_estado,
			q.fecha_pago = @fecha_abono,
			q.forma_pago_id = @forma_pago_id
		FROM cuotas q
		INNER JOIN @nuevas n ON n.cuota_id = q.cuota_id
		WHERE n.aplicado > 0;

		INSERT INTO abonos (factura_id, fecha_abono, monto, forma_pago_id, saldo_despues, concepto)
		VALUES (@factura_id, @fecha_abono, @monto, @forma_pago_id, @saldo - @monto, 'Abono');

		EXEC sp_RecalcularEstadoFactura @factura_id, @fecha_abono;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'El abono se registro correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Salda de un solo clic lo que falta de una cuota, con el metodo de pago de la factura.
CREATE OR ALTER PROCEDURE sp_SaldarCuota
	@cuota_id INT,
	@fecha_pago DATE = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @fecha_pago = ISNULL(@fecha_pago, CAST(GETDATE() AS DATE));

	DECLARE @factura_id INT, @forma_pago_id INT, @numero INT, @saldo MONEY, @saldo_total MONEY;

	SELECT @factura_id = cc.factura_id,
		   @forma_pago_id = f.forma_pago_id,
		   @numero = q.numero_cuota,
		   @saldo = q.monto_cuota - ISNULL(q.monto_pagado, 0)
	FROM cuotas q
	INNER JOIN cuentas_por_cobrar cc ON cc.cuenta_id = q.cuenta_id
	INNER JOIN facturas f ON f.factura_id = cc.factura_id
	WHERE q.cuota_id = @cuota_id;

	IF @factura_id IS NULL
	BEGIN
		SET @mensaje = 'La cuota no existe.';
		RETURN;
	END

	IF @saldo <= 0
	BEGIN
		SET @mensaje = 'La cuota ya esta pagada.';
		RETURN;
	END

	SELECT @saldo_total = SUM(q.monto_cuota - ISNULL(q.monto_pagado, 0))
	FROM cuentas_por_cobrar cc
	INNER JOIN cuotas q ON q.cuenta_id = cc.cuenta_id
	WHERE cc.factura_id = @factura_id;

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE cuotas
		SET monto_pagado = monto_cuota,
			estado_cuota = 'Pagada',
			fecha_pago = @fecha_pago,
			forma_pago_id = @forma_pago_id
		WHERE cuota_id = @cuota_id;

		INSERT INTO abonos (factura_id, fecha_abono, monto, forma_pago_id, saldo_despues, concepto)
		VALUES (@factura_id, @fecha_pago, @saldo, @forma_pago_id, @saldo_total - @saldo,
				'Cuota ' + CAST(@numero AS VARCHAR(5)));

		EXEC sp_RecalcularEstadoFactura @factura_id, @fecha_pago;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'La cuota ' + CAST(@numero AS VARCHAR(5)) + ' quedo saldada.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ListarAbonosFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.fecha_abono, a.concepto, a.monto, ISNULL(fp.nombre_forma_pago, '') AS nombre_forma_pago, a.saldo_despues
	FROM abonos a
	LEFT JOIN formas_pago fp ON fp.forma_pago_id = a.forma_pago_id
	WHERE a.factura_id = @factura_id
	ORDER BY a.abono_id;
END
GO

CREATE OR ALTER PROCEDURE sp_PagarFactura
	@factura_id INT,
	@forma_pago_id INT,
	@fecha_pago DATE = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @fecha_pago = ISNULL(@fecha_pago, CAST(GETDATE() AS DATE));

	DECLARE @tipo VARCHAR(10), @estado VARCHAR(50);

	SELECT @tipo = tipo_pago, @estado = estado_pago FROM facturas WHERE factura_id = @factura_id;

	IF @tipo IS NULL
	BEGIN
		SET @mensaje = 'La factura no existe.';
		RETURN;
	END

	IF @tipo = 'Credito'
	BEGIN
		SET @mensaje = 'Las facturas a credito se pagan por cuotas.';
		RETURN;
	END

	IF @estado = 'Pagada'
	BEGIN
		SET @mensaje = 'La factura ya esta pagada.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM formas_pago WHERE forma_pago_id = @forma_pago_id)
	BEGIN
		SET @mensaje = 'La forma de pago no existe.';
		RETURN;
	END

	UPDATE facturas
	SET estado_pago = 'Pagada',
		fecha_pago = @fecha_pago,
		forma_pago_id = @forma_pago_id
	WHERE factura_id = @factura_id;

	SET @resultado = 1;
	SET @mensaje = 'El pago se registro correctamente.';
END
GO

-- Cuentas por cobrar y cuotas

CREATE OR ALTER PROCEDURE sp_ListarCuotasPorFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT q.cuota_id,
		   q.numero_cuota,
		   q.fecha_vencimiento,
		   q.monto_cuota,
		   ISNULL(q.monto_pagado, 0) AS monto_pagado,
		   q.monto_cuota - ISNULL(q.monto_pagado, 0) AS saldo,
		   q.fecha_pago,
		   fp.nombre_forma_pago,
		   q.estado_cuota
	FROM cuotas q
	INNER JOIN cuentas_por_cobrar cc ON cc.cuenta_id = q.cuenta_id
	LEFT JOIN formas_pago fp ON fp.forma_pago_id = q.forma_pago_id
	WHERE cc.factura_id = @factura_id
	ORDER BY q.numero_cuota;
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarCuotasVencidas
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

	UPDATE cuotas
	SET estado_cuota = 'Vencida'
	WHERE estado_cuota IN ('Pendiente', 'Parcial')
	  AND fecha_vencimiento < @hoy;

	UPDATE cuentas_por_cobrar
	SET estado_cuenta = 'Vencida'
	WHERE estado_cuenta = 'Vigente'
	  AND EXISTS (SELECT 1 FROM cuotas q WHERE q.cuenta_id = cuentas_por_cobrar.cuenta_id
				  AND q.estado_cuota = 'Vencida');
END
GO

CREATE OR ALTER PROCEDURE sp_ListarCuentasPorCobrar
	@texto VARCHAR(50) = NULL,
	@estado_cuenta VARCHAR(20) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @estado_cuenta = LTRIM(RTRIM(ISNULL(@estado_cuenta, '')));

	SELECT cc.cuenta_id,
		   cc.factura_id,
		   f.orden_id,
		   ISNULL(c.nombre_empresa, o.identificacion_cliente) AS cliente,
		   cc.monto_total,
		   ISNULL(cc.prima, 0) AS prima,
		   cc.numero_cuotas,
		   cc.frecuencia,
		   cc.estado_cuenta,
		   ISNULL((SELECT SUM(q.monto_pagado) FROM cuotas q WHERE q.cuenta_id = cc.cuenta_id), 0) AS pagado_cuotas,
		   (SELECT SUM(q.monto_cuota - ISNULL(q.monto_pagado, 0)) FROM cuotas q WHERE q.cuenta_id = cc.cuenta_id) AS saldo,
		   (SELECT MIN(q.fecha_vencimiento) FROM cuotas q
			WHERE q.cuenta_id = cc.cuenta_id AND q.estado_cuota <> 'Pagada') AS proximo_vencimiento
	FROM cuentas_por_cobrar cc
	INNER JOIN facturas f ON f.factura_id = cc.factura_id
	INNER JOIN ordenes o ON o.orden_id = f.orden_id
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	WHERE (@texto = '' OR f.orden_id LIKE '%' + @texto + '%'
		   OR o.identificacion_cliente LIKE '%' + @texto + '%'
		   OR c.nombre_empresa LIKE '%' + @texto + '%')
	  AND (@estado_cuenta = '' OR cc.estado_cuenta = @estado_cuenta)
	ORDER BY cc.cuenta_id DESC;
END
GO

-- Bitácora

CREATE OR ALTER PROCEDURE sp_ListarBitacora
	@desde DATE = NULL,
	@hasta DATE = NULL,
	@usuario_id INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT b.bitacora_id,
		   b.fecha_bitacora,
		   u.usuario,
		   b.accion,
		   b.tabla,
		   b.equipo_sistema
	FROM bitacoras b
	LEFT JOIN usuarios u ON u.usuario_id = b.usuario_id
	WHERE (@desde IS NULL OR b.fecha_bitacora >= @desde)
	  AND (@hasta IS NULL OR b.fecha_bitacora < DATEADD(DAY, 1, @hasta))
	  AND (@usuario_id IS NULL OR b.usuario_id = @usuario_id)
	ORDER BY b.fecha_bitacora DESC, b.bitacora_id DESC;
END
GO

-- Reportes

CREATE OR ALTER PROCEDURE sp_ReporteOrdenes
	@desde DATE,
	@hasta DATE,
	@estado_orden_id INT = NULL,
	@servicio_id INT = NULL,
	@cliente VARCHAR(50) = ''
AS
BEGIN
	SET NOCOUNT ON;

	SET @cliente = LTRIM(RTRIM(ISNULL(@cliente, '')));

	SELECT o.orden_id,
		   ISNULL(c.nombre_empresa, o.identificacion_cliente) AS cliente,
		   ISNULL(s.nombre_servicio, 'Sin servicio') AS nombre_servicio,
		   LTRIM(ISNULL(sup.nombre_empleado, '') + ' ' + ISNULL(sup.apellido_empleado, '')) AS supervisor,
		   COALESCE(o.fecha_servicio, o.fecha_solicitud) AS fecha,
		   ISNULL(eo.nombre_estado_orden, 'Sin estado') AS nombre_estado_orden,
		   o.conformidad_cliente,
		   CASE WHEN o.conformidad_cliente IS NULL THEN 'Sin evaluar'
				WHEN o.conformidad_cliente = 1 THEN 'Conforme'
				ELSE 'No conforme' END AS conformidad
	FROM ordenes o
	LEFT JOIN clientes c ON c.identificacion_cliente = o.identificacion_cliente
	LEFT JOIN servicios s ON s.servicio_id = o.servicio_id
	LEFT JOIN empleados sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN estados_orden eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE COALESCE(o.fecha_servicio, o.fecha_solicitud) BETWEEN @desde AND @hasta
	  AND (@estado_orden_id IS NULL OR o.estado_orden_id = @estado_orden_id)
	  AND (@servicio_id IS NULL OR o.servicio_id = @servicio_id)
	  AND (@cliente = ''
		   OR c.nombre_empresa LIKE '%' + @cliente + '%'
		   OR (c.nombre_cliente + ' ' + c.apellido_cliente) LIKE '%' + @cliente + '%'
		   OR o.identificacion_cliente LIKE '%' + @cliente + '%')
	ORDER BY COALESCE(o.fecha_servicio, o.fecha_solicitud), o.orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteAsistencia
	@desde DATE,
	@hasta DATE,
	@empleado_id INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.empleado_id,
		   LTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')) AS empleado,
		   ISNULL(c.nombre_cargo, 'Sin cargo') AS nombre_cargo,
		   a.fecha_asistencia,
		   CONVERT(VARCHAR(5), a.hora_entrada, 108) AS hora_entrada,
		   CONVERT(VARCHAR(5), a.hora_salida, 108) AS hora_salida,
		   CASE WHEN a.hora_entrada IS NOT NULL AND a.hora_salida IS NOT NULL
				THEN CAST(DATEDIFF(MINUTE, a.hora_entrada, a.hora_salida) / 60.0 AS DECIMAL(6, 2))
				ELSE 0 END AS horas
	FROM asistencias a
	INNER JOIN empleados e ON e.empleado_id = a.empleado_id
	LEFT JOIN cargos c ON c.cargo_id = e.cargo_id
	WHERE a.fecha_asistencia BETWEEN @desde AND @hasta
	  AND (@empleado_id IS NULL OR a.empleado_id = @empleado_id)
	ORDER BY a.fecha_asistencia, e.nombre_empleado, e.apellido_empleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteEmpleados
	@desde DATE,
	@hasta DATE,
	@cargo_id INT = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id,
		   LTRIM(ISNULL(e.nombre_empleado, '') + ' ' + ISNULL(e.apellido_empleado, '')) AS nombre_completo,
		   ISNULL(c.nombre_cargo, 'Sin cargo') AS nombre_cargo,
		   CASE WHEN e.estado_empleado = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado,
		   (SELECT COUNT(*)
			FROM orden_empleados oe
			INNER JOIN ordenes o ON o.orden_id = oe.orden_id
			WHERE oe.empleado_id = e.empleado_id
			  AND o.fecha_servicio BETWEEN @desde AND @hasta) AS ordenes,
		   (SELECT COUNT(*)
			FROM asistencias a
			WHERE a.empleado_id = e.empleado_id
			  AND a.fecha_asistencia BETWEEN @desde AND @hasta) AS dias,
		   (SELECT CAST(ISNULL(SUM(CASE WHEN a.hora_entrada IS NOT NULL AND a.hora_salida IS NOT NULL
										 THEN DATEDIFF(MINUTE, a.hora_entrada, a.hora_salida)
										 ELSE 0 END), 0) / 60.0 AS DECIMAL(10, 2))
			FROM asistencias a
			WHERE a.empleado_id = e.empleado_id
			  AND a.fecha_asistencia BETWEEN @desde AND @hasta) AS horas
	FROM empleados e
	LEFT JOIN cargos c ON c.cargo_id = e.cargo_id
	WHERE (@cargo_id IS NULL OR e.cargo_id = @cargo_id)
	  AND (@estado IS NULL OR e.estado_empleado = @estado)
	ORDER BY e.nombre_empleado, e.apellido_empleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteFacturacion
	@desde DATE,
	@hasta DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.tipo_pago,
		   f.estado_pago,
		   COUNT(1) AS facturas,
		   ISNULL(SUM(f.precio_facturado), 0) AS total
	FROM facturas f
	WHERE f.fecha_factura BETWEEN @desde AND @hasta
	GROUP BY f.tipo_pago, f.estado_pago
	ORDER BY f.tipo_pago, f.estado_pago;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteInventarioBajo
	@minimo INT = 5
AS
BEGIN
	SET NOCOUNT ON;

	SELECT producto_id, nombre_producto, tipo_producto, ISNULL(stock, 0) AS stock
	FROM productos
	WHERE ISNULL(stock, 0) <= @minimo
	ORDER BY stock, nombre_producto;
END
GO

-- Bitácora automática (triggers)
-- Cada INSERT, UPDATE o DELETE sobre estas tablas deja un registro en BITACORAS.
-- El usuario sale de SESSION_CONTEXT('usuario_id'), que la aplicación escribe al abrir cada conexión
-- (ver Conexion.cs). Si el cambio se hace directo en SSMS, usuario_id queda en NULL.
-- Se guarda una fila por instrucción, no una por fila afectada.

CREATE OR ALTER TRIGGER trg_bitacora_CLIENTES
ON clientes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'clientes',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_LOCACIONES
ON locaciones
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'locaciones',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_EMPLEADOS
ON empleados
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'empleados',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_USUARIOS
ON usuarios
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'usuarios',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_VEHICULOS
ON vehiculos
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'vehiculos',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_ORDENES
ON ordenes
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'ordenes',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_FACTURAS
ON facturas
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'facturas',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_CUENTAS_POR_COBRAR
ON cuentas_por_cobrar
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'cuentas_por_cobrar',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_CUOTAS
ON cuotas
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'cuotas',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_PRODUCTOS
ON productos
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'productos',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_EQUIPOS
ON equipos
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'equipos',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_PROVEEDORES
ON proveedores
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'proveedores',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_COMPRAS
ON compras
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO bitacoras (usuario_id, accion, tabla, equipo_sistema, fecha_bitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'compras',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

-- Limpieza de tipos que ya no se usan (solo existen en bases creadas con versiones anteriores)

IF TYPE_ID('dbo.tipo_lista_actividades') IS NOT NULL
	DROP TYPE dbo.tipo_lista_actividades;
GO

IF TYPE_ID('dbo.tipo_lista_productos') IS NOT NULL
	DROP TYPE dbo.tipo_lista_productos;
GO

IF TYPE_ID('dbo.tipo_lista_equipos') IS NOT NULL
	DROP TYPE dbo.tipo_lista_equipos;
GO

IF TYPE_ID('dbo.tipo_lista_lineas') IS NOT NULL
	DROP TYPE dbo.tipo_lista_lineas;
GO

DROP PROCEDURE IF EXISTS sp_ListarOrdenesFacturables;
GO

DROP PROCEDURE IF EXISTS sp_RegistrarPagoCuota;
GO

DROP PROCEDURE IF EXISTS sp_InsertarProducto;
GO

SET NOEXEC OFF;
GO
