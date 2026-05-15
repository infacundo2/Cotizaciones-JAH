# Deploy Linux + Cloudflare Tunnel

Ruta recomendada en el servidor:

```bash
/opt/sistema-cotizaciones
```

Servicio systemd:

```bash
/etc/systemd/system/sistema-cotizaciones.service
```

Cloudflare Tunnel:

```bash
/etc/cloudflared/config.yml
```

La aplicación escucha localmente en:

```bash
http://127.0.0.1:5082
```

Cloudflare publica:

```bash
https://cotizaciones.jahmantencion.cl
```
