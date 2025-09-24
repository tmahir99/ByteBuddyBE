# ByteBuddy Frontend Development Guide

## 📋 Overview

This document provides comprehensive information for developing the Angular frontend for ByteBuddy - a social platform for sharing code snippets, connecting with developers, and collaborative coding. The backend is built with ASP.NET Core 7 Web API and provides a complete set of RESTful endpoints.

## 🏗️ Backend Architecture

### Base URL
- **Development**: `https://localhost:7082` (or `http://localhost:5078` for HTTP)
- **API Base Path**: `/api`

### Authentication
- **Type**: JWT Bearer Token
- **Header**: `Authorization: Bearer <token>`
- **Token Expiration**: 1 hour (configurable)

## 🔐 Authentication & User Management

### Auth Controller (`/api/auth`)

#### User Registration
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "userName": "string",
  "password": "string",
  "dateOfBirth": "YYYY-MM-DD",
  "gender": "string",
  "birthPlace": "string",
  "address": "string"
}
```
- **Note:** All fields are required. Password must meet complexity requirements (min 6 chars, upper/lowercase, digit, special char).
- **Response:**
```json
{
  "isSucceed": true,
  "message": "User Created Successfully"
}
```
- **Error:**
```json
{
  "isSucceed": false,
  "message": "UserName Already Exists" // or other error message
}
```

#### User Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "string",
  "password": "string"
}
```
- **Response:**
```json
{
  "isSucceed": true,
  "message": "jwt_token_here",
  "roles": ["USER"],
  "user": {
    "userName": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "roles": ["USER"]
  }
}
```

#### Email Activation
```http
GET /api/auth/activate?token={activation_token}
```
- **Response:**
```json
{
  "isSucceed": true,
  "message": "Email activated successfully! Welcome to ByteBuddy!"
}
```

#### Resend Activation Email
```http
POST /api/auth/resend-activation
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Get All Users (Admin only)
```http
GET /api/auth/GetAllUsers
Authorization: Bearer <admin_token>
```

#### Get All Usernames
```http
GET /api/auth/GetAllUserNames
```

#### Get All Users (Public Info, Any User)
```http
GET /api/auth/all-public
```
- **Response:** List of all users with public info (userName, firstName, lastName, email, roles).

#### Get User by Username or ID (Public Info, Any User)
```http
GET /api/auth/public-user?user={usernameOrId}
```
- **Response:**
```json
{
  "userName": "string",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "roles": ["USER"]
}
```
- **Error:**
```json
{
  "message": "User not found"
}
```

#### Admin Functions (Admin Role Required)
- `POST /api/auth/make-admin` - Promote user to admin
- `POST /api/auth/make-user` - Assign user role
- `POST /api/auth/remove-admin` - Remove admin role
- `POST /api/auth/remove-user` - Remove user role

## 📝 Code Snippets Management

### CodeSnippets Controller (`/api/codesnippets`)

#### Get All Code Snippets (with Filtering)
```http
GET /api/codesnippets?searchTerm=...&programmingLanguage=...&createdById=...&userName=...&sortBy=...&page=1&pageSize=10
Authorization: Bearer <token>
```
- **Filtering:**
  - `searchTerm`: Search in title, description, language, tags
  - `programmingLanguage`: Filter by language
  - `createdById`: Filter by user ID
  - `userName`: Filter by username (NEW, case-insensitive, partial match supported)
  - `sortBy`: `latest`, `popular`, `title` (default: latest)
  - `page`, `pageSize`: Pagination

#### Get Code Snippet by ID
```http
GET /api/codesnippets/{id}
Authorization: Bearer <token>
```

#### Create Code Snippet
```http
POST /api/codesnippets
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "string",
  "codeContent": "string",
  "description": "string",
  "programmingLanguage": "string",
  "tagIds": [1,2,3],
  "createdById": "userId"
}
```

#### Update Code Snippet
```http
PUT /api/codesnippets/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "string",
  "codeContent": "string",
  "description": "string",
  "programmingLanguage": "string",
  "tagIds": [1,2,3],
  "createdById": "userId"
}
```

#### Delete Code Snippet
```http
DELETE /api/codesnippets/{id}
Authorization: Bearer <token>
```

#### Get User's Code Snippets
```http
GET /api/codesnippets/user/{userId}
Authorization: Bearer <token>
```

#### Like/Unlike Code Snippet
```http
POST /api/codesnippets/{id}/like
Authorization: Bearer <token>
```

#### Get Code Snippet Likes
```http
GET /api/codesnippets/{id}/likes
Authorization: Bearer <token>
```

## 📁 File Upload Management

### File Controller (`/api/file`)

#### Upload File/Image for Code Snippet
```http
POST /api/file/upload
Authorization: Bearer <token>
Content-Type: multipart/form-data

Form Data:
- file: (binary file)
- codeSnippetId: (number)
```
- **Supported File Types:**
  - Images: .jpg, .jpeg, .png, .gif, .bmp, .webp (Max: 5MB, max 4096x4096px)
  - Code/Text: .txt, .md, .json, .xml, .csv, .log, .sql, .js, .ts, .html, .css, .py, .cs, .java, .cpp, .c, .h, .php, .rb, .go, .rs, .kt, .swift, .yml, .yaml (Max: 10MB)
- **Response:**
```json
{
  "isSucceed": true,
  "message": "File uploaded successfully",
  "fileUrl": "/uploads/codesnippets/filename.ext",
  "fileName": "unique_filename.ext",
  "fileSize": 12345,
  "contentType": "image/jpeg"
}
```

#### Delete File
```http
DELETE /api/file/delete/{codeSnippetId}
Authorization: Bearer <token>
```

#### Get File Info
```http
GET /api/file/info/{codeSnippetId}
Authorization: Bearer <token>
```

#### Download File
```http
GET /api/file/download/{codeSnippetId}
Authorization: Bearer <token>
```

## 🔍 Advanced Search

### Search Controller (`/api/search`)

#### Advanced Search
```http
POST /api/search/codesnippets
Authorization: Bearer <token>
Content-Type: application/json

{
  "query": "string",
  "programmingLanguage": "string",
  "tags": ["tag1", "tag2"],
  "userId": "string",
  "userName": "string",
  "createdAfter": "YYYY-MM-DD",
  "createdBefore": "YYYY-MM-DD",
  "hasFile": false,
  "sortBy": "CreatedAt", // CreatedAt, Title, Likes, Comments, Language
  "sortOrder": "desc", // asc, desc
  "pageSize": 10,
  "pageNumber": 1
}
```
- **Note:** `userName` is supported for searching by username (case-insensitive, partial match).

#### Quick Search
```http
GET /api/search/quick?q=search_term&limit=5
Authorization: Bearer <token>
```

#### Get Search Suggestions
```http
GET /api/search/suggestions?query=partial_term
Authorization: Bearer <token>
```

#### Get Popular Tags
```http
GET /api/search/popular-tags?count=10
Authorization: Bearer <token>
```

#### Get Programming Languages
```http
GET /api/search/programming-languages
Authorization: Bearer <token>
```

#### Get Active Users
```http
GET /api/search/active-users?count=10
Authorization: Bearer <token>
```

## 🏷️ Tags Management

### Tags Controller (`/api/tags`)

#### Get All Tags
```http
GET /api/tags
Authorization: Bearer <token>
```

#### Get Tag by ID
```http
GET /api/tags/{id}
Authorization: Bearer <token>
```

#### Create Tag
```http
POST /api/tags
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "string",
  "description": "string"
}
```

#### Update Tag
```http
PUT /api/tags/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "name": "string",
  "description": "string"
}
```

#### Delete Tag
```http
DELETE /api/tags/{id}
Authorization: Bearer <token>
```

## 👥 Social Features

### Friendships Controller (`/api/friendships`)

#### Send Friend Request
```http
POST /api/friendships/request
Authorization: Bearer <token>
Content-Type: application/json

{
  "addresseeId": "user_id"
}
```

#### Accept Friend Request
```http
POST /api/friendships/accept/{friendshipId}
Authorization: Bearer <token>
```

#### Reject Friend Request
```http
POST /api/friendships/reject/{friendshipId}
Authorization: Bearer <token>
```

#### Get User's Friends
```http
GET /api/friendships/friends/{userId}
Authorization: Bearer <token>
```

#### Get Pending Friend Requests
```http
GET /api/friendships/pending/{userId}
Authorization: Bearer <token>
```

### Social Interactions Controller (`/api/socialinteractions`)

#### Get Comments for Code Snippet
```http
GET /api/socialinteractions/comments/{codeSnippetId}
Authorization: Bearer <token>
```

#### Add Comment
```http
POST /api/socialinteractions/comment
Authorization: Bearer <token>
Content-Type: application/json

{
  "codeSnippetId": 1,
  "content": "string"
}
```

#### Update Comment
```http
PUT /api/socialinteractions/comment/{commentId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "content": "string"
}
```

#### Delete Comment
```http
DELETE /api/socialinteractions/comment/{commentId}
Authorization: Bearer <token>
```

## 📊 Data Models

### User Model (ApplicationUserDto)
```typescript
interface ApplicationUserDto {
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
  // Optionally: dateOfBirth, gender, birthPlace, address, etc. if needed for FE
}
```

### Code Snippet Model (CodeSnippetDto)
```typescript
interface CodeSnippetDto {
  id: number;
  title: string;
  codeContent: string;
  description: string;
  programmingLanguage: string;
  fileUrl?: string;
  createdAt: Date;
  createdById: string;
  createdBy: ApplicationUserDto;
  tags: TagDto[];
  likesCount: number;
  commentsCount: number;
  isLikedByCurrentUser?: boolean;
}
```

### Tag Model (TagDto)
```typescript
interface TagDto {
  id: number;
  name: string;
  area: string;
  codeSnippetsCount?: number;
}
```

### Comment Model (CommentDto)
```typescript
interface CommentDto {
  id: number;
  content: string;
  createdAt: Date;
  createdById: string;
  createdBy: ApplicationUserDto;
  codeSnippetId: number;
}
```

### Friendship Model (FriendshipDto)
```typescript
interface FriendshipDto {
  requesterId: string;
  requester: ApplicationUserDto;
  addresseeId: string;
  addressee: ApplicationUserDto;
status: 'Pending' | 'Accepted' | 'Declined' | 'Blocked';
createdAt: Date;
  respondedAt?: Date;
}
```

## ⚠️ Important Backend Details for FE
- All endpoints requiring authentication must include the JWT token in the `Authorization` header.
- For file uploads, use `multipart/form-data` and provide both the file and the `codeSnippetId`.
- For user search and code snippet search, both `userId` and `userName` are supported as filters.
- All date fields are in ISO 8601 format (e.g., `2024-09-24T21:00:00Z`).
- Error responses are always JSON with a `message` property.
- Pagination is 1-based (page=1 is the first page).

## 🧑‍💻 Example Angular Service Usage

```typescript
@Injectable({ providedIn: 'root' })
export class AuthService {
  private baseUrl = 'https://localhost:7082/api/auth';

  register(userData: RegisterDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, userData);
  }

  login(credentials: LoginDto): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, credentials);
  }

  getUserByUsernameOrId(user: string): Observable<ApplicationUserDto> {
    return this.http.get<ApplicationUserDto>(`${this.baseUrl}/public-user`, { params: { user } });
  }
}
```

## 🛠️ General Notes
- Always check for `isSucceed` in responses.
- Handle error messages from the `message` property in error responses.
- Use the `/api/auth/public-user?user=...` endpoint to fetch user info by username or ID for profile pages, mentions, etc.
- Use the `userName` filter for searching code snippets by username.
- For any new backend changes, check Swagger UI at `/swagger` for up-to-date API docs.

---

This guide is up-to-date with the latest backend changes and should be sufficient for FE development without blocking or misunderstanding. If you need more details, check the Swagger UI or contact the backend team.
