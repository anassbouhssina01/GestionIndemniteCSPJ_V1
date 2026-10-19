# LOCK_DECISION — Décision verrou de clé (`sp_getapplock`) par service

> Date : 2026-10-19 — basé sur les tests témoin M5 (`PeriodeServiceTests`).

## Test témoin (sans verrou)

Scénario : 20 tentatives parallèles d'ajout de la **même période fermée** pour la même clé, sans verrou, transaction `ReadCommitted`.

Résultat observé : la majorité des 20 tentatives réussissent (pas de détection de chevauchement entre elles), confirmant la course réelle entre la lecture (`SELECT` de contrôle) et l'écriture. SQL Server n'offre aucune contrainte dure contre le chevauchement entre enregistrements fermés.

## Test de charge (avec verrou)

Même scénario avec `lockKey = "rh:periode:{table}:{clé}"` passé à `PeriodeService.AddAsync`.

Résultat observé : **exactement 1 succès**, 19 rejets métier (`توجد فترة متداخلة مع سجل موجود`). Le verrou sérialise efficacement les écritures concurrentes.

## Décision

Le verrou est **activé** pour tous les services passant par `PeriodeService` :

| Service | Verrou | Clé |
| --- | --- | --- |
| `PeriodeService.AddAsync` / `UpdateAsync` | Oui (paramètre `lockKey` non null) | `rh:periode:{nom table}:{clé de regroupement}` |
| `HistoriqueCadreGradeService` (M6) | Oui | `rh:periode:HistoriqueCadreGrade:{idFonctionnaire}` |
| `SituationAdministrativeService` (M6) | Oui | `rh:periode:SituationAdministrative:{idFonctionnaire}` |
| `HistoriqueAffectationService` (M6) | Oui | `rh:periode:HistoriqueAffectation:{idFonctionnaire}` |
| `HistoriqueResponsabiliteService` (M6) | Oui | `rh:periode:HistoriqueResponsabilite:{idFonct}:{idResp}:{idTrib|idUnite}` |
| `HistoriqueOrganigrammeTribunalService` (M6) | Oui | `rh:periode:HistOrgTribunal:{idTribunal}` |
| `HistoriqueOrganigrammeUniteService` (M6) | Oui | `rh:periode:HistOrgUnite:{idUnite}` |
| `RibService.ChangeActiveAsync` | Non (transaction suffit — l'index unique filtre garantit l'atomicité structurelle) | — |

## Justification

La course est **démontrée** par le test témoin : sans verrou, plusieurs écritures concurrentes passent toutes le contrôle de chevauchement. Le verrou d'application est le seul mécanisme disponible dans SQL Server pour sérialiser cette fenêtre lecture-écriture sans contrainte d'exclusion native.

## Limite connue

La protection ne couvre que les chemins passant par `PeriodeService`. Un SQL direct contourne le verrou. Filet de sécurité : le rapport de contrôle de chevauchement (M7).
