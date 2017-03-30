using Griffin;
using Griffin.Data;
using Griffin.Data.Mapper;
using GriffinLab.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GriffinLab
{
    public class AccountRepository
    {
        private IAdoNetUnitOfWork _uow;

        public AccountRepository(IAdoNetUnitOfWork uow)
        {
            if (uow == null) throw new ArgumentNullException("uow");
            _uow = uow;
        }

        public void Create(AccountEntity account)
        {

            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText =
                    "INSERT INTO Accounts (Id, Username, HashedPassword, Salt, CreatedAtUtc, AccountState, Email, UpdatedAtUtc, ActivationKey, LoginAttempts, LastLoginAtUtc) " +
                    " VALUES(@Id, @Username, @HashedPassword, @Salt, @CreatedAtUtc, @AccountState, @Email, @UpdatedAtUtc, @ActivationKey, @LoginAttempts, @LastLoginAtUtc)";
                cmd.AddParameter("@Id", account.Id);
                cmd.AddParameter("@Username", account.UserName);
                cmd.AddParameter("@HashedPassword", account.HashedPassword);
                cmd.AddParameter("@Salt", account.Salt);
                cmd.AddParameter("@CreatedAtUtc", account.CreatedAtUtc);
                cmd.AddParameter("@AccountState", account.AccountState.ToString());
                cmd.AddParameter("@Email", account.Email);
                cmd.AddParameter("@UpdatedAtUtc", account.UpdatedAtUtc == DateTime.MinValue ? (object)null : account.UpdatedAtUtc);
                cmd.AddParameter("@ActivationKey", account.ActivationKey);
                cmd.AddParameter("@LoginAttempts", account.LoginAttempts);
                cmd.AddParameter("@LastLoginAtUtc", account.LastLoginAtUtc == DateTime.MinValue ? (object)null : account.LastLoginAtUtc);
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateName(int id, string name)
        {
            using (var cmd = _uow.CreateCommand())
            {
                cmd.ExecuteNonQuery("update accounts set UserName=@UserName where Id=@Id", new { UserName = name, Id = id });
            }
        }

        public async Task CreateAsync(AccountEntity account)
        {
            await _uow.InsertAsync(account);
        }

        public async Task<AccountEntity> FindByActivationKeyAsync(string activationKey)
        {
            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts WHERE ActivationKey=@key";
                cmd.AddParameter("key", activationKey);
                return await cmd.FirstOrDefaultAsync<AccountEntity>();
            }
        }

        public async Task UpdateAsync(AccountEntity account)
        {
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText =
                    "UPDATE Accounts SET " +
                    " Username = @Username, " +
                    " HashedPassword = @HashedPassword, " +
                    " Salt = @Salt, " +
                    " CreatedAtUtc = @CreatedAtUtc, " +
                    " AccountState = @AccountState, " +
                    " Email = @Email, " +
                    " UpdatedAtUtc = @UpdatedAtUtc, " +
                    " ActivationKey = @ActivationKey, " +
                    " LoginAttempts = @LoginAttempts, " +
                    " LastLoginAtUtc = @LastLoginAtUtc " +
                    "WHERE Id = @Id";
                cmd.AddParameter("@Id", account.Id);
                cmd.AddParameter("@Username", account.UserName);
                cmd.AddParameter("@HashedPassword", account.HashedPassword);
                cmd.AddParameter("@Salt", account.Salt);
                cmd.AddParameter("@CreatedAtUtc", account.CreatedAtUtc);
                cmd.AddParameter("@AccountState", account.AccountState.ToString());
                cmd.AddParameter("@Email", account.Email);
                cmd.AddParameter("@UpdatedAtUtc", account.UpdatedAtUtc == DateTime.MinValue ? (object)null : account.UpdatedAtUtc);
                cmd.AddParameter("@ActivationKey", account.ActivationKey);
                cmd.AddParameter("@LoginAttempts", account.LoginAttempts);
                cmd.AddParameter("@LastLoginAtUtc", account.LastLoginAtUtc == DateTime.MinValue ? (object)null : account.LastLoginAtUtc);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public AccountEntity GetByUserName(string userName)
        {
            return _uow.First<AccountEntity>(new { UserName = userName });
        }

        public async Task<AccountEntity> FindByUserNameAsync(string userName)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText = "SELECT TOP 1 * FROM Accounts WHERE UserName=@uname";
                cmd.AddParameter("uname", userName);
                return await cmd.FirstOrDefaultAsync<AccountEntity>();
            }
        }

        public async Task<AccountEntity> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentNullException("id");
            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts WHERE Id=@id";
                cmd.AddParameter("id", id);
                return await cmd.FirstAsync<AccountEntity>();
            }
        }

        public async Task<AccountEntity> FindByEmailAsync(string emailAddress)
        {
            if (emailAddress == null) throw new ArgumentNullException("emailAddress");
            using (var cmd = _uow.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts WHERE Email=@email";
                cmd.AddParameter("email", emailAddress);
                return await cmd.FirstOrDefaultAsync<AccountEntity>();
            }
        }

        public async Task<IEnumerable<AccountEntity>> GetByIdAsync(int[] ids)
        {
            using (var cmd = (DbCommand)_uow.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM Accounts WHERE Id IN (@ids)";
                cmd.AddParameter("ids", string.Join(",", ids.Select(x => "'" + x + "'")));
                return await cmd.ToListAsync<AccountEntity>();
            }
        }



        public async Task<bool> IsEmailAddressTakenAsync(string email)
        {
            if (email == null) throw new ArgumentNullException("email");
            using (var cmd = _uow.CreateDbCommand())
            {
                cmd.CommandText = "SELECT TOP 1 Email FROM Accounts WHERE Email = @Email";
                cmd.AddParameter("Email", email);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value;
            }
        }


        public async Task<bool> IsUserNameTakenAsync(string userName)
        {
            if (userName == null) throw new ArgumentNullException("userName");
            using (var cmd = _uow.CreateDbCommand())
            {
                cmd.CommandText = "SELECT TOP 1 UserName FROM Accounts WHERE UserName = @userName";
                cmd.AddParameter("userName", userName);
                var result = await cmd.ExecuteScalarAsync();
                return result != null && result != DBNull.Value;
            }
        }
    }

    public static class MyExtenstion
    {
        public static void ExecuteNonQuery(this IDbCommand cmd, string sql, object parameters = null)
        {
            if (cmd == null) throw new ArgumentNullException(",");
            try
            {
                cmd.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var kvp in parameters.ToDictionary())
                    {
                        cmd.AddParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                    }
                }
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw cmd.CreateDataException(e);
            }
        }


    }
}
