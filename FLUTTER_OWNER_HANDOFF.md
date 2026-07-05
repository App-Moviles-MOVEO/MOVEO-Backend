# MOVEO Backend — Guía para Flutter (flujo OWNER completo)

> Documento para el equipo de la app en **Flutter**. Cubre **todo el flujo de un usuario `owner`**:
> crear la cuenta, iniciar sesión, verificar identidad (KYC), **publicar vehículos**, **publicar rutas
> de aventura / carpool**, gestionar reservas y aprobar pasajeros.
>
> Todos los endpoints de abajo están **implementados en el backend** (verificados contra los
> controllers). La fuente de verdad del contrato es el **Swagger**: `/swagger/index.html`.

---

## 0. Configuración base

| Entorno | Base URL |
|---|---|
| PC / Web | `http://localhost:8080/api/v1` |
| Emulador Android | `http://10.0.2.2:8080/api/v1` |
| Simulador iOS | `http://localhost:8080/api/v1` |
| Dispositivo físico (misma Wi‑Fi) | `http://<IP-LOCAL-DEL-PC>:8080/api/v1` |
| Swagger (solo docs, NO llamar desde la app) | `http://localhost:8080/swagger/index.html` |

**Reglas generales:**
- **Prefijo:** todas las rutas van bajo `/api/v1`.
- **Auth:** sin token/JWT. Se identifica al usuario enviando su `id` / `userId` / `ownerId`
  (en el body o como query param, según el endpoint).
- **Content-Type:** `application/json` salvo el KYC, que es `multipart/form-data`.
- **Fechas:** UTC ISO 8601 (`2026-07-01T00:00:00Z`). Rango de reservas `[startDate, endDate)` (fin exclusivo).
- **HTTP en claro:** como es `http://`, en Android hay que permitir tráfico no cifrado
  (`android:usesCleartextTraffic="true"` o un `network_security_config`).

---

# PARTE A — Cuenta del owner

## A1. Registrar owner

```
POST /api/v1/auth/register
Content-Type: application/json
```
```json
{
  "firstName": "Rosa",
  "lastName": "Martinez",
  "email": "rosa@moveo.com",
  "password": "MiClave123",
  "phone": "+51980000001",
  "dni": "40000001",
  "licenseNumber": "Q0000001",
  "address": "Av. Siempre Viva 123",
  "role": "owner"
}
```
- **Obligatorios:** `firstName`, `lastName`, `email`, `password`.
- **Opcionales:** `phone`, `dni`, `licenseNumber`, `address`, `preferences`.
- **`role`**: enviar **`"owner"`** para que sea propietario. Default si se omite: `"renter"`.

**Respuesta `201 Created`:**
```json
{
  "id": 1,
  "firstName": "Rosa",
  "lastName": "Martinez",
  "email": "rosa@moveo.com",
  "phone": "+51980000001",
  "dni": "40000001",
  "licenseNumber": "Q0000001",
  "role": "owner",
  "address": "Av. Siempre Viva 123",
  "kycStatus": "not_submitted"
}
```
- `400` si falta email/password o firstName/lastName.
- `409 Conflict` → `{ "message": "User with this email already exists" }`.

> 👉 Guarda el `id` devuelto: es el **`ownerId`** que usarás en todo el resto del flujo.

---

## A2. Login

```
POST /api/v1/auth/login
Content-Type: application/json
```
```json
{ "email": "rosa@moveo.com", "password": "MiClave123" }
```
**Respuesta `200 OK`:** mismo objeto que el registro (incluye `id`, `role`, `kycStatus`).
- `401 Unauthorized` → `{ "message": "Invalid email or password" }`.

---

## A3. Perfil / usuario actual

```
GET /api/v1/auth/me?userId=1
```
**Respuesta `200 OK`:** mismo DTO de usuario (con `kycStatus`).
- `400` si falta `userId`, `404` si no existe.

### Perfil detallado (stats, verificación, banco)
```
GET /api/v1/users/{id}
```
**Respuesta `200 OK`:**
```json
{
  "id": 1,
  "role": "owner",
  "firstName": "Rosa",
  "lastName": "Martinez",
  "email": "rosa@moveo.com",
  "phone": "+51980000001",
  "dni": "40000001",
  "licenseNumber": "Q0000001",
  "avatar": null,
  "kycStatus": "not_submitted",
  "verified": { "email": false, "phone": false, "dni": false, "license": false },
  "stats": {
    "totalRentals": 0, "totalSpent": 0, "totalEarned": 0,
    "activeRentals": 0, "completedRentals": 0, "canceledRentals": 0
  },
  "preferences": {
    "language": "es",
    "notifications": { "email": true, "push": true, "sms": false },
    "autoAcceptRentals": false, "minimumRentalDays": 1, "instantBooking": false
  },
  "bankAccount": null,
  "createdAt": "...", "updatedAt": "..."
}
```

### Actualizar perfil
```
PUT   /api/v1/users/{id}     (actualización completa)
PATCH /api/v1/users/{id}     (actualización parcial)
```
Body = campos de `UpdateUserResource` (nombres, teléfono, avatar, preferencias, `bankAccount`, etc.).
- `404` si el usuario no existe.

### Otros de cuenta
```
POST /api/v1/auth/change-password   { "userId": 1, "currentPassword": "...", "newPassword": "..." }
POST /api/v1/auth/logout            → { "message": "Logged out successfully" }
GET  /api/v1/users?email=rosa@moveo.com   → busca usuario por email (array, vacío si no existe)
DELETE /api/v1/users/{id}           → 204
```

---

## A4. Recuperar contraseña (2 pasos)

```
POST /api/v1/auth/forgot-password    { "email": "rosa@moveo.com" }
```
**Respuesta `200 OK` (SIEMPRE 200**, exista o no el email):
```json
{ "message": "If an account with that email exists, a reset link has been sent" }
```
- En **desarrollo** incluye además `"resetToken": "..."` para probar sin servidor de correo.
- Token de **un solo uso**, expira a los **30 minutos**. `400` solo si falta `email`.

```
POST /api/v1/auth/reset-password     { "token": "...", "newPassword": "NuevaClave123" }
```
- `200` → `{ "message": "Password reset successfully" }`
- `400` → `{ "message": "Invalid or expired token" }` (o si falta `token`/`newPassword`).

---

## A5. KYC — verificación de identidad (multipart)

```
POST /api/v1/auth/kyc
Content-Type: multipart/form-data
```
Campos del form:
| Campo | Tipo | Obligatorio |
|---|---|---|
| `userId` | int | ✅ |
| `dniFront` | file (image/jpeg) | al menos uno |
| `dniBack` | file (image/jpeg) | al menos uno |
| `selfie` | file (image/jpeg) | al menos uno |

**Respuesta `200 OK`:** `{ "status": "pending" }`
- Estados de `status`: `"not_submitted"` | `"pending"` | `"approved"` | `"rejected"`.
- `400` si falta `userId` o no se envía ningún documento. `404` si el `userId` no existe.
- Aprobación/rechazo es **manual** por ahora. Al subir, el usuario pasa a `kycStatus = "pending"`.

**Ejemplo Flutter (`http` con `MultipartRequest`):**
```dart
final req = http.MultipartRequest('POST', Uri.parse('$baseUrl/auth/kyc'))
  ..fields['userId'] = userId.toString()
  ..files.add(await http.MultipartFile.fromPath('dniFront', dniFrontPath))
  ..files.add(await http.MultipartFile.fromPath('dniBack', dniBackPath))
  ..files.add(await http.MultipartFile.fromPath('selfie', selfiePath));
final res = await req.send();
```

---

# PARTE B — Publicar y gestionar vehículos

## B1. Crear (publicar) un vehículo

```
POST /api/v1/vehicles
Content-Type: application/json
```
```json
{
  "ownerId": 1,
  "brand": "Toyota",
  "model": "Yaris",
  "year": 2022,
  "color": "Rojo",
  "transmission": "automatic",
  "fuelType": "gasoline",
  "seats": 5,
  "licensePlate": "ABC-123",
  "location": {
    "district": "Miraflores",
    "address": "Av. Larco 456",
    "lat": -12.1211,
    "lng": -77.0301
  },
  "dailyPrice": 120.0,
  "depositAmount": 300.0,
  "description": "Auto ideal para ciudad",
  "images": ["https://.../foto1.jpg", "https://.../foto2.jpg"],
  "features": ["Aire acondicionado", "Bluetooth"],
  "restrictions": ["No fumar"],
  "bodyType": "sedan"
}
```
**Campos:**
- **Obligatorios:** `ownerId`, `brand`, `model`, `year`, `color`, `transmission`, `fuelType`, `seats`, `licensePlate`, `location` (con `district`, `address`, `lat`, `lng`), `dailyPrice`.
- **Opcionales:** `depositAmount` (default 0), `description`, `images`, `features`, `restrictions`, `bodyType`.

**Respuesta `201 Created`** (VehicleResource):
```json
{
  "id": 10, "ownerId": 1, "brand": "Toyota", "model": "Yaris", "year": 2022,
  "color": "Rojo", "transmission": "automatic", "fuelType": "gasoline", "seats": 5,
  "licensePlate": "ABC-123",
  "location": { "district": "Miraflores", "address": "Av. Larco 456", "lat": -12.1211, "lng": -77.0301 },
  "dailyPrice": 120.0, "depositAmount": 300.0, "status": "active",
  "description": "Auto ideal para ciudad",
  "images": ["..."], "features": ["..."], "restrictions": ["..."],
  "createdAt": "...", "updatedAt": "...",
  "bodyType": "sedan", "ownerName": "Rosa Martinez", "rating": 0, "reviewsCount": 0
}
```
- `400` si el body es inválido.

---

## B2. Listar / ver / editar / borrar vehículos del owner

```
GET /api/v1/vehicles?ownerId=1
```
Lista **solo los vehículos del owner**. La respuesta ya viene enriquecida con `ownerName`, `rating` y `reviewsCount`.

**Otros filtros disponibles en `GET /api/v1/vehicles`:**
`status`, `minPrice`, `maxPrice`, `district`, `bodyType`, `transmission`, `fuelType`,
`startDate`+`endDate` (excluye ocupados), `lat`+`lng`+`sort=distance`, `page`, `pageSize`.

```
GET    /api/v1/vehicles/{id}          → detalle
PUT    /api/v1/vehicles/{id}          → actualización completa (mismo body que crear + "status")
PATCH  /api/v1/vehicles/{id}          → actualización parcial (solo campos a cambiar)
DELETE /api/v1/vehicles/{id}          → 204 (borra el vehículo)
```

**PATCH ejemplo** (cambiar precio y pausar publicación):
```json
{ "dailyPrice": 150.0, "status": "inactive" }
```
`status` típicos: `"active"` (publicado) / `"inactive"` (pausado). `404` si el vehículo no existe.

---

## B3. Disponibilidad (para el calendario)

```
GET /api/v1/vehicles/{id}/availability?from=2026-07-01T00:00:00Z&to=2026-09-01T00:00:00Z
```
```json
{ "vehicleId": 10, "busyRanges": [ { "start": "...", "end": "..." } ] }
```
- `from`/`to` opcionales (default: hoy → +3 meses). Solo cuentan reservas `pending/accepted/active`.

---

# PARTE C — Reservas (gestión por el owner)

## C1. Ver las reservas de mis autos

```
GET /api/v1/rentals?ownerId=1
```
Otros filtros: `renterId`, `vehicleId`, `status`. La respuesta viene enriquecida con
`vehicleName` y `vehicleImage`.

```
GET /api/v1/rentals/{id}          → detalle de una reserva
GET /api/v1/rentals/active        → reservas activas
GET /api/v1/rentals/user/{userId} → reservas de un usuario
```

**RentalResource (respuesta):**
```json
{
  "id": 5, "vehicleId": 10, "renterId": 3, "ownerId": 1,
  "startDate": "...", "endDate": "...", "totalPrice": 360.0,
  "status": "pending", "pickupLocation": "...", "returnLocation": "...",
  "notes": null, "adventureRouteId": null,
  "vehicleRated": false, "vehicleRating": null,
  "createdAt": "...", "acceptedAt": null, "completedAt": null,
  "vehicleName": "Toyota Yaris", "vehicleImage": "https://.../foto1.jpg"
}
```

## C2. Cambiar el estado de una reserva (aceptar/activar/completar/cancelar)

```
PATCH /api/v1/rentals/{id}
Content-Type: application/json
```
```json
{ "status": "accepted" }
```
- `status` posibles: `"accepted"`, `"active"`, `"completed"`, `"cancelled"`.
- Campos opcionales: `vehicleRated`, `vehicleRating`, `acceptedAt`, `completedAt`.
- Al pasar a `cancelled`/`completed`, **las fechas se liberan automáticamente**.
- `404` si la reserva no existe.

## C3. Estados y anti-solapamiento (referencia)

- **Bloquean fechas:** `pending`, `accepted`, `active`.
- **Liberan fechas:** `cancelled`, `completed`.
- Al **crear** una reserva (`POST /api/v1/rentals`), si el rango se cruza con otra
  reserva bloqueante del mismo vehículo → **`409 Conflict`**:
  ```json
  { "error": "vehicle_not_available", "message": "...", "conflictingRanges": [ { "start": "...", "end": "..." } ] }
  ```
  Otros errores de creación: `400 invalid_request`, `404 vehicle_not_found`, `409 vehicle_not_active`.
- Hay un job en background que expira reservas `pending` no pagadas (transparente para la app).

## C4. Pagar una reserva (1 paso, Yape por defecto)

```
POST /api/v1/rentals/{id}/pay
Content-Type: application/json
```
```json
{ "paymentMethod": "yape", "amount": 360.0, "currency": "PEN", "transactionId": "abc123" }
```
- Todos los campos son opcionales: si no envías `amount` usa el `totalPrice`; `paymentMethod`
  default `"yape"`, `currency` default `"PEN"`, `type` default `"rental_payment"`.
- **Respuesta `200 OK`:** `{ "rental": {...}, "payment": {...} }`.
- `404` si la reserva no existe.

```
PUT    /api/v1/rentals/{id}   → actualización completa
DELETE /api/v1/rentals/{id}   → 204
```

---

# PARTE D — Rutas de aventura / Carpool (owner)

## D1. Publicar una ruta

```
POST /api/v1/adventure-routes
Content-Type: application/json
```
```json
{
  "ownerId": 1,
  "name": "Ruta Lunahuaná",
  "title": "Aventura de fin de semana en Lunahuaná",
  "description": "Río, canotaje y camping",
  "startLocation": "Lima",
  "endLocation": "Lunahuaná",
  "type": "adventure",
  "duration": 2,
  "difficulty": "medium",
  "estimatedCost": 250.0,
  "vehicleName": "Toyota Hilux",
  "imageUrl": "https://.../ruta.jpg",
  "tags": ["río", "camping"],
  "featured": false,
  "maxCapacity": 6,

  "departureDate": "2026-08-15T08:00:00Z",
  "departureTime": "08:00",
  "seatsTotal": 4,
  "seatsAvailable": 4,
  "pricePerSeat": 60.0,
  "onlyWomen": false,
  "community": "Universitarios UNI",
  "lat": -12.9631,
  "lng": -76.1401
}
```
- **Obligatorios:** `ownerId`, `name`, `title`, `description`, `startLocation`, `endLocation`, `type`, `duration`, `difficulty`, `estimatedCost`.
- **Opcionales generales:** `vehicleName`, `imageUrl`, `tags`, `featured`, `maxCapacity`.
- **Opcionales de carpool:** `departureDate`, `departureTime`, `seatsTotal`, `seatsAvailable`, `pricePerSeat`, `onlyWomen`, `community`, `lat`, `lng`.

**Respuesta `201 Created`** (AdventureRouteResource, con `id`, `status: "active"`, `rating`, `reviewsCount`, y el bloque de carpool).
- `400` si no se pudo crear.

## D2. Listar / ver / editar / borrar rutas

```
GET /api/v1/adventure-routes?ownerId=1
```
**Filtros (usar de a uno; se aplican en orden de prioridad `ownerId` > `type` > `difficulty` > `featured`):**
`ownerId`, `type`, `difficulty`, `featured=true`.
**Filtros de carpool combinables encima del resultado:** `onlyWomen=true`, `community=...`.
```
GET /api/v1/adventure-routes?community=Universitarios%20UNI
GET /api/v1/adventure-routes?onlyWomen=true
```

```
GET    /api/v1/adventure-routes/{routeId}   → detalle (embebe el array "passengers")
PUT    /api/v1/adventure-routes/{routeId}   → actualizar
DELETE /api/v1/adventure-routes/{routeId}   → 204
```

## D3. Carpool — gestión de cupos y pasajeros

| Acción | Endpoint | Body / Query |
|---|---|---|
| Solicitar asiento (flujo con aprobación) | `POST /api/v1/adventure-routes/{routeId}/book` | `{ "passengerId": 3, "seats": 1 }` → crea **PENDING**, no descuenta cupo |
| Reservar directo (legacy) | `POST /api/v1/adventure-routes/{routeId}/book` | `{ "seats": 1 }` → descuenta cupo al instante |
| Listar pasajeros | `GET /api/v1/adventure-routes/{routeId}/passengers?ownerId=1` | requiere `ownerId` |
| Aceptar | `POST /api/v1/adventure-routes/{routeId}/passengers/{passengerId}/accept?ownerId=1` | → **CONFIRMED** + descuenta cupo |
| Rechazar | `POST /api/v1/adventure-routes/{routeId}/passengers/{passengerId}/reject?ownerId=1` | → **REJECTED**, libera cupo tentativo |
| Quitar confirmado | `DELETE /api/v1/adventure-routes/{routeId}/passengers/{passengerId}?ownerId=1` | → **CANCELLED**, libera cupo |

**Respuesta de listar pasajeros (`GET .../passengers`):**
```json
{
  "routeId": 7, "seatsTotal": 4, "seatsAvailable": 3,
  "passengers": [
    {
      "id": 20, "passengerId": 3, "fullName": "Juan Pérez",
      "avatarUrl": null, "reputation": 4.8, "verificationStatus": "VERIFIED",
      "status": "PENDING", "seats": 1, "requestedAt": "..."
    }
  ]
}
```
- `verificationStatus`: `"VERIFIED"` | `"UNVERIFIED"`. `status`: `"PENDING"` | `"CONFIRMED"` | `"REJECTED"` | `"CANCELLED"`.
- Al agotarse los asientos, la ruta pasa a `status = "full"`.
- Errores de carpool devuelven `{ "error": "...", "message": "..." }` con `403` (no es dueño),
  `404` (ruta/solicitud no encontrada), `409` (sin cupo / solicitud duplicada / ruta no activa).

---

# Checklist de implementación (Flutter)

**Cuenta owner**
- [ ] `POST /auth/register` con `role: "owner"` → guardar `id` como `ownerId`.
- [ ] `POST /auth/login`, `GET /auth/me?userId=`, `GET /users/{id}` (perfil detallado).
- [ ] Recuperar contraseña: `POST /auth/forgot-password` + `POST /auth/reset-password`.
- [ ] KYC: `POST /auth/kyc` (multipart: `userId`, `dniFront`, `dniBack`, `selfie`).
- [ ] Leer `kycStatus` en el DTO de usuario (default `"not_submitted"`).

**Vehículos**
- [ ] Publicar: `POST /vehicles`.
- [ ] Mis autos: `GET /vehicles?ownerId=`.
- [ ] Editar/pausar/borrar: `PATCH` / `PUT` / `DELETE /vehicles/{id}`.
- [ ] Calendario: `GET /vehicles/{id}/availability`.

**Reservas**
- [ ] Reservas de mis autos: `GET /rentals?ownerId=`.
- [ ] Cambiar estado: `PATCH /rentals/{id}` (`accepted`/`active`/`completed`/`cancelled`).
- [ ] Cobro: `POST /rentals/{id}/pay`.

**Carpool / rutas**
- [ ] Publicar ruta: `POST /adventure-routes`.
- [ ] Mis rutas: `GET /adventure-routes?ownerId=`.
- [ ] Pasajeros: listar / aceptar / rechazar / quitar (todos con `?ownerId=`).

---

## Convenciones que deben coincidir app ↔ backend
- Fechas **UTC ISO 8601**; rango `[start, end)` con fin **exclusivo**.
- Estados de reserva que **bloquean** = `pending/accepted/active`; que **liberan** = `cancelled/completed`.
- Sin token: siempre se envía `id`/`userId`/`ownerId`.
- Storage KYC hoy en disco del servidor (`/uploads/kyc/{userId}/...`); en producción migrará a
  S3/Cloudinary — **para la app no cambia nada**.
