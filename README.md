# TeamScheduler – Application de gestion de planning et de présence

## Présentation
TeamScheduler est une application web Blazor dédiée à la gestion collaborative des plannings, présences, congés, compétences et ressources humaines d’une équipe ou d’une organisation. Elle offre une interface moderne, des fonctionnalités avancées de reporting, d’analytics et de personnalisation de l’expérience utilisateur.

## Fonctionnalités principales
- **Planning hebdomadaire** : visualisation, édition et export du planning de présence des employés (présent, absent, congé, télétravail, site, client), coloration dynamique par statut, filtres avancés et réorganisés.
- **Gestion des équipes** : création, modification, activation/désactivation, affectation de membres, gestion du statut et des rôles, hiérarchie manager/subordonnés.
- **Gestion des compétences** : ajout/retrait de compétences, catégorisation, gestion des niveaux, affectation aux membres.
- **Gestion des clients et projets** : suivi des affectations, historique, détails client/projet, description client.
- **Congés et absences** : import/export, validation, suivi du solde, affichage dans le planning.
- **Analytics** : indicateurs clés (taux de présence, couverture, absences, congés, etc.), graphiques et statistiques en temps réel.
- **Rapports** : génération de rapports PDF/Excel, export des données, personnalisation des exports.
- **Paramètres d’affichage dynamiques** : thème (clair/sombre/auto), taille de police, mode compact, animations, préférences utilisateur persistantes, propagation dynamique à toutes les pages.
- **Notifications** : toasts, alertes, notifications email/Teams/SMS selon préférences.
- **Hiérarchie et reporting** : gestion des managers, subordonnés, reporting RH avancé.
- **Historique de présence** : suivi détaillé des présences, télétravail, site, client, avec commentaires.
- **Navigation moderne** : menu principal restauré, navigation fluide, liens harmonisés.
- **Synchronisation modèle/base** : migrations EF Core à jour, entités synchronisées avec la base de données.
- **Sécurité avancée** : gestion des rôles, routes sécurisées, timeout session, confirmation automatique de l’email à l’inscription.
- **UI/UX Material Design** : pages d’authentification (connexion, inscription, mot de passe oublié) modernisées en style Google Material.
- **Synchronisation automatique des données utilisateur** : Prénom, Nom, Téléphone synchronisés avec l’entité Employee, notifications planning à jour.

## Architecture technique
- **Frontend** : Blazor Server (.NET 8), Bootstrap 5, Chart.js
- **Backend** : Entity Framework Core, base SQLite (fichier `planning.db`)
- **Organisation** :
  - `Pages/` : pages principales (Planning, Teams, Clients, Analytics, Reports, Settings, etc.)
  - `Shared/` : composants réutilisables (layouts, cards, notifications, modals)
  - `Application/DTOs/` : objets de transfert de données (TeamDto, EmployeeDto, etc.)
  - `Core/Entities/` : entités métier (Employee, Team, PresenceRecord, etc.)
  - `Services/` : services métier (PlanningService, TeamService, NotificationService, etc.)
  - `Data/` : contexte EF, seed, configurations
  - `Migrations/` : scripts de migration de la base
  - `wwwroot/` : ressources statiques (js, css, images)

## Installation & lancement
1. **Prérequis** : .NET 8 SDK, Visual Studio 2022+ ou VS Code
2. **Cloner le dépôt**
3. **Restaurer les packages NuGet**
4. **Lancer la migration EF Core si besoin** :
   ```bash
   dotnet ef database update
   ```
5. **Démarrer l’application** :
   ```bash
   dotnet run --project PlanningPresenceBlazor.csproj
   ```
6. Accéder à l’URL locale affichée (ex : http://localhost:64207)

## Personnalisation & extensions
- Les paramètres d’affichage sont accessibles dans la page `/settings` et sont appliqués dynamiquement à toute l’application (thème, couleurs, etc.).
- L’architecture permet d’ajouter facilement de nouveaux modules (ex : badge, workflow RH, SSO, etc.).
- Les DTOs et entités sont extensibles pour intégrer de nouveaux champs métiers.

## Sécurité & gestion des accès
- Gestion des rôles (Administrateur, Manager, Utilisateur)
- Authentification intégrée (à compléter selon besoins SSO/OAuth)
- Journalisation des actions (logs)
- Timeout de session configurable
- Confirmation automatique de l’email à l’inscription (aucun mail à cliquer)
- Synchronisation automatique des données utilisateur (Prénom, Nom, Téléphone)
- UI/UX Material Design sur toutes les pages d’authentification

## Tests & validation
- Tests unitaires dans `Tests/Unit/`
- Validation UX sur tous les navigateurs modernes

## Auteurs & contact
- Développement : Équipe TeamScheduler (2024-2025)
- Contact support : support@planningpresence.com

---

Pour toute contribution, suggestion ou bug, merci d’ouvrir une issue ou de contacter l’équipe.
