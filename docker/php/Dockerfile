# ============================================================
# PHP 8.3-FPM — Image pour Laravel (projet Charles & Nadejda)
# ============================================================
FROM php:8.3-fpm

# Extensions système nécessaires pour Laravel + MySQL
RUN apt-get update && apt-get install -y \
    libpng-dev \
    libjpeg-dev \
    libfreetype6-dev \
    libzip-dev \
    libonig-dev \
    libxml2-dev \
    zip \
    unzip \
    git \
    curl \
    && docker-php-ext-configure gd --with-freetype --with-jpeg \
    && docker-php-ext-install \
        pdo \
        pdo_mysql \
        mbstring \
        exif \
        pcntl \
        bcmath \
        gd \
        zip \
        xml \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# OPcache optimisé — élimine la lenteur du bind mount Windows/Docker
COPY docker/php/opcache.ini /usr/local/etc/php/conf.d/opcache-custom.ini

# Composer (copié depuis l'image officielle)
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www

# L'user www-data évite les problèmes de permissions sur les fichiers Laravel
USER www-data
