CREATE TABLE IF NOT EXISTS `CuentasCorrientesCotizacion` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Nombre` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_CuentasCorrientesCotizacion` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE UNIQUE INDEX `IX_CuentasCorrientesCotizacion_Nombre`
    ON `CuentasCorrientesCotizacion` (`Nombre`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260512120000_CuentasCorrientesCotizacion', '8.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260512120000_CuentasCorrientesCotizacion'
);
