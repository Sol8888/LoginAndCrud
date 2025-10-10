Descripción
API en .NET 8 con autenticación JWT, EF Core (SQL Server) y CRUD de usuarios. Incluye Swagger y manejo de secretos fuera del repositorio. Tabla Users: Id, Username, Email, PasswordHash, PasswordSalt, Role, IsActive, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy.

Índice

Arquitectura

Funcionalidades

Demo / Swagger

Requisitos

Cómo ejecutar

Seguridad y secretos

Endpoints principales

Problemas comunes

Tecnologías

Contribuciones

Autores

Licencia

Arquitectura
Cliente (Blazor u otro) -> Web API (Controllers/Services) -> EF Core (AppDbContext) -> SQL Server

Funcionalidades

Autenticación con JWT.

Rol Admin para endpoints de administración.

Listado paginado y con búsqueda.

Crear, actualizar, cambiar contraseña y borrar usuarios.

Auditoría de altas y modificaciones mediante CreatedBy y UpdatedBy.

Demo / Swagger
Abrir la aplicación y navegar a https://localhost:7162/swagger

En el botón Authorize, pegar solo el token (no escribir la palabra Bearer; Swagger la añade).

Requisitos

.NET 8 SDK

SQL Server (Local/Express o Docker)

Visual Studio 2022 o VS Code

Cómo ejecutar
a) Clonar y restaurar:
git clone https://github.com/
<tu-usuario>/<tu-repo>.git
cd <tu-repo>
dotnet restore

b) Configurar secretos con User Secrets (desarrollo):
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=LoginAndCrud;Trusted_Connection=True;TrustServerCertificate=True;"
(Alternativa con usuario/clave)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=LoginAndCrud;User Id=sa;Password=<TU_PASS>;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Issuer" "LoginAndCrud.Api"
dotnet user-secrets set "Jwt:Audience" "LoginAndCrud.Client"
dotnet user-secrets set "Jwt:Key" "CLAVE-LARGA-UNICA-Y-SECRETA"
dotnet user-secrets set "Jwt:ExpiresMinutes" "60"

c) Ejecutar:
dotnet run

d) Abrir Swagger:
https://localhost:7162/swagger

Seguridad y secretos
No subir secretos al repositorio. Usar User Secrets en desarrollo y GitHub Secrets o variables de entorno en CI/Producción.
.gitignore recomendado:
appsettings.json
appsettings.*.local.json
secrets.json
bin/
obj/
.vs/
*.user
*.suo

Archivo de ejemplo para referencia (no contiene secretos):
appsettings.example.json, con claves ConnectionStrings:DefaultConnection vacío y Jwt con Issuer, Audience, Key vacío y ExpiresMinutes.

Endpoints principales

Auth
POST /api/Auth/register Crea usuario (rol por defecto User)
POST /api/Auth/login Devuelve JWT

Ejemplo login (línea única):
curl -X POST https://localhost:7162/api/Auth/login
 -H "Content-Type: application/json" -d "{"usernameOrEmail":"sun","password":"sun"}"

Users (requiere rol Admin)
GET /api/Users?page=1&pageSize=20&search=txt
GET /api/Users/{id}
POST /api/Users
PATCH /api/Users/{id}
PUT /api/Users/{id}/password
DELETE /api/Users/{id}

Ejemplos rápidos (pegar el token real en <TOKEN>):

Listar:
curl "https://localhost:7162/api/Users?page=1&pageSize=20
" -H "Authorization: Bearer <TOKEN>"

Crear:
curl -X POST https://localhost:7162/api/Users
 -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" -d "{"username":"camila","email":"camila@example.com
","password":"P@ssw0rd!","role":"User","isActive":true}"

Actualizar (PATCH):
curl -X PATCH https://localhost:7162/api/Users/2
 -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" -d "{"email":"camila+upd@example.com
","role":"Admin","isActive":true}"

Cambiar contraseña (Admin no requiere currentPassword):
curl -X PUT https://localhost:7162/api/Users/2/password
 -H "Authorization: Bearer <TOKEN>" -H "Content-Type: application/json" -d "{"currentPassword":"","newPassword":"Nuev0P@ss!"}"

Borrar:
curl -X DELETE https://localhost:7162/api/Users/2
 -H "Authorization: Bearer <TOKEN>"

Nota: para pruebas iniciales, si necesitas un Admin rápido, puedes promover un usuario existente con SQL: UPDATE dbo.Users SET Role = 'Admin' WHERE Username = 'sun';

Problemas comunes
401 Unauthorized: falta el header Authorization, token caducado o cambió la clave JWT. En Swagger pegar solo el token; la UI añade Bearer. Si cambiaste Jwt:Key, vuelve a iniciar sesión y usa el nuevo token.
403 Forbidden: token válido pero sin rol Admin.
Errores de conexión SQL: revisar cadena de conexión, instancia y TrustServerCertificate=True en desarrollo.
404 en /swagger: agregar app.UseSwagger() y app.UseSwaggerUI() en Program.cs.

Tecnologías
.NET 8 (Web API)
Entity Framework Core (SqlServer)
JWT Bearer Authentication
Swashbuckle (Swagger)
PBKDF2 para contraseñas

Contribuciones
Abrir un issue con propuesta o bug. Hacer fork y crear rama feat/mi-mejora. Enviar Pull Request a main con descripción clara.
