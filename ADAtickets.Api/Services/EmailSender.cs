using Microsoft.AspNetCore.Identity;

namespace ADAtickets.Api.Services;

internal sealed class EmailSender : IEmailSender<IdentityUser<Guid>>
{
    public Task SendConfirmationLinkAsync(IdentityUser<Guid> user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetLinkAsync(IdentityUser<Guid> user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    public Task SendPasswordResetCodeAsync(IdentityUser<Guid> user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}