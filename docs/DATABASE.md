# Database Schema

## Overview
This document describes the database schema for the Hotel Booking API, including tables, relationships, and important constraints.

## Tables

### Users
- `Id` (PK, Guid)
- `UserName` (nvarchar(256), not null)
- `Email` (nvarchar(256), not null, unique)
- `PasswordHash` (nvarchar(max), not null)
- `FirstName` (nvarchar(100))
- `LastName` (nvarchar(100))
- `PhoneNumber` (nvarchar(20))
- `CreatedAt` (datetime2, not null)
- `LastLogin` (datetime2)
- `IsActive` (bit, not null, default: 1)

### Roles
- `Id` (PK, nvarchar(450))
- `Name` (nvarchar(256), not null, unique)
- `NormalizedName` (nvarchar(256), not null, unique)

### UserRoles
- `UserId` (PK, FK to Users.Id)
- `RoleId` (PK, FK to Roles.Id)

### Hotels
- `Id` (PK, Guid)
- `Name` (nvarchar(200), not null)
- `Description` (nvarchar(2000))
- `Address` (nvarchar(500), not null)
- `City` (nvarchar(100), not null)
- `Country` (nvarchar(100), not null)
- `StarRating` (int, not null, default: 3)
- `PhoneNumber` (nvarchar(20))
- `Email` (nvarchar(100))
- `IsActive` (bit, not null, default: 1)
- `CreatedAt` (datetime2, not null)
- `UpdatedAt` (datetime2)
- `CreatedById` (FK to Users.Id)
- `UpdatedById` (FK to Users.Id, nullable)

### Rooms
- `Id` (PK, Guid)
- `HotelId` (FK to Hotels.Id, not null)
- `RoomNumber` (nvarchar(20), not null)
- `RoomTypeId` (FK to RoomTypes.Id, not null)
- `FloorNumber` (int, not null)
- `PricePerNight` (decimal(18,2), not null)
- `IsAvailable` (bit, not null, default: 1)
- `MaxOccupancy` (int, not null)
- `Description` (nvarchar(1000))
- `CreatedAt` (datetime2, not null)
- `UpdatedAt` (datetime2)
- `CreatedById` (FK to Users.Id)
- `UpdatedById` (FK to Users.Id, nullable)

### RoomTypes
- `Id` (PK, int, identity)
- `Name` (nvarchar(100), not null, unique)
- `Description` (nvarchar(500))

### Bookings
- `Id` (PK, Guid)
- `UserId` (FK to Users.Id, not null)
- `RoomId` (FK to Rooms.Id, not null)
- `CheckInDate` (date, not null)
- `CheckOutDate` (date, not null)
- `TotalPrice` (decimal(18,2), not null)
- `Status` (int, not null) -- Enum: Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
- `PaymentStatus` (int, not null) -- Enum: Pending, Paid, Refunded, Failed
- `PaymentIntentId` (nvarchar(100)) -- Stripe Payment Intent ID
- `CreatedAt` (datetime2, not null)
- `UpdatedAt` (datetime2)
- `CancellationDate` (datetime2, nullable)
- `CancellationReason` (nvarchar(500), nullable)

### Reviews
- `Id` (PK, Guid)
- `UserId` (FK to Users.Id, not null)
- `HotelId` (FK to Hotels.Id, not null)
- `Rating` (int, not null) -- 1-5
- `Title` (nvarchar(200), not null)
- `Comment` (nvarchar(2000))
- `CreatedAt` (datetime2, not null)
- `UpdatedAt` (datetime2)
- `IsApproved` (bit, not null, default: 0)

## Relationships

1. **User - UserRoles - Roles**: Many-to-Many
   - A user can have multiple roles
   - A role can be assigned to multiple users

2. **Hotel - Room**: One-to-Many
   - A hotel can have multiple rooms
   - A room belongs to exactly one hotel

3. **Room - RoomType**: Many-to-One
   - Multiple rooms can be of the same type
   - A room has exactly one room type

4. **User - Booking**: One-to-Many
   - A user can have multiple bookings
   - A booking belongs to exactly one user

5. **Room - Booking**: One-to-Many
   - A room can have multiple bookings (over different date ranges)
   - A booking is for exactly one room

6. **User - Review**: One-to-Many
   - A user can write multiple reviews
   - A review is written by exactly one user

7. **Hotel - Review**: One-to-Many
   - A hotel can have multiple reviews
   - A review is for exactly one hotel

## Indexes

1. `IX_Users_Email` on `Users(Email)` - For quick user lookup by email
2. `IX_Hotels_City_Country` on `Hotels(City, Country)` - For location-based hotel searches
3. `IX_Bookings_UserId` on `Bookings(UserId)` - For quick access to a user's bookings
4. `IX_Bookings_RoomId_CheckInDate_CheckOutDate` on `Bookings(RoomId, CheckInDate, CheckOutDate)` - For availability checks
5. `IX_Reviews_HotelId` on `Reviews(HotelId)` - For quick access to a hotel's reviews
6. `IX_Rooms_HotelId` on `Rooms(HotelId)` - For quick access to a hotel's rooms

## Constraints

1. `CK_Bookings_CheckOutAfterCheckIn` - Ensures check-out date is after check-in date
2. `CK_Reviews_RatingRange` - Ensures rating is between 1 and 5
3. `CK_Rooms_PricePositive` - Ensures room price is positive
4. `CK_Bookings_BookingWindow` - Ensures bookings are made within a reasonable time frame (e.g., not more than 1 year in advance)

## Sample Queries

### Get Available Rooms
```sql
SELECT r.*, rt.Name as RoomType, h.Name as HotelName
FROM Rooms r
JOIN RoomTypes rt ON r.RoomTypeId = rt.Id
JOIN Hotels h ON r.HotelId = h.Id
WHERE r.IsAvailable = 1
AND r.HotelId = @HotelId
AND r.Id NOT IN (
    SELECT RoomId 
    FROM Bookings 
    WHERE Status IN (1, 2, 3) -- Confirmed, CheckedIn, or CheckedOut
    AND (
        (@CheckInDate BETWEEN CheckInDate AND DATEADD(day, -1, CheckOutDate))
        OR (@CheckOutDate BETWEEN DATEADD(day, 1, CheckInDate) AND CheckOutDate)
        OR (CheckInDate >= @CheckInDate AND CheckOutDate <= @CheckOutDate)
    )
)
```

### Get Hotel with Average Rating
```sql
SELECT 
    h.*,
    AVG(CAST(r.Rating AS DECIMAL(10,2))) as AverageRating,
    COUNT(r.Id) as ReviewCount
FROM Hotels h
LEFT JOIN Reviews r ON h.Id = r.HotelId AND r.IsApproved = 1
WHERE h.IsActive = 1
GROUP BY h.Id, h.Name, h.Description, h.Address, h.City, h.Country, h.StarRating, 
         h.PhoneNumber, h.Email, h.IsActive, h.CreatedAt, h.UpdatedAt, h.CreatedById, h.UpdatedById
```

## Maintenance

### Index Maintenance
```sql
-- Rebuild all indexes on a table
ALTER INDEX ALL ON dbo.Hotels REBUILD;

-- Update statistics
UPDATE STATISTICS dbo.Hotels WITH FULLSCAN;
```

### Backup
```sql
-- Full database backup
BACKUP DATABASE [HotelBookingDb] 
TO DISK = N'C:\Backup\HotelBookingDb_Full.bak' 
WITH INIT, COMPRESSION, STATS = 10;

-- Transaction log backup (for full recovery model)
BACKUP LOG [HotelBookingDb] 
TO DISK = N'C:\Backup\HotelBookingDb_Log.trn' 
WITH INIT, COMPRESSION, STATS = 10;
```
