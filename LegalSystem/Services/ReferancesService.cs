using LegalSystem.DataAccess;
using LegalSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LegalSystem.Services
{
    public class ReferancesService
    {
        private readonly UnitOfWork unitOfWork;
        public ReferancesService(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<RefefancesQuery> GetAllReferancesAsync()
        {
            var data = new RefefancesQuery();

            data.CaseTypes = await unitOfWork.RefCaseTypeRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive==true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.CaseLevels = await unitOfWork.RefCaseLevelRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.CaseStatuses = await unitOfWork.RefCaseStatusRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.Courts = await unitOfWork.RefCourtRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.ReliefSoughts = await unitOfWork.RefReliefSoughtRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.UserTypes = await unitOfWork.RefUserTypeRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.TaskPriorities = await unitOfWork.RefTaskPriorityRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.TaskStatuses = await unitOfWork.RefTaskStatusRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.DocumentClassifications = await unitOfWork.RefDocumentClassificationRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.EventTypes = await unitOfWork.RefEventTypeRepository.GetAllQuerable()
                .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
                {
                    Id = e.Id,
                    NameEN = e.NameEN,
                    NameAR = e.NameAR,
                    Order = e.Order,
                    IsActive = e.IsActive,
                }).ToListAsync();

            data.Reasons = await unitOfWork.ReasonRepository.GetAllQuerable()
               .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
               {
                   Id = e.Id,
                   NameEN = e.NameEN,
                   NameAR = e.NameAR,
                   Order = e.Order,
                   IsActive = e.IsActive,
               }).ToListAsync();

            data.TaskTypes = await unitOfWork.RefTaskTypeRepository.GetAllQuerable()
              .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
              {
                  Id = e.Id,
                  NameEN = e.NameEN,
                  NameAR = e.NameAR,
                  Order = e.Order,
                  IsActive = e.IsActive,
              }).ToListAsync();


            data.Companies = await unitOfWork.RefCompanyRepository.GetAllQuerable()
             .AsNoTracking().Where(e => e.IsActive == true).Select(e => new BaseRefQuery
             {
                 Id = e.Id,
                 NameEN = e.NameEN,
                 NameAR = e.NameAR,
                 Order = e.Order,
                 IsActive = e.IsActive,
             }).ToListAsync();
            return data;
        }
    }
}
