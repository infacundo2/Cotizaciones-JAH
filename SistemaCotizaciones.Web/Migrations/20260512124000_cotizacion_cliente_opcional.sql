ALTER TABLE `Cotizaciones`
    DROP FOREIGN KEY `FK_Cotizaciones_Clientes_ClienteId`;

ALTER TABLE `Cotizaciones`
    MODIFY COLUMN `ClienteId` int NULL;

ALTER TABLE `Cotizaciones`
    ADD CONSTRAINT `FK_Cotizaciones_Clientes_ClienteId`
    FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`)
    ON DELETE SET NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
SELECT '20260512124000_CotizacionClienteOpcional', '8.0.0'
WHERE NOT EXISTS (
    SELECT 1 FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '20260512124000_CotizacionClienteOpcional'
);
