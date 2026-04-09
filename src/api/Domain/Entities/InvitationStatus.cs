namespace Todo.Api.Domain.Entities;

/// <summary>WO-11: lifecycle for <see cref="UserInvitation"/>.</summary>
public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Expired = 2,
}
