# Despliegue SistemaCotizaciones en Linux

## 1. Publicar desde Windows

Desde la carpeta raíz del proyecto:

```powershell
dotnet publish SistemaCotizaciones.Web/SistemaCotizaciones.Web.csproj -c Release -o publish
```

Luego copia el contenido de `publish` al servidor Linux:

```bash
/opt/sistema-cotizaciones
```

## 2. Configurar variables de entorno en Linux

No guardes la contraseña de MySQL en archivos del repositorio. Configúrala en systemd:

```bash
sudo systemctl edit sistema-cotizaciones
```

Agrega:

```ini
[Service]
Environment=ConnectionStrings__DefaultConnection=Server=127.0.0.1;Port=3306;Database=db_cotizaciones;Uid=user_cotiza;Pwd=TU_PASSWORD;SslMode=None;AllowPublicKeyRetrieval=True;
Environment=SeedAdmin__Password=CAMBIA_ESTA_CLAVE
```

## 3. Instalar servicio systemd

```bash
sudo cp deploy/linux/sistema-cotizaciones.service /etc/systemd/system/sistema-cotizaciones.service
sudo systemctl daemon-reload
sudo systemctl enable sistema-cotizaciones
sudo systemctl start sistema-cotizaciones
sudo systemctl status sistema-cotizaciones
```

Logs:

```bash
journalctl -u sistema-cotizaciones -f
```

## 4. Configurar Cloudflare Tunnel

El tunnel debe apuntar a:

```bash
http://127.0.0.1:5082
```

Hostname:

```bash
cotizaciones.jahmantencion.cl
```

Ejemplo:

```bash
sudo cp deploy/linux/cloudflared-config.yml.example /etc/cloudflared/config.yml
sudo systemctl restart cloudflared
sudo systemctl status cloudflared
```

## 5. Actualizar base de datos

En el servidor, desde la carpeta de publicación o desde el repo:

```bash
dotnet ef database update --project SistemaCotizaciones.Web/SistemaCotizaciones.Web.csproj --startup-project SistemaCotizaciones.Web/SistemaCotizaciones.Web.csproj
```

Si el servidor solo tiene los binarios publicados, genera el SQL desde desarrollo:

```powershell
dotnet ef migrations script --idempotent --project SistemaCotizaciones.Web/SistemaCotizaciones.Web.csproj --startup-project SistemaCotizaciones.Web/SistemaCotizaciones.Web.csproj --output deploy-db.sql
```

y ejecútalo en MySQL.

## 6. Logo

Coloca el logo en:

```bash
/opt/sistema-cotizaciones/wwwroot/img/logo-jah.png
```

## 7. Verificar

```bash
curl -I http://127.0.0.1:5082/login
```

Luego abre:

```text
https://cotizaciones.jahmantencion.cl
```
