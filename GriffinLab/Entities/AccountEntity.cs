﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GriffinLab.Entities
{
    public class AccountEntity
    {
        /// <summary>
        ///     Maximum number of password attempts before account becomes locked.
        /// </summary>
        public const int MaxPasswordAttempts = 3;

        /// <summary>
        ///     Create a new instance of <see cref="Account" />-
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">password</param>
        public AccountEntity(string userName, string password)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            if (password == null) throw new ArgumentNullException("password");

            UserName = userName;
            CreatedAtUtc = DateTime.UtcNow;
            ActivationKey = Guid.NewGuid().ToString("N");
            AccountState = AccountState.VerificationRequired;
            HashedPassword = HashNewPassword(password);
        }

        /// <summary>
        ///     Serialization constructor
        /// </summary>
        protected AccountEntity()
        {
        }

        /// <summary>
        ///     Current state
        /// </summary>
        public AccountState AccountState { get; private set; }

        /// <summary>
        ///     Used to verify the mail address (if verifiaction is activated)
        /// </summary>
        public string ActivationKey { get; private set; }

        /// <summary>
        ///     When this account was created.
        /// </summary>
        public DateTime CreatedAtUtc { get; private set; }

        /// <summary>
        ///     Private setter since new emails needs to be verifier (verification email with a link)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Password salted and hashed.
        /// </summary>
        public string HashedPassword { get; private set; }


        /// <summary>
        ///     Primary key
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int Id { get; private set; }

        /// <summary>
        ///     When last successful login attempt was made.
        /// </summary>
        public DateTime LastLoginAtUtc { get; private set; }

        /// <summary>
        ///     Number of failed login attempts (reseted on each successfull login attempt).
        /// </summary>
        public int LoginAttempts { get; private set; }

        /// <summary>
        ///     Password salt.
        /// </summary>
        public string Salt { get; private set; }


        /// <summary>
        ///     Last time a property was updated.
        /// </summary>
        public DateTime UpdatedAtUtc { get; private set; }

        /// <summary>
        ///     Username
        /// </summary>
        public string UserName { get; private set; }

        public string KimDummy { get; set; }

        /// <summary>
        ///     Activate account (i.e. allow logins).
        /// </summary>
        public void Activate()
        {
            AccountState = AccountState.Active;
            ActivationKey = null;
            UpdatedAtUtc = DateTime.UtcNow;
            LoginAttempts = 0;
            LastLoginAtUtc = DateTime.UtcNow;
        }

        /// <summary>
        ///     Change password
        /// </summary>
        /// <param name="newPassword">New password as entered by the user.</param>
        public void ChangePassword(string newPassword)
        {
            if (newPassword == null) throw new ArgumentNullException("newPassword");
            HashedPassword = HashNewPassword(newPassword);
            ActivationKey = null;
            UpdatedAtUtc = DateTime.UtcNow;
            AccountState = AccountState.Active;
            LoginAttempts = 0;
        }

        /// <summary>
        ///     Login
        /// </summary>
        /// <param name="password">Password as specified by the user</param>
        /// <returns><c>true</c> if password was the correct one; otherwise <c>false</c>.</returns>
        /// <exception cref="AuthenticationException">Account is not active, or too many failed login attempts.</exception>
        public bool Login(string password)
        {
            if (AccountState == AccountState.VerificationRequired)
                throw new AuthenticationException("You have to activate your account first. Check your email.");

            if (AccountState == AccountState.Locked)
                throw new AuthenticationException("Your account has been locked. Contact support.");

            // null for cookie logins.
            if (password == null)
            {
                LastLoginAtUtc = DateTime.UtcNow;
                LoginAttempts = 0;
                return true;
            }

            var validPw = ValidatePassword(password);
            if (validPw)
            {
                LastLoginAtUtc = DateTime.UtcNow;
                LoginAttempts = 0;
                return true;
            }
            LoginAttempts++;

            //need to have it at the bottom too so that we can throw on the failed max attempt.
            if (LoginAttempts >= MaxPasswordAttempts)
            {
                AccountState = AccountState.Locked;
                throw new AuthenticationException("Too many login attempts.");
            }
            return false;
        }

        /// <summary>
        ///     Want to reset password.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Changes user state to <see cref="Accounts.AccountState.ResetPassword" /> and generates a new
        ///         <see cref="ActivationKey" />.
        ///     </para>
        /// </remarks>
        public void RequestPasswordReset()
        {
            AccountState = AccountState.ResetPassword;
            ActivationKey = Guid.NewGuid().ToString("N");
        }


        /// <summary>
        ///     Email has been verified.
        /// </summary>
        /// <param name="email">Email address</param>
        public void SetVerifiedEmail(string email)
        {
            if (email == null) throw new ArgumentNullException("email");
            Email = email;
        }

        /// <summary>
        ///     Check if the given password is the current one.
        /// </summary>
        /// <param name="enteredPassword">Password as entered by the user.</param>
        /// <returns><c>true</c> if the password is the same as the current one; otherwise false.</returns>
        public bool ValidatePassword(string enteredPassword)
        {
            if (enteredPassword == null) throw new ArgumentNullException("enteredPassword");
            var salt = Convert.FromBase64String(Salt);
            var algorithm2 = new Rfc2898DeriveBytes(enteredPassword, salt);
            var pw = algorithm2.GetBytes(128);

            var hashedPw = Convert.ToBase64String(pw);
            return hashedPw == HashedPassword;
        }


        /// <summary>
        ///     Hash password and generate a new salt.
        /// </summary>
        /// <param name="password">Password as entered by the user</param>
        /// <returns>Salted and hashed password</returns>
        private string HashNewPassword(string password)
        {
            if (password == null) throw new ArgumentNullException("password");
            var algorithm2 = new Rfc2898DeriveBytes(password, 64);
            var salt = algorithm2.Salt;
            Salt = Convert.ToBase64String(salt);
            var pw = algorithm2.GetBytes(128);
            return Convert.ToBase64String(pw);
        }
    }

    public enum AccountState
    {
        /// <summary>
        ///     Account have been created but not yet verified.
        /// </summary>
        VerificationRequired,

        /// <summary>
        ///     Account is active
        /// </summary>
        Active,

        /// <summary>
        ///     Account have been locked, typically by too many login attempts.
        /// </summary>
        Locked,

        /// <summary>
        ///     Password reset have been requested (an password reset link have been sent).
        /// </summary>
        ResetPassword
    }
}
