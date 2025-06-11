# TeamScheduler

**TeamScheduler** est une application web développée en **Blazor** (Server ou WebAssembly) permettant la **gestion intelligente et l’analyse de planning de présence** pour une équipe ou une entreprise.

## ✨ Fonctionnalités principales

### 1. 🗓 Génération de planning hebdomadaire
- Sélection de la semaine via un calendrier.
- Génération automatique du planning pour la semaine choisie.
- Intégration des congés depuis un fichier CSV.
- Affichage sous forme de tableau avec statut par jour : présent, en congé, absent.
- Statistiques instantanées :  
  - Nombre total d’employés  
  - Nombre d’employés en congé  
  - Total de présences  
  - Pourcentage de couverture

### 2. 📥 Importation et gestion des congés
- Import de fichiers CSV contenant les périodes d'absence.
- Prise en compte automatique des congés dans le planning généré.

### 3. 📤 Exportation & notifications
- Export du planning au format **CSV**.
- (Prévu) Système de **notification** pour informer les employés (email ou autre canal).

### 4. 🔁 Analyse de rotation multi-semaines
- Génération de plannings sur 2, 3, 4, 6, 8 ou 12 semaines.
- Visualisation des variations hebdomadaires.
- Statistiques avancées :
  - Total de présences par semaine
  - Moyenne, variabilité, écart-type
  - Score d’équité
  - Analyse détaillée par employé et par semaine

### 5. 🚨 Visualisation claire & alertes
- Alertes en cas de non-respect des contraintes (ex. : effectif minimum non atteint).
- Interface moderne avec badges, couleurs et icônes pour chaque statut.

---

## 🎯 Public cible

- Responsables RH  
- Managers, chefs d’équipe  
- Toute organisation devant optimiser la **présence et l’équité** des équipes

---

## ⚙️ Points techniques

- Framework : **Blazor Server/WebAssembly**
- Architecture modulaire avec **services injectés** pour :
  - Génération du planning
  - Gestion des congés
  - Envoi de notifications
- Composants UI modernes : **Bootstrap**, icônes, badges dynamiques
- Gestion **import/export CSV**
- Analyse statistique intégrée : **moyenne, variance, écart-type, équité**

---

## ✅ En résumé

**TeamScheduler** est un outil complet pour :
- Générer, analyser et exporter des plannings
- Gérer les absences et congés
- Visualiser l’équité de répartition
- Optimiser la rotation et le respect des contraintes
- Faciliter la communication autour du planning

---

