INSERT INTO Users (BusinessId, BranchId, Role, FullName, Email, PasswordHash, ContactNumber, Status, CreatedAt)
VALUES (
    NULL, 
    NULL, 
    'SuperAdmin', 
    'System Administrator', 
    'superadmin@agrinexus.com', 
    'Admin@123456', 
    '', 
    'Active', 
    GETDATE()
);