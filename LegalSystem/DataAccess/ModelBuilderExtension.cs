using LegalSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LegalSystem.DataAccess
{
    public static class ModelBuilderExtension
    {
        internal static ModelBuilder ConfigureWorkflow(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TblWorkFlow>()
                .HasKey(c => new { c.CaseTypeId, c.CaseStatusId });

            modelBuilder.Entity<TblWorkFlow>()
                .HasOne(c => c.CaseType)
                .WithMany(c => c.WorkflowSteps)
                .HasForeignKey(c => c.CaseTypeId);

            modelBuilder.Entity<TblWorkFlow>()
                .HasOne(c => c.CaseStatus)
                .WithMany(a => a.CaseTypes)
                .HasForeignKey(c => c.CaseStatusId);

            return modelBuilder;
        }
        //internal static ModelBuilder Seed(this ModelBuilder modelBuilder)
        //{
        //    SeedSuperAdmin(modelBuilder);

        //    return modelBuilder;
        //}
        //private static void SeedSuperAdmin(ModelBuilder modelBuilder)
        //{
        //    long superAdminId = 9999;
        //    string dateString = "01-01-2025";
        //    DateTime date = DateTime.ParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

        //    modelBuilder.Entity<TblUser>().HasData(
        //                new TblUser()
        //                {
        //                    Id = superAdminId,
        //                    RefId = 1,
        //                    NameEn = "Super Admin",
        //                    NameAr = "مدير النظام",
        //                    TypeId = 4,
        //                    CreatedById = superAdminId,
        //                    CreatedByName = "support@tigergroup.ae",

        //                }
        //                );

        //}
    }
}
