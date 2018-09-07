using DiplomMSSQLApp.DAL.Interfaces;

namespace DiplomMSSQLApp.BLL.UnitTests
{
    public abstract class BaseServiceTests<T> where T : class
    {
        protected abstract T GetNewService();
        protected abstract T GetNewService(IUnitOfWork uow);

        public abstract void GetPage_CallsWithGoodParams_FillsPageInfoProperty();
        public abstract void GetPage_RequestedPageLessThan1_ReturnsFirstPage();
        public abstract void GetPage_RequestedPageMoreThanTotalPages_ReturnsLastPage();
        public abstract void GetPage_CallsExistingPage_ReturnsSpecifiedPage();
    }
}