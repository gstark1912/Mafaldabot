# Sistema de Lectura Diaria de PDF por WhatsApp

Este sistema permite enviar diariamente páginas de un PDF por WhatsApp de forma automatizada.

## Componentes

1. **API Web**: Procesa PDFs y envía mensajes por WhatsApp
2. **Servicio de Windows**: Ejecuta la tarea diariamente
3. **Base de Datos MongoDB**: Almacena el estado de lectura

## Requisitos Previos

- .NET 8.0 Runtime
- MongoDB Community Server
- Visual Studio 2022 o Visual Studio Code
- Windows 10/11 (para el servicio de Windows)

## Instalación

### 1. Configurar MongoDB

```bash
# Instalar MongoDB Community Server
# Crear la base de datos y colección inicial
```

### 2. Configurar la API

```bash
cd PdfReaderSystem.Api
dotnet restore
dotnet build
dotnet run
```

### 3. Configurar el Servicio de Windows

```bash
cd PdfReaderSystem.WindowsService
dotnet restore
dotnet build
dotnet publish -c Release -r win-x64 --self-contained
```

### 4. Instalar el Servicio

1. Copiar los archivos publicados a una carpeta permanente (ej: C:\Services\PdfReader)
2. Ejecutar `install-service.bat` como Administrador

## Configuración

### API (appsettings.json)
- Configurar cadena de conexión a MongoDB
- Configurar endpoints de WhatsApp

### Servicio (appsettings.json)
- Configurar URL de la API
- Configurar MongoDB

### MongoDB - Documento Inicial
```json
{
  "runTimeOfDay": "09:00:00",
  "lastRunDateTime": "2020-01-01T00:00:00Z",
  "filePath": "C:\\temp\\libro.pdf",
  "startPage": 0,
  "currentPage": 0,
  "endPage": 299
}
```

## Uso

1. El servicio verificará cada 5 minutos si debe enviar el mensaje diario
2. A la hora configurada enviará la página correspondiente
3. Tu novia puede solicitar páginas adicionales (función a implementar)

## Logs

- API: Consola y archivos de log
- Servicio: Event Viewer de Windows

## Troubleshooting

### El servicio no inicia
- Verificar que MongoDB esté ejecutándose
- Revisar permisos de archivo
- Consultar Event Viewer

### La API no responde
- Verificar puerto disponible
- Revisar firewall
- Verificar conexión a MongoDB

### PDF no se procesa
- Verificar ruta del archivo
- Verificar permisos de lectura
- Instalar Microsoft Visual C++ Redistributable