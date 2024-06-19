using System.Collections.ObjectModel;

namespace FoundationaLLM.Authorization.Models
{
    public static class RoleDefinitions
    {

        public static readonly ReadOnlyDictionary<string, RoleDefinition> All = new (
            new Dictionary<string, RoleDefinition>()
            {
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/17ca4b59-3aee-497d-b43b-95dd7d916f99",
                    new RoleDefinition
                    {
                        Name = "17ca4b59-3aee-497d-b43b-95dd7d916f99",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/17ca4b59-3aee-497d-b43b-95dd7d916f99",
                        DisplayName = "Role Based Access Control Administrator",
                        Description = "Manage access to FoundationaLLM resources by assigning roles using FoundationaLLM RBAC.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "FoundationaLLM.Authorization/roleAssignments/read",
                                    "FoundationaLLM.Authorization/roleAssignments/write",
                                    "FoundationaLLM.Authorization/roleAssignments/delete",
                                    "FoundationaLLM.Authorization/roleDefinitions/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
                    new RoleDefinition
                    {
                        Name = "00a53e72-f66e-4c03-8f81-7e885fd2eb35",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/00a53e72-f66e-4c03-8f81-7e885fd2eb35",
                        DisplayName = "Reader",
                        Description = "View all resources without the possiblity of making any changes.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*/read",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
                    new RoleDefinition
                    {
                        Name = "a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/a9f0020f-6e3a-49bf-8d1d-35fd53058edf",
                        DisplayName = "Contributor",
                        Description = "Full access to manage all resources without the possiblity of assigning roles in FoundationaLLM RBAC.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*",],
                                NotActions = [
                                    "FoundationaLLM.Authorization/*/write",
                                    "FoundationaLLM.Authorization/*/delete",],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
                    new RoleDefinition
                    {
                        Name = "fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/fb8e0fd0-f7e2-4957-89d6-19f44f7d6618",
                        DisplayName = "User Access Administrator",
                        Description = "Manage access to FoundationaLLM resources.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*/read",
                                    "FoundationaLLM.Authorization/*",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
                {
                    "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
                    new RoleDefinition
                    {
                        Name = "1301f8d4-3bea-4880-945f-315dbd2ddb46",
                        Type = "FoundationaLLM.Authorization/roleDefinitions",
                        ObjectId = "/providers/FoundationaLLM.Authorization/roleDefinitions/1301f8d4-3bea-4880-945f-315dbd2ddb46",
                        DisplayName = "Owner",
                        Description = "Full access to manage all resources, including the ability to assign roles in FoundationaLLM RBAC.",
                        AssignableScopes = [
                            "/",],
                        Permissions = [                            
                            new RoleDefinitionPermissions
                            {
                                Actions = [
                                    "*",],
                                NotActions = [],
                                DataActions = [],
                                NotDataActions = [],
                            },],
                        CreatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        UpdatedOn = DateTimeOffset.Parse("2024-03-07T00:00:00.0000000Z"),
                        CreatedBy = null,
                        UpdatedBy = null
                    }
                },
            });
    }
}
