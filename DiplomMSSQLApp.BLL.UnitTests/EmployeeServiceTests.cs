using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.DAL.Interfaces;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    [TestFixture]
    public class EmployeeServiceTests : BaseServiceTests<EmployeeService>
    {
        public override Task DeleteAllAsync_Calls_RemoveAllAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce()
        {
            throw new NotImplementedException();
        }

        public override Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe()
        {
            throw new NotImplementedException();
        }

        public override Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe()
        {
            throw new NotImplementedException();
        }

        public override Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2()
        {
            throw new NotImplementedException();
        }

        public override void FindByIdAsync_IdParameterIsNull_Throws()
        {
            throw new NotImplementedException();
        }

        public override Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray()
        {
            throw new NotImplementedException();
        }

        public override void GetPage_CallsExistingPage_ReturnsSpecifiedPage()
        {
            throw new NotImplementedException();
        }

        public override void GetPage_CallsWithGoodParams_FillsPageInfoProperty()
        {
            throw new NotImplementedException();
        }

        public override void GetPage_RequestedPageLessThan1_ReturnsFirstPage()
        {
            throw new NotImplementedException();
        }

        public override void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage()
        {
            throw new NotImplementedException();
        }

        protected override EmployeeService GetNewService()
        {
            throw new NotImplementedException();
        }

        protected override EmployeeService GetNewService(IUnitOfWork uow)
        {
            throw new NotImplementedException();
        }
    }
}
