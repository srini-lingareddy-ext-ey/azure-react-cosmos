using Todo.Api.Application.Transport;
using Todo.Api.Application.Validation;
using Todo.Api.Domain.Entities;
using Xunit;

namespace Todo.Api.Tests.Application;

public sealed class AddUserRequestValidatorTests
{
    private readonly AddUserRequestValidator _validator = new();

    [Fact]
    public void BothUserIdAndEmail_Invalid()
    {
        var result = _validator.Validate(new AddUserRequest
        {
            UserId = "u1",
            Email = "a@b.com",
            Role = UserRole.Admin,
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void NeitherUserIdNorEmail_Invalid()
    {
        var result = _validator.Validate(new AddUserRequest
        {
            Role = UserRole.Admin,
        });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ExactlyOneIdentifier_Valid()
    {
        var result = _validator.Validate(new AddUserRequest
        {
            UserId = "u1",
            Role = UserRole.Viewer,
        });

        Assert.True(result.IsValid);
    }
}
