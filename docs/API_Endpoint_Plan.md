\# RaceDay API Endpoint Plan



\## Authentication Endpoints



| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |

|-------------|-------|-------------|---------------|--------------|-------------------|

| POST | `/api/Auth/register` | Registers a new user | None (Public) | `{ "email": "string", "password": "string", "role": "Participant|Organiser", "firstName": "string", "lastName": "string", "organiserName": "string (optional)", "dateOfBirth": "date (optional)" }` | `201 Created` - User details, `400 Bad Request` - Invalid data, `409 Conflict` - Email exists |

| POST | `/api/Auth/login` | Authenticates a user | None (Public) | `{ "email": "string", "password": "string" }` | `200 OK` - User details with role, `401 Unauthorized` - Invalid credentials |

| POST | `/api/Auth/logout` | Logs out a user | Any (Logged in) | None | `200 OK` - "Logged out successfully" |

| GET | `/api/Auth/me` | Gets current user | Any (Logged in) | None | `200 OK` - User details, `401 Unauthorized` - Not logged in |



\## Event Endpoints



| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |

|-------------|-------|-------------|---------------|--------------|-------------------|

| GET | `/api/Events` | Gets all events | None (Public) | None | `200 OK` - List of events |

| GET | `/api/Events/{id}` | Gets a specific event | None (Public) | None | `200 OK` - Event object, `404 Not Found` - Event not found |

| POST | `/api/Events` | Creates a new event | Organiser | `{ "name": "string", "description": "string", "eventDate": "datetime", "location": "string", "distance": 10.00, "eventType": "Run|Walk|Cycle" }` | `201 Created` - New event, `400 Bad Request` - Invalid data, `403 Forbidden` - Not Organiser |

| PUT | `/api/Events/{id}` | Updates an event | Organiser | `{ "name": "string", "description": "string", "eventDate": "datetime", "location": "string", "distance": 10.00, "eventType": "Run|Walk|Cycle" }` | `200 OK` - Updated event, `404 Not Found` - Event not found, `403 Forbidden` - Not your event |

| DELETE | `/api/Events/{id}` | Deletes an event | Organiser | None | `204 No Content` - Deleted, `404 Not Found` - Event not found, `403 Forbidden` - Not your event |



\## Category Endpoints



| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |

|-------------|-------|-------------|---------------|--------------|-------------------|

| GET | `/api/Categories/event/{eventId}` | Gets categories for an event | None (Public) | None | `200 OK` - List of categories |

| POST | `/api/Categories` | Creates a new category | Organiser | `{ "eventId": 1, "name": "string", "description": "string" }` | `201 Created` - New category, `400 Bad Request` - Invalid data, `403 Forbidden` - Not your event |

| PUT | `/api/Categories/{id}` | Updates a category | Organiser | `{ "name": "string", "description": "string" }` | `200 OK` - Updated category, `404 Not Found` - Category not found, `403 Forbidden` - Not your event |

| DELETE | `/api/Categories/{id}` | Deletes a category | Organiser | None | `204 No Content` - Deleted, `404 Not Found` - Category not found, `403 Forbidden` - Not your event |



\## Enrolment Endpoints



| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |

|-------------|-------|-------------|---------------|--------------|-------------------|

| GET | `/api/Enrolments/my` | Gets my enrolments | Participant | None | `200 OK` - List of enrolments, `403 Forbidden` - Not Participant |

| POST | `/api/Enrolments/event/{eventId}/enrol` | Enrols in an event | Participant | `{ "categoryId": 1 }` | `201 Created` - Enrolment details, `400 Bad Request` - Already enrolled, `403 Forbidden` - Not Participant |

| GET | `/api/Enrolments/event/{eventId}` | Gets enrolments for an event | Organiser | None | `200 OK` - List of enrolments, `403 Forbidden` - Not your event |

| PUT | `/api/Enrolments/{id}/status` | Updates enrolment status | Organiser | `{ "status": "Pending|Confirmed|Completed" }` | `200 OK` - Updated enrolment, `400 Bad Request` - Invalid status, `403 Forbidden` - Not your event |



\## Result Endpoints



| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |

|-------------|-------|-------------|---------------|--------------|-------------------|

| GET | `/api/Results/my` | Gets my results | Participant | None | `200 OK` - List of results, `403 Forbidden` - Not Participant |

| POST | `/api/Results/enrolment/{enrolmentId}` | Records a result | Organiser | `{ "finishTime": "time", "finishingPosition": 47 }` | `201 Created` - Result details, `400 Bad Request` - Invalid data, `403 Forbidden` - Not your event |

| GET | `/api/Results/event/{eventId}` | Gets results for an event | Organiser | None | `200 OK` - List of results, `403 Forbidden` - Not your event |

| PUT | `/api/Results/{id}` | Updates a result | Organiser | `{ "finishTime": "time", "finishingPosition": 47 }` | `200 OK` - Updated result, `404 Not Found` - Result not found, `403 Forbidden` - Not your event |

| DELETE | `/api/Results/{id}` | Deletes a result | Organiser | None | `204 No Content` - Deleted, `404 Not Found` - Result not found, `403 Forbidden` - Not your event |



\## Summary



| Category | Endpoints |

|----------|-----------|

| Authentication | 4 |

| Events | 5 |

| Categories | 4 |

| Enrolments | 4 |

| Results | 5 |

| \*\*Total\*\* | \*\*22\*\* |

