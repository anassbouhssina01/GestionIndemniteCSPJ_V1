# PROJECT_MAP — GestionIndemnite / RHOrganisation

> External memory. Update on every architecture change. Read before touching any file.
> Last sync: 2026-10-19 — M11 complete (162 tests green, smoke test UI OK).

## [TECH_STACK]

- .NET 10 LTS (SDK 10.0.302) — net10.0, nullable enable, TreatWarningsAsErrors
- EF Core 10.0.13 (Application: base package only; Infrastructure + Tests: SqlServer provider)
- SQL Server **LocalDB** `(localdb)\GestionIndemnite` — schéma `rh`
  - Raison : l'utilisateur Windows n'a aucun droit sur `localhost\MSSQLSERVER` ni `SQLEXPRESS`
  - LocalDB fournit sysadmin + CREATE DATABASE pour `CSPJ\a.bouhssina`
- ClosedXML 0.105.1 (MIT, import Excel — M8)
- xunit 2.9.3 + Microsoft.NET.Test.Sdk 18.11.0
- System.Security.Cryptography.Pkcs 10.0.13 (pin — corrige NU1902/NU1903 de EF SqlServer)
- Logging : `ILogger` + `AsyncFileLoggerProvider` (BCL Channel + BackgroundService, 3 niveaux, drop si plein). Zéro framework externe.
- UI (décidé) : MVC / Razor Pages, libellés **français**, messages `RegleMetierException` **arabe** (mandat 11.2.9)
- Pas de Serilog, pas de Repository/UoW, pas de InMemory/Mock dans les tests

## [SYSTEM_FLOW]

```
Referentiels(6 CRUD) → PieceJointe(PDF %PDF ≤10Mo SHA-256)
→ Fonctionnaire + Rib(transaction active change)
→ Tribunal/Unite(+1er organigramme, 1 SaveChanges atomique)
→ Historiques(×6) via PeriodeService
    [ReadCommitted txn → DateDebut≤DateFin → overlap → 1 open max → Commit]
    [AcquireKeyLockAsync(sp_getapplock) : implémenté, activé APRÈS test témoin M5]
→ Rapports(trous / sans-justificatif / contrôle-overlap)
→ Import Excel(dry-run → rapport erreurs → 1 txn/fonctionnaire → mêmes services)
UI MVC(FR) → Controllers minces → Services → DTOs. Messages métier AR dans bandeau d'erreur.
```

## [ARCHITECTURE]

```
Domain (0 refs, 0 packages)
  RHOrganisation/{Common,AuditableEntity,IPeriode; Entities,Cadre; Exceptions,RegleMetierException}
Application (→ Domain + EF Core base pkg)
  RHOrganisation/Common/{ICurrentUser, IRHOrganisationDbContext}
    IRHOrganisationDbContext : DbSet<Cadre> + SaveChangesAsync + BeginTransactionAsync(IsolationLevel) + AcquireKeyLockAsync
Infrastructure (→ App + Domain + EFCore.SqlServer)
  RHOrganisation/Persistence/{RHOrganisationDbContext, Configurations/CadreConfiguration, Migrations/}
Web (→ App + Infrastructure, racine DI)
  RHOrganisation/CurrentUser/SystemCurrentUser ; Logging/AsyncFileLoggerProvider
Tests (→ les 4 + xunit + SqlServer)
  Infrastructure/TestDatabaseFixture (LocalDB, drop+EnsureCreated, [Collection("SqlServer")])
  AuditTests (test 46, 7 cas) + AcquireKeyLockTests (6 cas)
```

**Règles actives :**
- Audit/soft-delete dans `RHOrganisationDbContext.SaveChangesAsync` : Added→CreatedAt/By, Modified→UpdatedAt/By, Deleted→IsDeleted=true (state→Modified)
- Query filter `IsDeleted=false` : déclaré **par entité** dans `OnModelCreating` (pattern type-safe EF Core — chaque nouvelle entité ajoute sa ligne)
- Schéma `rh`, UX uniques filtrés `[IsDeleted]=0`, FK Restrict, Enums nombres, DateOnly
- `AcquireKeyLockAsync` : ADO.NET direct (sp_getapplock, @LockOwner=Transaction), retour 0/1=ok, -1→RegleMetierException AR, -2/-3→retry, default→generic
- `BeginTransactionAsync(IsolationLevel)` : délègue à `Database.BeginTransactionAsync`
- Services (M2+) : classes concettes injectant `IRHOrganisationDbContext`, retournent DTOs, jamais d'entités

## [MILESTONES]

| M | Statut | Contenu |
|---|---|---|
| M1 Fondations | ✅ 2026-10-19 | Solution, 5 projets, AuditableEntity, IPeriode, RegleMetierException, Cadre, ICurrentUser, IRHOrganisationDbContext, RHOrganisationDbContext (audit+soft-delete+filter+lock), SystemCurrentUser, AsyncFileLoggerProvider, Migration RH_01, 13 tests verts |
| M2 Référentiels | ✅ 2026-10-19 | Grade, Responsabilite, StatutAdministratif, Pays, Ville + EstActif sur les 4 + 6 services CRUD + DTOs + Migration RH_02 + 26 tests verts |
| M3 Documents+Personnes | ✅ 2026-10-19 | PieceJointe, TextNormalizer (arabe/diacritiques/tatweel), IFileStorage, FileStorageService (Guid.pdf hors wwwroot, %PDF magic, SHA-256, ≤10Mo, compensation delete), Fonctionnaire, Rib (4 CHECK digit/length), RibRules, 3 services + 19 tests (62 total) |
| M4 Organigramme | ✅ 2026-10-19 | Tribunal, UniteOrganisationnelle, HistoriqueOrganigrammeTribunal/Unite, OrganigrammeRules (niveaux CJR→TPI→CA→Cassation, cycles), 2 services (création atomique 1 SaveChanges, GetParentAt date), Migration RH_04 + 26 tests |
| M5 PeriodeService | ✅ 2026-10-19 | PeriodeService générique (Add/Update/GetGaps, ReadCommitted, verrou optionnel), PeriodeTest entity+context, tests 42/47/48/49 (témoin sans verrou → course démontrée), LOCK_DECISION.md (verrou activé pour les 6 services historique), +15 tests |
| M6 Historiques | ✅ 2026-10-19 | 4 entités historique (CadreGrade, SituationAdmin, Affectation, Responsabilite) + 2 services organigramme ChangeParent (CloseAndAddAsync ferme la période ouverte atomiquement), AffectationRules (exactement-un tribunal/unite, Section+UniteTravail tribunal only), contrôles différés (désactivation référentiels utilisés interdite, suppression fonctionnaire/document avec historique interdite), Migration RH_05 + 29 tests (132 total) |
| M7 Rapports | ✅ 2026-10-19 | RapportService (trous via PeriodeService.GetGaps, sans-justificatif (IdDocument null) sur les 6 tables, contrôle-overlap détection par tri+adjacent), RapportDtos (TypeHistoriqueRapport enum ×6), +10 tests (142 total) |
| M8 Import Excel | ✅ 2026-10-19 | ExcelImportReader (ClosedXML, 6 feuilles, 5 Mo/5000 lignes, dates jj/mm/aaaa strictes), ImportService (groupage par Matricule, résolution référentielles, 1 txn ReadCommitted/fonctionnaire avec rollback dry-run, `RegleMetierException or DbUpdateException` → REJETÉ, ClearTrackedChanges, rapport RapportImport), entité RapportImport + migration RH_06 + script, guards `HasActiveTransaction`/`ClearTrackedChanges` sur PeriodeService+RibService, DI Web, +15 tests (157 total) |
| M9 Socle MVC | ✅ 2026-10-19 | RegleMetierExceptionFilter (message AR → TempData["Erreur"], redirection Referer même hôte/page différente sinon accueil), _ErrorBanner (erreur dir=rtl + succès), _Layout FR (nav complète 6 sections), accueil tableau de bord, Error FR, DI filtre global, +5 tests (162 total) |
| M10 Écrans réf.+docs | ✅ 2026-10-19 | 6 CRUD (Cadre, Grade+filtre cadre, Responsabilité, Statut, Pays (EstLocal), Ville+filtre pays) — Index/Create/Edit + Disable/Enable POST, checkboxes hidden-then-checkbox pour bool, PieceJointe (recherche numéro/émetteur/année, upload PDF multipart, download, delete), smoke test navigateur 200 + POST-redirect-GET TempData OK |
| M11 Écrans person.+org. | ✅ 2026-10-19 | Fonctionnaire (recherche matricule/CIN/nom, create, edit immuable matricule/CIN, delete avec garde-fous, Details identité+RIB actif+formulaire ChangeActive RIB+historique), Tribunal (CRUD minimal type/niveau FR, parent actuel via GetParentAt, cycle déjà géré), Unite (idem TypeUnite FR), smoke test create+RIB OK |
| M12 Écrans historiques | ⬜ | 6 formulaires période |
| M12 Écrans historiques | ⬜ | 6 formulaires période |
| M13 Rapports+Import UI | ⬜ | 3 rapports + écran import |

## [ORPHANS & PENDING]

- ⬜ M9–M13 UI MVC français
- ⬜ Activez le pin System.Security.Cryptography.Pkcs si EF 10.0.14+ corrige la vulnérabilité en amont (sinon garder le pin)

## [RESOLVED DECISIONS]

- UI = MVC/Razor Pages, FR only (choix utilisateur 2026-10-19)
- Messages RegleMetierException = arabe (mandat document 11.2.9), affichés dans bandeau d'erreur
- Tâche 1 reconstruite de zéro (repo vide)
- 12.10 n°1,3,4,5,6 : adoptés les 6 par défaut
- Cadre reshufflé M2→M1 (audit tests sur vraie table, pas d'entité test)
- BDD : LocalDB au lieu de MSSQLSERVER (absence de droits sysadmin sur l'instance nommée)
- Global query filter : par-entité (pas de boucle reflection) — pattern EF Core recommandé
- TextNormalizer : range Unicode `\u064B–\u065F` + `\u0640` + `\u0670` pour diacritiques/tatweel (liste énumérée ratait shadda `ّ`)
- Application référence Microsoft.Extensions.Logging.Abstractions 10.0.13 (ILogger<T> dans PieceJointeService — port technique, pas un framework)
- Fichiers stockés sous `App_Data/files` (hors wwwroot), nom = GUID `.pdf`, jamais le nom d'origine
