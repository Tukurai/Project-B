using Depot.Common.Validation;
using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depot.Common.Workflow
{
    public class CreateUsersFlow : Workflow
    {
        public int Amount { get; set; }
        public List<User> Users { get; set; } = new List<User>();

        public CreateUsersFlow(DepotContext context) : base(context)
        {
        }

        public bool SetUserAmount(int? amount)
        {
            if(amount == null)
            {
                return false;
            }
            
            Amount = amount.Value;
            return true;
        }

        public bool AddUser(User user)
        {
            Users.Add(user);
            return true;
        }

        public override string Validate(out bool valid)
        {
            Context.Users.AddRange(Users);
            Context.SaveChanges();
            valid = true;

            string response = "";
            foreach (var user in Users)
            {
                response += $"{Localization.Aangemaakt}: {user.Role}, {user.Id}, {user.Name}.\n";
            }

            return response;
        }
    }
}
