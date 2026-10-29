# Emby User Data

Plugin para Emby 4.8+ que añade una página administrativa **User Data** con campos personalizados por usuario.

## Compilar

```bash
docker build --target artifact --output type=local,dest=artifacts .
```

Copia `artifacts/Emby.UserData.dll` al directorio `plugins` de Emby y reinicia el servidor. En Docker, ese directorio debe estar dentro del volumen persistente de configuración de Emby.

Los valores se almacenan en `plugins/configurations/UserData/user-data/store.json` y no modifican la base de datos de Emby.
