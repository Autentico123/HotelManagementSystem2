-- SQL Script to Add GCash Payment Fields to Bookings Table
-- Run this script in MySQL Workbench or MySQL command line

USE HotelManagementDb;

-- Add GCash payment columns to Bookings table
ALTER TABLE `Bookings`
ADD COLUMN `GCashNumber` VARCHAR(20) NULL AFTER `PaymentMethod`,
ADD COLUMN `GCashAccountName` VARCHAR(100) NULL AFTER `GCashNumber`,
ADD COLUMN `GCashReferenceNumber` VARCHAR(50) NULL AFTER `GCashAccountName`,
ADD COLUMN `GCashPaymentDate` DATETIME(6) NULL AFTER `GCashReferenceNumber`;

-- Verify the columns were added
DESCRIBE `Bookings`;

-- Display success message
SELECT 'GCash payment fields added successfully!' AS Result;
