# Emby User Data / Datos de Usuario para Emby

[English](#english) · [Español](#español)

---

## English

**Emby User Data** is an Emby Server 4.8+ plugin for storing administrator-managed custom data for Emby users.

### Features

- Define custom fields: short text, long text, date, number, yes/no, and option lists.
- Store values separately for every Emby user.
- Reorder fields with drag and drop.
- Add row separators to display related fields on the same line.
- Import and export full backups, or field structure only.
- Restrict data access and changes to server administrators.
- Store plugin data independently from Emby's internal database.

### Installation

1. Download the latest `Emby.UserData-<version>.zip` from the [Releases](../../releases) page.
2. Stop Emby Server.
3. Extract `Emby.UserData.dll` into Emby's `plugins` directory.
4. Start Emby Server again.
5. Refresh Emby Web with `Ctrl+F5`.

The plugin has no additional DLL dependencies.

### Backup

Inside the plugin's **Copia de seguridad / Backup** tab, administrators can:

- Export all field definitions, layout, and user values.
- Export only the field structure and layout.
- Import a backup by merging it with existing information or replacing the imported scope.

### Build and test

Docker is used so a local .NET SDK is not required:

```bash
# Run persistence tests
docker build --target test .

# Build the plugin artifact
docker build --target artifact --output type=local,dest=artifacts .
```

The resulting assembly is `artifacts/Emby.UserData.dll`.

---

## Español

**Datos de Usuario para Emby** es un plugin para Emby Server 4.8+ que permite guardar información personalizada de los usuarios, gestionada por administradores.

### Funcionalidades

- Campos personalizados: texto corto, texto largo, fecha, número, sí/no y listas de opciones.
- Valores independientes para cada usuario de Emby.
- Reordenación de campos mediante arrastrar y soltar.
- Separadores de fila para mostrar campos relacionados en la misma línea.
- Importación y exportación de copias completas o solo de la estructura de campos.
- Acceso y modificación restringidos a administradores del servidor.
- Datos guardados fuera de la base de datos interna de Emby.

### Instalación

1. Descarga `Emby.UserData-<versión>.zip` desde [Releases](../../releases).
2. Detén Emby Server.
3. Extrae `Emby.UserData.dll` en el directorio `plugins` de Emby.
4. Inicia de nuevo Emby Server.
5. Fuerza la recarga de Emby Web con `Ctrl+F5`.

El plugin no requiere DLL adicionales.

### Copia de seguridad

Desde la pestaña **Copia de seguridad / Backup** del plugin, los administradores pueden:

- Exportar campos, diseño y valores de usuarios.
- Exportar solo la estructura y el diseño de campos.
- Importar una copia combinando la información existente o reemplazando el alcance importado.

### Compilación y pruebas

Se utiliza Docker, por lo que no es necesario instalar .NET localmente:

```bash
# Ejecutar las pruebas de persistencia
docker build --target test .

# Generar el artefacto del plugin
docker build --target artifact --output type=local,dest=artifacts .
```

La DLL resultante se genera en `artifacts/Emby.UserData.dll`.
