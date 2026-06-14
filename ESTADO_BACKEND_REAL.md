# MOVEO / WheelsPe — Estado REAL del Backend (lo que de verdad existe en el código)

> Escrito por el backend mismo, leyendo el código actual (no el backlog). Fecha: 2026-06-13.
> **Propósito:** que los equipos móviles (Cliente Kotlin, y la futura app de Proveedor / Flutter)
> sepan exactamente qué endpoints existen HOY, qué sirve para cliente y qué para proveedor, y qué
> del reporte/backlog **no coincide** con la realidad. Hay **un solo backend** para las dos apps.

---

## 0. Lo más importante (léelo primero)

1. **NO hay JWT. El backend es sin token (stateless por `userId`).**
   `POST /api/v1/auth/login` devuelve el **objeto usuario** (con `id` y `role`). La "sesión" es
   guardar ese `id` y mandarlo como `?userId=` / `renterId` / `ownerId` / `payerId` según el endpoint.
   **No existe header `Authorization: Bearer`.** (El backlog dice "Auth JWT/Bearer" — eso es falso hoy.)

2. **Los nombres de endpoints del backlog NO son los reales.** El backend usa:
   `/rentals` (no `/reservations`), `/adventure-routes` (no `/routes`), `/rentals/{id}/pay` + `/payments`
   (no `/transactions`), `/reviews` + `/user-reviews` (no `/users/{id}/ratings`).

3. **Un solo backend sirve a las dos apps.** No hay "API de cliente" y "API de proveedor" separadas.
   El mismo endpoint sirve a ambos roles; lo que cambia es **qué id mandas** (`renterId` vs `ownerId`)
   y el `role` del usuario. Ej: `GET /rentals?renterId=1` (mis reservas como cliente) vs
   `GET /rentals?ownerId=3` (reservas de mis autos como proveedor).

4. **Hay módulos del backlog que NO EXISTEN como endpoint** (ver §5): KYC, incidents/pánico,
   invoices, escrow, métodos de pago tokenizados, contactos de confianza, cupones, GPS/trips,
   recuperar contraseña. **No marcar como "hecho en backend".**

5. **Base URL:** `http://<host>:8080/api/v1` · **Formato:** JSON · **Swagger:** `/swagger/index.html`.

---

## 1. Inventario REAL de endpoints (lo que el código expone hoy)

Solo existen **11 controllers**. Todo bajo `/api/v1`.

### 1.1 IAM / Auth — `/auth`
| Método | Ruta | Para qué | Cliente | Proveedor |
|---|---|---|:--:|:--:|
| POST | `/auth/register` | Registro (devuelve el usuario, **no** JWT) | ✅ | ✅ |
| POST | `/auth/login` | Login (devuelve el usuario, **no** JWT) | ✅ | ✅ |
| POST | `/auth/logout` | Logout simbólico (solo mensaje) | ✅ | ✅ |
| GET | `/auth/me?userId=` | Datos del usuario logueado | ✅ | ✅ |
| POST | `/auth/change-password` | Cambiar contraseña (requiere la actual) | ✅ | ✅ |

> El `register` recibe `role` (`renter` = cliente, `owner` = proveedor). El mismo endpoint crea ambos.

### 1.2 Usuarios — `/users`
| Método | Ruta | Para qué | Cliente | Proveedor |
|---|---|---|:--:|:--:|
| GET | `/users` / `/users?email=` | Listar / buscar por email | ✅ | ✅ |
| GET | `/users/{id}` | Perfil + reputación (stats, ratings) | ✅ | ✅ |
| POST | `/users` | Crear usuario | ✅ | ✅ |
| PUT/PATCH | `/users/{id}` | Editar perfil/preferencias | ✅ | ✅ |
| DELETE | `/users/{id}` | Eliminar usuario (hard delete) | ✅ | ✅ |

### 1.3 Vehículos — `/vehicles`  *(Rental)*
| Método | Ruta | Para qué | Cliente | Proveedor |
|---|---|---|:--:|:--:|
| GET | `/vehicles` | Catálogo + filtros (ver abajo) | ✅ | ✅ (`?ownerId=`) |
| GET | `/vehicles/{id}` | Detalle (incluye `ownerName`, `rating`, `reviewsCount`) | ✅ | ✅ |
| GET | `/vehicles/{id}/availability` | **Fechas ocupadas (calendario)** ⭐ nuevo | ✅ | ✅ |
| POST | `/vehicles` | **Publicar vehículo** | — | ✅ |
| PUT/PATCH | `/vehicles/{id}` | Editar / cambiar estado | — | ✅ |
| DELETE | `/vehicles/{id}` | Eliminar publicación | — | ✅ |

Filtros en `GET /vehicles` (todos opcionales, combinables): `ownerId`, `status`, `minPrice`,
`maxPrice`, `district`, `bodyType`, `transmission`, `fuelType`, `startDate`+`endDate`
(solo libres en el rango), `lat`+`lng`+`sort=distance`, `page`+`pageSize`.

### 1.4 Reservas / Alquileres — `/rentals`  *(Rental)*
| Método | Ruta | Para qué | Cliente | Proveedor |
|---|---|---|:--:|:--:|
| GET | `/rentals?renterId=` | Mis reservas como cliente | ✅ | — |
| GET | `/rentals?ownerId=` | Reservas de mis autos | — | ✅ |
| GET | `/rentals?vehicleId=` / `?status=` | Filtros adicionales | ✅ | ✅ |
| GET | `/rentals/{id}` | Detalle de reserva | ✅ | ✅ |
| GET | `/rentals/user/{userId}` | Todas las del usuario (como renter u owner) | ✅ | ✅ |
| GET | `/rentals/active` | Reservas activas | ✅ | ✅ |
| POST | `/rentals` | **Crear reserva** (valida solapamiento → 409) ⭐ | ✅ | — |
| PUT | `/rentals/{id}` | Actualizar reserva | ✅ | ✅ |
| PATCH | `/rentals/{id}` | Cambiar estado: `accepted`/`active`/`completed`/`cancelled` | ✅ (cancelar) | ✅ (aceptar/activar/completar) |
| POST | `/rentals/{id}/pay` | **Pagar reserva en un paso (Yape)** | ✅ | — |
| DELETE | `/rentals/{id}` | Eliminar reserva | ✅ | ✅ |

> Estados de reserva: `pending` → `accepted` → `active` → `completed`, o `cancelled`.
> El **proveedor** acepta/activa/completa vía `PATCH status`. El **cliente** crea, paga y cancela.

### 1.5 Carpooling / Rutas — `/adventure-routes`  *(Adventure)*
> Una sola entidad sirve para **rutas de aventura** y para **viajes compartidos (carpool)**.
> Trae campos de carpool: `departureDate`, `departureTime`, `seatsTotal`, `seatsAvailable`,
> `pricePerSeat`, `onlyWomen`, `community`, `lat`, `lng`, `status`.

| Método | Ruta | Para qué | Cliente (pasajero) | Proveedor (conductor) |
|---|---|---|:--:|:--:|
| GET | `/adventure-routes` | Buscar rutas (filtros abajo) | ✅ | ✅ (`?ownerId=`) |
| GET | `/adventure-routes/{id}` | Detalle de ruta | ✅ | ✅ |
| POST | `/adventure-routes` | **Publicar ruta/viaje** | — | ✅ |
| POST | `/adventure-routes/{id}/book` | **Reservar asiento(s)** (descuenta cupos) | ✅ | — |
| PUT | `/adventure-routes/{id}` | Editar ruta | — | ✅ |
| DELETE | `/adventure-routes/{id}` | Eliminar ruta | — | ✅ |

Filtros: `ownerId`, `type`, `difficulty`, `featured`, `onlyWomen`.
> **Filtro por comunidad/dominio institucional (US14):** el campo `community` existe en el modelo,
> pero **el filtro por query param aún NO está implementado** en `GET`. (Reporte correcto: 🟡 parcial.)

### 1.6 Pagos — `/payments`  *(Payment)*
| Método | Ruta | Para qué | Cliente | Proveedor |
|---|---|---|:--:|:--:|
| GET | `/payments?payerId=` | Mis pagos | ✅ | — |
| GET | `/payments?recipientId=` | **Lo que me han pagado (ingresos)** | — | ✅ |
| GET | `/payments?rentalId=` / `?status=` / `?type=` | Filtros | ✅ | ✅ |
| GET | `/payments/{id}` · `/payments/payer/{id}` · `/payments/recipient/{id}` · `/payments/rental/{id}` | Consultas | ✅ | ✅ |
| POST | `/payments` | Crear pago | ✅ | ✅ |
| PUT/PATCH | `/payments/{id}` | Actualizar / completar / reembolsar (`status`) | ✅ | ✅ |
| DELETE | `/payments/{id}` | Eliminar | ✅ | ✅ |

> **Importante:** NO hay pasarela real (Stripe/Yape/Plin). Es registro de pagos en BD.
> `POST /rentals/{id}/pay` marca el pago como `completed` directamente. SP01 sigue pendiente.

### 1.7 Reseñas de vehículo — `/Reviews`  *(Rental)*
| Método | Ruta | Para qué |
|---|---|---|
| GET | `/Reviews?vehicleId=` / `?rentalId=` / `?reviewerId=` / `?revieweeId=` | Listar reseñas |
| GET | `/Reviews/{id}` · `/Reviews/rental/{id}` · `/Reviews/reviewer/{id}` · `/Reviews/reviewee/{id}` | Consultas |
| POST/PUT/DELETE | `/Reviews[/{id}]` | Crear/editar/borrar reseña de un vehículo |

### 1.8 Reseñas entre usuarios — `/user-reviews`  *(UserReview)*
| Método | Ruta | Para qué |
|---|---|---|
| GET | `/user-reviews?reviewedUserId=` / `?reviewerId=` / `?rentalId=` / `?type=` | Reputación de usuarios |
| GET/POST/PUT/DELETE | `/user-reviews[/{id}]` | Calificación mutua (`type`: `owner_to_renter` / `renter_to_owner`) |

> **Esto cubre la evaluación bidireccional cliente↔proveedor (US28/US35/US19/US37).**

### 1.9 Notificaciones — `/Notifications`
| Método | Ruta | Para qué |
|---|---|---|
| GET | `/Notifications?userId=` / `?userId=&read=false` | Bandeja / no leídas |
| GET | `/Notifications/user/{id}` · `/Notifications/user/{id}/unread` | Por usuario |
| POST | `/Notifications` | Crear notificación |
| PATCH | `/Notifications/{id}` (`{"read":true}`) · PUT `/{id}/read` · PUT `/user/{id}/read-all` | Marcar leídas |
| DELETE | `/Notifications/{id}` | Eliminar |

### 1.10 Chat 1‑a‑1 — `/messages`  *(Chat)*
| Método | Ruta | Para qué |
|---|---|---|
| GET | `/messages?userId=&otherUserId=` | Conversación entre dos usuarios |
| GET | `/messages/conversations/{userId}` | Lista de conversaciones (último msg + no leídos) |
| POST | `/messages` | Enviar mensaje |
| PUT | `/messages/{id}/read` · `/messages/read?userId=&otherUserId=` | Marcar leído(s) |

> **El chat YA existe y funciona (US18).** El backlog lo marcaba pendiente — incorrecto.

### 1.11 Soporte / Ayuda — `/support-tickets`  *(Support)*
| Método | Ruta | Para qué |
|---|---|---|
| GET | `/support-tickets?userId=` / `?status=` / `?type=` | Listar tickets |
| GET | `/support-tickets/{id}` · `/user/{id}` · `/status/{status}` | Consultas |
| POST | `/support-tickets` | Crear ticket |
| PUT/PATCH | `/support-tickets/{id}` · PATCH `/{id}/close` | Actualizar / cerrar |
| GET/POST | `/support-tickets/{ticketId}/messages` | Hilo de mensajes del ticket |

---

## 2. ⭐ Lo que YO (backend) avancé recientemente — Disponibilidad por fechas

Implementado y probado contra la BD (ver `BACKEND_REQUESTS.md` original P1–P6):

- **P1 — Validación de solapamiento al crear reserva.** `POST /rentals` ahora rechaza dobles
  reservas con **409 `vehicle_not_available`** (+ `conflictingRanges`). También valida:
  `endDate<=startDate` → 400, fecha pasada → 400, `renterId==ownerId` → 400, vehículo inexistente
  → 404, vehículo no `active` → 409 `vehicle_not_active`.
- **P2 — `GET /vehicles/{id}/availability`** → devuelve `busyRanges` (para bloquear días en el calendario).
- **P3 — Catálogo por fechas:** `GET /vehicles?startDate=&endDate=` filtra solo autos libres.
- **P4 — Filtros `transmission` y `fuelType`** en `GET /vehicles`.
- **P5 — Expiración automática** de reservas `pending` sin pagar tras 30 min (libera fechas).
- **P6 — Orden por cercanía** (`lat`/`lng`/`sort=distance`), **paginación** (`page`/`pageSize`),
  y cancelar (`PATCH status=cancelled`) **libera las fechas**.

Convención: todo en **UTC**, rangos `[start, end)` (fin exclusivo), estados que bloquean
`pending/accepted/active`, que liberan `cancelled/completed`.

> Esto significa que la app **ya no necesita** validar solapamiento por su cuenta leyendo
> `GET /rentals?vehicleId=` — puede usar `/vehicles/{id}/availability` y confiar en el 409.

---

## 3. Qué cubre el backend por ROL (resumen Cliente vs Proveedor)

| Capacidad | Endpoint(s) | Cliente | Proveedor |
|---|---|:--:|:--:|
| Registro / login / perfil | `/auth/*`, `/users/*` | ✅ | ✅ |
| Ver catálogo y detalle de autos | `/vehicles`, `/vehicles/{id}` | ✅ | ✅ |
| Ver disponibilidad por fechas | `/vehicles/{id}/availability` | ✅ | ✅ |
| **Publicar / gestionar autos** | `POST/PUT/PATCH/DELETE /vehicles` | — | ✅ |
| Crear reserva + pagar | `POST /rentals`, `/rentals/{id}/pay` | ✅ | — |
| **Aceptar/activar/completar reservas** | `PATCH /rentals/{id}` | — | ✅ |
| Cancelar reserva | `PATCH /rentals/{id}` (`cancelled`) | ✅ | ✅ |
| Ver mis reservas / ventas | `/rentals?renterId=` / `?ownerId=` | ✅ | ✅ |
| Buscar carpool / reservar asiento | `/adventure-routes`, `/{id}/book` | ✅ | — |
| **Publicar ruta de carpool** | `POST /adventure-routes` | — | ✅ |
| Pagos / ingresos | `/payments?payerId=` / `?recipientId=` | ✅ | ✅ |
| Reputación (mutua) | `/user-reviews`, `/Reviews` | ✅ | ✅ |
| Chat | `/messages` | ✅ | ✅ |
| Notificaciones | `/Notifications` | ✅ | ✅ |
| Soporte / ayuda | `/support-tickets` | ✅ | ✅ |

> **Conclusión de rol:** el backend ya da soporte completo al **Proveedor** (publicar autos/rutas,
> aceptar reservas, ver ingresos, reputación). La app de proveedor puede construirse YA sobre estos
> endpoints — no necesita backend nuevo para su núcleo.

---

## 3.5 User Stories — estado REAL en el backend

Estado **desde el punto de vista del backend** (qué endpoint lo soporta). Ojo: difiere de lo que
marca el backlog en varias US (KYC, incidents, invoices figuran como ✅/🟡 pero **no existen**).

Leyenda: ✅ soportado por endpoint real · 🟡 parcial (existe base, falta algo) · ⬜ sin endpoint · — no aplica al backend (landing/web)

### ✅ Hechas en el backend
| US | Título | Endpoint real que la soporta |
|---|---|---|
| US01 | Registro de cuenta con rol único | `POST /auth/register` |
| US03 | Iniciar sesión + sesión | `POST /auth/login` (sin JWT, sesión = `userId`) |
| US22 | Consultar catálogo + detalle de vehículos | `GET /vehicles`, `GET /vehicles/{id}` |
| — | **Disponibilidad por fechas + anti‑solapamiento** ⭐ | `GET /vehicles/{id}/availability`, `POST /rentals` (409) |
| US13 | Publicar ruta / viaje de carpool | `POST /adventure-routes` |
| US15 | Reservar asiento en ruta compartida | `POST /adventure-routes/{id}/book` |
| US11 | Filtro "Solo Mujeres" (en rutas) | `GET /adventure-routes?onlyWomen=true` |
| US28 / US35 | Evaluación bidireccional (calificar) | `POST /user-reviews`, `POST /Reviews` |
| US19 / US37 | Consultar reputación / historial de reseñas | `GET /user-reviews?reviewedUserId=`, `GET /users/{id}`, `GET /Reviews?revieweeId=` |
| US18 | Mensajería / chat | `/messages` (1‑a‑1, conversaciones, leídos) |
| — | Notificaciones | `/Notifications` |
| — | Mis reservas / ventas | `GET /rentals?renterId=` / `?ownerId=` |
| — | Soporte / tickets de ayuda | `/support-tickets` |
| — | Crear/gestionar/pagar reserva (flujo completo) | `POST /rentals`, `PATCH /rentals/{id}`, `POST /rentals/{id}/pay` |

### 🟡 Parciales en el backend
| US | Título | Qué hay / qué falta |
|---|---|---|
| US05 | Acreditar propiedad de vehículo | Hay `POST /vehicles`, pero **falta validación de documentos/SOAT** |
| US14 | Buscar rutas con segmentación institucional | El campo `community` existe; **falta el filtro por query param** en `GET /adventure-routes` |
| US16 | Aprobar pasajeros / aforo | `book` descuenta cupos y marca `full`, pero **no hay lista de pasajeros ni aprobar/rechazar** |
| US20 | Confirmar llegada al destino | Se puede pasar la ruta a `completed` (PUT), pero **sin geolocalización ni confirmación formal** |
| US31 | Pago de alquiler multicanal | `POST /rentals/{id}/pay` + `/payments` registran el pago, **sin pasarela real (SP01)** |
| US32 | Liquidar cuota de carpooling | Cubierto por `/payments` genérico, **sin pasarela real ni endpoint específico de cuota** |
| US26 / US33 | Reembolsos | `PATCH /payments/{id}` con `status="refunded"` existe; **no hay flujo/política automática** |
| US45 | Baja voluntaria / eliminación de datos | `DELETE /users/{id}` existe (hard delete), **no es borrado GDPR (soft + purge)** |

### ⬜ Sin endpoint en el backend (NO marcar como hechas)
| US | Título | Nota |
|---|---|---|
| US02 | Verificación KYC | **No hay endpoint KYC.** Solo flags `DniVerified`/`LicenseVerified` en el modelo `User` |
| US04 | Recuperar contraseña | Solo existe `change-password` (con la clave actual); **no hay forgot/reset** |
| US06 | Monitoreo GPS en tiempo real | Sin `/trips`/ubicación en vivo |
| US07 | Rastreo de viaje / desvíos | Sin GPS ni incidents |
| US08 | Botón de pánico / emergencia | **No hay `/incidents`** |
| US09 | PIN de inicio de viaje | — |
| US10 | Contactos de confianza | — |
| US12 | Checklist fotográfico | — |
| US17 | Rutas recurrentes | — |
| US21 | Vincular métodos de pago (tokenizados) | — |
| US24 | Escrow / retención de garantía | Sin hold/release |
| US25 | Comprobantes / contratos (PDF) | **No hay `/invoices`** |
| US27 | Cupones y beneficios | — |
| US29 / US36 | Badges / incentivos por reputación | — |
| US30 / US38 | Umbrales de reputación | — |
| US34 | Ofertas promocionales temporales | — |
| US40 | Monitorear anomalías financieras (Admin) | **No hay `/incidents`** |
| US41 | Mediar disputas de reputación (Admin) | **No hay `/incidents`** |
| SP01 | Spike pasarelas de pago | Pendiente (no integrado) |
| SP02 | Spike GPS / mapas | Pendiente |
| SP03 | Spike KYC con IA | Pendiente |
| SP04 | Spike microservicios / contenedores | Backend es **monolito** hoy |

### — No aplican al backend (landing / web)
US39, US42, US43, US44 (Landing), US46 (alianza corporativa — formulario web).

> **Diferencias clave con el backlog:** US02 (KYC), US25 (invoices), US08/US40/US41 (incidents)
> aparecen como ✅/🟡 en el backlog/reporte pero en el código son **⬜ (no existen)**. En cambio,
> **US18 (chat) y notificaciones** ya están ✅ aunque el backlog los daba pendientes, y la
> **disponibilidad por fechas** (que la app cubría por su cuenta) ya está en el backend.

---

## 4. El modelo `User` ya guarda datos que no tienen endpoint propio

El `User` tiene campos que **existen en BD** pero **no se exponen con un endpoint dedicado**
(solo se pueden setear/leer vía crear/editar usuario o se quedan internos):

- **Verificación:** `EmailVerified`, `PhoneVerified`, `DniVerified`, `LicenseVerified`.
  → Hay flags, pero **NO hay flujo KYC** (no existe `/kyc/upload`, `/status`, `/verify`, `/reject`).
- **Datos bancarios (para pagos al proveedor):** `BankName`, `BankAccountType`, `BankAccountNumber`,
  `BankAccountVerified`.
- **Estadísticas de reputación/actividad:** `TotalRentals`, `TotalSpent`, `TotalEarned`,
  `ActiveRentals`, `CompletedRentals`, `CanceledRentals`, `Avatar`.

> Si la app de proveedor necesita mostrar ingresos/cuenta bancaria, los campos **ya existen** en el
> usuario; solo faltaría exponer endpoints específicos si se quieren editar por separado.

---

## 5. Lo que el backlog/reporte marca como "backend ✅" pero **NO EXISTE** en el código

Verificado por búsqueda en todo el repo (sin resultados):

| Backlog dice | Realidad en este backend |
|---|---|
| KYC: `/auth/kyc/upload`, `/status`, `/users/{id}/kyc/verify·reject` (US02) | ❌ **No existe ningún endpoint KYC.** Solo flags en el modelo `User`. |
| `incidents-controller` (US07/US08/US40/US41 — pánico, anomalías, disputas) | ❌ **No existe.** No hay `/incidents`. |
| `/invoices` (US25 comprobantes) | ❌ **No existe.** |
| Escrow/retención (US24): `/transactions/{id}/hold·release` | ❌ **No existe.** |
| Métodos de pago tokenizados (US21): `/users/{id}/payment-methods` | ❌ **No existe.** |
| Contactos de confianza (US10): `/trusted-contacts` | ❌ **No existe.** |
| GPS/trips (US06/US07/US09): `/trips/*`, PIN, ubicación en vivo | ❌ **No existe.** |
| Cupones/promos (US27/US34) | ❌ **No existe.** |
| Recuperar contraseña (US04): `/auth/password/forgot·reset` | ❌ **No existe.** (Solo `change-password` con la clave actual.) |
| `/transactions/*` (Billing) | ⚠️ Existe como **`/payments`** + `/rentals/{id}/pay`, con otros nombres. |
| `/reservations/*` | ⚠️ Existe como **`/rentals`**. |
| `/routes/*` (carpool) | ⚠️ Existe como **`/adventure-routes`**. |
| `/users/{id}/ratings` | ⚠️ Existe como **`/Reviews`** + **`/user-reviews`**. |

---

## 6. Correcciones concretas que conviene hacer en el reporte/backlog

1. **Quitar "Auth JWT/Bearer".** El backend es **sin token**; la sesión es el `userId`. Alinear el
   `FRONTEND-BACKLOG.md` (que dice "guardar JWT") con esto.
2. **Renombrar endpoints al contrato real** (`/rentals`, `/adventure-routes`, `/payments` +
   `/rentals/{id}/pay`, `/Reviews` + `/user-reviews`). Usar **Swagger** (`/swagger`) como fuente única.
3. **Marcar como NO hechos en backend** (no solo "parcial"): KYC, incidents/pánico, invoices, escrow,
   métodos de pago, contactos de confianza, GPS/trips, cupones, recuperar contraseña. Hoy figuran
   como ✅ o 🟡 en algunos lados (sobre todo KYC US02, que el reporte de la app marca ✅ "conectado").
4. **Marcar como YA hechos** (el backlog los tenía pendientes): **chat/mensajería (US18)** y
   **notificaciones** — ambos existen y funcionan.
5. **Disponibilidad por fechas: ya está en backend** (P1–P6). Actualizar `BACKEND_REQUESTS.md` a
   "implementado" y que la app deje de validar solapamiento por su cuenta.
6. **Carpooling (US14):** el filtro por comunidad/dominio **sigue pendiente** (el campo existe,
   el query param no). Mantener 🟡.
7. **Pago real (SP01):** sigue pendiente. `/payments` y `/rentals/{id}/pay` registran el pago pero
   **no hay pasarela**. Mantener 🟡 "demo".

---

## 7. TL;DR para los dos equipos móviles

- **Cliente (Kotlin):** tu núcleo (catálogo → fechas → reserva → pago → carpool → chat → reseñas →
  notificaciones) está **soportado por el backend real**, con los nombres de §1, **sin JWT**, y ahora
  con **disponibilidad por fechas (409 + `/availability`)**.
- **Proveedor (próxima app):** el backend **ya soporta** publicar autos/rutas, aceptar/gestionar
  reservas (`PATCH /rentals/{id}`), ver ingresos (`/payments?recipientId=`) y reputación
  (`/user-reviews`). No necesitas backend nuevo para el núcleo del proveedor.
- **Lo que falta es backend nuevo** (KYC real, pánico/incidents, GPS, escrow, pasarela real, métodos
  de pago, cupones, recuperar contraseña). Pídanlo formalmente; hoy esas pantallas no tienen API.
