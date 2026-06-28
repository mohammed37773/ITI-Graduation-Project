IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NurseriesDB')
BEGIN
    CREATE DATABASE NurseriesDB;
END

EXEC('USE NurseriesDB;');

CREATE TABLE [Parents] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [full_name] nvarchar(255),
  [email] nvarchar(255) UNIQUE,
  [phone_number] nvarchar(255),
  [password_hash] nvarchar(255),
  [location_lat] float,
  [location_lng] float,
  [created_at] timestamp
)
GO

CREATE TABLE [Children] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [parent_id] integer NOT NULL,
  [full_name] nvarchar(255),
  [date_of_birth] date,
  [special_needs] text
)
GO

CREATE TABLE [Nurseries] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [name] nvarchar(255),
  [description] text,
  [daily_price] decimal,
  [age_range_min] integer,
  [age_range_max] integer,
  [capacity] integer,
  [avg_rating] float,
  [is_verified] bit,
  [created_at] timestamp
)
GO

CREATE TABLE [Locations] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [nursery_id] integer NOT NULL,
  [address] nvarchar(255),
  [city] nvarchar(255),
  [district] nvarchar(255),
  [latitude] float,
  [longitude] float
)
GO

CREATE TABLE [Reviews] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [parent_id] integer NOT NULL,
  [nursery_id] integer NOT NULL,
  [rating] integer,
  [comment] text,
  [created_at] timestamp
)
GO

CREATE TABLE [Bookings] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [parent_id] integer NOT NULL,
  [nursery_id] integer NOT NULL,
  [child_id] integer NOT NULL,
  [start_date] date,
  [status] nvarchar(255),
  [total_price] decimal,
  [created_at] timestamp
)
GO

CREATE TABLE [NurseryImages] (
  [id] integer PRIMARY KEY IDENTITY(1, 1),
  [nursery_id] integer NOT NULL,
  [image_url] nvarchar(255),
  [uploaded_at] timestamp
)
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '1 to 5',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Reviews',
@level2type = N'Column', @level2name = 'rating';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Pending, Confirmed, Cancelled, Completed',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Bookings',
@level2type = N'Column', @level2name = 'status';
GO

ALTER TABLE [Children] ADD CONSTRAINT [parent_children] FOREIGN KEY ([parent_id]) REFERENCES [Parents] ([id])
GO

ALTER TABLE [Nurseries] ADD CONSTRAINT [nursery_location] FOREIGN KEY ([id]) REFERENCES [Locations] ([nursery_id])
GO

ALTER TABLE [Reviews] ADD CONSTRAINT [parent_reviews] FOREIGN KEY ([parent_id]) REFERENCES [Parents] ([id])
GO

ALTER TABLE [Reviews] ADD CONSTRAINT [nursery_reviews] FOREIGN KEY ([nursery_id]) REFERENCES [Nurseries] ([id])
GO

ALTER TABLE [Bookings] ADD CONSTRAINT [parent_bookings] FOREIGN KEY ([parent_id]) REFERENCES [Parents] ([id])
GO

ALTER TABLE [Bookings] ADD CONSTRAINT [nursery_bookings] FOREIGN KEY ([nursery_id]) REFERENCES [Nurseries] ([id])
GO

ALTER TABLE [Bookings] ADD CONSTRAINT [child_bookings] FOREIGN KEY ([child_id]) REFERENCES [Children] ([id])
GO

ALTER TABLE [NurseryImages] ADD CONSTRAINT [nursery_images] FOREIGN KEY ([nursery_id]) REFERENCES [Nurseries] ([id])
GO
