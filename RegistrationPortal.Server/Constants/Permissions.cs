namespace RegistrationPortal.Server.Constants;

public static class Permissions
{
    // Customer Management Permissions
    public static class Customers
    {
        public const string Create = "customers.create";
        public const string Update = "customers.update";
        public const string Delete = "customers.delete";
        public const string List = "customers.list";
        public const string ViewDetails = "customers.view_details";
        public const string Approve = "customers.approve";
        public const string Reject = "customers.reject";
        public const string Review = "customers.review";
    }

    // Account Management Permissions
    public static class Accounts
    {
        public const string Create = "accounts.create";
        public const string List = "accounts.list";
        public const string Update = "accounts.update";
        public const string Delete = "accounts.delete";
    }

    // User Management Permissions
    public static class Users
    {
        public const string Create = "users.create";
        public const string List = "users.list";
        public const string AssignRoles = "users.assign_roles";
        public const string ViewDetails = "users.view_details";
        public const string Update = "users.update";
    }

    // Role Management Permissions
    public static class Roles
    {
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string List = "roles.list";
        public const string ViewDetails = "roles.view_details";
        public const string AssignPermissions = "roles.assign_permissions";
    }

    // System Administration Permissions
    public static class System
    {
        public const string Maintenance = "system.maintenance";
    }

    // Reporting Permissions

    // Audit Permissions

    // Dashboard Permissions
    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }

    // Get all permissions as a list
    public static readonly List<string> All = new()
    {
        // Customer permissions
        Customers.Create, Customers.Update, Customers.Delete,
        Customers.List, Customers.ViewDetails,
        Customers.Review,

        // Account permissions
        Accounts.Create, Accounts.Update, Accounts.Delete,
        Accounts.List,


        // User permissions
        Users.Create, Users.Update,
        Users.List, Users.ViewDetails, Users.AssignRoles,

        // Role permissions
        Roles.Create, Roles.Update, Roles.Delete,
        Roles.List, Roles.ViewDetails, Roles.AssignPermissions,

        // System permissions
        System.Maintenance,



        // Dashboard permissions
        Dashboard.View
    };

    // Get permissions by module
    public static readonly Dictionary<string, List<string>> ByModule = new()
    {
        ["Customers"] = new List<string>
        {
            Customers.Create, Customers.Update, Customers.Delete,
            Customers.List, Customers.ViewDetails,
            Customers.Review
        },
        ["Accounts"] = new List<string>
        {
            Accounts.Create, Accounts.Update, Accounts.Delete,
            Accounts.List
        },
        ["Users"] = new List<string>
        {
            Users.Create, Users.Update,
            Users.List, Users.ViewDetails, Users.AssignRoles
        },
        ["Roles"] = new List<string>
        {
            Roles.Create, Roles.Update, Roles.Delete,
            Roles.List, Roles.ViewDetails, Roles.AssignPermissions
        },
        ["System"] = new List<string>
        {
            System.Maintenance
        },
        ["Dashboard"] = new List<string>
        {
            Dashboard.View
        }
    };
}
