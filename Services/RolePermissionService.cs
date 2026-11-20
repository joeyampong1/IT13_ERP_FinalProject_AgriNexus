using System;
using System.Collections.Generic;
using System.Linq;

namespace AgriNexus.Services
{
    public record ModuleDef(string Key, string Name, string Category, string Icon);

    public class RolePermissionService
    {
        // Restricted governance modules for non-admin staff
        public static readonly HashSet<string> RestrictedStaffModules = new(StringComparer.OrdinalIgnoreCase)
        {
            "branches",
            "staff",
            "subscription",
            "business-settings"
        };

        public static readonly List<ModuleDef> AllBusinessModules = new()
        {
            // Overview
            new("dashboard", "Dashboard", "Overview", "bi-grid-1x2"),

            // Business
            new("branches", "Branches", "Business", "bi-shop"),
            new("staff", "Staff & Permissions", "Business", "bi-people"),
            new("subscription", "My Subscription", "Business", "bi-credit-card"),
            new("consolidated-reports", "Consolidated Reports", "Business", "bi-bar-chart-line"),
            new("business-settings", "Business Settings", "Business", "bi-gear"),

            // Farm Operations
            new("farms-plots", "Farms & Plots", "Farm Operations", "bi-map"),
            new("crop-planning", "Crop Planning", "Farm Operations", "bi-flower1"),

            // Inventory
            new("inventory", "Inventory", "Inventory", "bi-box-seam"),
            new("stock-movements", "Stock Movements", "Inventory", "bi-arrow-left-right"),
            new("suppliers", "Suppliers", "Inventory", "bi-truck"),

            // Labor & Harvest
            new("workers", "Workers", "Labor & Harvest", "bi-person-badge"),
            new("work-assignments", "Work Assignments", "Labor & Harvest", "bi-clipboard-check"),
            new("attendance-logs", "Attendance & Logs", "Labor & Harvest", "bi-clock"),
            new("harvest-recording", "Harvest Recording", "Labor & Harvest", "bi-basket"),

            // Sales & Cashier
            new("new-sale", "New Sale", "Sales & Cashier", "bi-cart-plus"),
            new("sales-transactions", "Sales Transactions", "Sales & Cashier", "bi-receipt"),
            new("customers", "Customers", "Sales & Cashier", "bi-person"),
            new("payments-collections", "Payments & Collections", "Sales & Cashier", "bi-cash-coin"),
            new("cash-drawer", "Cash Drawer", "Sales & Cashier", "bi-safe"),

            // Finance & Audit
            new("expenses", "Expenses", "Finance", "bi-wallet2"),
            new("production-costing", "Production Costing", "Finance", "bi-layers"),
            new("financial-reports", "Financial Reports", "Finance", "bi-graph-up-arrow"),
            new("financial-audit-trail", "Financial Audit Trail", "Finance", "bi-shield-check")
        };

        public static IEnumerable<ModuleDef> CustomizableStaffModules =>
            AllBusinessModules.Where(m => !RestrictedStaffModules.Contains(m.Key));

        // Default baseline modules per role (restricted modules excluded for staff)
        private static readonly Dictionary<string, HashSet<string>> _defaultRolePermissions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["BranchManager"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "dashboard", "consolidated-reports",
                "farms-plots", "crop-planning",
                "inventory", "stock-movements", "suppliers",
                "workers", "work-assignments", "attendance-logs", "harvest-recording",
                "new-sale", "sales-transactions", "customers", "payments-collections", "cash-drawer",
                "expenses"
            },
            ["InventoryOfficer"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "dashboard",
                "inventory", "stock-movements", "suppliers"
            },
            ["LaborOfficer"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "dashboard",
                "workers", "work-assignments", "attendance-logs", "harvest-recording"
            },
            ["SalesOfficer"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "dashboard",
                "new-sale", "sales-transactions", "customers", "payments-collections", "cash-drawer"
            },
            ["Accountant"] = new(StringComparer.OrdinalIgnoreCase)
            {
                "dashboard",
                "stock-movements",
                "attendance-logs",
                "sales-transactions", "payments-collections",
                "expenses", "production-costing", "financial-reports", "financial-audit-trail"
            }
        };

        private readonly Dictionary<string, HashSet<string>> _rolePermissions;
        private readonly Dictionary<long, HashSet<string>> _staffPermissions = new();

        public RolePermissionService()
        {
            _rolePermissions = _defaultRolePermissions.ToDictionary(
                kvp => kvp.Key,
                kvp => SanitizeStaffModules(kvp.Value),
                StringComparer.OrdinalIgnoreCase
            );
        }

        public event Action? OnPermissionsChanged;

        private static HashSet<string> SanitizeStaffModules(IEnumerable<string> moduleKeys) =>
            moduleKeys
                .Where(k => !string.IsNullOrWhiteSpace(k) && !RestrictedStaffModules.Contains(k))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> GetDefaultModules(string roleKey)
        {
            if (_defaultRolePermissions.TryGetValue(roleKey, out var set))
            {
                return SanitizeStaffModules(set);
            }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public HashSet<string> GetAllowedModules(string roleKey)
        {
            if (_rolePermissions.TryGetValue(roleKey, out var set))
            {
                return SanitizeStaffModules(set);
            }
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void SaveRolePermissions(string roleKey, IEnumerable<string> moduleKeys)
        {
            _rolePermissions[roleKey] = SanitizeStaffModules(moduleKeys);
            OnPermissionsChanged?.Invoke();
        }

        public HashSet<string> GetStaffAllowedModules(long userId, string roleKey)
        {
            if (_staffPermissions.TryGetValue(userId, out var customSet))
            {
                return SanitizeStaffModules(customSet);
            }
            return GetAllowedModules(roleKey);
        }

        public void SaveStaffPermissions(long userId, IEnumerable<string> moduleKeys)
        {
            _staffPermissions[userId] = SanitizeStaffModules(moduleKeys);
            OnPermissionsChanged?.Invoke();
        }

        public void ResetStaffPermissions(long userId, string roleKey)
        {
            _staffPermissions.Remove(userId);
            OnPermissionsChanged?.Invoke();
        }

        public bool HasAccess(string roleKey, string moduleKey, long? userId = null)
        {
            if (string.Equals(roleKey, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(roleKey, "TenantAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (RestrictedStaffModules.Contains(moduleKey))
            {
                return false;
            }

            if (userId.HasValue && _staffPermissions.TryGetValue(userId.Value, out var staffSet))
            {
                return staffSet.Contains(moduleKey);
            }

            return _rolePermissions.TryGetValue(roleKey, out var roleSet) && roleSet.Contains(moduleKey);
        }
    }
}