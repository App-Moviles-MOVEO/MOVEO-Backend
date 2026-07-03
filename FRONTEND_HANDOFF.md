# MOVEO Backend — Handoff para el Frontend (cuenta de usuario + KYC + carpool)

> Documento para el equipo de la app. Resume **qué endpoints están listos para conectar**
> tras cerrar los gaps P1–P6. Todo lo de abajo está **implementado y probado end-to-end**
> contra la BD real. La fuente de verdad del contrato sigue siendo el **Swagger** (`/swagger`).
>
> **TL;DR:** ya no hay 404. `forgot-password`, `reset-password` y `kyc` existen.
> El login/perfil ahora devuelven `kycStatus`. El anti-solapamiento de reservas,
> disponibilidad, filtros por fecha y la gestión de carpool ya estaban y siguen operativos.

- **Prefijo de rutas:** `/api/v1`
- **Auth:** sin token; se sigue enviando el `id`/`userId` como en el resto de la API.
- **Fechas:** UTC ISO 8601. Rango de reservas `[startDate, endDate)` (fin exclusivo).

---

## 🆕 1. Recuperar contraseña (olvidé mi contraseña)

Ya se puede conectar la pantalla de "Olvidé mi contraseña". Flujo de 2 pasos.

### Paso 1 — Solicitar el reset
```
POST /api/v1/auth/forgot-password
Content-Type: application/json

{ "email": "usuario@correo.com" }
```
**Respuesta `200 OK` (SIEMPRE 200**, exista o no el email — no se revela si la cuenta existe):
```json
{ "message": "If an account with that email exists, a reset link has been sent" }
```
- En **desarrollo** (no producción) la respuesta incluye además el token para poder
  probar sin servidor de correo:
  ```json
  { "message": "...", "resetToken": "704a1c1a7d4a408781e4af14c0d4c6a8" }
  ```
- El token es de **un solo uso** y expira a los **30 minutos**.
- `400` solo si el body no trae `email`.

### Paso 2 — Confirmar el reset
```
POST /api/v1/auth/reset-password
Content-Type: application/json

{ "token": "704a1c1a7d4a408781e4af14c0d4c6a8", "newPassword": "NuevaClave123" }
```
**Respuesta:**
- `200 OK` → `{ "message": "Password reset successfully" }`
- `400 Bad Request` → `{ "message": "Invalid or expired token" }` (token inválido, expirado o ya usado)
- `400` si falta `token` o `newPassword`.

> En producción el `resetToken` **no** se devuelve en la respuesta: llegará por correo
> (queda pendiente conectar el servidor de mail; el flujo de la app no cambia).

---

## 🆕 2. Subida de documentos KYC (verificación de identidad)

Ya se puede conectar la pantalla de KYC. Es **multipart/form-data**.

```
POST /api/v1/auth/kyc
Content-Type: multipart/form-data

campos:
  userId   : int    (obligatorio)
  dniFront : file   (image/jpeg) — opcional individualmente
  dniBack  : file   (image/jpeg)
  selfie   : file   (image/jpeg)
```
- Los nombres de campo son **`dniFront`, `dniBack`, `selfie`** (camelCase).
- Debe enviarse `userId` y **al menos un** documento.

**Respuesta `200 OK`:**
```json
{ "status": "pending" }
```
Estados posibles de `status`: `"not_submitted"` | `"pending"` | `"approved"` | `"rejected"`.

- `400` si falta `userId` o no se envía ningún documento.
- `404` si el `userId` no existe.
- Al subir, el usuario pasa a `kycStatus = "pending"`.
- La **aprobación/rechazo** es manual por ahora (no hay verificación automática). Cuando
  se aprueba, el backend marca `dniVerified` y `licenseVerified` en el usuario.

> **Storage:** hoy las imágenes se guardan en disco del servidor y se sirven en
> `/uploads/kyc/{userId}/...`. En Railway el disco es efímero (se pierde en cada deploy),
> así que para producción real conviene migrar a S3/Cloudinary. Para la app **no cambia
> nada**: solo consume el endpoint y lee `kycStatus`.

---

## 🔵 3. `kycStatus` ahora viene en login / perfil (cambio de DTO)

Se agregó el campo **`kycStatus`** a las respuestas de usuario. La app puede leerlo para
saber si mostrar KYC como pendiente/aprobado.

**`POST /auth/login`, `POST /auth/register`, `GET /auth/me`** → ahora incluyen:
```json
{
  "id": 1, "firstName": "Rosa", "lastName": "Martinez",
  "email": "rosa@moveo.com", "phone": "+51980000001",
  "dni": "40000001", "licenseNumber": "Q0000001",
  "role": "owner", "address": "",
  "kycStatus": "not_submitted"
}
```

**`GET /users/{id}`** (UserResource) → ahora incluye `kycStatus` junto al bloque `verified`:
```json
{
  "id": 1, "role": "owner", "...": "...",
  "kycStatus": "not_submitted",
  "verified": { "email": false, "phone": false, "dni": false, "license": false },
  "...": "..."
}
```

> 👉 **Acción en la app (Kotlin):** agregar el campo opcional `kycStatus: String?`
> (default `"not_submitted"`) al DTO de usuario en `data/remote/dto/Dtos.kt`.
> Es aditivo: no rompe nada si se ignora.

---

## 🟢 4. Reservas: anti-solapamiento, disponibilidad y filtros por fecha (ya operativo)

Esto ya estaba implementado y sigue funcionando. Resumen para la app:

### Crear reserva con validación de choque
```
POST /api/v1/rentals
```
- Si el rango `[startDate, endDate)` se cruza con otra reserva `pending/accepted/active`
  del mismo vehículo → **`409 Conflict`**:
  ```json
  {
    "error": "vehicle_not_available",
    "message": "...",
    "conflictingRanges": [ { "start": "...", "end": "..." } ]
  }
  ```
- Otros errores: `400 invalid_request`, `404 vehicle_not_found`, `409 vehicle_not_active`.

### Disponibilidad (para pintar el calendario)
```
GET /api/v1/vehicles/{id}/availability?from=2026-07-01T00:00:00Z&to=2026-09-01T00:00:00Z
```
**Respuesta:**
```json
{ "vehicleId": 10, "busyRanges": [ { "start": "...", "end": "..." } ] }
```
- `from`/`to` opcionales (default: hoy → +3 meses). Solo cuentan reservas `pending/accepted/active`.

### Catálogo filtrado por fechas libres + filtros
```
GET /api/v1/vehicles?startDate=...&endDate=...&transmission=automatic&fuelType=gasoline&bodyType=suv
```
- `startDate`+`endDate` → excluye vehículos ocupados en ese rango.
- Filtros disponibles: `ownerId`, `status`, `minPrice`, `maxPrice`, `district`, `bodyType`,
  **`transmission`**, **`fuelType`**.
- Extra: orden por cercanía (`lat`, `lng`, `sort=distance`) y paginación (`page`, `pageSize`).

### Expiración de reservas sin pagar
- Hay un job en background que libera reservas `pending` no pagadas para que no bloqueen
  fechas para siempre. Transparente para la app.

**Convenciones (deben coincidir app↔backend):** fechas UTC ISO 8601, rango `[start, end)`
fin exclusivo, estados que **bloquean** = `pending/accepted/active`, que **liberan** =
`cancelled/completed`.

---

## 🟢 5. Gestión de reservas por el owner (ya operativo)

- **Cambiar estado:** `PATCH /api/v1/rentals/{id}` con `{ "status": "accepted" | "active" | "completed" | "cancelled" }`
  (campos opcionales adicionales: `vehicleRated`, `vehicleRating`, `acceptedAt`, `completedAt`).
- **Reservas de mis autos:** `GET /api/v1/rentals?ownerId={id}`.
- Al pasar a `cancelled`/`completed` las fechas se liberan automáticamente (filtro de estados de la sección 4).

---

## 🟢 6. Carpooling: cupos y aprobación de pasajeros (ya operativo)

Endpoints listos (todos requieren `ownerId` como query param donde aplique):

| Acción | Endpoint |
|---|---|
| Solicitar asiento (flujo con aprobación) | `POST /adventure-routes/{routeId}/book` con `{ "passengerId": X, "seats": 1 }` → crea solicitud **PENDING** (no descuenta cupo) |
| Reservar directo (legacy) | `POST /adventure-routes/{routeId}/book` con `{ "seats": 1 }` → descuenta cupo |
| Listar pasajeros | `GET /adventure-routes/{routeId}/passengers?ownerId={id}` |
| Aceptar | `POST /adventure-routes/{routeId}/passengers/{passengerId}/accept?ownerId={id}` → CONFIRMED + descuenta cupo |
| Rechazar | `POST /adventure-routes/{routeId}/passengers/{passengerId}/reject?ownerId={id}` |
| Quitar confirmado | `DELETE /adventure-routes/{routeId}/passengers/{passengerId}?ownerId={id}` |

- Al agotarse los asientos, el viaje pasa a `status = "full"`.
- El detalle de la ruta (`GET /adventure-routes/{routeId}`) embebe el array `passengers`.

### 🆕 Filtro `community` agregado
`GET /api/v1/adventure-routes` ahora acepta, además de `onlyWomen`:
```
GET /api/v1/adventure-routes?community=Universitarios%20UNI
GET /api/v1/adventure-routes?onlyWomen=true
```
Otros filtros ya existentes: `ownerId`, `type`, `difficulty`, `featured`.

---

## Resumen de estado

| Prioridad | Qué | Estado |
|---|---|---|
| P1 | `forgot-password` y `kyc` ya no dan 404 | ✅ Implementado |
| P2 | Flujo recuperar contraseña (forgot + reset) | ✅ Implementado y probado |
| P3 | Subida real de KYC + storage en disco/estático | ✅ Implementado (storage a migrar a S3/Cloudinary para prod) |
| P4 | Anti-solapamiento + availability + filtros por fecha | ✅ Operativo |
| P5 | Gestión de estados por owner | ✅ Operativo |
| P6 | Carpool: cupos, aprobación de pasajeros, filtro `community` | ✅ Operativo |

### Checklist para la app (Kotlin)
- [ ] Conectar pantalla "Olvidé mi contraseña" a `POST /auth/forgot-password` + `POST /auth/reset-password`.
- [ ] Conectar pantalla KYC a `POST /auth/kyc` (multipart: `userId`, `dniFront`, `dniBack`, `selfie`).
- [ ] Agregar `kycStatus: String?` al DTO de usuario en `data/remote/dto/Dtos.kt`.
- [ ] (Opcional) Usar el filtro `?community=` en la lista de carpool.

> Pendiente de infraestructura (no bloquea a la app): servidor de correo para enviar el
> `resetToken` en producción y storage S3/Cloudinary para las imágenes KYC.
