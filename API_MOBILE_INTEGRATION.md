# MOVEO Backend — Guía de integración para la App Móvil

Documento con **todos los datos que la app móvil debe pasar** para funcionar con el backend MOVEO.

---

## 1. Configuración base (lo primero que debes setear)

| Dato | Valor |
|------|-------|
| **Base URL (local / mismo PC)** | `http://localhost:8080` |
| **Base URL (emulador Android)** | `http://10.0.2.2:8080` |
| **Base URL (dispositivo físico, misma red Wi‑Fi)** | `http://<IP-de-tu-PC>:8080` (ej. `http://192.168.1.45:8080`) |
| **Prefijo de todos los endpoints** | `/api/v1` |
| **Documentación interactiva (Swagger)** | `http://localhost:8080/swagger/index.html` |
| **Formato** | JSON (`Content-Type: application/json`) |
| **Puerto** | `8080` (definido por la variable `PORT`, default `8080`) |

> ⚠️ **Importante:** `http://localhost:8080/swagger/index.html` es solo la documentación.
> La app **NO** debe llamar a `/swagger`. Las llamadas reales van a `http://localhost:8080/api/v1/...`

### Notas para la app móvil
- **Emulador Android**: `localhost` apunta al propio emulador, NO a tu PC. Usa `http://10.0.2.2:8080`.
- **Dispositivo físico**: usa la IP local de tu PC (ejecuta `ipconfig` en Windows y toma la "Dirección IPv4").
- **HTTP en claro**: como es `http://` (no `https`), en Android necesitas permitir tráfico no cifrado
  (`android:usesCleartextTraffic="true"` en el `AndroidManifest.xml`, o un `network_security_config`).
- **CORS**: las apps nativas no envían cabecera `Origin`, así que **no hay problema de CORS** desde móvil.
  (El backend ya permite `capacitor://localhost`, `ionic://localhost` y `http://localhost` para apps híbridas.)

---

## 2. Autenticación — ⚠️ NO hay token JWT

Este backend **no usa tokens ni Bearer**. El flujo es:

1. La app hace `POST /api/v1/auth/login` con email y password.
2. El backend responde con el **objeto del usuario** (incluye su `id` y su `role`).
3. La app **guarda ese `id` y `role` localmente** (ej. en SharedPreferences / SecureStorage).
4. En las siguientes peticiones, la app **envía ese id** como parámetro:
   `userId`, `renterId`, `ownerId`, `payerId`, etc. (según el endpoint).

> No hay header de autorización. La sesión = "guardar el id del usuario logueado".

### POST `/api/v1/auth/login`
**Request:**
```json
{
  "email": "usuario@correo.com",
  "password": "miPassword123"
}
```
**Respuesta 200 (lo que debes guardar):**
```json
{
  "id": 1,
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "usuario@correo.com",
  "phone": "987654321",
  "dni": "12345678",
  "licenseNumber": "Q12345678",
  "role": "renter",
  "address": "Av. Lima 123"
}
```
**Errores:** `400` (faltan email/password), `401` (credenciales inválidas).

### POST `/api/v1/auth/register`
**Request** (campos con `?` son opcionales):
```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "usuario@correo.com",
  "password": "miPassword123",
  "phone": "987654321",
  "dni": "12345678",
  "licenseNumber": "Q12345678",
  "address": "Av. Lima 123",
  "role": "renter",
  "preferences": null
}
```
| Campo | Obligatorio | Notas |
|-------|-------------|-------|
| `firstName`, `lastName`, `email`, `password` | ✅ Sí | |
| `phone`, `dni`, `licenseNumber`, `address`, `preferences` | ❌ No | |
| `role` | ❌ No | Default `"renter"`. Valores típicos: `"renter"` (cliente) / `"owner"` (dueño) |

**Respuesta 201:** mismo objeto de usuario que el login. **Error:** `409` si el email ya existe.

### POST `/api/v1/auth/change-password`
```json
{ "userId": 1, "currentPassword": "vieja", "newPassword": "nueva" }
```

### GET `/api/v1/auth/me?userId=1`
Devuelve los datos del usuario por su id.

### POST `/api/v1/auth/logout`
No requiere nada; solo devuelve `{ "message": "Logged out successfully" }`. La app simplemente borra el id guardado.

---

## 3. Roles disponibles
- `renter` → cliente que alquila vehículos.
- `owner` → dueño que publica vehículos.

La pantalla/sección de **"clientes"** corresponde a `role = "renter"`.

---

## 4. Endpoints principales

Todos llevan el prefijo `http://localhost:8080/api/v1`.

### 👤 Usuarios — `/users`
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/users` | Todos los usuarios |
| GET | `/users?email=correo@x.com` | Buscar por email |
| GET | `/users/{userId}` | Usuario por id |
| POST | `/users` | Crear usuario |
| PUT | `/users/{userId}` | Actualizar usuario |
| PATCH | `/users/{userId}` | Actualizar parcialmente |
| DELETE | `/users/{userId}` | Eliminar |

### 🚗 Vehículos — `/vehicles`
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/vehicles` | Listar (con filtros, ver abajo) |
| GET | `/vehicles/{id}` | Por id (incluye `ownerName`, `rating`, `reviewsCount`) |
| GET | `/vehicles/{id}/availability` | **Fechas ocupadas** del vehículo (para el calendario) |
| POST | `/vehicles` | Crear vehículo |
| PUT | `/vehicles/{id}` | Actualizar completo |
| PATCH | `/vehicles/{id}` | Actualizar parcial |
| DELETE | `/vehicles/{id}` | Eliminar |

**Filtros (query params) en GET `/vehicles`:** (todos opcionales y combinables)

| Param | Tipo | Ejemplo / valores |
|-------|------|-------------------|
| `ownerId` | int | `?ownerId=3` |
| `status` | string | `active`, `suspended`, … |
| `minPrice` / `maxPrice` | decimal | `?minPrice=50&maxPrice=200` |
| `district` | string | `?district=Miraflores` (búsqueda parcial) |
| `bodyType` | string | `compact` \| `sedan` \| `suv` \| `pickup` |
| `transmission` | string | `automatic` \| `manual` |
| `fuelType` | string | `gasoline` \| `diesel` \| `electric` \| `hybrid` \| `gas` |
| `startDate` + `endDate` | fecha | **Devuelve solo autos libres en ese rango** (ver abajo) |
| `lat` + `lng` + `sort=distance` | double | Ordena por cercanía a esa coordenada |
| `page` + `pageSize` | int | Paginación (default `pageSize=20`) |

Ej: `/vehicles?district=Miraflores&transmission=automatic&minPrice=50&maxPrice=200`

**🔑 Catálogo filtrado por fechas** — si envías `startDate` **y** `endDate`, la lista excluye los
vehículos que ya tienen una reserva (`pending`/`accepted`/`active`) que se solape con ese rango:
```
GET /api/v1/vehicles?startDate=2026-06-20&endDate=2026-06-22
```
- Acepta fecha simple `yyyy-MM-dd` o ISO completo; se interpreta en **UTC**.
- Si `endDate <= startDate` → **400** `{ "error": "invalid_request", "message": "endDate debe ser posterior a startDate" }`.
- Si no envías fechas, comportamiento normal (todos los autos).

**📅 Disponibilidad de un vehículo (GET `/vehicles/{id}/availability`)**
Para pintar en el calendario las fechas NO seleccionables cuando el cliente va a reservar.
```
GET /api/v1/vehicles/5/availability?from=2026-06-01&to=2026-08-31
```
- `from` / `to` opcionales. Default: **desde hoy hasta +3 meses**.
- Solo cuenta reservas `pending`, `accepted`, `active`.

**Respuesta 200:**
```json
{
  "vehicleId": 5,
  "busyRanges": [
    { "startDate": "2026-06-20T10:00:00", "endDate": "2026-06-22T10:00:00" },
    { "startDate": "2026-07-01T09:00:00", "endDate": "2026-07-05T09:00:00" }
  ]
}
```
> Los rangos usan `[startDate, endDate)` — **fin exclusivo**: una reserva que termina el día 22
> permite a otra empezar el día 22 (entrega y recojo el mismo día).

**Body para crear vehículo (POST `/vehicles`):**
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
    "address": "Av. Larco 123",
    "lat": -12.1211,
    "lng": -77.0297
  },
  "dailyPrice": 120.0,
  "depositAmount": 200.0,
  "description": "Auto en excelente estado",
  "images": ["https://...jpg"],
  "features": ["Aire acondicionado", "GPS"],
  "restrictions": ["No fumar"],
  "bodyType": "sedan"
}
```
> Obligatorios: `ownerId`, `brand`, `model`, `year`, `color`, `transmission`, `fuelType`, `seats`, `licensePlate`, `location`, `dailyPrice`.
> Opcionales: `depositAmount`, `description`, `images`, `features`, `restrictions`, `bodyType`.

### 📅 Reservas / Alquileres — `/rentals`
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/rentals` | Listar (filtros: `renterId`, `ownerId`, `vehicleId`, `status`) |
| GET | `/rentals/{id}` | Por id |
| GET | `/rentals/user/{userId}` | Reservas de un usuario |
| GET | `/rentals/active` | Reservas activas |
| POST | `/rentals` | Crear reserva |
| PUT | `/rentals/{id}` | Actualizar |
| PATCH | `/rentals/{id}` | Actualizar estado/campos |
| POST | `/rentals/{id}/pay` | Pagar la reserva (Yape, un solo paso) |
| DELETE | `/rentals/{id}` | Eliminar |

**Body para crear reserva (POST `/rentals`):**
```json
{
  "vehicleId": 5,
  "renterId": 1,
  "ownerId": 3,
  "startDate": "2026-06-20T10:00:00",
  "endDate": "2026-06-22T10:00:00",
  "totalPrice": 240.0,
  "pickupLocation": "Av. Larco 123",
  "returnLocation": "Av. Larco 123",
  "notes": "Recojo a las 10am",
  "adventureRouteId": null
}
```
> `renterId` = el id del cliente logueado. `ownerId` = dueño del vehículo.

**⚠️ Validaciones al crear reserva (POST `/rentals`)** — el backend valida en el servidor (no confíes solo en la app):

| Caso | Respuesta |
|------|-----------|
| Reserva OK | **201** con la reserva creada (`status: "pending"`) |
| `endDate <= startDate` | **400** `{ "error": "invalid_request", "message": "endDate debe ser posterior a startDate" }` |
| `startDate` en el pasado (antes de hoy 00:00 UTC) | **400** `invalid_request` |
| `renterId == ownerId` (dueño reservando su propio auto) | **400** `invalid_request` |
| Vehículo inexistente | **404** `{ "error": "vehicle_not_found" }` |
| Vehículo con `status` ≠ `active` | **409** `{ "error": "vehicle_not_active" }` |
| **Fechas ya ocupadas por otra reserva** | **409** (ver abajo) |

**Conflicto de fechas (409):** si otro cliente ya tiene el auto en ese rango, recibes:
```json
{
  "error": "vehicle_not_available",
  "message": "El vehículo ya tiene una reserva en ese rango de fechas",
  "conflictingRanges": [
    { "startDate": "2026-07-10T00:00:00", "endDate": "2026-07-12T00:00:00" }
  ]
}
```
> La app puede usar `conflictingRanges` para sugerirle al cliente otras fechas libres.
> Recomendado: antes de crear, consulta `GET /vehicles/{id}/availability` para deshabilitar
> los días ocupados en el calendario y evitar el 409.

**Cancelar libera las fechas:** `PATCH /rentals/{id}` con `{ "status": "cancelled" }` deja el rango
disponible de nuevo (vuelve a aparecer en el catálogo y en `availability`).

**Body para pagar (POST `/rentals/{id}/pay`):** todos opcionales (si no mandas `amount`, usa el total de la reserva).
```json
{
  "paymentMethod": "yape",
  "amount": 240.0,
  "currency": "PEN",
  "type": "rental_payment",
  "transactionId": "YAPE-123456",
  "description": "Pago de reserva"
}
```

### ⭐ Reseñas de vehículos — `/Reviews`
| Método | Ruta |
|--------|------|
| GET | `/Reviews` (filtros: `vehicleId`, `rentalId`, `reviewerId`, `revieweeId`) |
| GET | `/Reviews/{id}`, `/Reviews/rental/{rentalId}`, `/Reviews/reviewer/{reviewerId}`, `/Reviews/reviewee/{revieweeId}` |
| POST | `/Reviews` |
| PUT | `/Reviews/{id}` |
| DELETE | `/Reviews/{id}` |

### ⭐ Reseñas entre usuarios — `/user-reviews`
| Método | Ruta |
|--------|------|
| GET | `/user-reviews` (filtros: `reviewedUserId`, `reviewerId`, `rentalId`, `type`) |
| GET/POST/PUT/DELETE | `/user-reviews/{id}` |

`type`: `owner_to_renter` o `renter_to_owner`.

### 🏔️ Rutas de aventura / Carpool — `/adventure-routes`
| Método | Ruta |
|--------|------|
| GET | `/adventure-routes` (filtros: `ownerId`, `type`, `difficulty`, `featured`, `onlyWomen`) |
| GET | `/adventure-routes/{routeId}` |
| POST | `/adventure-routes` |
| POST | `/adventure-routes/{routeId}/book` → body `{ "seats": 1 }` |
| PUT | `/adventure-routes/{routeId}` |
| DELETE | `/adventure-routes/{routeId}` |

### 💳 Pagos — `/payments`
| Método | Ruta |
|--------|------|
| GET | `/payments` (filtros: `payerId`, `recipientId`, `rentalId`, `status`, `type`) |
| GET | `/payments/{id}`, `/payments/payer/{payerId}`, `/payments/recipient/{recipientId}`, `/payments/rental/{rentalId}` |
| POST | `/payments` |
| PUT / PATCH / DELETE | `/payments/{id}` |

### 🔔 Notificaciones — `/Notifications`
| Método | Ruta |
|--------|------|
| GET | `/Notifications?userId=1` / `?userId=1&read=false` |
| GET | `/Notifications/user/{userId}`, `/Notifications/user/{userId}/unread` |
| POST | `/Notifications` |
| PATCH | `/Notifications/{id}` → body `{ "read": true }` |
| PUT | `/Notifications/{id}/read`, `/Notifications/user/{userId}/read-all` |
| DELETE | `/Notifications/{id}` |

### 💬 Chat 1 a 1 — `/messages`
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/messages?userId=1&otherUserId=2` | Conversación entre dos usuarios |
| GET | `/messages/conversations/{userId}` | Lista de conversaciones del usuario |
| POST | `/messages` | Enviar mensaje |
| PUT | `/messages/{id}/read` | Marcar 1 mensaje como leído |
| PUT | `/messages/read?userId=1&otherUserId=2` | Marcar toda la conversación como leída |

**Body para enviar mensaje (POST `/messages`):**
```json
{ "senderId": 1, "receiverId": 2, "content": "Hola, ¿sigue disponible el auto?" }
```

### 🎫 Soporte — `/support-tickets`
| Método | Ruta |
|--------|------|
| GET | `/support-tickets` (filtros: `userId`, `status`, `type`) |
| GET | `/support-tickets/{id}`, `/support-tickets/user/{userId}`, `/support-tickets/status/{status}` |
| POST | `/support-tickets` |
| PUT / PATCH | `/support-tickets/{id}` |
| PATCH | `/support-tickets/{id}/close` |
| GET | `/support-tickets/{ticketId}/messages` |
| POST | `/support-tickets/{ticketId}/messages` |

---

## 5. Flujo de renta por fechas (catálogo → detalle → fechas → reserva → pago)

Así funciona hoy el backend para el flujo completo del cliente/renter:

1. **Catálogo:** `GET /vehicles` con los filtros que use el cliente (distrito, transmisión,
   combustible, precio, y opcionalmente `startDate`/`endDate` para ver solo libres en ese rango).
2. **Detalle:** `GET /vehicles/{id}` (trae `ownerName`, `rating`, `reviewsCount`).
3. **Selección de fechas:** `GET /vehicles/{id}/availability` → deshabilita en el calendario los
   `busyRanges` para que el cliente no elija días ocupados.
4. **Reserva:** `POST /rentals`. El servidor revalida y puede devolver **409** (`vehicle_not_available`)
   si justo otro cliente tomó esas fechas (protección ante carreras). Se crea con `status: "pending"`.
5. **Pago:** `POST /rentals/{id}/pay` (Yape, un solo paso).

### ⏱️ Expiración de reservas `pending` sin pagar
Una reserva `pending` **bloquea las fechas**. Si el cliente la crea y **no paga en 30 minutos**,
un proceso automático del backend la pasa a `cancelled` y **libera las fechas**.
- Una `pending` **ya pagada** (tiene un pago `completed`) **no** se expira.
- Mensaje sugerido en la app: *"Tu reserva expira en 30 minutos si no completas el pago."*

### Convenciones (app y backend deben coincidir)
| Tema | Convención |
|------|------------|
| Zona horaria | Todo en **UTC**, ISO 8601 (`2026-06-20T10:00:00Z`) |
| Rango de reserva | `[startDate, endDate)` — fin **exclusivo** |
| Estados que **bloquean** fechas | `pending`, `accepted`, `active` |
| Estados que **liberan** fechas | `cancelled`, `completed` |
| Conflicto de fechas | HTTP **409** `error: "vehicle_not_available"` (+ `conflictingRanges`) |
| Datos inválidos | HTTP **400** `error: "invalid_request"` |

---

## 6. Resumen rápido para arrancar la app

1. **Configura la Base URL** según dónde corras la app:
   - Emulador Android → `http://10.0.2.2:8080`
   - Dispositivo físico → `http://<IP-de-tu-PC>:8080`
   - PC / web → `http://localhost:8080`
2. Todas las rutas: **`{BaseURL}/api/v1/...`**
3. **Login:** `POST /api/v1/auth/login` → guarda `id` y `role` del usuario.
4. **No hay token.** Manda el `id` guardado como `userId` / `renterId` / `ownerId` en cada petición.
5. La sección **"clientes"** = usuarios con `role: "renter"`.

---

## 7. Códigos de respuesta comunes
| Código | Significado |
|--------|-------------|
| 200 | OK |
| 201 | Creado correctamente |
| 204 | Eliminado (sin contenido) |
| 400 | Datos inválidos / faltan campos (`invalid_request`) |
| 401 | Credenciales inválidas (login) |
| 404 | No encontrado (`vehicle_not_found`) |
| 409 | Conflicto: email ya registrado, fechas ocupadas (`vehicle_not_available`), o vehículo no activo (`vehicle_not_active`) |
