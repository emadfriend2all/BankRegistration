# Branch Filtering Implementation Summary

## Overview
This implementation adds branch-based filtering to the GetAllCustomers functionality, where:
- Admin users can see all customers from all branches
- Non-admin users can only see customers from their own branch

## Changes Made

### 1. Database Schema Changes
**File:** `Scripts/AddBranchColumnToUsers.sql`
- Added `BRANCH VARCHAR2(50) NULL` column to `USERS` table
- Added index `IX_USERS_BRANCH` for better performance

### 2. Entity Model Updates
**File:** `Entities/Identity/User.cs`
- Added `Branch` property as string with max length 50, nullable

### 3. Database Context Configuration
**File:** `Data/RegistrationPortalDbContext.cs`
- Added Branch property configuration in `ConfigureIdentityEntities` method

### 4. Controller Updates
**File:** `Controllers/CustMastController.cs`
- Modified `GetAllCustomers` method to:
  - Retrieve current user information including branch
  - Pass user branch to service layer
  - Maintain existing role-based permission checks

### 5. Service Layer Updates
**File:** `Services/ICustMastService.cs`
- Added `userBranch` parameter to `GetAllCustomersAsync` method

**File:** `Services/CustMastService.cs`
- Updated method signature to accept `userBranch` parameter
- Pass branch information to repository layer

### 6. Repository Layer Updates
**File:** `Repositories/ICustMastRepository.cs`
- Added `userRole` and `userBranch` parameters to `GetAllCustomersPaginatedAsync`

**File:** `Repositories/CustMastRepository.cs`
- Updated method signature to accept branch filtering parameters
- Added branch filtering logic in WHERE clauses for both main query and count query
- Added branch parameter to SQL commands
- Filtering logic: `userRole != "Admin"` AND `userBranch` is not null

## Implementation Logic

### Branch Filtering Rules
1. **Admin Users**: No branch filtering applied - can see all customers
2. **Non-Admin Users**: Filtered by their assigned branch code
3. **Users without Branch**: If userBranch is null, no filtering is applied (fallback behavior)

### SQL Query Changes
The following condition is added to WHERE clauses when filtering is enabled:
```sql
c."branch_c_code" = :userBranch
```

## Testing Instructions

### 1. Database Setup
Execute the SQL script to add the branch column:
```sql
-- Run Scripts/AddBranchColumnToUsers.sql
```

### 2. Update User Records
Update existing users to have appropriate branch assignments:
```sql
UPDATE SSDBONLINE.USERS SET BRANCH = 'BRANCH001' WHERE USERNAME = 'your_username';
```

### 3. Test Scenarios

#### Admin User Test
- Login as admin user
- Call GET `/api/custmast`
- Expected: Should see customers from all branches

#### Non-Admin User Test
- Login as regular user with assigned branch
- Call GET `/api/custmast`
- Expected: Should only see customers from user's assigned branch

#### Search and Filtering Test
- Test search functionality with branch filtering
- Test pagination with branch filtering
- Expected: All filters should work together correctly

## Files Modified
1. `Entities/Identity/User.cs` - Added Branch property
2. `Data/RegistrationPortalDbContext.cs` - Added Branch configuration
3. `Controllers/CustMastController.cs` - Added branch retrieval logic
4. `Services/ICustMastService.cs` - Updated interface
5. `Services/CustMastService.cs` - Updated implementation
6. `Repositories/ICustMastRepository.cs` - Updated interface
7. `Repositories/CustMastRepository.cs` - Updated implementation
8. `Scripts/AddBranchColumnToUsers.sql` - Database migration script
9. `Scripts/BranchFilteringImplementation.md` - This documentation

## Notes
- The implementation preserves existing functionality while adding branch filtering
- Admin role is hardcoded - consider using a more flexible role-based system
- Branch filtering is applied at the database level for optimal performance
- All existing search, sort, and pagination features continue to work
