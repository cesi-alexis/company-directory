Voici un **guide de test complet pour l'API**, organisé par endpoint et adapté aux spécifications fournies dans le fichier JSON disponible à https://localhost:7055/swagger/v1/swagger.json.

---

## Guide de Test pour l'API

### Pré-requis
1. **Outils nécessaires** :
   - **Postman** pour tester manuellement les endpoints.
   - **Swagger UI** pour explorer et documenter les endpoints.
   - **JSON Validator** (en ligne ou intégré) pour valider les schémas des réponses.
   
2. **Préparation** :
   - Lancer l'API localement ou sur un environnement accessible.
   - Configurer les en-têtes nécessaires (ex. `Content-Type: application/json`).
   - Importer la collection générée à partir du fichier JSON (Swagger).

---

### Endpoints pour **Employés** (`/api/Workers`)

#### **GET /api/Workers**
- **Objectif** : Récupérer la liste de tous les employés.
- **Étapes** :
  1. Créer une requête GET vers `localhost:7055/api/Workers`.
  2. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : Liste JSON contenant les objets `Worker` avec les champs :
    - `id` (entier)
    - `firstName`, `lastName`, `email` (chaînes)
    - `phoneFixed`, `phoneMobile` (chaînes au format téléphone)
    - `serviceId`, `siteId` (entiers).

#### **POST /api/Workers**
- **Objectif** : Créer un nouvel employé.
- **Étapes** :
  1. Créer une requête POST vers `localhost:7055/api/Workers`.
  2. Ajouter le corps suivant dans l'onglet `Body` :
  ```json
  {
    "firstName": "John",
    "lastName": "Doe",
    "phoneFixed": "0123456789",
    "phoneMobile": "0612345678",
    "email": "john.doe@example.com",
    "serviceId": 1,
    "siteId": 1
  }
  ```
  3. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200` ou `201`.
  - Contenu : L'objet créé avec un `id` généré.

#### **GET /api/Workers/{id}**
- **Objectif** : Récupérer les détails d'un employé spécifique.
- **Étapes** :
  1. Créer une requête GET vers `localhost:7055/api/Workers/1` (remplacez `1` par un `id` valide).
  2. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : Un objet `Worker` correspondant à l'ID.

#### **PUT /api/Workers/{id}**
- **Objectif** : Modifier les détails d'un employé existant.
- **Étapes** :
  1. Créer une requête PUT vers `localhost:7055/api/Workers/1`.
  2. Ajouter le corps suivant :
  ```json
  {
    "id": 1,
    "firstName": "UpdatedFirstName",
    "lastName": "UpdatedLastName",
    "phoneFixed": "0987654321",
    "phoneMobile": "0611111111",
    "email": "updated.email@example.com",
    "serviceId": 2,
    "siteId": 2
  }
  ```
  3. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : L'objet mis à jour.

#### **DELETE /api/Workers/{id}**
- **Objectif** : Supprimer un employé.
- **Étapes** :
  1. Créer une requête DELETE vers `localhost:7055/api/Workers/1`.
  2. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : Vide.

---

### Endpoints pour **Services** (`/api/Service`)

#### **GET /api/Service**
- **Objectif** : Récupérer la liste des services.
- **Étapes** :
  1. Créer une requête GET vers `localhost:7055/api/Service`.
  2. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : Liste JSON d'objets `Service` avec :
    - `id` (entier)
    - `name` (chaîne).

#### **POST /api/Service**
- **Objectif** : Créer un nouveau service.
- **Étapes** :
  1. Créer une requête POST vers `localhost:7055/api/Service`.
  2. Ajouter le corps suivant :
  ```json
  {
    "name": "New Service"
  }
  ```
  3. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200` ou `201`.
  - Contenu : L'objet créé avec un `id`.

#### **GET /api/Service/{id}**
- **Objectif** : Récupérer un service spécifique.
- **Étapes** :
  1. Créer une requête GET vers `localhost:7055/api/Service/1`.
  2. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : Un objet `Service` avec ses propriétés.

#### **PUT /api/Service/{id}**
- **Objectif** : Modifier un service existant.
- **Étapes** :
  1. Créer une requête PUT vers `localhost:7055/api/Service/1`.
  2. Ajouter le corps suivant :
  ```json
  {
    "id": 1,
    "name": "Updated Service Name"
  }
  ```
  3. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : L'objet mis à jour.

#### **DELETE /api/Service/{id}**
- **Objectif** : Supprimer un service.
- **Étapes** :
  1. Créer une requête DELETE vers `localhost:7055/api/Service/1`.
  2. Exécuter la requête.
- **Résultats attendus** :
  - Code HTTP : `200`.
  - Contenu : Vide.

---

### Endpoints pour **Locations** (`/api/Location`)

#### **GET /api/Location**
- **Objectif** : Récupérer la liste des sites.
- **Étapes** :
  - Suivez les mêmes étapes que pour **GET /api/Service**.

#### **POST /api/Location**
- **Objectif** : Créer un nouveau site.
- **Étapes** :
  - Suivez les mêmes étapes que pour **POST /api/Service**, en utilisant :
  ```json
  {
    "city": "New City"
  }
  ```

#### **GET /api/Location/{id}**
- **Objectif** : Récupérer un site spécifique.
- **Étapes** :
  - Similaire à **GET /api/Service/{id}**.

#### **PUT /api/Location/{id}**
- **Objectif** : Modifier un site.
- **Étapes** :
  - Suivez les mêmes étapes que pour **PUT /api/Service/{id}**, en modifiant la propriété `city`.

#### **DELETE /api/Location/{id}**
- **Objectif** : Supprimer un site.
- **Étapes** :
  - Similaire à **DELETE /api/Service/{id}**.

---

Ce guide couvre tous les endpoints avec des instructions détaillées.