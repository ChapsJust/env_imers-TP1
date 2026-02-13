# TicTacToe AR — TP1 Environnement Immersif

**Auteur :** Chaps  
**Projet :** TicTacToe en réalité augmentée (AR)

## Description brève

Ce projet est un **TicTacToe en réalité augmentée** réalisé dans le cadre du TP1.  
L’idée est de poser une grille dans l’environnement réel (via AR Foundation), puis de jouer une partie classique de TicTacToe avec une interface simple et lisible.

---

## Version Unity et packages utilisés

- **Unity :** `6000.3.8f1`
- **Version complète éditeur :** `6000.3.8f1 (1c7db571dde0)`

### Packages principaux

- `com.unity.xr.arfoundation` — `6.3.3`
- `com.unity.xr.arcore` — `6.3.3`
- `com.unity.inputsystem` — `1.18.0`
- `com.unity.render-pipelines.universal` — `17.3.0`
- `com.unity.ugui` — `2.0.0`
- `com.unity.timeline` — `1.8.10`
- `com.unity.visualscripting` — `1.9.9`
- `com.unity.ai.navigation` — `2.0.10`

---

## Captures d’écran

![Placement AR de la grille](docs/screenshots/01-placement-ar.png)
![Partie en cours](docs/screenshots/02-partie-en-cours.png)
![Écran de fin / victoire](docs/screenshots/03-fin-partie.png)

---

## Défis rencontrés et solutions

### 1) Placement stable de la grille en AR

- **Défi :** la grille pouvait être mal orientée ou instable au premier placement.
- **Solution :** ajout d’une logique de placement contrôlée (surface détectée + validation avant instanciation).

### 2) Interaction utilisateur (mobile + AR)

- **Défi :** gérer les interactions de manière fiable entre l’Input System et les objets de jeu.
- **Solution :** séparation claire entre la gestion des entrées, la logique de jeu et l’UI (scripts dédiés : contrôleur, UI, cellule).

### 3) Logique de partie lisible

- **Défi :** éviter les conflits d’état (tour du joueur, victoire, égalité, reset).
- **Solution :** centralisation des règles dans un contrôleur de partie avec mise à jour explicite des états.

---

## Annexe — Requêtes utilisées pour écrire du code

| Date       | Outil    | Requête utilisée                                                                           | Objectif                                  |
| ---------- | -------- | ------------------------------------------------------------------------------------------ | ----------------------------------------- |
| 02/13/2026 | Claude   | "Peux-tu faire le README en restant le plus humain possible"                               | Structurer la documentation du projet     |
| 02/13/2026 | ClaudeAI | "Peux-tu m'aidé avec ce problème de conditions avec le UI plusieurs problèmes d'affichage" | Réparation des conditions pour le UI      |
| 02/13/2026 | ClaudeAI | "Que recommandes-tu comme structure idéal pour un Jeu tictactoe en ARCore sur Unity 6"     | Montre une structure idéal pour ce projet |
| 02/13/2026 | Copilot  |                                                                                            | Rédaction commentraires                   |
