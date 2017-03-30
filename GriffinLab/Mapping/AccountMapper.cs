using Griffin.Data.Mapper;
using GriffinLab.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GriffinLab.Mapping
{
    public class AccountMapper : CrudEntityMapper<AccountEntity>
    {
        public AccountMapper() : base("Accounts")
        {
            Property(x => x.Id)
                .PrimaryKey(true);

            Property(x => x.AccountState)
                .ToPropertyValue(o => (AccountState)Enum.Parse(typeof(AccountState), (string)o, true))
                .ToColumnValue(o => o.ToString());
            Property(x => x.KimDummy).NotForCrud().NotForQueries();

   
        }
    }
}
