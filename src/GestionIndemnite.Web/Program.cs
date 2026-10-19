using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Infrastructure.RHOrganisation.Files;
using GestionIndemnite.Infrastructure.RHOrganisation.Persistence;
using GestionIndemnite.Web.Logging;
using GestionIndemnite.Web.RHOrganisation.CurrentUser;
using GestionIndemnite.Web.RHOrganisation.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<RegleMetierExceptionFilter>();

builder.Services.AddControllersWithViews(options =>
    options.Filters.AddService<RegleMetierExceptionFilter>());

builder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();

builder.Services.AddDbContext<RHOrganisationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RHOrganisation")));

builder.Services.AddScoped<IRHOrganisationDbContext>(sp =>
    sp.GetRequiredService<RHOrganisationDbContext>());

builder.Services.AddScoped<CadreService>();
builder.Services.AddScoped<GradeService>();
builder.Services.AddScoped<ResponsabiliteService>();
builder.Services.AddScoped<StatutAdministratifService>();
builder.Services.AddScoped<PaysService>();
builder.Services.AddScoped<VilleService>();

var filesRoot = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "files");
builder.Services.AddSingleton<IFileStorage>(
    sp => new FileStorageService(filesRoot, sp.GetRequiredService<ILogger<FileStorageService>>()));

builder.Services.AddScoped<FonctionnaireService>();
builder.Services.AddScoped<RibService>();
builder.Services.AddScoped<PieceJointeService>();
builder.Services.AddScoped<TribunalService>();
builder.Services.AddScoped<UniteOrganisationnelleService>();
builder.Services.AddScoped<PeriodeService>();
builder.Services.AddScoped<HistoriqueCadreGradeService>();
builder.Services.AddScoped<SituationAdministrativeService>();
builder.Services.AddScoped<HistoriqueAffectationService>();
builder.Services.AddScoped<HistoriqueResponsabiliteService>();
builder.Services.AddScoped<HistoriqueOrganigrammeTribunalService>();
builder.Services.AddScoped<HistoriqueOrganigrammeUniteService>();
builder.Services.AddScoped<RapportService>();
builder.Services.AddScoped<ImportService>();

builder.Logging.ClearProviders();
builder.Logging.AddProvider(
    new AsyncFileLoggerProvider(
        Path.Combine(builder.Environment.ContentRootPath, "logs")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
