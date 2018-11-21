using DiplomMSSQLApp.DAL.Interfaces;
using DiplomMSSQLApp.DAL.Repositories;
using Ninject.Modules;

namespace DiplomMSSQLApp.BLL.Infrastructure {
    public class ServiceModule : NinjectModule {
        private string connectionString;
        public ServiceModule(string connection) {
            connectionString = connection;
        }
        public override void Load() {
            Bind<IUnitOfWork>().To<EFUnitOfWork>().WithConstructorArgument(connectionString);
        }
    }
}
