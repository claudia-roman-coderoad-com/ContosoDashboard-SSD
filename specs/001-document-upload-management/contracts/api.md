# API Contracts: Document Upload and Management

## Upload Document

POST /api/documents/upload

- Headers: Authorization
- Body (multipart/form-data): file, title (string), category (string), description (string, optional), projectId (int, optional), tags (string, optional)
- Response: 201 Created { documentId, message }

## Download Document

GET /api/documents/{id}/download

- Headers: Authorization
- Response: 200 file stream (with Content-Type), or 403/404

## Preview Document

GET /api/documents/{id}/preview

- Headers: Authorization
- Response: 200 partial content for browser preview (PDF, images), or 415 if unsupported

## List Documents

GET /api/documents?filter&sort&page

- Headers: Authorization
- Query: title, tags, projectId, category, uploader, sort, page, pageSize
- Response: 200 { items: [Document], total }

## Share Document

POST /api/documents/{id}/share

- Headers: Authorization
- Body: { recipientUserId?: int, recipientTeamId?: int, permission: 'read'|'write' }
- Response: 200 { message }

## Edit Metadata

PUT /api/documents/{id}

- Body: { title?, description?, category?, tags? }
- Response: 200 { document }

## Delete Document

DELETE /api/documents/{id}

- Headers: Authorization
- Response: 204 No Content

Authorization: All endpoints perform authorization checks; team shares resolve access dynamically by membership lookup.

---
