# Dummy_Micro_Service
# Apprentissage du modèle Microservices avec C# et .NET

## 📚 Qu'est-ce qu'une Architecture Microservices ?

Une architecture microservices est un style architectural qui structure une application comme une collection de **services indépendants**, faiblement couplés et déployables de manière autonome.

### Principes Clés

1. **Décomposition par domaine métier** - Chaque service représente une capacité métier
2. **Indépendance** - Chaque service peut être développé, déployé et mis à l'échelle indépendamment
3. **Communication via API** - Les services communiquent via des protocoles légers (HTTP/REST, gRPC, messages)
4. **Décentralisation** - Chaque service peut avoir sa propre base de données
5. **Résilience** - La défaillance d'un service n'affecte pas l'ensemble du système

### Avantages

✅ **Scalabilité** - Mise à l'échelle indépendante de chaque service
✅ **Flexibilité technologique** - Chaque service peut utiliser différentes technologies
✅ **Déploiement continu** - Déploiements indépendants et fréquents
✅ **Isolation des pannes** - Les erreurs sont contenues
✅ **Organisation d'équipe** - Équipes autonomes par service

### Défis

⚠️ **Complexité opérationnelle** - Plus de services à gérer
⚠️ **Communication réseau** - Latence et gestion des erreurs
⚠️ **Cohérence des données** - Transactions distribuées complexes
⚠️ **Tests** - Tests d'intégration plus complexes
⚠️ **Monitoring** - Nécessite des outils de surveillance distribués

## 🏗️ Architecture de ce Projet

Ce projet démontre une architecture microservices avec **C# et ASP.NET Core** :

```
┌─────────────────────────────────────────────────────────┐
│                      CLIENT (Browser/App)                │
└────────────────────┬────────────────────────────────────┘
                     │
                     ↓
            ┌────────────────┐
            │  API GATEWAY   │ Port 5000
            │  (Point d'entrée unique)
            └────────┬───────┘
                     │
         ┌───────────┼───────────┐
         │           │           │
         ↓           ↓           ↓
    ┌────────┐  ┌────────┐  ┌────────┐
    │ USER   │  │PRODUCT │  │ ORDER  │
    │SERVICE │  │SERVICE │  │SERVICE │
    │Port    │  │Port    │  │Port    │
    │5001    │  │5002    │  │5003    │
    └────────┘  └────────┘  └────────┘
```

### Services Inclus

#### 1. **API Gateway** (Port 5000)
- Point d'entrée unique pour tous les clients
- Routage des requêtes vers les services appropriés
- Agrégation des réponses
- Construit avec ASP.NET Core

#### 2. **User Service** (Port 5001)
- Gestion des utilisateurs
- API REST CRUD sur les utilisateurs
- Base de données indépendante (simulée en mémoire avec Entity Framework Core InMemory)

#### 3. **Product Service** (Port 5002)
- Gestion du catalogue de produits
- API REST CRUD sur les produits
- Base de données indépendante (simulée en mémoire)

#### 4. **Order Service** (Port 5003)
- Gestion des commandes
- Création de commandes
- Communication avec User Service et Product Service via HttpClient
- Démontre la communication inter-services

## 🚀 Démarrage Rapide

### Prérequis

- .NET 8.0 SDK ou supérieur ([Télécharger](https://dotnet.microsoft.com/download))
- Docker et Docker Compose (optionnel mais recommandé)

### Option 1 : Avec Docker (Recommandé)

```bash
# Construire et démarrer tous les services
docker-compose up --build

# Arrêter les services
docker-compose down
```

### Option 2 : Sans Docker

```bash
# Terminal 1 - API Gateway
cd ApiGateway
dotnet run

# Terminal 2 - User Service
cd UserService
dotnet run

# Terminal 3 - Product Service
cd ProductService
dotnet run

# Terminal 4 - Order Service
cd OrderService
dotnet run
```

## 🧪 Tester l'Application

### 1. Créer un utilisateur

```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"name": "Alice Dupont", "email": "alice@example.com"}'
```

### 2. Obtenir tous les utilisateurs

```bash
curl http://localhost:5000/api/users
```

### 3. Créer un produit

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Laptop", "price": 999.99, "stock": 10}'
```

### 4. Obtenir tous les produits

```bash
curl http://localhost:5000/api/products
```

### 5. Créer une commande (démontre la communication inter-services)

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": 1,
    "productId": 1,
    "quantity": 2
  }'
```

### 6. Obtenir toutes les commandes

```bash
curl http://localhost:5000/api/orders
```

### 7. Swagger UI (Documentation Interactive)

Chaque service expose une interface Swagger pour tester les APIs :

- **API Gateway**: http://localhost:5000/swagger
- **User Service**: http://localhost:5001/swagger
- **Product Service**: http://localhost:5002/swagger
- **Order Service**: http://localhost:5003/swagger

## 📖 Concepts Démontrés

### 1. **ASP.NET Core Web API**
Chaque service est une Web API ASP.NET Core indépendante avec son propre controller.

### 2. **Communication Inter-Services avec HttpClient**
L'Order Service utilise HttpClient pour communiquer avec User Service et Product Service.

### 3. **API Gateway Pattern**
L'API Gateway route toutes les requêtes vers les services appropriés.

### 4. **Entity Framework Core InMemory**
Chaque service utilise une base de données en mémoire pour la persistance (pour la simplicité).

### 5. **Dependency Injection**
Utilisation du DI container intégré d'ASP.NET Core pour l'injection de dépendances.

### 6. **Swagger/OpenAPI**
Documentation automatique de l'API avec Swagger UI.

## 🔍 Explorez le Code

### Structure des Fichiers

```
.
├── ApiGateway/                # Point d'entrée unique
│   ├── Program.cs
│   ├── Controllers/
│   ├── ApiGateway.csproj
│   └── Dockerfile
├── UserService/               # Gestion des utilisateurs
│   ├── Program.cs
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── UserService.csproj
│   └── Dockerfile
├── ProductService/            # Gestion des produits
│   ├── Program.cs
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── ProductService.csproj
│   └── Dockerfile
├── OrderService/              # Gestion des commandes
│   ├── Program.cs
│   ├── Controllers/
│   ├── Models/
│   ├── Data/
│   ├── Services/
│   ├── OrderService.csproj
│   └── Dockerfile
├── docker-compose.yml         # Orchestration de tous les services
└── README.md                  # Cette documentation
```

## 🎓 Points d'Apprentissage

1. **Examinez** comment chaque service est un projet .NET indépendant
2. **Observez** l'utilisation de HttpClient dans OrderService pour communiquer avec les autres services
3. **Testez** l'arrêt d'un service - les autres continuent de fonctionner
4. **Modifiez** un service et redéployez-le sans toucher aux autres
5. **Comprenez** comment l'API Gateway délègue les requêtes
6. **Explorez** Swagger UI pour visualiser les APIs de chaque service

## 🛠️ Technologies Utilisées

- **C# 12** - Langage de programmation
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM avec base InMemory
- **Swashbuckle** - Génération Swagger/OpenAPI
- **Docker** - Conteneurisation
- **Docker Compose** - Orchestration multi-conteneurs

## 📚 Pour Aller Plus Loin

### Améliorations Possibles

- [ ] Ajouter une vraie base de données (SQL Server, PostgreSQL)
- [ ] Implémenter l'authentification JWT avec IdentityServer
- [ ] Ajouter un message broker (RabbitMQ, Azure Service Bus) pour la communication asynchrone
- [ ] Implémenter le pattern Circuit Breaker avec Polly
- [ ] Ajouter des logs structurés avec Serilog
- [ ] Implémenter le tracing distribué avec OpenTelemetry
- [ ] Ajouter des health checks ASP.NET Core
- [ ] Implémenter le pattern Saga pour les transactions distribuées
- [ ] Utiliser Ocelot comme API Gateway plus avancé
- [ ] Ajouter un service de configuration centralisé (Azure App Configuration, Consul)
- [ ] Utiliser Kubernetes pour l'orchestration en production
- [ ] Implémenter CQRS avec MediatR
- [ ] Ajouter des tests unitaires et d'intégration avec xUnit

### Ressources

- [Microservices Pattern](https://microservices.io/patterns/index.html)
- [Microsoft - .NET Microservices Architecture](https://dotnet.microsoft.com/learn/aspnet/microservices-architecture)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Docker Documentation](https://docs.docker.com/)
- [eShopOnContainers - Référence Microsoft](https://github.com/dotnet-architecture/eShopOnContainers)

## 🤝 Contribution

Ce projet est destiné à l'apprentissage. N'hésitez pas à :
- Modifier le code
- Ajouter de nouveaux services
- Expérimenter avec différents patterns
- Casser des choses et apprendre de vos erreurs !

## 📝 Licence

MIT - Libre d'utilisation pour l'apprentissage

---

**Bon apprentissage avec C# et .NET ! 🚀**
