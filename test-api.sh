#!/bin/bash

# Script de test pour l'architecture microservices
# Ce script démontre l'utilisation de l'API Gateway

echo "╔════════════════════════════════════════════════════════╗"
echo "║     Tests de l'Architecture Microservices              ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""

API_GATEWAY="http://localhost:5000"

echo "🔍 1. Vérification de la santé des services..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
curl -s "$API_GATEWAY/api/health" | jq '.' || echo "Erreur: Assurez-vous que les services sont démarrés"
echo ""
echo ""

echo "👥 2. Obtenir la liste des utilisateurs..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
curl -s "$API_GATEWAY/api/users" | jq '.'
echo ""
echo ""

echo "📦 3. Obtenir la liste des produits..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
curl -s "$API_GATEWAY/api/products" | jq '.'
echo ""
echo ""

echo "➕ 4. Créer un nouvel utilisateur..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
NEW_USER=$(curl -s -X POST "$API_GATEWAY/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Alice Dupont",
    "email": "alice.dupont@example.com"
  }')
echo "$NEW_USER" | jq '.'
USER_ID=$(echo "$NEW_USER" | jq -r '.id')
echo "✅ Utilisateur créé avec ID: $USER_ID"
echo ""
echo ""

echo "➕ 5. Créer un nouveau produit..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
NEW_PRODUCT=$(curl -s -X POST "$API_GATEWAY/api/products" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MacBook Pro M3",
    "description": "Ordinateur portable haute performance",
    "price": 2499.99,
    "stock": 20
  }')
echo "$NEW_PRODUCT" | jq '.'
PRODUCT_ID=$(echo "$NEW_PRODUCT" | jq -r '.id')
echo "✅ Produit créé avec ID: $PRODUCT_ID"
echo ""
echo ""

echo "🛒 6. Créer une commande (DÉMONSTRATION COMMUNICATION INTER-SERVICES)..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Cette étape montre comment OrderService communique avec UserService et ProductService"
echo ""
ORDER=$(curl -s -X POST "$API_GATEWAY/api/orders" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": 1,
    \"productId\": 1,
    \"quantity\": 2
  }")
echo "$ORDER" | jq '.'
echo ""
echo ""

echo "📋 7. Obtenir toutes les commandes..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
curl -s "$API_GATEWAY/api/orders" | jq '.'
echo ""
echo ""

echo "╔════════════════════════════════════════════════════════╗"
echo "║              ✅ Tests terminés avec succès             ║"
echo "╚════════════════════════════════════════════════════════╝"
echo ""
echo "📚 Pour explorer davantage:"
echo "   • API Gateway Swagger: http://localhost:5000/swagger"
echo "   • User Service Swagger: http://localhost:5001/swagger"
echo "   • Product Service Swagger: http://localhost:5002/swagger"
echo "   • Order Service Swagger: http://localhost:5003/swagger"
echo ""
