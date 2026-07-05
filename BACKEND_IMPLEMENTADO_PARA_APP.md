# Backend implementado — MOVEO / WheelsPe

Respuesta al documento `FLUTTER_OWNER_HANDOFF.md`. Se implementaron **13 de los 14 ítems** pedidos. El único que queda pendiente **a propósito** es el **#1 (JWT)**, acordado para el final una vez la app funcione al 100%.

- **API:** `https://successful-recreation-production-7377.up.railway.app/api/v1`
- **Base de datos:** las tablas y columnas nuevas se crean solas al desplegar (migración EF `AppFeatureCompletion` se aplica en el arranque). No hay que correr scripts a mano.
- **Estado del build:** compila sin errores ni warnings.

> ⚠️ **Identidad todavía por query param.** Como el #1 (JWT) sigue pendiente, los endpoints que necesitan saber "quién soy" siguen recibiendo `ownerId` / `userId` / `payerId` por query o body, igual que antes. Cuando se implemente JWT esto se deriva del token y se elimina.

---

## 1. 🔴 JWT — PENDIENTE (a propósito)
No implementado aún, según lo acordado. `POST /auth/login` sigue devolviendo solo el usuario (sin `accessToken`). Se hará al final. Todo lo demás ya está listo para migrar a `Authorization: Bearer` sin cambiar contratos de datos.

---

## 2. 🟢 Subida real de imágenes de vehículos — LISTO

Dos formas:

**a) Subir imágenes a un vehículo (recomendado):**
```
POST /api/v1/vehicles/{id}/images     (multipart/form-data, campo "files" — 1 o varias)
→ 201 { "urls": ["https://.../uploads/vehicles/2/img_xxx.jpg", ...],
        "images": [ ...galería completa del vehículo ya actualizada... ] }
```
Las URLs quedan agregadas a la galería del vehículo. Ya **no** manden rutas locales ni `blob:`.

**b) Subida genérica reutilizable:**
```
POST /api/v1/files                    (multipart/form-data, campo "files"; form field opcional "folder")
→ 201 { "urls": ["https://.../uploads/files/202607/file_xxx.jpg", ...] }
```

- Formatos permitidos: `jpg, jpeg, png, webp, heic, pdf`. Máx **10 MB** por archivo.
- Las URLs son absolutas si el server tiene `PUBLIC_BASE_URL`; si no, relativas (`/uploads/...`) servidas por el mismo host.
- ⚠️ En Railway el disco es efímero: sobreviven mientras no haya redeploy. Para durabilidad real migrar a S3/Cloudinary (cambia solo la implementación interna, no el contrato).

---

## 3. 🔴 Documentos de propiedad del vehículo (US05) — LISTO

Ahora `documents` se **persiste y se devuelve**. Dos formas:

**a) En el JSON de `POST /vehicles` o `PUT/PATCH /vehicles/{id}`** (URLs ya subidas):
```json
"documents": {
  "propertyCardFront": "https://.../tarjeta-frente.jpg",
  "propertyCardBack":  "https://.../tarjeta-reverso.jpg",
  "soat":              "https://.../soat.jpg"
}
```

**b) Subida multipart directa:**
```
POST /api/v1/vehicles/{id}/documents  (multipart/form-data: propertyCardFront, propertyCardBack, soat)
→ 200  (devuelve el vehículo con documents + ownershipStatus)
```

`GET /vehicles/{id}` ahora incluye:
```json
"documents": { "propertyCardFront": "...", "propertyCardBack": "...", "soat": "..." },
"ownershipStatus": "pending",            // not_submitted | pending | approved | rejected
"ownershipRejectionReason": null
```
Al subir documentos el `ownershipStatus` pasa a `pending`. Un admin lo resuelve con:
```
PATCH /api/v1/vehicles/{id}/ownership-status
  { "status": "approved" }                       // o "rejected" con "rejectionReason"
→ 200  (notifica al owner)
```

---

## 4. 🔴 Carpooling: book ahora exige passengerId — LISTO (bug corregido)

Se **eliminó el flujo legacy** que descontaba asiento sin registrar al pasajero (la causa del `passengers: []` con asientos ocupados).

```
POST /api/v1/adventure-routes/{id}/book
  { "passengerId": 5, "seats": 1 }
→ 201  crea la solicitud PENDING  ✅
```
- Sin `passengerId` → **400** `passenger_id_required` (antes corrompía el aforo, ahora se rechaza).
- `seats > seatsAvailable` → **409** `no_seats_available`.
- Solicitud duplicada activa del mismo pasajero → **409** `already_requested`.
- Ruta no activa → **409** `route_not_active`.

**Modelo de cupo (sin doble conteo):**
```
book    → PENDING   (no descuenta; el asiento se descuenta al aceptar)
accept  → CONFIRMED (descuenta SeatsAvailable una sola vez)
reject / remove / cancelar ruta → libera el cupo
```
Los endpoints de gestión (`/passengers`, `/passengers/{id}/accept|reject`, `DELETE /passengers/{id}`) siguen igual que antes.

---

## 5. 🔴 Correo institucional server-side — LISTO

- **Publicar ruta de carpool:** `POST /adventure-routes` valida que el owner tenga correo `@upc.edu.pe`. Si no → **403** `not_institutional_email`.
  - Solo aplica a rutas de carpool (las que traen `seatsTotal`/`departureDate`). Las rutas de aventura clásicas no se ven afectadas.
- **Unirse a ruta de comunidad:** si la ruta tiene `community`, el pasajero también debe tener correo institucional al hacer `book` → **403** si no.
- El dominio es configurable con la variable de entorno `INSTITUTIONAL_EMAIL_DOMAIN` (por defecto `upc.edu.pe`).
- ⚠️ **onlyWomen no se valida server-side**: el modelo `User` no guarda género. Queda como filtro client-side hasta que se agregue ese campo.

---

## 6. 🔴 Checklist fotográfico de inspección (US12) — LISTO

```
POST /api/v1/rentals/{id}/inspections     (multipart/form-data)
  type: "PRE" | "POST"                     (form field)
  createdById: 4                           (opcional, form field)
  notes: "..."                             (opcional)
  + archivos por punto: campos front, rearSide, ...  (el nombre del campo = punto del checklist)
→ 201 { "id": 1, "type": "PRE",
        "photos": { "front": "https://...", "rearSide": "https://..." },
        "createdById": 4, "notes": null, "createdAt": "..." }

GET /api/v1/rentals/{id}/inspections           → lista (para disputas/incidentes)
GET /api/v1/rentals/{id}/inspections?type=PRE  → filtra por tipo
```
Cada archivo se sube usando **el nombre de su campo** como punto (`front`, `rearSide`, ...). También acepta nombres tipo `photos[front]`.

---

## 7. 🟡 Renter embebido en /rentals — LISTO (adiós N+1)

`GET /rentals`, `GET /rentals/{id}`, `GET /rentals/user/{id}`, `GET /rentals/active` ahora incluyen:
```json
"renter": {
  "id": 5,
  "fullName": "esther abigail",
  "avatarUrl": null,
  "reputation": 4.5,          // promedio de reseñas recibidas (UserReviews)
  "kycStatus": "approved"
}
```
Ya no hace falta el `GET /users/{id}` extra por cada arrendatario.

---

## 8. 🟡 Wallet y retiros — LISTO

```
GET /api/v1/wallet/{userId}
→ { "userId": 4, "balance": 350.00, "pendingWithdrawals": 50.00, "totalEarned": 900.00 }
```
`balance = totalEarned (pagos completados recibidos) − retiros (pending + completed)`.

```
POST /api/v1/withdrawals
  { "userId": 4, "amount": 100.00, "method": "yape" | "plin" | "bank", "destination": "987654321" }
→ 201 { "id": 1, "status": "pending", ... }
→ 409 insufficient_balance  (si amount > balance disponible)

GET   /api/v1/withdrawals/user/{userId}   → historial con estados
PATCH /api/v1/withdrawals/{id}            → { "status": "completed" | "rejected", "rejectionReason": "..." }  (admin)
```

---

## 9. 🟡 Reembolsos con políticas (US26/US33) — LISTO

```
POST /api/v1/payments/{id}/refund
  { "reason": "..." }                      (body opcional)
→ 200 { "refundedAmount": 58.00, "policy": "50%", "status": "refunded" }
→ 422 refund_not_allowed                   (política 0%)
→ 422 already_refunded / rental_not_found
```
Política por antelación respecto a `startDate` del alquiler:
`≥48h = 100%` · `24–48h = 50%` · `<24h = 0% (422)`.

Efectos: marca el pago original como `refunded`, crea el movimiento de reembolso (`type: "refund"`) y **notifica a ambas partes**.

---

## 10. 🟡 Comprobante server-side (US25) — LISTO

```
GET /api/v1/rentals/{id}/invoice
→ 200 {
  "invoiceNumber": "WPE-2026-000123",      // correlativo oficial y determinístico
  "issuedAt": "...", "rentalId": 12, "status": "issued",
  "customer": { "fullName": "...", "dni": "...", "email": "..." },
  "vehicle":  { "name": "Toyota Yaris", "licensePlate": "ABC-123" },
  "period":   { "start": "...", "end": "...", "days": 3 },
  "payment":  { "id": 123, "method": "yape", "currency": "PEN", "transactionId": "..." },
  "amount":   { "total": 180.00, "currency": "PEN" }
}
→ 422 no_completed_payment
```
Devuelve JSON con numeración correlativa (`WPE-{año}-{idPago}`). Ya no inventen el número en el dispositivo. PDF / facturación electrónica SUNAT queda como mejora futura si se requiere validez tributaria.

---

## 11. 🟡 Badges y puntualidad (US36) — LISTO

`GET /users/{id}` → `stats` extendido:
```json
"stats": {
  ...los de antes...,
  "reputation": 4.7,
  "onTimeRate": 0.93,
  "badges": ["VERIFIED", "PUNCTUAL", "TOP_RENTER", "FIVE_STARS"]
}
```
Todo calculado server-side:
- `reputation`: promedio de reseñas recibidas.
- `onTimeRate`: alquileres completados cerrados dentro de la fecha pactada (+2h de gracia).
- Badges: `VERIFIED` (KYC aprobado o DNI verificado) · `PUNCTUAL` (≥3 completados y onTime ≥0.9) · `TOP_RENTER` (≥10 completados) · `FIVE_STARS` (≥3 reseñas y reputación ≥4.8).

---

## 12. 🟡 KYC real (cola de revisión) — LISTO

Ya no hay auto-aprobación. Flujo `pending → approved/rejected`:
```
GET  /api/v1/auth/kyc/pending             → cola de solicitudes en revisión (admin)
POST /api/v1/auth/kyc/{userId}/review
  { "approve": true }                             // aprueba
  { "approve": false, "rejectionReason": "..." }  // rechaza (motivo obligatorio)
→ 200 { "userId": 4, "status": "approved", "rejectionReason": null }
```
`GET /users/{id}` ahora también expone `kycRejectionReason`. Cuando esto esté en producción, la app puede quitar el botón "Saltar verificación (modo prueba)".

---

## 13. 🟢 Push notifications (FCM) — registro LISTO / envío por configurar

```
POST   /api/v1/users/{userId}/devices     { "token": "fcm_token", "platform": "android" | "ios" | "web" }
→ 201  (registra o reactiva el token; token único)
GET    /api/v1/users/{userId}/devices     → dispositivos activos
DELETE /api/v1/users/{userId}/devices/{token}   → baja (logout)
```
El **registro de tokens ya funciona**. El **envío efectivo del push** por FCM queda como paso de infra: falta cargar las credenciales de Firebase (service account) en el server. Mientras tanto las notificaciones internas se siguen creando en la tabla `Notifications` (el badge/lista in-app funciona); solo falta el "empujón" al dispositivo cerrado.

---

## 14. 🟢 Transiciones de estado de rutas — LISTO

```
POST /api/v1/adventure-routes/{id}/start?ownerId=4       active/full  → in_progress
POST /api/v1/adventure-routes/{id}/complete?ownerId=4    in_progress  → completed
POST /api/v1/adventure-routes/{id}/cancel?ownerId=4      → cancelled (libera cupos + notifica pasajeros)
```
- Transición ilegal → **409** `illegal_transition` (p. ej. completar una ruta cancelada).
- `ownerId` que no es dueño → **403** `not_route_owner`.
- `cancel` notifica a todos los pasajeros activos y libera los cupos.

Nuevo estado posible en `status` de la ruta: **`in_progress`** (además de active/full/cancelled/completed).

---

## Resumen

| # | Ítem | Estado |
|---|------|--------|
| 1 | JWT | ⏸️ Pendiente (al final, acordado) |
| 2 | Subida de imágenes | ✅ |
| 3 | Documentos de propiedad + ownershipStatus | ✅ |
| 4 | Book carpool (passengerId, aforo, sin doble conteo) | ✅ |
| 5 | Correo institucional server-side | ✅ (onlyWomen limitado: falta campo género) |
| 6 | Inspecciones checklist | ✅ |
| 7 | Renter embebido en rentals | ✅ |
| 8 | Wallet y retiros | ✅ |
| 9 | Reembolsos con políticas | ✅ |
| 10 | Comprobantes correlativos | ✅ (JSON; PDF/SUNAT futuro) |
| 11 | Badges / puntualidad | ✅ |
| 12 | KYC real | ✅ |
| 13 | Push FCM | ✅ registro / ⚙️ envío requiere credenciales Firebase |
| 14 | Transiciones de ruta | ✅ |

### Notas para el deploy
- La migración `AppFeatureCompletion` se aplica sola al arrancar (crea tablas `DeviceTokens`, `RentalInspections`, `Withdrawals` y columnas nuevas en `Vehicles`/`Users`). No se pierden datos.
- Variables de entorno opcionales: `PUBLIC_BASE_URL` (URLs absolutas de archivos), `INSTITUTIONAL_EMAIL_DOMAIN` (por defecto `upc.edu.pe`).
