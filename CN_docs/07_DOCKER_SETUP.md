# 07 — Environnement Docker

> Configuration Docker Desktop pour le projet Charles & Nadejda.
> Remplace WAMP/XAMPP. Le prof utilise WAMP — ce n'est pas un problème,
> la base MySQL est identique, seul l'hébergement local diffère.

---

## 🗂️ Ce qui tourne dans Docker vs en natif

| Composant | Environnement | URL / Port |
|-----------|---------------|------------|
| **MySQL 8** | Docker container | `localhost:3306` |
| **Site Laravel** (PHP + Nginx) | Docker container | `http://localhost:8000` |
| **phpMyAdmin** | Docker container | `http://localhost:8080` |
| **App C# Windows Forms** | Natif Windows (Visual Studio) | — (se connecte à `localhost:3306`) |

> L'app C# est une application desktop Windows — elle ne peut pas tourner dans Docker.
> Elle se connecte au container MySQL exactement comme si c'était WAMP : `localhost:3306`.

---

## 📁 Structure des fichiers Docker

```
CharlesNadejda_Project/
├── docker-compose.yml          ← Orchestration de tous les services
├── docker/
│   ├── php/
│   │   └── Dockerfile          ← Image PHP 8.3 + extensions Laravel
│   └── nginx/
│       └── default.conf        ← Config Nginx pour Laravel
├── site-laravel/               ← Code Laravel (monté en volume)
├── sql/
│   ├── create_database.sql
│   └── seed_data.sql
└── docs/
```

---

## 🐳 `docker-compose.yml`

```yaml
services:

  # ─── MySQL 8 ─────────────────────────────────────────────────────
  mysql:
    image: mysql:8.0
    container_name: charlesnadejda_mysql
    restart: unless-stopped
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: charlesnadejda
      MYSQL_USER: cn_user
      MYSQL_PASSWORD: cn_password
    ports:
      - "3306:3306"              # Accessible depuis l'app C# via localhost:3306
    volumes:
      - mysql_data:/var/lib/mysql
      - ./sql:/docker-entrypoint-initdb.d  # Importe create_database.sql au 1er démarrage
    networks:
      - cn_network

  # ─── PHP 8.3 + Nginx (Laravel) ──────────────────────────────────
  app:
    build:
      context: .
      dockerfile: docker/php/Dockerfile
    container_name: charlesnadejda_app
    restart: unless-stopped
    working_dir: /var/www
    volumes:
      - ./site-laravel:/var/www   # Code Laravel monté en live-reload
    networks:
      - cn_network
    depends_on:
      - mysql

  nginx:
    image: nginx:alpine
    container_name: charlesnadejda_nginx
    restart: unless-stopped
    ports:
      - "8000:80"                # Site Laravel → http://localhost:8000
    volumes:
      - ./site-laravel:/var/www
      - ./docker/nginx/default.conf:/etc/nginx/conf.d/default.conf
    networks:
      - cn_network
    depends_on:
      - app

  # ─── phpMyAdmin ──────────────────────────────────────────────────
  phpmyadmin:
    image: phpmyadmin:latest
    container_name: charlesnadejda_pma
    restart: unless-stopped
    ports:
      - "8080:80"                # phpMyAdmin → http://localhost:8080
    environment:
      PMA_HOST: mysql
      PMA_PORT: 3306
      MYSQL_ROOT_PASSWORD: root
    networks:
      - cn_network
    depends_on:
      - mysql

volumes:
  mysql_data:

networks:
  cn_network:
    driver: bridge
```

---

## 🐳 `docker/php/Dockerfile`

```dockerfile
FROM php:8.3-fpm

# Extensions nécessaires pour Laravel + MySQL
RUN apt-get update && apt-get install -y \
    libpng-dev \
    libjpeg-dev \
    libfreetype6-dev \
    zip \
    unzip \
    git \
    curl \
    && docker-php-ext-install pdo pdo_mysql mbstring exif pcntl bcmath gd

# Composer
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www

USER www-data
```

---

## 🐳 `docker/nginx/default.conf`

```nginx
server {
    listen 80;
    server_name localhost;
    root /var/www/public;
    index index.php index.html;

    location / {
        try_files $uri $uri/ /index.php?$query_string;
    }

    location ~ \.php$ {
        fastcgi_pass app:9000;
        fastcgi_index index.php;
        fastcgi_param SCRIPT_FILENAME $realpath_root$fastcgi_script_name;
        include fastcgi_params;
    }

    location ~ /\.ht {
        deny all;
    }
}
```

---

## 🚀 Démarrage du projet (première fois)

```bash
# 1. Cloner / ouvrir le projet dans VS Code ou autre éditeur

# 2. Démarrer Docker Desktop (s'assurer qu'il tourne)

# 3. Lancer tous les containers
docker-compose up -d --build

# 4. Vérifier que tout tourne
docker-compose ps
# Doit afficher 4 containers : mysql, app, nginx, phpmyadmin — tous "Up"

# 5. Créer le projet Laravel dans site-laravel/
docker-compose exec app composer create-project laravel/laravel .

# 6. Configurer le .env Laravel
# Copier .env.example → .env et modifier :
APP_URL=http://localhost:8000

DB_CONNECTION=mysql
DB_HOST=mysql          # ← NOM DU SERVICE Docker, PAS localhost !
DB_PORT=3306
DB_DATABASE=charlesnadejda
DB_USERNAME=root
DB_PASSWORD=root

# 7. Générer la clé Laravel
docker-compose exec app php artisan key:generate

# 8. Importer le schéma BDD (si pas auto-importé via initdb.d)
docker exec -i charlesnadejda_mysql mysql -uroot -proot charlesnadejda \
  < sql/create_database.sql

docker exec -i charlesnadejda_mysql mysql -uroot -proot charlesnadejda \
  < sql/seed_data.sql

# 9. Vérifier
# → http://localhost:8000  = Page Laravel (Laravel welcome page)
# → http://localhost:8080  = phpMyAdmin (user: root, pass: root)
```

---

## 🔄 Commandes quotidiennes

```bash
# Démarrer l'environnement
docker-compose up -d

# Arrêter l'environnement (sans supprimer les données)
docker-compose stop

# Voir les logs Laravel/PHP
docker-compose logs app

# Exécuter une commande Artisan
docker-compose exec app php artisan migrate
docker-compose exec app php artisan cache:clear
docker-compose exec app php artisan route:list

# Ouvrir un shell dans le container PHP
docker-compose exec app bash

# Installer un package Composer (ex: Stripe)
docker-compose exec app composer require stripe/stripe-php

# Tout réinitialiser (ATTENTION : supprime les données MySQL)
docker-compose down -v
```

---

## 🖥️ App C# — Connexion à MySQL Docker

L'app Windows Forms se connecte à MySQL exactement comme avec WAMP.
Docker mappe le port 3306 du container sur `localhost:3306` de ta machine.

**String de connexion dans `config/database.cs` :**

```csharp
public static class DatabaseConfig
{
    // MySQL tourne dans Docker mais est accessible sur localhost:3306
    public const string ConnectionString =
        "Server=localhost;" +
        "Port=3306;" +
        "Database=charlesnadejda;" +
        "Uid=root;" +
        "Pwd=root;" +
        "CharSet=utf8mb4;";
}
```

> Si tu veux utiliser le user dédié (recommandé) :
> `Uid=cn_user;Pwd=cn_password;`

---

## 🔔 Stripe Webhook en local (pour les tests de paiement)

Stripe ne peut pas appeler `localhost:8000` directement. Il faut un tunnel.
Deux options :

**Option 1 — Stripe CLI (recommandé, gratuit)**
```bash
# Installer Stripe CLI : https://stripe.com/docs/stripe-cli
stripe login
stripe listen --forward-to localhost:8000/webhook/stripe
# → Affiche une clé webhook temporaire à mettre dans .env : STRIPE_WEBHOOK_SECRET=whsec_...
```

**Option 2 — ngrok**
```bash
ngrok http 8000
# → Donne une URL publique ex: https://abc123.ngrok.io
# → Configurer dans Stripe Dashboard : https://abc123.ngrok.io/webhook/stripe
```

---

## 📝 Notes importantes

- Le fichier `sql/create_database.sql` est **auto-importé** au premier démarrage du container MySQL (via `docker-entrypoint-initdb.d`). Si la DB existe déjà, il n'est pas réexécuté.
- Pour réinitialiser complètement la BDD : `docker-compose down -v` puis `docker-compose up -d`
- Le dossier `site-laravel/` est monté en volume → toute modification de fichier est **immédiatement visible** sans reconstruire le container.
- Le prof utilise WAMP : si tu dois lui montrer le projet sur sa machine, tu peux exporter la BDD via phpMyAdmin et l'importer dans son WAMP en 2 minutes.
