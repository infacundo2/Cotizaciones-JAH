ALTER TABLE `Clientes`
    ADD COLUMN `Region` varchar(100) CHARACTER SET utf8mb4 NOT NULL DEFAULT '';

CREATE TABLE IF NOT EXISTS `CategoriasCotizacion` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_CategoriasCotizacion` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_CategoriasCotizacion_Nombre`
    ON `CategoriasCotizacion` (`Nombre`);

CREATE TABLE IF NOT EXISTS `FormasPagoCotizacion` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_FormasPagoCotizacion` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_FormasPagoCotizacion_Nombre`
    ON `FormasPagoCotizacion` (`Nombre`);

INSERT IGNORE INTO `CategoriasCotizacion` (`Id`, `Nombre`) VALUES
    (1, 'Mantencion'),
    (2, 'Reparacion'),
    (3, 'Instalacion'),
    (4, 'Servicio tecnico'),
    (5, 'Repuestos');

INSERT IGNORE INTO `FormasPagoCotizacion` (`Id`, `Nombre`) VALUES
    (1, 'Efectivo'),
    (2, 'Transferencia'),
    (3, 'Cheque');

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260508120000_ClienteRegionOpciones', '8.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260508120000_ClienteRegionOpciones'
);
