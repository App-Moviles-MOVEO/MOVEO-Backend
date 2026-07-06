# Backend MOVEO / WheelsPe — Entrega completa para las apps

Documento único con **todo lo implementado en el backend** para las apps Renter y Owner.
Consolida las dos iteraciones de trabajo. Ambas apps pueden dejar de resolver estos flujos
solo en el cliente y pasarlos a server-side compartido entre dispositivos.

- **API:** `https://successful-recreation-production-7377.up.railway.app/api/v1`
- **Deploy:** las migraciones EF se aplican solas al arrancar (crean tablas y columnas nuevas; no se pierden datos).
- **Build:** compila sin errores ni warnings.
- **Único pendiente a propósito:** **JWT** (identidad por token), acordado para el cierre. Hoy la
  identidad sigue viajando por `userId` / `ownerId` / `payerId` en query o body.

> Variables de entorno opcionales: `PUBLIC_BASE_URL` (URLs absolutas de archivos subidos),
> `INSTITUTIONAL_EMAIL_DOMAIN` (por defecto `upc.edu.pe`).

---

# PARTE A — Integración base de las apps

## A1 · Subida real de imágenes de vehículos
```
POST /api/v1/vehicles/{id}/images     (multipart/form-data, campo "files" — 1 o varias)
→ 201 { "urls": ["https://.../uploads/vehicles/2/img_xxx.jpg", ...], "images": [ ...galería actualizada... ] }

POST /api/v1/files                    (multipart/form-data, campo "files"; form field opcional "folder")
→ 201 { "urls": ["https://.../uploads/files/202607/file_xxx.jpg", ...] }
```
- Formatos: `jpg, jpeg, png, webp, heic, pdf`. Máx **10 MB** por archivo.
- Ya **no** manden rutas locales ni `blob:`.
- ⚠️ En Railway el disco es efímero (sobreviven hasta el siguiente redeploy). Para durabilidad real, migrar a S3/Cloudinary (cambia solo la implementación, no el contrato).

## A2 · Documentos de propiedad del vehículo (US05)
```
// En POST /vehicles o PUT/PATCH /vehicles/{id} (URLs ya subidas):
"documents": { "propertyCardFront": "...", "propertyCardBack": "...", "soat": "..." }

// O subida multipart directa:
POST /api/v1/vehicles/{id}/documents  (multipart: propertyCardFront, propertyCardBack, soat)  → 200
```
`GET /vehicles/{id}` ahora incluye `documents`, `ownershipStatus` (`not_submitted|pending|approved|rejected`) y `ownershipRejectionReason`. Al subir documentos pasa a `pending`. Un admin resuelve:
```
PATCH /api/v1/vehicles/{id}/ownership-status   { "status":"approved" }  // o "rejected" + "rejectionReason"  (notifica al owner)
```

## A3 · Carpooling: book exige passengerId (bug corregido)
Se **eliminó el flujo legacy** que descontaba asiento sin registrar al pasajero.
```
POST /api/v1/adventure-routes/{id}/book   { "passengerId":5, "seats":1 }   → 201 (solicitud PENDING)
```
- Sin `passengerId` → **400** `passenger_id_required`.
- `seats > seatsAvailable` → **409** `no_seats_available`.
- Solicitud duplicada → **409** `already_requested`. · Ruta no activa → **409** `route_not_active`.

**Modelo de cupo (sin doble conteo):** `book → PENDING` (no descuenta) · `accept → CONFIRMED` (descuenta 1 vez) · `reject/remove/cancelar ruta → libera`.

## A4 · Correo institucional server-side (US14)
- `POST /adventure-routes` (rutas de carpool con asientos): owner sin `@upc.edu.pe` → **403** `not_institutional_email`.
- `book` en ruta con `community`: pasajero sin correo institucional → **403**.
- Dominio configurable con `INSTITUTIONAL_EMAIL_DOMAIN`.

## A5 · Checklist fotográfico de inspección (US12)
```
POST /api/v1/rentals/{id}/inspections   (multipart)
  type: "PRE" | "POST"   createdById (opc)   notes (opc)
  + archivos por punto: campos front, rearSide, ...  (nombre del campo = punto del checklist)
→ 201 { "id":1, "type":"PRE", "photos": { "front":"https://...", ... }, "createdAt":"..." }

GET /api/v1/rentals/{id}/inspections[?type=PRE]   → lista (para disputas/incidentes)
```

## A6 · Renter embebido en /rentals (adiós N+1)
`GET /rentals`, `/rentals/{id}`, `/rentals/user/{id}`, `/rentals/active` incluyen:
```json
"renter": { "id":5, "fullName":"esther abigail", "avatarUrl":null, "reputation":4.5, "kycStatus":"approved" }
```

## A7 · Wallet y retiros (US32)
```
GET  /api/v1/wallet/{userId}          → { "balance":350.00, "pendingWithdrawals":50.00, "totalEarned":900.00 }
POST /api/v1/withdrawals              { "userId":4, "amount":100.00, "method":"yape|plin|bank", "destination":"987..." }
      → 201 pending · 409 insufficient_balance
GET  /api/v1/withdrawals/user/{userId}   → historial
PATCH /api/v1/withdrawals/{id}           { "status":"completed|rejected", "rejectionReason":"..." }  (admin)
```
`balance = totalEarned (pagos completados recibidos) − retiros (pending + completed)`.

## A8 · Reembolsos con políticas (US26/US33)
```
POST /api/v1/payments/{id}/refund   { "reason":"..." }
→ 200 { "refundedAmount":58.00, "policy":"50%", "status":"refunded" }
→ 422 refund_not_allowed | already_refunded | rental_not_found
```
Política por antelación al `startDate`: `≥48h=100%` · `24–48h=50%` · `<24h=0% (422)`. Marca el pago `refunded`, crea el movimiento `type:"refund"` y **notifica a ambas partes**.

## A9 · Comprobante server-side (US25)
```
GET /api/v1/rentals/{id}/invoice
→ 200 { "invoiceNumber":"WPE-2026-000123", "customer":{...}, "vehicle":{...}, "period":{...}, "payment":{...}, "amount":{...} }
→ 422 no_completed_payment
```
Numeración correlativa `WPE-{año}-{idPago}`. PDF/SUNAT queda como mejora futura.

## A10 · Badges y puntualidad (US36)
`GET /users/{id}` → `stats` extendido:
```json
"stats": { ..., "reputation":4.7, "onTimeRate":0.93, "badges":["VERIFIED","PUNCTUAL","TOP_RENTER","FIVE_STARS"] }
```
Calculado server-side. Badges: `VERIFIED` (KYC aprobado/DNI verificado) · `PUNCTUAL` (≥3 completados y onTime≥0.9) · `TOP_RENTER` (≥10 completados) · `FIVE_STARS` (≥3 reseñas y reputación≥4.8).

## A11 · KYC real (cola de revisión, US02)
```
GET  /api/v1/auth/kyc/pending            → cola de solicitudes en revisión (admin)
POST /api/v1/auth/kyc/{userId}/review    { "approve":true }  |  { "approve":false, "rejectionReason":"..." }
```
`GET /users/{id}` expone `kycRejectionReason`. Fin del modo demo.

## A12 · Push notifications (FCM) — registro listo / envío por configurar
```
POST   /api/v1/users/{userId}/devices     { "token":"fcm_token", "platform":"android|ios|web" }
GET    /api/v1/users/{userId}/devices
DELETE /api/v1/users/{userId}/devices/{token}
```
El registro funciona. El **envío efectivo** por FCM requiere cargar credenciales de Firebase en el server; mientras tanto la notificación in-app (tabla `Notifications`) ya funciona.

## A13 · Transiciones de estado de rutas
```
POST /api/v1/adventure-routes/{id}/start?ownerId=4      active/full → in_progress
POST /api/v1/adventure-routes/{id}/complete?ownerId=4   in_progress → completed
POST /api/v1/adventure-routes/{id}/cancel?ownerId=4     → cancelled (libera cupos + notifica pasajeros)
```
Transición ilegal → **409** `illegal_transition`. `ownerId` no dueño → **403** `not_route_owner`.

---

# PARTE B — Historias pendientes de backend (estado 2026-07-06)

## US11 · Filtro por género (validación server-side)
Campo **`gender`** en User (`male|female|other|unspecified`). Se envía en `POST /auth/register` y se
devuelve en `login`, `auth/me` y `GET /users/{id}`. Al reservar una ruta `onlyWomen:true`, si el
pasajero no es `female` → **403** `women_only_route`.

## US21 · Métodos de pago y de cobro
Misma entidad, dos usos por `category`: **payment** (renter) y **payout** (owner). Solo se guarda un enmascarado.
```
GET/POST/DELETE  /api/v1/users/{userId}/payment-methods[/{id}]
GET/POST/DELETE  /api/v1/users/{userId}/payout-methods[/{id}]
POST body: { "type":"card|yape|plin|bank", "label":"Visa ••4242", "maskedNumber":"4242", "holder":"...", "isDefault":true }
```
El primero de cada categoría queda como default; marcar otro lo reemplaza.

## US27 / US29 / US34 · Promociones y cupones
```
GET    /api/v1/promotions?ownerId=&onlyActive=true          GET /api/v1/promotions/{id}
POST   /api/v1/promotions   { "code":"MOVEO10", "discountType":"percent|fixed", "discountValue":10,
                              "startsAt":"...", "endsAt":"...", "minReputation":4.5, "maxUses":100, "ownerId":4 }
PATCH  /api/v1/promotions/{id}    DELETE /api/v1/promotions/{id}
POST   /api/v1/promotions/validate  { "code":"MOVEO10", "amount":120.00, "userReputation":4.6 }
       → { "valid":true, "discount":12.00, "finalAmount":108.00 }
       → { "valid":false, "reason":"expired|scheduled|inactive|reputation_below_minimum|not_found" }
POST   /api/v1/promotions/{id}/redeem   (consume un uso al confirmar el pago)
```
`status` calculado (`scheduled|active|expired|inactive`). Código único. `minReputation` cubre US29.

## US10 · Contactos de confianza
```
GET/POST/DELETE  /api/v1/users/{userId}/trusted-contacts[/{id}]
POST body: { "name":"Mamá", "phone":"987654321", "relationship":"familiar" }
```

## US06 / US07 · Tracking GPS en tiempo real
```
POST /api/v1/rentals/{id}/location   { "lat":-12.09, "lng":-77.03, "heading":45, "speed":32, "progress":0.4, "etaText":"12 min" }
GET  /api/v1/rentals/{id}/location    → última posición  ·  404 no_location si aún no hay ping
```
El conductor publica, el pasajero consulta. Reemplaza la fuente simulada.

## US23 · Cobro de la cuota de asiento (carpool)
Al **aceptar** un pasajero, si la ruta tiene `pricePerSeat > 0`, se crea un pago pendiente:
`Payment { payerId:pasajero, recipientId:owner, type:"carpool_seat", amount:pricePerSeat×seats, status:"pending" }`.
Se completa con `PATCH /payments/{id}`. Falta solo la decisión de producto (prepago vs. al aceptar).

## US46 · Alianzas corporativas (entidad propia)
```
POST  /api/v1/partnerships   { "userId":4, "companyName":"...", "ruc":"20123456789", "contactName":"...",
                               "contactEmail":"...", "contactPhone":"...", "fleetSize":8 }
      → auto-evalúa: approved si RUC de 11 dígitos y flota ≥5; si no, pending
GET   /api/v1/partnerships?userId=&status=      GET /api/v1/partnerships/{id}
PATCH /api/v1/partnerships/{id}   { "status":"approved|rejected", "reviewNote":"..." }
```

## US40 · Anomalías financieras (server-side)
```
GET /api/v1/payments/anomalies?userId=4
→ { "scanned":25, "flagged":2, "anomalies":[
     { "type":"amount_outlier", "paymentId":12, "detail":"...", "severity":"high" },
     { "type":"duplicate_charge", "paymentIds":[8,9], "detail":"...", "severity":"medium" },
     { "type":"excess_refunds", "detail":"...", "severity":"medium" } ] }
```
Montos atípicos (media+3σ), duplicados (mismo pagador/monto/día), exceso de reembolsos (>30%).

## US41 · Disputas de reputación (con exclusión real)
`GET /user-reviews` devuelve `id`, `status` (`active|disputed|excluded`) y `disputeReason`.
```
POST /api/v1/user-reviews/{id}/dispute   { "reason":"..." }
→ { "status":"excluded", "outcome":"excluded", "adjustedReputation":4.8 }
```
Mediación automática: excluye si voto bajo (≤2) + atípico (≥1.5 bajo el promedio) + sin justificación
(<10 caracteres); si no, queda `disputed`. **La reputación agregada ya excluye las `excluded`** en todos
los cálculos (`renter.reputation`, `stats.reputation`, reputación de pasajeros).

## US17 · Rutas recurrentes semanales
```
POST /api/v1/adventure-routes/recurring
{ "route": { ...cuerpo de POST /adventure-routes... }, "weekdays":[1,3,5], "weeks":4 }   // 1=Lun..7=Dom
→ 201 { "recurrenceGroupId":"ab12...", "count":12, "routes":[ ... ] }
```
Una ruta por ocurrencia con `recurrenceGroupId` compartido (expuesto en `GET /adventure-routes/{id}`).
Para cancelar la serie: listar por grupo y llamar `/cancel` en cada una.

## US38 · Filtro por umbral de confianza
```
GET /api/v1/adventure-routes/{id}/passengers?ownerId=4&minReputation=4.0
```

---

# Resumen general

| # / US | Ítem | Estado |
|--------|------|--------|
| — | JWT (identidad por token) | ⏸️ Pendiente a propósito (cierre) |
| A1 | Subida de imágenes | ✅ |
| A2 · US05 | Documentos de propiedad + ownershipStatus | ✅ |
| A3 | Book carpool (passengerId, aforo, sin doble conteo) | ✅ |
| A4 · US14 | Correo institucional server-side | ✅ |
| A5 · US12 | Inspecciones checklist | ✅ |
| A6 | Renter embebido en rentals | ✅ |
| A7 · US32 | Wallet y retiros | ✅ |
| A8 · US26/33 | Reembolsos con políticas | ✅ |
| A9 · US25 | Comprobantes correlativos | ✅ (JSON; PDF/SUNAT futuro) |
| A10 · US36 | Badges / puntualidad | ✅ |
| A11 · US02 | KYC real (cola de revisión) | ✅ |
| A12 | Push FCM | ✅ registro / ⚙️ envío requiere credenciales Firebase |
| A13 | Transiciones de ruta | ✅ |
| US11 | Género + validación "solo mujeres" | ✅ |
| US21 | Métodos de pago/cobro | ✅ |
| US27/29/34 | Promociones | ✅ |
| US10 | Contactos de confianza | ✅ |
| US06/07 | Tracking GPS | ✅ |
| US23 | Cuota de asiento carpool | ✅ (hook listo; falta decisión de producto) |
| US46 | Alianzas corporativas | ✅ |
| US40 | Anomalías financieras | ✅ |
| US41 | Disputas de reseñas | ✅ |
| US17 | Rutas recurrentes | ✅ |
| US38 | Filtro por reputación | ✅ |
| SP03/SP04 | KYC por IA · microservicios | ❌ investigación/infra, fuera de alcance |

### Tablas / columnas creadas por las migraciones (auto-aplicadas)
- **Tablas:** `DeviceTokens`, `RentalInspections`, `Withdrawals`, `PaymentMethods`, `Promotions`,
  `Partnerships`, `TrustedContacts`, `TripLocations`.
- **Columnas:** `Vehicles` (documentos + ownershipStatus), `Users` (KYC review, `Gender`),
  `AdventureRoutes` (`RecurrenceGroupId`, estado `in_progress`), `UserReviews` (`Status`, `DisputeReason`, `DisputedAt`).
