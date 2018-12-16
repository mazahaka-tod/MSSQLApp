using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using Ninject;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Util {
    public class NinjectDependencyResolver : IDependencyResolver {
        private IKernel kernel;
        public NinjectDependencyResolver(IKernel kernelParam) {
            kernel = kernelParam;
            AddBindings();
        }
        public object GetService(Type serviceType) {
            return kernel.TryGet(serviceType);
        }
        public IEnumerable<object> GetServices(Type serviceType) {
            return kernel.GetAll(serviceType);
        }
        private void AddBindings() {
            kernel.Bind<IService<AnnualLeaveDTO>>().To<AnnualLeaveService>();
            kernel.Bind<IService<BusinessTripDTO>>().To<BusinessTripService>();
            kernel.Bind<IService<DepartmentDTO>>().To<DepartmentService>();
            kernel.Bind<IService<EmployeeDTO>>().To<EmployeeService>();
            kernel.Bind<IService<LeaveScheduleDTO>>().To<LeaveScheduleService>();
            kernel.Bind<IService<OrganizationDTO>>().To<OrganizationService>();
            kernel.Bind<IService<PostDTO>>().To<PostService>();
        }
    }
}
