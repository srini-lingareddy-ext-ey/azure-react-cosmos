namespace Todo.Api.Api.Authorization;

/// <summary>
/// Marks a controller or action as requiring <c>X-Tenant-Id</c> and a valid user role assignment for that tenant (WO-6).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class RequireTenantContextAttribute : Attribute { }
