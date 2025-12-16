using Microsoft.AspNetCore.Identity;

namespace ADAtickets.Api.Services;

/// <summary>
///     Implements email sending functionality for identity users.
/// </summary>
internal sealed class EmailSender : IEmailSender<IdentityUser<Guid>>
{
    /// <summary>
    ///     Sends the email confirmation link for a user.
    /// </summary>
    /// <param name="user">The user to confirm.</param>
    /// <param name="email">The email to send the message to.</param>
    /// <param name="confirmationLink">The confirmation link.</param>
    /// <returns>A <see cref="Task"/> that indicates whether the operation has completed.</returns>
    public Task SendConfirmationLinkAsync(IdentityUser<Guid> user, string email, string confirmationLink)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sends the password reset link for a user.
    /// </summary>
    /// <param name="user">The user that wants to reset the password.</param>
    /// <param name="email">The email to send the message to.</param>
    /// <param name="resetLink">The reset link.</param>
    /// <returns>A <see cref="Task"/> that indicates whether the operation has completed.</returns>
    public Task SendPasswordResetLinkAsync(IdentityUser<Guid> user, string email, string resetLink)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sends the password reset code for a user.
    /// </summary>
    /// <param name="user">The user that wants to reset the password.</param>
    /// <param name="email">The email to send the message to.</param>
    /// <param name="resetCode">The reset code.</param>
    /// <returns>A <see cref="Task"/> that indicates whether the operation has completed.</returns>
    public Task SendPasswordResetCodeAsync(IdentityUser<Guid> user, string email, string resetCode)
    {
        throw new NotImplementedException();
    }
}