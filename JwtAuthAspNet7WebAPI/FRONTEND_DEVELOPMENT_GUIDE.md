# ByteBuddy Frontend Development Guide

## 📋 Overview

This document provides comprehensive information for developing the Angular frontend for ByteBuddy - a social platform for sharing code snippets, connecting with developers, and collaborative coding. The backend is built with ASP.NET Core 7 Web API and provides a complete set of RESTful endpoints.

## 🏗️ Backend Architecture

### Base URL
- **Development**: `https://localhost:7082`
- **API Base Path**: `/api`

### Authentication
- **Type**: JWT Bearer Token
- **Header**: `Authorization: Bearer <token>`
- **Token Expiration**: Configurable (default: varies by implementation)

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
  "dateOfBirth": "2024-01-01T00:00:00.000Z",
  "gender": "string",
  "birthPlace": "string",
  "address": "string"
}
```

**Response:**
```json
{
  "isSucceed": true,
  "message": "User created successfully. Please check your email to activate your account."
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

**Response:**
```json
{
  "isSucceed": true,
  "message": "jwt_token_here",
  "roles": ["USER"],
  "user": {
    "id": "string",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "userName": "string",
    "dateOfBirth": "2024-01-01T00:00:00.000Z",
    "gender": "string",
    "birthPlace": "string",
    "address": "string",
    "fullName": "string"
  }
}
```

#### Email Activation
```http
GET /api/auth/activate?token={activation_token}
```

#### Resend Activation Email
```http
POST /api/auth/resend-activation
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Admin Functions (Admin Role Required)
- `POST /api/auth/make-admin` - Promote user to admin
- `POST /api/auth/make-user` - Assign user role
- `POST /api/auth/remove-admin` - Remove admin role
- `POST /api/auth/remove-user` - Remove user role
- `GET /api/auth/GetAllUsers` - Get all users
- `GET /api/auth/GetAllUserNames` - Get all usernames

## 📝 Code Snippets Management

### CodeSnippets Controller (`/api/codesnippets`)

#### Get All Code Snippets
```http
GET /api/codesnippets
Authorization: Bearer <token>
```

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
  "tags": ["tag1", "tag2"]
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
  "tags": ["tag1", "tag2"]
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

## 📄 Page Management

### Page Controller (`/api/page`)

#### Get All Pages
```http
GET /api/page
Authorization: Bearer <token>
```

#### Get Page by ID
```http
GET /api/page/{id}
Authorization: Bearer <token>
```

#### Create Page
```http
POST /api/page
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "string",
  "description": "string"
}
```

#### Update Page
```http
PUT /api/page/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "string",
  "description": "string"
}
```

#### Delete Page
```http
DELETE /api/page/{id}
Authorization: Bearer <token>
```

#### Like/Unlike Page
```http
POST /api/page/{id}/like
Authorization: Bearer <token>
```

#### Get Page Likes
```http
GET /api/page/{id}/likes
Authorization: Bearer <token>
```

#### Get User's Pages
```http
GET /api/page/user/{userId}
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

**Supported File Types:**
- **Images**: .jpg, .jpeg, .png, .gif, .bmp, .webp (Max: 5MB)
- **Code Files**: .txt, .md, .json, .xml, .csv, .log, .sql, .js, .ts, .html, .css, .py, .cs, .java, .cpp, .c, .h, .php, .rb, .go, .rs, .kt, .swift, .yml, .yaml (Max: 10MB)

**Response:**
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
  "createdAfter": "2024-01-01T00:00:00.000Z",
  "createdBefore": "2024-12-31T23:59:59.999Z",
  "hasFile": false,
  "sortBy": "CreatedAt", // CreatedAt, Title, Likes, Comments, Language
  "sortOrder": "desc", // asc, desc
  "pageSize": 10,
  "pageNumber": 1
}
```

**Response:**
```json
{
  "results": [/* CodeSnippetDto array */],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "query": "search term",
  "appliedFilters": ["Query: search term", "Language: JavaScript"]
}
```

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

**Response:**
```json
{
  "tags": ["tag1", "tag2"],
  "programmingLanguages": ["JavaScript", "Python"],
  "users": ["user1", "user2"]
}
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
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  userName: string;
  dateOfBirth: Date;
  gender: string;
  birthPlace: string;
  address: string;
  fullName: string;
  isEmailConfirmed?: boolean;
  createdAt?: Date;
  lastLoginAt?: Date;
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

### Page Model (PageDto)
```typescript
interface PageDto {
  id: number;
  title: string;
  description: string;
  createdAt: Date;
  createdById: string;
  createdBy: ApplicationUserDto;
  likesCount: number;
  isLikedByCurrentUser?: boolean;
}
```

### Tag Model (TagDto)
```typescript
interface TagDto {
  id: number;
  name: string;
  description: string;
  createdAt: Date;
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
  id: number;
  requesterId: string;
  requester: ApplicationUserDto;
  addresseeId: string;
  addressee: ApplicationUserDto;
  status: 'Pending' | 'Accepted' | 'Rejected';
  createdAt: Date;
  respondedAt?: Date;
}
```

## 🎨 Frontend Implementation Recommendations

### Angular Services Structure

#### 1. Authentication Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = 'https://localhost:7082/api/auth';
  
  register(userData: RegisterDto): Observable<AuthResponse> { }
  login(credentials: LoginDto): Observable<AuthResponse> { }
  logout(): void { }
  activateEmail(token: string): Observable<AuthResponse> { }
  resendActivation(email: string): Observable<AuthResponse> { }
  getCurrentUser(): ApplicationUserDto | null { }
  isAuthenticated(): boolean { }
  getToken(): string | null { }
}
```

#### 2. Code Snippet Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class CodeSnippetService {
  private baseUrl = 'https://localhost:7082/api/codesnippets';
  
  getAll(): Observable<CodeSnippetDto[]> { }
  getById(id: number): Observable<CodeSnippetDto> { }
  create(codeSnippet: CreateCodeSnippetDto): Observable<CodeSnippetDto> { }
  update(id: number, codeSnippet: UpdateCodeSnippetDto): Observable<CodeSnippetDto> { }
  delete(id: number): Observable<void> { }
  getUserSnippets(userId: string): Observable<CodeSnippetDto[]> { }
  toggleLike(id: number): Observable<void> { }
  getLikes(id: number): Observable<ApplicationUserDto[]> { }
}
```

#### 3. File Upload Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class FileService {
  private baseUrl = 'https://localhost:7082/api/file';
  
  uploadFile(file: File, codeSnippetId: number): Observable<FileUploadResponseDto> { }
  deleteFile(codeSnippetId: number): Observable<FileUploadResponseDto> { }
  getFileInfo(codeSnippetId: number): Observable<any> { }
  downloadFile(codeSnippetId: number): Observable<Blob> { }
}
```

#### 4. Search Service
```typescript
@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private baseUrl = 'https://localhost:7082/api/search';
  
  searchCodeSnippets(searchRequest: SearchRequestDto): Observable<SearchResponseDto> { }
  quickSearch(query: string, limit?: number): Observable<SearchResponseDto> { }
  getSuggestions(query: string): Observable<SearchSuggestionsDto> { }
  getPopularTags(count?: number): Observable<string[]> { }
  getProgrammingLanguages(): Observable<string[]> { }
  getActiveUsers(count?: number): Observable<ApplicationUserDto[]> { }
}
```

### Key Angular Components to Implement

#### 1. Authentication Components
- `LoginComponent` - User login form
- `RegisterComponent` - User registration form
- `EmailActivationComponent` - Email activation handler
- `ProfileComponent` - User profile management

#### 2. Code Snippet Components
- `CodeSnippetListComponent` - Display list of code snippets
- `CodeSnippetDetailComponent` - View single code snippet
- `CodeSnippetCreateComponent` - Create new code snippet
- `CodeSnippetEditComponent` - Edit existing code snippet
- `CodeEditorComponent` - Syntax-highlighted code editor

#### 3. File Management Components
- `FileUploadComponent` - File upload interface
- `FilePreviewComponent` - Preview uploaded files/images
- `FileManagerComponent` - Manage attached files

#### 4. Search Components
- `SearchComponent` - Main search interface
- `AdvancedSearchComponent` - Advanced search filters
- `SearchResultsComponent` - Display search results
- `SearchSuggestionsComponent` - Auto-complete suggestions

#### 5. Social Components
- `FriendListComponent` - Display user's friends
- `FriendRequestsComponent` - Manage friend requests
- `CommentsComponent` - Display and manage comments
- `UserProfileComponent` - View other users' profiles

#### 6. Page Management Components
- `PageListComponent` - Display pages
- `PageDetailComponent` - View single page
- `PageCreateComponent` - Create new page
- `PageEditComponent` - Edit existing page

### HTTP Interceptors

#### 1. Auth Interceptor
```typescript
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    return next.handle(req);
  }
}
```

#### 2. Error Interceptor
```typescript
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          this.authService.logout();
          this.router.navigate(['/login']);
        }
        return throwError(error);
      })
    );
  }
}
```

### Routing Structure

```typescript
const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'activate', component: EmailActivationComponent },
  { 
    path: 'dashboard', 
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'code-snippets',
    canActivate: [AuthGuard],
    children: [
      { path: '', component: CodeSnippetListComponent },
      { path: 'create', component: CodeSnippetCreateComponent },
      { path: ':id', component: CodeSnippetDetailComponent },
      { path: ':id/edit', component: CodeSnippetEditComponent }
    ]
  },
  {
    path: 'pages',
    canActivate: [AuthGuard],
    children: [
      { path: '', component: PageListComponent },
      { path: 'create', component: PageCreateComponent },
      { path: ':id', component: PageDetailComponent },
      { path: ':id/edit', component: PageEditComponent }
    ]
  },
  { path: 'search', component: SearchComponent, canActivate: [AuthGuard] },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'friends', component: FriendListComponent, canActivate: [AuthGuard] },
  { path: '**', component: NotFoundComponent }
];
```

## 🔒 Security Considerations

1. **JWT Token Management**
   - Store tokens securely (consider HttpOnly cookies)
   - Implement token refresh mechanism
   - Handle token expiration gracefully

2. **Input Validation**
   - Validate all user inputs on frontend
   - Sanitize HTML content
   - Implement proper form validation

3. **File Upload Security**
   - Validate file types and sizes
   - Preview files safely
   - Handle upload errors gracefully

4. **CORS Configuration**
   - Backend is configured for `http://localhost:4200`
   - Update for production domains

## 📱 Responsive Design Recommendations

1. **Mobile-First Approach**
   - Design for mobile screens first
   - Progressive enhancement for larger screens

2. **Key Breakpoints**
   - Mobile: 320px - 768px
   - Tablet: 768px - 1024px
   - Desktop: 1024px+

3. **Touch-Friendly Interface**
   - Adequate touch targets (44px minimum)
   - Swipe gestures for mobile navigation

## 🎯 Performance Optimization

1. **Lazy Loading**
   - Implement lazy loading for routes
   - Load images on demand

2. **Caching Strategy**
   - Cache API responses appropriately
   - Implement service worker for offline capability

3. **Bundle Optimization**
   - Use Angular CLI build optimizations
   - Implement tree shaking

## 🧪 Testing Strategy

1. **Unit Tests**
   - Test all services and components
   - Mock HTTP calls

2. **Integration Tests**
   - Test component interactions
   - Test routing

3. **E2E Tests**
   - Test critical user flows
   - Test authentication flow

## 📚 Recommended Libraries

1. **UI Framework**: Angular Material or PrimeNG
2. **Code Editor**: Monaco Editor or CodeMirror
3. **File Upload**: ng2-file-upload or ngx-dropzone
4. **HTTP Client**: Angular HttpClient (built-in)
5. **State Management**: NgRx (for complex state)
6. **Forms**: Angular Reactive Forms
7. **Animations**: Angular Animations
8. **Icons**: Angular Material Icons or Font Awesome
9. **Charts**: Chart.js or ng2-charts (if analytics needed)
10. **Date Handling**: date-fns or moment.js

## 🚀 Deployment Considerations

1. **Environment Configuration**
   - Separate configs for dev/staging/prod
   - Environment-specific API URLs

2. **Build Process**
   - Use Angular CLI for production builds
   - Enable AOT compilation
   - Implement proper error handling

3. **SEO Optimization**
   - Implement Angular Universal for SSR
   - Add proper meta tags
   - Implement structured data

This comprehensive guide provides all the necessary information to build a complete Angular frontend for the ByteBuddy platform. The backend API is fully functional and ready to support all these features.
