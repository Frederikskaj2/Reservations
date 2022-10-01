| Role | Orders | Owner Orders | Payments/Postings | Users | Lock Box Codes | Cleaning Schedule |
|-|-|-|-|-|-|-|
| Order Handling | RW | RW |  | R | | |
| Bookkeeping | R | R | RW | R | | |
| User Administration | R | R | | RW | | |
| Lock Box Codes | | | | | R | |
| Cleaning | | | | | | R |

| Endpoint | Roles |
|-|-|
| `GET /cleaning-tasks` | Cleaning |
| `POST /cleaning-tasks/send` | Cleaning |
| `GET /configuration` | |
| `GET /creditors` | Bookkeeping |
| `GET /debtors` | Bookkeeping |
| `GET /lock-box-codes` | LockBoxCodes |
| `POST /lock-box-codes/send` | LockBoxCodes |
| `GET /orders/any/{orderId}` | OrderHandling, Bookkeeping, UserAdministration |
| `GET /orders/my` | Resident |
| `POST /orders/my` | Resident |
| `GET /orders/my/{orderId}` | Resident |
| `PATCH /orders/my/{orderId}` | Resident |
| `GET /orders/owner` | OrderHandling, Bookkeeping, UserAdministration |
| `POST /orders/owner` | OrderHandling |
| `PATCH /orders/owner/{orderId}` | OrderHandling |
| `GET /orders/user` | OrderHandling, Bookkeeping, UserAdministration |
| `PATCH /orders/user/{orderId}` | OrderHandling |
| `POST /orders/user/{orderId}/settle-reservation` | OrderHandling |
| `POST /payments/{paymentId}` | Bookkeeping |
| `GET /postings` | Bookkeeping |
| `GET /postings/range` | Bookkeeping |
| `POST /postings/send` | Bookieeping |
| `GET /reserved-days` | |
| `GET /reserved-days/confirmed` | |
| `GET /reserved-days/my` | Resident |
| `GET /reserved-days/owner` | OrderHandling |
| `GET /transactions` | Resident |
| `GET /user` | Any |
| `PATCH /user` | Any |
| `POST /user/confirm-email` | |
| `POST /user/create-access-token` | |
| `POST /user/resend-confirm-email-email` | Any |
| `POST /user/sign-in` | |
| `POST /user/sign-out` | Any |
| `POST /user/sign-out-everywhere-else` | Any |
| `POST /user/sign-up` | |
| `POST /user/update-password` | Any |
| `GET /users` | OrderHandling, Bookkeeping, UserAdministration |
| `GET /users/{userId}` | OrderHandling, Bookkeeping, UserAdministration |
| `POST /users/{userId}/pay-out` | Bookkeeping |
| `GET /users/{userId}/transactions` | OrderHandling, Bookkeeping, UserAdministration |
