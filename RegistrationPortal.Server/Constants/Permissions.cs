namespace RegistrationPortal.Server.Constants;

public static class Permissions
{
    // Customer Management Permissions
    public static class Customers
    {
        public const string Create = "customers.create";
        public const string Read = "customers.read";
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
        public const string Read = "accounts.read";
        public const string Update = "accounts.update";
        public const string Delete = "accounts.delete";
        public const string List = "accounts.list";
        public const string ViewDetails = "accounts.view_details";
        public const string Activate = "accounts.activate";
        public const string Deactivate = "accounts.deactivate";
        public const string Freeze = "accounts.freeze";
        public const string Unfreeze = "accounts.unfreeze";
    }

    // Document Management Permissions
    public static class Documents
    {
        public const string Upload = "documents.upload";
        public const string Download = "documents.download";
        public const string View = "documents.view";
        public const string Delete = "documents.delete";
        public const string Verify = "documents.verify";
        public const string Reject = "documents.reject";
        public const string List = "documents.list";
    }

    // User Management Permissions
    public static class Users
    {
        public const string Create = "users.create";
        public const string Read = "users.read";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string List = "users.list";
        public const string ViewDetails = "users.view_details";
        public const string Activate = "users.activate";
        public const string Deactivate = "users.deactivate";
        public const string ResetPassword = "users.reset_password";
        public const string AssignRoles = "users.assign_roles";
    }

    // Role Management Permissions
    public static class Roles
    {
        public const string Create = "roles.create";
        public const string Read = "roles.read";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string List = "roles.list";
        public const string ViewDetails = "roles.view_details";
        public const string AssignPermissions = "roles.assign_permissions";
    }

    // System Administration Permissions
    public static class System
    {
        public const string ViewLogs = "system.view_logs";
        public const string ManageSettings = "system.manage_settings";
        public const string Backup = "system.backup";
        public const string Restore = "system.restore";
        public const string Monitor = "system.monitor";
        public const string Maintenance = "system.maintenance";
    }

    // Reporting Permissions
    public static class Reports
    {
        public const string Generate = "reports.generate";
        public const string View = "reports.view";
        public const string Export = "reports.export";
        public const string Schedule = "reports.schedule";
        public const string Manage = "reports.manage";
    }

    // Audit Permissions
    public static class Audit
    {
        public const string View = "audit.view";
        public const string Export = "audit.export";
        public const string Search = "audit.search";
    }

    // Dashboard Permissions
    public static class Dashboard
    {
        public const string View = "dashboard.view";
        public const string Admin = "dashboard.admin";
        public const string Analytics = "dashboard.analytics";
    }

    // Get all permissions as a list
    public static readonly List<string> All = new()
    {
        // Customer permissions
        Customers.Create, Customers.Read, Customers.Update, Customers.Delete,
        Customers.List, Customers.ViewDetails, Customers.Approve, Customers.Reject,
        Customers.Review,

        // Account permissions
        Accounts.Create, Accounts.Read, Accounts.Update, Accounts.Delete,
        Accounts.List, Accounts.ViewDetails, Accounts.Activate, Accounts.Deactivate,
        Accounts.Freeze, Accounts.Unfreeze,

        // Document permissions
        Documents.Upload, Documents.Download, Documents.View, Documents.Delete,
        Documents.Verify, Documents.Reject, Documents.List,

        // User permissions
        Users.Create, Users.Read, Users.Update, Users.Delete,
        Users.List, Users.ViewDetails, Users.Activate, Users.Deactivate,
        Users.ResetPassword, Users.AssignRoles,

        // Role permissions
        Roles.Create, Roles.Read, Roles.Update, Roles.Delete,
        Roles.List, Roles.ViewDetails, Roles.AssignPermissions,

        // System permissions
        System.ViewLogs, System.ManageSettings, System.Backup, System.Restore,
        System.Monitor, System.Maintenance,

        // Report permissions
        Reports.Generate, Reports.View, Reports.Export, Reports.Schedule, Reports.Manage,

        // Audit permissions
        Audit.View, Audit.Export, Audit.Search,

        // Dashboard permissions
        Dashboard.View, Dashboard.Admin, Dashboard.Analytics
    };

    // Get permissions by module
    public static readonly Dictionary<string, List<string>> ByModule = new()
    {
        ["Customers"] = new List<string>
        {
            Customers.Create, Customers.Read, Customers.Update, Customers.Delete,
            Customers.List, Customers.ViewDetails, Customers.Approve, Customers.Reject,
            Customers.Review
        },
        ["Accounts"] = new List<string>
        {
            Accounts.Create, Accounts.Read, Accounts.Update, Accounts.Delete,
            Accounts.List, Accounts.ViewDetails, Accounts.Activate, Accounts.Deactivate,
            Accounts.Freeze, Accounts.Unfreeze
        },
        ["Documents"] = new List<string>
        {
            Documents.Upload, Documents.Download, Documents.View, Documents.Delete,
            Documents.Verify, Documents.Reject, Documents.List
        },
        ["Users"] = new List<string>
        {
            Users.Create, Users.Read, Users.Update, Users.Delete,
            Users.List, Users.ViewDetails, Users.Activate, Users.Deactivate,
            Users.ResetPassword, Users.AssignRoles
        },
        ["Roles"] = new List<string>
        {
            Roles.Create, Roles.Read, Roles.Update, Roles.Delete,
            Roles.List, Roles.ViewDetails, Roles.AssignPermissions
        },
        ["System"] = new List<string>
        {
            System.ViewLogs, System.ManageSettings, System.Backup, System.Restore,
            System.Monitor, System.Maintenance
        },
        ["Reports"] = new List<string>
        {
            Reports.Generate, Reports.View, Reports.Export, Reports.Schedule, Reports.Manage
        },
        ["Audit"] = new List<string>
        {
            Audit.View, Audit.Export, Audit.Search
        },
        ["Dashboard"] = new List<string>
        {
            Dashboard.View, Dashboard.Admin, Dashboard.Analytics
        }
    };
}
