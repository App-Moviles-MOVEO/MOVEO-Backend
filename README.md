# MOVEO Backend — Documentación de la API para la App Móvil

API REST para la plataforma de **alquiler de vehículos entre particulares** MOVEO.
Este documento contiene **todo lo que la app móvil necesita** para conectarse: URL base, autenticación, todos los endpoints, los cuerpos de petición/respuesta exactos, los valores válidos de cada campo y **datos de prueba listos para usar**.

---

## 1. Resumen técnico

| Tema | Detalle |
|------|---------|
| Framework | C# / **.NET 9** (ASP.NET Core Web API) |
| Base de datos | **MySQL 8** (Entity Framework Core + Migraciones) |
| Formato de datos | **JSON** en todas las peticiones y respuestas |
| Prefijo de rutas | **`/api/v1`** |
| Autenticación | **Sin token / sin JWT** — el login devuelve el usuario con su `id` (ver §3) |
| Documentación interactiva | **Swagger UI** en `/swagger` |
| Hashing de contraseñas | BCrypt |

---

## 2. URL base y entornos

Todas las rutas cuelgan del prefijo **`/api/v1`**.

| Entorno | URL base |
|---------|----------|
| Local (genérico, el que arranca por defecto) | `http://localhost:8080/api/v1` |
| Local (perfil http) | `http://localhost:5128/api/v1` |
| **Emulador Android** | `http://10.0.2.2:8080/api/v1` |
| Simulador iOS | `http://localhost:8080/api/v1` |
| Dispositivo físico (misma red Wi‑Fi) | `http://<IP-LOCAL-DEL-PC>:8080/api/v1` |
| Producción (Railway) | `https://<TU-DOMINIO-RAILWAY>/api/v1` |

> **Android:** dentro del emulador, el `localhost` de tu PC se accede como **`10.0.2.2`**.
> **Dispositivo físico:** usa la IP local del PC (ej. `http://192.168.1.50:8080/api/v1`) y asegúrate de estar en la misma red.
> Define la URL base como **una sola constante** en la app para cambiar dev/prod fácilmente.

---

## 3. Autenticación y manejo de sesión

El servidor es **stateless**: no usa tokens JWT ni header `Authorization`.

1. La app llama a `POST /auth/register` o `POST /auth/login`.
2. La respuesta incluye el objeto del usuario con su **`id`**.
3. La app **guarda ese `id`** localmente (`SharedPreferences` / `SecureStorage`).
4. En las siguientes peticiones, la app envía ese `id` como parámetro donde el endpoint lo pida (`userId`, `renterId`, `ownerId`, `reviewerId`, etc.).

El endpoint `POST /auth/logout` solo responde un mensaje; la sesión real se borra en el cliente.

### Headers requeridos en toda petición
```
Content-Type: application/json
Accept: application/json
```

---

## 4. Convenciones generales

- **IDs:** enteros (`int`), autoincrementales.
- **Fechas:** **ISO 8601 UTC** → `"2026-06-10T09:00:00Z"`. Envía y espera siempre ISO 8601.
- **Dinero:** número decimal (`decimal`). Moneda por defecto **PEN** (soles).
- **Campos opcionales:** los marcados con `?` o "opcional" pueden omitirse o ir como `null`.
- **`PUT`** reemplaza el recurso completo; **`PATCH`** actualiza solo los campos enviados.
- Las listas (`images`, `features`, etc.) se devuelven como `[]` cuando están vacías.

### Códigos de estado HTTP
| Código | Significado |
|--------|-------------|
| `200 OK` | Operación correcta (GET, PUT, PATCH) |
| `201 Created` | Recurso creado (POST) |
| `204 No Content` | Borrado correcto (DELETE) |
| `400 Bad Request` | Datos inválidos o faltantes |
| `401 Unauthorized` | Credenciales inválidas (solo login) |
| `404 Not Found` | Recurso no encontrado |
| `409 Conflict` | Conflicto (ej. email ya registrado) |

### Formato de error
```json
{ "message": "Invalid email or password" }
```
```json
{ "error": { "code": "NOT_FOUND", "message": "Vehículo no encontrado" } }
```

---

## 5. 🔑 Datos de prueba (ya cargados en la BD local)

Estos usuarios y vehículos están **sembrados** para que la app pueda probar de inmediato.
**Contraseña de todos:** `password123`

| User ID | Nombre | Email | Rol | Vehículo (id) | bodyType | Precio/día |
|---------|--------|-------|-----|---------------|----------|-----------|
| 1 | Rosa Martinez | `rosa@moveo.com` | owner | Toyota Corolla (id 1) | sedan | S/120 |
| 2 | Carlos Diaz | `carlos@moveo.com` | owner | Kia Rio (id 2) | compact | S/90 |
| 3 | Juan Perez | `juan@moveo.com` | renter | Hyundai Tucson (id 3) | suv | S/180 |
| 4 | Maria Lopez | `maria@moveo.com` | renter | Nissan Versa (id 4) | sedan | S/100 |

Ejemplo de login para probar:
```json
POST /api/v1/auth/login
{ "email": "rosa@moveo.com", "password": "password123" }
```

> Cada usuario tiene **un vehículo** registrado a su nombre. Hay vehículos de distintos `bodyType` (`sedan`, `compact`, `suv`) para probar los filtros del catálogo, p. ej. `GET /api/v1/vehicles?bodyType=suv`.

---

## 6. Autenticación (`/auth`)

### `POST /api/v1/auth/register` — Registrar usuario
```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan@example.com",
  "password": "miClave123",
  "phone": "+51987654321",
  "dni": "12345678",
  "licenseNumber": "Q12345678",
  "address": "Av. Siempre Viva 123",
  "role": "renter"
}
```
- Obligatorios: `firstName`, `lastName`, `email`, `password`.
- `phone`, `dni`, `licenseNumber`, `address` → opcionales.
- `role` → `"renter"` (arrendatario) o `"owner"` (propietario). Por defecto `"renter"`.

**Respuesta `201 Created`:**
```json
{
  "id": 1, "firstName": "Juan", "lastName": "Pérez",
  "email": "juan@example.com", "phone": "+51987654321",
  "dni": "12345678", "licenseNumber": "Q12345678",
  "role": "renter", "address": "Av. Siempre Viva 123"
}
```
`409 Conflict` si el email ya existe.

### `POST /api/v1/auth/login` — Iniciar sesión
```json
{ "email": "juan@example.com", "password": "miClave123" }
```
**`200 OK`:** mismo objeto que register (con el `id`). **`401`** si las credenciales son incorrectas.
> Guarda el `id` de la respuesta: es la "sesión" de la app.

### `GET /api/v1/auth/me?userId={id}` — Datos del usuario actual
`400` si falta `userId`, `404` si no existe.

### `POST /api/v1/auth/change-password`
```json
{ "userId": 1, "currentPassword": "miClave123", "newPassword": "nuevaClave456" }
```

### `POST /api/v1/auth/logout`
Responde `{ "message": "Logged out successfully" }`.

---

## 7. Usuarios (`/users`)

Perfil completo (verificaciones, estadísticas, preferencias, cuenta bancaria).

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/users` | Lista todos |
| `GET` | `/api/v1/users?email={email}` | Busca por email (array de 0 o 1) |
| `GET` | `/api/v1/users/{userId}` | Por ID |
| `POST` | `/api/v1/users` | Crear (administración; para la app usa `/auth/register`) |
| `PUT` | `/api/v1/users/{userId}` | Actualizar |
| `PATCH` | `/api/v1/users/{userId}` | Actualización parcial |
| `DELETE` | `/api/v1/users/{userId}` | Eliminar (`204`) |

**Respuesta `UserResource` (GET):**
```json
{
  "id": 1, "role": "renter",
  "firstName": "Juan", "lastName": "Pérez", "email": "juan@example.com",
  "phone": "+51987654321", "dni": "12345678", "licenseNumber": "Q12345678",
  "avatar": null,
  "verified": { "email": false, "phone": false, "dni": false, "license": false },
  "stats": { "totalRentals": 0, "totalSpent": 0, "totalEarned": 0, "activeRentals": 0, "completedRentals": 0, "canceledRentals": 0 },
  "preferences": {
    "language": "es",
    "notifications": { "email": true, "push": true, "sms": false },
    "autoAcceptRentals": false, "minimumRentalDays": 1, "instantBooking": false
  },
  "bankAccount": null,
  "createdAt": "2026-06-03T10:00:00Z", "updatedAt": "2026-06-03T10:00:00Z"
}
```

**Body de `PUT`/`PATCH`** (todos opcionales):
```json
{
  "firstName": "Juan Carlos",
  "lastName": "Pérez",
  "phone": "+51900000000",
  "avatar": "https://.../foto.jpg",
  "bankAccount": { "bankName": "BCP", "accountType": "ahorro", "accountNumber": "1234567890", "cci": "00212312345678901234", "verified": false },
  "preferences": { "language": "es", "notifications": { "email": true, "push": true, "sms": false } }
}
```

---

## 8. Vehículos (`/vehicles`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/vehicles` | Lista con filtros |
| `GET` | `/api/v1/vehicles/{id}` | Por ID |
| `POST` | `/api/v1/vehicles` | Crear |
| `PUT` | `/api/v1/vehicles/{id}` | Actualización completa |
| `PATCH` | `/api/v1/vehicles/{id}` | Actualización parcial |
| `DELETE` | `/api/v1/vehicles/{id}` | Eliminar (`204`) |

**Filtros (query params) en `GET /vehicles`:**
- `ownerId` (int) — vehículos de un propietario.
- `status` (string) — `"active"` (disponible), etc.
- `minPrice`, `maxPrice` (decimal) — rango de precio diario.
- `district` (string) — distrito.
- `bodyType` (string) — tipo de carrocería: `"compact"`, `"sedan"`, `"suv"`, `"pickup"`, etc.

Ejemplo: `GET /api/v1/vehicles?district=Miraflores&maxPrice=150&bodyType=suv`

**Body `POST` / `PUT`:**
```json
{
  "ownerId": 1,
  "brand": "Toyota", "model": "Corolla", "year": 2022,
  "color": "Blanco", "transmission": "automatic", "fuelType": "gasoline",
  "seats": 5, "licensePlate": "ABC-123",
  "location": { "district": "Miraflores", "address": "Av. Larco 1000", "lat": -12.1211, "lng": -77.0297 },
  "dailyPrice": 120.00, "depositAmount": 300.00,
  "description": "Auto en excelente estado",
  "images": ["https://.../1.jpg"],
  "features": ["Aire acondicionado", "Bluetooth"],
  "restrictions": ["No fumar"],
  "bodyType": "suv"
}
```
- Obligatorios: `ownerId`, `brand`, `model`, `year`, `color`, `transmission`, `fuelType`, `seats`, `licensePlate`, `location`, `dailyPrice`.
- `depositAmount`, `description`, `images`, `features`, `restrictions`, `bodyType` → opcionales.
- En `PUT` se agrega `status` (obligatorio). En `PATCH` todos los campos son opcionales.

**Respuesta `VehicleResource`:**
```json
{
  "id": 10, "ownerId": 1, "brand": "Toyota", "model": "Corolla",
  "year": 2022, "color": "Blanco", "transmission": "automatic",
  "fuelType": "gasoline", "seats": 5, "licensePlate": "ABC-123",
  "location": { "district": "Miraflores", "address": "Av. Larco 1000", "lat": -12.1211, "lng": -77.0297 },
  "dailyPrice": 120.00, "depositAmount": 300.00, "status": "active",
  "description": "Auto en excelente estado",
  "images": ["..."], "features": ["..."], "restrictions": ["..."],
  "createdAt": "2026-06-03T10:00:00Z", "updatedAt": "2026-06-03T10:00:00Z",
  "bodyType": "suv",
  "ownerName": "Rosa Martínez",
  "rating": 4.5,
  "reviewsCount": 12
}
```
**Campos calculados por el backend** (no se envían al crear):
- `ownerName`: nombre del propietario.
- `rating`: promedio de reseñas del vehículo (0 si no tiene).
- `reviewsCount`: cantidad de reseñas.

---

## 9. Alquileres / Rentals (`/rentals`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/rentals` | Lista (filtros: `renterId`, `ownerId`, `vehicleId`, `status`) |
| `GET` | `/api/v1/rentals/{id}` | Por ID |
| `GET` | `/api/v1/rentals/user/{userId}` | Del usuario (como renter u owner) |
| `GET` | `/api/v1/rentals/active` | Activos |
| `POST` | `/api/v1/rentals` | Crear |
| `PUT` | `/api/v1/rentals/{id}` | Actualización completa |
| `PATCH` | `/api/v1/rentals/{id}` | Cambiar estado, etc. |
| `POST` | `/api/v1/rentals/{id}/pay` | **Pagar la reserva en un paso** (recomendado) |
| `DELETE` | `/api/v1/rentals/{id}` | Eliminar (`204`) |

**Estados (`status`) y su flujo:**
```
pending  →  accepted  →  active  →  completed
   └──────────────┴───────────┴──→  cancelled
```

**Body `POST`:**
```json
{
  "vehicleId": 10, "renterId": 3, "ownerId": 1,
  "startDate": "2026-06-10T09:00:00Z", "endDate": "2026-06-12T18:00:00Z",
  "totalPrice": 240.00,
  "pickupLocation": "Av. Larco 1000, Miraflores",
  "returnLocation": "Av. Larco 1000, Miraflores",
  "notes": "Recoger con tanque lleno",
  "adventureRouteId": null
}
```

**Respuesta `RentalResource`:**
```json
{
  "id": 100, "vehicleId": 10, "renterId": 3, "ownerId": 1,
  "startDate": "2026-06-10T09:00:00Z", "endDate": "2026-06-12T18:00:00Z",
  "totalPrice": 240.00, "status": "pending",
  "pickupLocation": "...", "returnLocation": "...", "notes": "...",
  "adventureRouteId": null, "vehicleRated": false, "vehicleRating": null,
  "createdAt": "2026-06-03T10:00:00Z", "acceptedAt": null, "completedAt": null,
  "vehicleName": "Toyota Corolla",
  "vehicleImage": "https://.../1.jpg"
}
```
**Campos calculados:** `vehicleName` (marca + modelo) y `vehicleImage` (primera imagen) vía JOIN con el vehículo.

**`PATCH`** — transiciones de estado:
```json
{ "status": "accepted", "acceptedAt": "2026-06-04T08:00:00Z" }
```
Campos opcionales: `status`, `vehicleRated`, `vehicleRating`, `acceptedAt`, `completedAt`.

### `POST /api/v1/rentals/{id}/pay` — Pagar la reserva en un paso
Crea el pago, lo enlaza a la reserva y lo marca **completado**. Ideal para el botón **Yape**.

**Body (todo opcional):**
```json
{
  "paymentMethod": "yape",
  "amount": 240.00,
  "currency": "PEN",
  "type": "rental_payment",
  "transactionId": "yape-abc123",
  "description": "Pago reserva #100"
}
```
- Si no envías `amount`, se usa el `totalPrice` de la reserva.
- `paymentMethod` por defecto `"yape"`. También: `"card"`, `"cash"`, `"transfer"`, `"plin"`.

**Respuesta `200 OK`:**
```json
{
  "rental": { "...RentalResource..." },
  "payment": { "id": 50, "status": "completed", "method": "yape", "amount": 240.00, "...": "..." }
}
```

---

## 10. Reseñas de vehículos (`/reviews`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/reviews` | Lista (filtros: `vehicleId`, `rentalId`, `reviewerId`, `revieweeId`) |
| `GET` | `/api/v1/reviews/{id}` | Por ID |
| `GET` | `/api/v1/reviews/rental/{rentalId}` | Por alquiler |
| `GET` | `/api/v1/reviews/reviewer/{reviewerId}` | Hechas por un usuario |
| `GET` | `/api/v1/reviews/reviewee/{revieweeId}` | Recibidas por un usuario |
| `POST` | `/api/v1/reviews` | Crear |
| `PUT` | `/api/v1/reviews/{id}` | Actualizar |
| `DELETE` | `/api/v1/reviews/{id}` | Eliminar (`204`) |

**Body `POST`:**
```json
{ "rentalId": 100, "vehicleId": 10, "reviewerId": 3, "revieweeId": 1, "rating": 5, "comment": "Excelente vehículo", "type": "vehicle" }
```
- `rating`: 1 a 5. `vehicleId` y `comment` opcionales.
- La respuesta incluye **`reviewerName`** (nombre de quien dejó la reseña).

---

## 11. Reseñas entre usuarios (`/user-reviews`)

Calificaciones **entre personas** (propietario ↔ arrendatario).

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/user-reviews` | Lista (filtros: `reviewedUserId`, `reviewerId`, `rentalId`, `type`) |
| `GET` | `/api/v1/user-reviews/{id}` | Por ID |
| `POST` | `/api/v1/user-reviews` | Crear |
| `PUT` | `/api/v1/user-reviews/{id}` | Actualizar |
| `DELETE` | `/api/v1/user-reviews/{id}` | Eliminar (`204`) |

**Body `POST`:**
```json
{ "reviewerId": 1, "reviewedUserId": 3, "rentalId": 100, "rating": 5, "comment": "Muy buen arrendatario", "type": "owner_to_renter" }
```
- `type`: `"owner_to_renter"` o `"renter_to_owner"`. `comment` opcional.
- La respuesta incluye **`reviewerName`**.

---

## 12. Rutas de aventura + Carpooling (`/adventure-routes`)

Una misma entidad sirve para **rutas/experiencias** (asociables a un alquiler vía `adventureRouteId`) y para **viajes compartidos (carpool)** usando los campos de carpool.

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/adventure-routes` | Lista (filtros: `ownerId`, `type`, `difficulty`, `featured`, `onlyWomen`) |
| `GET` | `/api/v1/adventure-routes/{routeId}` | Por ID |
| `POST` | `/api/v1/adventure-routes` | Crear (ruta o viaje carpool) |
| `PUT` | `/api/v1/adventure-routes/{routeId}` | Actualizar |
| `POST` | `/api/v1/adventure-routes/{routeId}/book` | Reservar asiento(s) de carpool |
| `DELETE` | `/api/v1/adventure-routes/{routeId}` | Eliminar (`204`) |

**Body `POST` (ruta de aventura):**
```json
{
  "ownerId": 1, "name": "ruta-canta", "title": "Ruta a Canta",
  "description": "Escapada a la sierra de Lima",
  "startLocation": "Lima", "endLocation": "Canta",
  "type": "mountain", "duration": 8, "difficulty": "medium",
  "estimatedCost": 180.00, "vehicleName": "SUV 4x4",
  "imageUrl": "https://.../canta.jpg", "tags": ["sierra", "naturaleza"],
  "featured": true, "maxCapacity": 4
}
```
- `duration` en horas. `difficulty`: `"easy"` | `"medium"` | `"hard"`.
- La respuesta incluye `rating`, `reviewsCount`, `createdAt`, `updatedAt`.

**Body `POST` (viaje carpool):** agrega los campos de carpool:
```json
{
  "ownerId": 1, "name": "lima-canta-0610", "title": "Viaje Lima → Canta",
  "description": "Salida sábado temprano",
  "startLocation": "Lima", "endLocation": "Canta",
  "type": "carpool", "duration": 3, "difficulty": "easy", "estimatedCost": 0,
  "departureDate": "2026-06-10T00:00:00Z", "departureTime": "06:30",
  "seatsTotal": 4, "seatsAvailable": 4, "pricePerSeat": 25.00,
  "onlyWomen": false, "community": "Universitarios UNI",
  "lat": -11.9, "lng": -76.9
}
```
- `seatsAvailable`: si no lo envías, se inicializa igual a `seatsTotal`.
- `status` del viaje: `"active"` | `"full"` | `"cancelled"` | `"completed"` (pasa a `"full"` al agotarse los asientos).
- Filtro `GET /adventure-routes?onlyWomen=true` para viajes solo de mujeres.

### `POST /api/v1/adventure-routes/{routeId}/book` — Reservar asiento(s)
```json
{ "seats": 1 }
```
`200 OK` devuelve la ruta con `seatsAvailable` reducido. `400` si no hay asientos suficientes; `404` si no existe.

---

## 13. Pagos (`/payments`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/payments` | Lista (filtros: `payerId`, `recipientId`, `rentalId`, `status`, `type`) |
| `GET` | `/api/v1/payments/{id}` | Por ID |
| `GET` | `/api/v1/payments/payer/{payerId}` | Pagos hechos por un usuario |
| `GET` | `/api/v1/payments/recipient/{recipientId}` | Pagos recibidos |
| `GET` | `/api/v1/payments/rental/{rentalId}` | Pagos de un alquiler |
| `POST` | `/api/v1/payments` | Crear |
| `PUT` | `/api/v1/payments/{id}` | Actualizar |
| `PATCH` | `/api/v1/payments/{id}` | Actualización parcial |
| `DELETE` | `/api/v1/payments/{id}` | Eliminar (`204`) |

**Body `POST`:**
```json
{
  "payerId": 3, "recipientId": 1, "rentalId": 100,
  "amount": 240.00, "currency": "PEN",
  "paymentMethod": "yape", "type": "rental_payment",
  "status": "pending", "description": "Pago del alquiler #100",
  "dueDate": "2026-06-10T00:00:00Z"
}
```
- `paymentMethod`: `"card"`, `"cash"`, `"transfer"`, `"yape"`, `"plin"`, etc. (texto libre; también acepta el alias `"method"`).
- `type`: `"rental_payment"` (por defecto), `"deposit"`, `"damage_payment"`.

> 💡 Para el flujo del cliente lo más simple es **`POST /rentals/{id}/pay`** (§9), que crea y completa el pago en un solo paso.

---

## 14. Notificaciones (`/notifications`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/notifications` | Lista (filtros: `userId`, `read`) |
| `GET` | `/api/v1/notifications/{id}` | Por ID |
| `GET` | `/api/v1/notifications/user/{userId}` | De un usuario |
| `GET` | `/api/v1/notifications/user/{userId}/unread` | No leídas |
| `POST` | `/api/v1/notifications` | Crear |
| `PATCH` | `/api/v1/notifications/{id}` | Marcar como leída (`{ "read": true }`) |
| `PUT` | `/api/v1/notifications/{id}/read` | Marcar una como leída |
| `PUT` | `/api/v1/notifications/user/{userId}/read-all` | Marcar todas como leídas |
| `DELETE` | `/api/v1/notifications/{id}` | Eliminar (`204`) |

**Body `POST`:**
```json
{
  "userId": 1, "title": "Reserva aceptada", "body": "Tu reserva #100 fue aceptada",
  "type": "rental_accepted", "relatedId": 100, "relatedType": "rental",
  "actionUrl": "/rentals/100", "actionLabel": "Ver reserva"
}
```
- `body` también acepta el alias `message`.

**Respuesta:** incluye `read` (bool), `createdAt`, `readAt`.

---

## 15. Soporte / Tickets (`/support-tickets`)

Tickets de soporte, incluidos **daños** y **disputas**, con hilo de mensajes.

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/support-tickets` | Lista (filtros: `userId`, `status`, `type`) |
| `GET` | `/api/v1/support-tickets/{id}` | Por ID (incluye `messages`) |
| `GET` | `/api/v1/support-tickets/user/{userId}` | De un usuario |
| `GET` | `/api/v1/support-tickets/status/{status}` | Por estado |
| `POST` | `/api/v1/support-tickets` | Crear |
| `PUT` / `PATCH` | `/api/v1/support-tickets/{id}` | Actualizar |
| `PATCH` | `/api/v1/support-tickets/{id}/close` | Cerrar |
| `DELETE` | `/api/v1/support-tickets/{id}` | Eliminar (`204`) |
| `GET` | `/api/v1/support-tickets/{ticketId}/messages` | Mensajes del ticket |
| `POST` | `/api/v1/support-tickets/{ticketId}/messages` | Añadir mensaje |

**Body `POST` ticket:**
```json
{
  "userId": 3, "subject": "Problema con el vehículo",
  "description": "El auto tenía un rayón al recogerlo",
  "category": "general", "priority": "medium", "type": "general",
  "relatedId": 100, "relatedType": "rental"
}
```
Campos extra para **daño**: `estimatedCost`, `vehicleId`, `vehicleName`, `rentalId`, `renterId`, `renterName`, `attachments` (lista de URLs).

**Body `POST` mensaje:**
```json
{ "senderId": 3, "message": "Adjunto fotos del daño", "isStaffReply": false }
```

---

## 16. Chat / Mensajes (`/messages`)

Chat **1 a 1** entre dos usuarios (arrendatario ↔ propietario, o pasajero ↔ conductor de carpool). Persistido en BD.

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET` | `/api/v1/messages?userId={a}&otherUserId={b}` | Conversación entre dos usuarios (orden cronológico) |
| `GET` | `/api/v1/messages/conversations/{userId}` | Lista de conversaciones (último mensaje + no leídos) |
| `POST` | `/api/v1/messages` | Enviar un mensaje |
| `PUT` | `/api/v1/messages/{id}/read` | Marcar un mensaje como leído |
| `PUT` | `/api/v1/messages/read?userId={a}&otherUserId={b}` | Marcar como leídos los recibidos de otro usuario |

**Body `POST`:**
```json
{ "senderId": 3, "receiverId": 1, "content": "Hola, ¿sigue disponible el auto?" }
```

**Respuesta `MessageResource`:**
```json
{ "id": 10, "senderId": 3, "receiverId": 1, "content": "Hola...", "read": false, "createdAt": "2026-06-03T20:00:00Z", "readAt": null }
```

**Respuesta de `GET /messages/conversations/{userId}`:**
```json
[
  {
    "otherUserId": 1, "otherUserName": "Rosa Martínez", "otherUserAvatar": null,
    "lastMessage": "Sí, está disponible", "lastMessageAt": "2026-06-03T20:05:00Z",
    "unreadCount": 1
  }
]
```

---

## 17. Tabla resumen de todos los endpoints

| Módulo | Endpoints base (prefijo `/api/v1`) |
|--------|-------------------------------------|
| Auth | `POST /auth/register`, `POST /auth/login`, `GET /auth/me`, `POST /auth/change-password`, `POST /auth/logout` |
| Usuarios | `GET/POST /users`, `GET/PUT/PATCH/DELETE /users/{id}` |
| Vehículos | `GET/POST /vehicles`, `GET/PUT/PATCH/DELETE /vehicles/{id}` (filtro `bodyType`; respuesta con `ownerName`/`rating`/`reviewsCount`) |
| Alquileres | `GET/POST /rentals`, `GET/PUT/PATCH/DELETE /rentals/{id}`, `GET /rentals/user/{id}`, `GET /rentals/active`, `POST /rentals/{id}/pay` |
| Reseñas vehículo | `GET/POST /reviews`, `GET/PUT/DELETE /reviews/{id}`, `GET /reviews/{rental\|reviewer\|reviewee}/{id}` |
| Reseñas usuario | `GET/POST /user-reviews`, `GET/PUT/DELETE /user-reviews/{id}` |
| Rutas aventura + Carpool | `GET/POST /adventure-routes`, `GET/PUT/DELETE /adventure-routes/{id}`, `POST /adventure-routes/{id}/book` |
| Pagos | `GET/POST /payments`, `GET/PUT/PATCH/DELETE /payments/{id}`, `GET /payments/{payer\|recipient\|rental}/{id}` |
| Notificaciones | `GET/POST /notifications`, `PATCH/DELETE /notifications/{id}`, `PUT /notifications/{id}/read`, `PUT /notifications/user/{id}/read-all` |
| Soporte | `GET/POST /support-tickets`, `GET/PUT/PATCH/DELETE /support-tickets/{id}`, `PATCH /support-tickets/{id}/close`, `GET/POST /support-tickets/{id}/messages` |
| Chat | `GET/POST /messages`, `GET /messages/conversations/{userId}`, `PUT /messages/{id}/read`, `PUT /messages/read` |

---

## 18. Ejecutar el backend localmente

### Requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- **MySQL 8** corriendo (servicio `MySQL80`)

### Pasos
1. Configura la conexión en `appsettings.Development.json` (ya viene apuntando a local):
   ```json
   { "ConnectionStrings": { "DefaultConnection": "server=localhost;user=root;password=1234;database=moveo_db" } }
   ```
2. Restaura, compila y ejecuta:
   ```bash
   dotnet restore
   dotnet run
   ```
3. El servidor arranca en `http://localhost:8080`. Swagger en `http://localhost:8080/swagger`.

### Base de datos (Migraciones EF Core)
El backend usa **Migraciones EF Core** que se aplican solas al arrancar (`Database.Migrate()`). Si la BD no existe, se crea automáticamente con todas las tablas. Para crear una nueva migración al cambiar un modelo:
```bash
dotnet ef migrations add NombreDelCambio
dotnet ef database update   # o simplemente "dotnet run"
```

### Variables de entorno para la BD (producción / Railway)
Orden de resolución: `ConnectionStrings:DefaultConnection` → `MYSQL_URL`/`DATABASE_URL` → `MYSQLHOST`/`MYSQLPORT`/`MYSQLUSER`/`MYSQLPASSWORD`/`MYSQLDATABASE`. `PORT` define el puerto HTTP (por defecto `8080`).

---

## 19. CORS

El backend permite peticiones desde `*.vercel.app`, `https://moveo-frontend-0sbk.onrender.com` y `http://localhost:*`.

> **Las apps móviles nativas no están sujetas a CORS** (es una restricción de navegadores), así que la app móvil llama a la API sin problema. Para web/PWA en otro dominio, añádelo en [Program.cs](Program.cs).

---

## 20. Pendientes a coordinar con la app móvil

1. **JWT / token:** hoy la sesión es por `userId` en claro. Para producción conviene login con JWT y proteger endpoints.
2. **Subida de imágenes:** `images`, `avatar`, `attachments` son **URLs**; falta definir storage (S3/Cloudinary).
3. **Validación de propiedad:** no se valida que `ownerId`/`renterId` sea el usuario logueado.
4. **Stripe:** por ahora solo Yape; el pago con tarjeta real requiere un endpoint de PaymentIntent + Stripe Secret Key.
5. **Paginación:** los GET de colección devuelven todo; convendrá paginar cuando crezcan los datos.

---

*Documentación para la integración de la app móvil MOVEO. Mantener actualizada al cambiar controllers o resources.*
