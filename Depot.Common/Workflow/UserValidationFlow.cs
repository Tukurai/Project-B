using Depot.DAL;
using Depot.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Depot.Common.Workflow
{
    public class UserValidationFlow : Workflow
    {
        public User? User { get; set; }
        public Role AllowedRole { get; private set; }

        public UserValidationFlow(DepotContext context, long? userId, Role allowedRole) : base(context)
        {
            Context = context;
            User = context.Users.Find(userId);
            AllowedRole = allowedRole;
        }

        public override string Validate(out bool valid)
        {
            if (User == null)
            {
                valid = false;
                return Localization.Gebruiker_niet_gevonden;
            }

            if (User.Role < (int)AllowedRole)
            {
                valid = false;
                return Localization.Gebruiker_heeft_geen_toegang;
            }

            valid = true;
            return Localization.Gebruiker_gevalideerd;
        }

    }
}
