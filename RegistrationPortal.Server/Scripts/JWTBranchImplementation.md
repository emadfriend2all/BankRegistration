# JWT Branch Claims Implementation

## Overview
This implementation adds user branch information to JWT claims during login, allowing the branch to be easily accessed throughout the application for authorization and filtering purposes.

## Changes Made

### 1. JWT Service Updates
**File:** `Services/JwtService.cs`
- Added `new Claim("Branch", user.Branch ?? "")` to the claims collection in `GenerateToken` method
- Added `GetBranchFromToken(string token)` method to extract branch claim from JWT tokens

### 2. Auth Service Updates
**File:** `Services/AuthService.cs`
- Updated `GetSingleUserAsync` method to include `Branch` field when reading user data from database
- Modified `LoginAsync` method to include branch in UserDto response
- Modified `RegisterAsync` method to include branch in UserDto response

### 3. DTO Updates
**File:** `DTOs/Auth/AuthResponseDto.cs`
- Added `Branch` property to `UserDto` class

### 4. Controller Updates
**File:** `Controllers/AuthController.cs`
- Updated `GetCurrentUser` endpoint to include branch in the response

## Implementation Details

### JWT Token Structure
The JWT token now includes the following claims:
- `NameIdentifier`: User ID
- `Name`: Username
- `Email`: User email
- `FirstName`: User first name
- `LastName`: User last name
- `Branch`: User branch (newly added)
- `Role`: User roles (multiple claims)
- `permission`: User permissions (multiple claims)

### Branch Access Methods
You can now access the user's branch in several ways:

1. **From JWT Token:**
   ```csharp
   var branch = _jwtService.GetBranchFromToken(token);
   ```

2. **From Current User Claims:**
   ```csharp
   var branch = User.FindFirst("Branch")?.Value;
   ```

3. **From Auth Response:**
   ```csharp
   // Login/Register response includes branch in UserDto
   var branch = authResponse.User.Branch;
   ```

4. **From Database User:**
   ```csharp
   var user = await _authService.GetUserByIdAsync(userId);
   var branch = user.Branch;
   ```

## Testing Instructions

### 1. Database Setup
Ensure the USERS table has the BRANCH column:
```sql
-- Run Scripts/AddBranchColumnToUsers.sql if not already done
```

### 2. Update User Records
Assign branches to users:
```sql
UPDATE SSDBONLINE.USERS SET BRANCH = 'BRANCH001' WHERE USERNAME = 'your_username';
```

### 3. Test Login Flow
1. Login with a user that has a branch assigned
2. Verify the JWT token contains the branch claim
3. Check that the auth response includes branch information

### 4. Test Branch Access
1. Call `/api/auth/me` endpoint
2. Verify the response includes the branch field
3. Test that branch filtering works in customer endpoints

## Files Modified
1. `Services/JwtService.cs` - Added branch claim and extraction method
2. `Services/AuthService.cs` - Updated user data retrieval and responses
3. `DTOs/Auth/AuthResponseDto.cs` - Added branch to UserDto
4. `Controllers/AuthController.cs` - Updated GetCurrentUser response
5. `Scripts/JWTBranchImplementation.md` - This documentation

## Security Considerations
- Branch claim is included in JWT token, so it's cryptographically signed
- Branch filtering is applied at database level for security
- Admin users bypass branch filtering as intended
- Users without branch assignments get empty string in claim

## Integration with Branch Filtering
This JWT branch implementation works seamlessly with the previously implemented branch filtering in `GetAllCustomers`:
- JWT provides the branch claim
- Controller extracts branch from current user
- Repository applies branch filtering for non-admin users
- Admin users see all data regardless of branch
