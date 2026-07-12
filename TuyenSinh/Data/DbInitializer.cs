using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TuyenSinh.Models;

namespace TuyenSinh.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // 1. Seed Role
                var roleName = "Admin";
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }

                // 2. Seed User
                var adminEmail = "admin@tuyensinh.edu.vn";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(user, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, roleName);
                    }
                }


                // 3. Seed Subjects
                var subjectsData = new List<(string Name, string Field)>
                {
                    ("Toán", "Toan"),
                    ("Vật lý", "VatLy"),
                    ("Hóa học", "HoaHoc"),
                    ("Ngoại ngữ", "NgoaiNgu"),
                    ("Sinh học", "SinhHoc"),
                    ("Ngữ văn", "Van"),
                    ("Địa lý", "DiaLy"),
                    ("Tin học", "TinHoc"),
                    ("Công nghệ công nghiệp", "CNCN")
                };

                var subjects = new Dictionary<string, MonHoc>();
                foreach (var s in subjectsData)
                {
                    var existing = await context.MonHocs.FirstOrDefaultAsync(m => m.TenMonHoc == s.Name || m.FieldName == s.Field);
                    if (existing == null)
                    {
                        existing = new MonHoc { TenMonHoc = s.Name, FieldName = s.Field };
                        context.MonHocs.Add(existing);
                        await context.SaveChangesAsync();
                    }
                    subjects[s.Name] = existing;
                }

                // 4. Seed Combinations
                var combosData = new List<(string Code, string Name, string[] SubjectNames)>
                {
                    ("A00", "Toán, Vật lý, Hóa học", new[] { "Toán", "Vật lý", "Hóa học" }),
                    ("A01", "Toán, Vật lý, Ngoại ngữ", new[] { "Toán", "Vật lý", "Ngoại ngữ" }),
                    ("B00", "Toán, Hóa học, Sinh học", new[] { "Toán", "Hóa học", "Sinh học" }),
                    ("C01", "Toán, Vật lý, Ngữ văn", new[] { "Toán", "Vật lý", "Ngữ văn" }),
                    ("C02", "Toán, Hóa học, Ngữ văn", new[] { "Toán", "Hóa học", "Ngữ văn" }),
                    ("C04", "Toán, Ngữ văn, Địa lý", new[] { "Toán", "Ngữ văn", "Địa lý" }),
                    ("D01", "Toán, Ngữ văn, Ngoại ngữ", new[] { "Toán", "Ngữ văn", "Ngoại ngữ" }),
                    ("D07", "Toán, Hóa học, Ngoại ngữ", new[] { "Toán", "Hóa học", "Ngoại ngữ" }),
                    ("X02", "Toán, Ngữ văn, Tin học", new[] { "Toán", "Ngữ văn", "Tin học" }),
                    ("X06", "Toán, Vật lý, Tin học", new[] { "Toán", "Vật lý", "Tin học" }),
                    ("X07", "Toán, Vật lý, Công nghệ công nghiệp", new[] { "Toán", "Vật lý", "Công nghệ công nghiệp" }),
                    ("X10", "Toán, Hóa học, Tin học", new[] { "Toán", "Hóa học", "Tin học" }),
                    ("X26", "Toán, Ngoại ngữ, Tin học", new[] { "Toán", "Ngoại ngữ", "Tin học" })
                };

                foreach (var c in combosData)
                {
                    var existingCombo = await context.ToHopMons.Include(t => t.MonHocs).FirstOrDefaultAsync(t => t.MaToHop == c.Code);
                    if (existingCombo == null)
                    {
                        existingCombo = new ToHopMon { MaToHop = c.Code, TenToHop = c.Name };
                        foreach (var subName in c.SubjectNames)
                        {
                            if (subjects.TryGetValue(subName, out var sub))
                            {
                                existingCombo.MonHocs.Add(sub);
                            }
                        }
                        context.ToHopMons.Add(existingCombo);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Skip seeding if database structure is not yet migrated/ready
            }
        }
    }
}
