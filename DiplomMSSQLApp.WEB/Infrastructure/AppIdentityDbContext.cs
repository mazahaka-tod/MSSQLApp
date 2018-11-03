using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace DiplomMSSQLApp.WEB.Infrastructure {
    public class AppIdentityDbContext : IdentityDbContext<AppUser> {
        public AppIdentityDbContext() : base("IdentityDb") { }

        static AppIdentityDbContext() {
            Database.SetInitializer(new IdentityDbInit());
        }

        public static AppIdentityDbContext Create() {
            return new AppIdentityDbContext();
        }
    }

    public class IdentityDbInit : DropCreateDatabaseIfModelChanges<AppIdentityDbContext> {

        protected override void Seed(AppIdentityDbContext context) {
            PerformInitialSetup(context);
            base.Seed(context);
        }

        public void PerformInitialSetup(AppIdentityDbContext context) {
            // initial configuration will go here
        }
    }
}
