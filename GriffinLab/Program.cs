using Griffin.Data;
using GriffinLab.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GriffinLab
{
    class Program
    {
        static IAdoNetUnitOfWork _uow;
        static void Main(string[] args)
        {
            Console.WriteLine("Begin");
            _uow = CreateConnection();


            Task.Run(async () =>
            {
                await AccountRepositoryLab();
                //  _uow.SaveChanges();
            }).GetAwaiter().GetResult();
            Console.ReadLine();
            Console.WriteLine("End");

        }

        private static async Task AccountRepositoryLab()
        {
            AccountRepository r = new AccountRepository(_uow);
            var account = await r.GetByIdAsync(1);
            Console.WriteLine($"GetByIdAsync:{ObjectToString(account)}");

            var newAccount = new AccountEntity("Kim", "kim123");
            newAccount.Activate();
            newAccount.Email = "Kimxinfo@gmail.com";
            await r.CreateAsync(newAccount);
            Console.WriteLine($"GetByIdAsync:{ObjectToString(newAccount)}");

            r.UpdateName(1, "Kim Update");
            account = await r.GetByIdAsync(1);
            Console.WriteLine($"UpdateName:{account.UserName}");

        }

        private static IAdoNetUnitOfWork CreateConnection()
        {
            var conStr = ConfigurationManager.ConnectionStrings["Db"];
            var connection = new SqlConnection(conStr.ConnectionString);
            connection.Open();
            return new AdoNetUnitOfWork(connection, true);
        }

        public static string ObjectToString<T>(T value)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.NullValueHandling = NullValueHandling.Ignore;//2015/05/04
                    serializer.Serialize(writer, value);
                }
                return sw.ToString();
            }

        }
    }
}
