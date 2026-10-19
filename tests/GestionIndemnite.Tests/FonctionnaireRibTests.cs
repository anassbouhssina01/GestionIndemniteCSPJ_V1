using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Infrastructure.RHOrganisation.Files;
using GestionIndemnite.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class FonctionnaireRibTests
{
    private readonly TestDatabaseFixture _fixture;

    public FonctionnaireRibTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static CreateFonctionnaireCommand NewFonc(string suffix) => new(
        $"M{suffix}", $"CIN{suffix}", "بنعلي", "محمد", "Benali", "Mohammed",
        new DateOnly(1985, 1, 1), new DateOnly(2010, 9, 1), Genre.Masculin);

    [Fact]
    public async Task Fonctionnaire_create_and_get()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new FonctionnaireService(context);

        var created = await svc.CreateAsync(NewFonc(Guid.NewGuid().ToString("N")[..6]));
        var fetched = await svc.GetAsync(created.Id);

        Assert.Equal(created.Matricule, fetched.Matricule);
    }

    [Fact]
    public async Task Matricule_duplicate_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new FonctionnaireService(context);
        var m = $"MD{Guid.NewGuid():N}"[..10];

        await svc.CreateAsync(NewFonc(m).WithMatricule(m));

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new FonctionnaireService(second);
        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc2.CreateAsync(NewFonc(m).WithMatricule(m)));
    }

    [Fact]
    public async Task Cin_duplicate_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new FonctionnaireService(context);
        var c = $"CD{Guid.NewGuid():N}"[..10];

        await svc.CreateAsync(NewFonc(c).WithCin(c));

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new FonctionnaireService(second);
        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc2.CreateAsync(NewFonc(Guid.NewGuid().ToString("N")[..6]).WithCin(c)));
    }

    [Fact]
    public async Task Rib_change_atomique_ancien_desactive()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var foncSvc = new FonctionnaireService(context);
        var ribSvc = new RibService(context);

        var fonc = await foncSvc.CreateAsync(NewFonc(Guid.NewGuid().ToString("N")[..6]));

        var rib1 = await ribSvc.ChangeActiveAsync(fonc.Id,
            new CreateRibCommand("011", "123", "0123456789012345", "12"));

        var rib2 = await ribSvc.ChangeActiveAsync(fonc.Id,
            new CreateRibCommand("022", "456", "9876543210987654", "34"));

        Assert.True(rib2.EstActif);
        Assert.NotEqual(rib1.Id, rib2.Id);

        var active = await ribSvc.GetActiveAsync(fonc.Id);
        Assert.NotNull(active);
        Assert.Equal(rib2.Id, active.Id);

        var history = await ribSvc.ListHistoryAsync(fonc.Id);
        Assert.Equal(2, history.Count);
        Assert.Single(history, r => r.EstActif);
    }

    [Fact]
    public async Task Rib_invalide_rejete_avant_transaction()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var foncSvc = new FonctionnaireService(context);
        var ribSvc = new RibService(context);

        var fonc = await foncSvc.CreateAsync(NewFonc(Guid.NewGuid().ToString("N")[..6]));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => ribSvc.ChangeActiveAsync(fonc.Id,
                new CreateRibCommand("AB1", "123", "0123456789012345", "12")));

        var active = await ribSvc.GetActiveAsync(fonc.Id);
        Assert.Null(active);
    }

    [Fact]
    public async Task Rib_check_viole_depuis_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var foncSvc = new FonctionnaireService(context);

        var fonc = await foncSvc.CreateAsync(NewFonc(Guid.NewGuid().ToString("N")[..6]));

        using var second = TestDatabaseFixture.CreateContext();
        second.Ribs.Add(new Rib
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = fonc.Id,
            CodeBanque = "12",   // 2 digits, violates CHECK
            CodeVille = "123",
            NumeroCompte = "0123456789012345",
            Cle = "12",
            EstActif = true
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Second_rib_actif_viole_index_depuis_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var foncSvc = new FonctionnaireService(context);
        var ribSvc = new RibService(context);

        var fonc = await foncSvc.CreateAsync(NewFonc(Guid.NewGuid().ToString("N")[..6]));
        await ribSvc.ChangeActiveAsync(fonc.Id,
            new CreateRibCommand("011", "123", "0123456789012345", "12"));

        using var second = TestDatabaseFixture.CreateContext();
        second.Ribs.Add(new Rib
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = fonc.Id,
            CodeBanque = "022",
            CodeVille = "456",
            NumeroCompte = "9876543210987654",
            Cle = "34",
            EstActif = true
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Fonctionnaire_matricule_reutilise_apres_suppression_logique()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new FonctionnaireService(context);
        var m = $"RU{Guid.NewGuid():N}"[..10];

        var f = await svc.CreateAsync(NewFonc(m).WithMatricule(m));

        var tracked = await context.Fonctionnaires.FirstAsync(x => x.Id == f.Id);
        context.Fonctionnaires.Remove(tracked);
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new FonctionnaireService(second);
        var recreated = await svc2.CreateAsync(NewFonc(m).WithMatricule(m));

        Assert.NotEqual(default, recreated.Id);
    }
}

internal static class FoncCommandExtensions
{
    public static CreateFonctionnaireCommand WithMatricule(this CreateFonctionnaireCommand cmd, string m) =>
        cmd with { Matricule = m };

    public static CreateFonctionnaireCommand WithCin(this CreateFonctionnaireCommand cmd, string c) =>
        cmd with { Cin = c };
}

[Collection("SqlServer")]
public class PieceJointeTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly string _tempDir;

    public PieceJointeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _tempDir = Path.Combine(Path.GetTempPath(), $"pj-test-{Guid.NewGuid():N}");
    }

    private PieceJointeService CreateService()
    {
        var storage = new FileStorageService(_tempDir, NullLogger<FileStorageService>.Instance);
        var context = TestDatabaseFixture.CreateContext();
        return new PieceJointeService(context, storage, NullLogger<PieceJointeService>.Instance);
    }

    private static byte[] ValidPdf() =>
        "%PDF-1.4 fake content for test"u8.ToArray();

    [Fact]
    public async Task Upload_pdf_valide_reussi()
    {
        var svc = CreateService();
        var tag = Guid.NewGuid().ToString("N")[..6];

        using var ms = new MemoryStream(ValidPdf());
        var result = await svc.UploadAsync(ms, new UploadPieceJointeCommand(
            $"DOC-{tag}", new DateOnly(2024, 6, 15), "المحكمة الابتدائية", "arrêt", "Objet test"));

        Assert.NotEqual(default, result.Id);
        Assert.Equal(2024, result.AnneeDocument);
        Assert.NotNull(result.HashFichier);
        Assert.Equal(64, result.HashFichier!.Length);
    }

    [Fact]
    public async Task Upload_non_pdf_rejete()
    {
        var svc = CreateService();

        using var ms = new MemoryStream("NOTPDF content here"u8.ToArray());
        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.UploadAsync(ms, new UploadPieceJointeCommand(
                "BAD-1", new DateOnly(2024, 1, 1), "Test", "type", "obj")));
    }

    [Fact]
    public async Task Upload_faux_pdf_extension_rejete()
    {
        var svc = CreateService();

        using var ms = new MemoryStream("MZ not a pdf at all content"u8.ToArray());
        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.UploadAsync(ms, new UploadPieceJointeCommand(
                "BAD-2", new DateOnly(2024, 1, 1), "Test", "type", "obj")));
    }

    [Fact]
    public async Task Upload_doublon_normalise_rejete()
    {
        var svc = CreateService();
        var tag = Guid.NewGuid().ToString("N")[..6];

        using var ms1 = new MemoryStream(ValidPdf());
        await svc.UploadAsync(ms1, new UploadPieceJointeCommand(
            $"DOC-{tag}", new DateOnly(2024, 3, 1), "المحكمة", "type", "obj1"));

        using var ms2 = new MemoryStream(ValidPdf());
        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.UploadAsync(ms2, new UploadPieceJointeCommand(
                $"doc-{tag}", new DateOnly(2024, 3, 1), "المحكمه", "type", "obj2")));
    }

    [Fact]
    public async Task Upload_memes_numeros_emetteurs_differents_accepte()
    {
        var svc = CreateService();
        var tag = Guid.NewGuid().ToString("N")[..6];

        using var ms1 = new MemoryStream(ValidPdf());
        await svc.UploadAsync(ms1, new UploadPieceJointeCommand(
            $"DOC-{tag}", new DateOnly(2024, 4, 1), "المدعي العام", "type", "obj1"));

        using var ms2 = new MemoryStream(ValidPdf());
        var result = await svc.UploadAsync(ms2, new UploadPieceJointeCommand(
            $"DOC-{tag}", new DateOnly(2024, 4, 1), "المحكمة الابتدائية", "type", "obj2"));

        Assert.NotEqual(default, result.Id);
    }

    [Fact]
    public async Task Upload_memes_numeros_annees_differentes_accepte()
    {
        var svc = CreateService();
        var tag = Guid.NewGuid().ToString("N")[..6];

        using var ms1 = new MemoryStream(ValidPdf());
        await svc.UploadAsync(ms1, new UploadPieceJointeCommand(
            $"DOC-{tag}", new DateOnly(2023, 5, 1), "Test", "type", "obj1"));

        using var ms2 = new MemoryStream(ValidPdf());
        var result = await svc.UploadAsync(ms2, new UploadPieceJointeCommand(
            $"DOC-{tag}", new DateOnly(2024, 5, 1), "Test", "type", "obj2"));

        Assert.NotEqual(default, result.Id);
    }

    [Fact]
    public async Task Download_renvoie_contenu()
    {
        var svc = CreateService();
        var tag = Guid.NewGuid().ToString("N")[..6];
        var original = ValidPdf();

        using var ms = new MemoryStream(original);
        var uploaded = await svc.UploadAsync(ms, new UploadPieceJointeCommand(
            $"DL-{tag}", new DateOnly(2024, 7, 1), "Test", "type", "obj"));

        using var stream = await svc.DownloadAsync(uploaded.Id);
        using var reader = new MemoryStream();
        await stream.CopyToAsync(reader);
        Assert.Equal(original, reader.ToArray());
    }

    [Fact]
    public async Task Chemin_fichier_ne_contient_pas_nom_original()
    {
        var svc = CreateService();
        var tag = Guid.NewGuid().ToString("N")[..6];

        using var ms = new MemoryStream(ValidPdf());
        var result = await svc.UploadAsync(ms, new UploadPieceJointeCommand(
            $"SEC-{tag}", new DateOnly(2024, 8, 1), "Test", "type", "obj"));

        var entity = await TestDatabaseFixture.CreateContext().PieceJointes
            .IgnoreQueryFilters()
            .FirstAsync(p => p.Id == result.Id);

        Assert.NotNull(entity.CheminFichier);
        Assert.DoesNotContain("SEC", entity.CheminFichier!);
        Assert.EndsWith(".pdf", entity.CheminFichier!);
        Assert.Matches(@"^[0-9a-f\-]{36}\.pdf$", entity.CheminFichier!);
    }
}
