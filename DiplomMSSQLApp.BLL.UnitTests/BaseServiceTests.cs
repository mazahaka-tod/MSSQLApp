﻿using DiplomMSSQLApp.DAL.Interfaces;
using System.Threading.Tasks;

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

        public abstract Task DeleteAsync_FindByIdAsyncMethodReturnsNull_RemoveMethodIsNeverCalled();
        public abstract Task DeleteAsync_FindByIdAsyncMethodReturnsNull_SaveAsyncMethodIsNeverCalled();
        public abstract Task DeleteAsync_FindByIdAsyncMethodReturnsObject_RemoveMethodIsCalledOnce();
        public abstract Task DeleteAsync_FindByIdAsyncMethodReturnsObject_SaveAsyncMethodIsCalledOnce();

        public abstract Task DeleteAllAsync_Calls_SaveAsyncMethodIsCalledOnce();

        public abstract Task EditAsync_CallsWithGoodParams_CallsUpdateMethodOnсe();
        public abstract Task EditAsync_CallsWithGoodParams_CallsSaveAsyncMethodOnсe();

        public abstract Task ExportJsonAsync_CreatesJsonFile();

        public abstract void FindByIdAsync_IdParameterIsNull_Throws();
        public abstract Task FindByIdAsync_IdEqualTo2_ReturnsObjectWithIdEqualTo2();

        public abstract Task GetAllAsync_GetAsyncMethodReturnsArray_ReturnsSameArray();
    }
}
