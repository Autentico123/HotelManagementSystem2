-- =====================================================
-- GCash Payment Verification System Setup
-- =====================================================
-- Run this after: dotnet ef database update

USE HotelManagementDb;

-- =====================================================
-- 1. Verify New Tables Exist
-- =====================================================
SELECT 'Checking if tables exist...' AS Status;

SHOW TABLES LIKE 'GCashPayments';
SHOW TABLES LIKE 'SystemSettings';

-- =====================================================
-- 2. Insert Default System Settings
-- =====================================================
SELECT 'Inserting default system settings...' AS Status;

INSERT INTO SystemSettings (
    AdminGCashNumber,
    AdminGCashAccountName,
    GCashQRCodeUrl,
    PaymentInstructions,
    LastUpdated,
    UpdatedBy
) VALUES (
    '09171234567',  -- ?? CHANGE THIS to your actual GCash number!
    'Hotel Administrator',  -- ?? CHANGE THIS to your business name!
    NULL,  -- Optional: Upload QR code and set path here
    'Please send payment to the GCash number above and upload a clear screenshot of the transaction.',
    NOW(),
    'System'
)
ON DUPLICATE KEY UPDATE 
    LastUpdated = NOW();

-- =====================================================
-- 3. Verify System Settings
-- =====================================================
SELECT 'Current System Settings:' AS Status;

SELECT 
    AdminGCashNumber,
    AdminGCashAccountName,
    GCashQRCodeUrl,
    PaymentInstructions,
    LastUpdated
FROM SystemSettings;

-- =====================================================
-- 4. Check Table Structures
-- =====================================================
SELECT '=== GCashPayments Table Structure ===' AS Info;
DESCRIBE GCashPayments;

SELECT '=== SystemSettings Table Structure ===' AS Info;
DESCRIBE SystemSettings;

-- =====================================================
-- 5. Update Your Admin GCash Details (CUSTOMIZE THIS)
-- =====================================================
-- Uncomment and run this section with your actual details:

/*
UPDATE SystemSettings 
SET 
    AdminGCashNumber = '09171234567',  -- Your GCash number
    AdminGCashAccountName = 'Your Hotel Name',  -- Your business name
    GCashQRCodeUrl = '/uploads/qr-codes/gcash-qr.png',  -- Optional: Your QR code path
    PaymentInstructions = 'Send payment to the GCash number above. Take a screenshot and upload it here.',
    LastUpdated = NOW(),
    UpdatedBy = 'Admin'
WHERE Id = 1;
*/

-- =====================================================
-- 6. Create Uploads Folders (Run in terminal)
-- =====================================================
-- mkdir -p wwwroot/uploads/gcash-proofs
-- mkdir -p wwwroot/uploads/qr-codes

-- =====================================================
-- 7. Sample Queries for Admin
-- =====================================================

-- View all pending payments
-- SELECT * FROM GCashPayments WHERE Status = 0 ORDER BY PaymentDate DESC;

-- View all verified payments
-- SELECT * FROM GCashPayments WHERE Status = 1 ORDER BY VerifiedDate DESC;

-- View payment statistics
-- SELECT 
--     COUNT(CASE WHEN Status = 0 THEN 1 END) as Pending,
--     COUNT(CASE WHEN Status = 1 THEN 1 END) as Verified,
--     COUNT(CASE WHEN Status = 2 THEN 1 END) as Rejected,
--     SUM(CASE WHEN Status = 1 THEN Amount ELSE 0 END) as TotalVerified
-- FROM GCashPayments;

-- =====================================================
-- 8. Test Data (Optional - For Testing Only)
-- =====================================================

-- Uncomment to create a test payment:
/*
INSERT INTO GCashPayments (
    BookingId,
    SenderGCashNumber,
    SenderAccountName,
    ReceiverGCashNumber,
    Amount,
    ReferenceNumber,
    ProofImageUrl,
    Status,
    PaymentDate,
    Notes
) VALUES (
    1,  -- Replace with actual booking ID
    '09171234567',
    'Juan Dela Cruz',
    (SELECT AdminGCashNumber FROM SystemSettings LIMIT 1),
    1000.00,
    CONCAT('GCASH', DATE_FORMAT(NOW(), '%Y%m%d%H%i%s'), FLOOR(RAND() * 9000 + 1000)),
    NULL,
    0,  -- 0 = Pending
    NOW(),
    'Test payment'
);
*/

-- =====================================================
-- 9. Verify Setup Complete
-- =====================================================
SELECT 'Setup Complete!' AS Status;

SELECT 
    'System Settings' as TableName,
    COUNT(*) as RecordCount
FROM SystemSettings
UNION ALL
SELECT 
    'GCash Payments' as TableName,
    COUNT(*) as RecordCount
FROM GCashPayments;

-- =====================================================
-- 10. Next Steps
-- =====================================================
SELECT '
? SETUP COMPLETE! 

Next Steps:
1. Update AdminGCashNumber above with your actual number
2. Run the application: dotnet run
3. Login as guest and test payment
4. Login as admin to verify payment

Default Logins:
- Admin: admin@hotel.com / Admin@123
- Guest: guest@hotel.com / Guest@123

URLs:
- Payment Form: /GCashPayments/Pay/{bookingId}
- Admin Dashboard: /GCashPayments/PendingPayments
' AS NextSteps;

-- =====================================================
-- END OF SETUP SCRIPT
-- =====================================================
