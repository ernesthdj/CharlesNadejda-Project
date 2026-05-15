# 09 — Dependances NuGet
> Packages externes et leur justification
> Derniere mise a jour : 2026-05-16

## Vue d'ensemble

Le projet cible **.NET Framework 4.8.1** (WinForms classique). Les dependances sont gerees via `packages.config` et le dossier `packages/` (format NuGet legacy).

---

## Packages principaux

| Package | Version | Justification |
|---------|---------|---------------|
| **MySql.Data** | 9.6.0 | Connecteur ADO.NET officiel pour MySQL/MariaDB. Utilise par `DbHelper.GetConnection()` dans toute la couche DAL. |
| **BCrypt.Net-Next** | 4.1.0 | Hachage de mots de passe. Interoperabilite avec le backend PHP/Laravel qui utilise le meme algorithme BCrypt. Permet de partager la table `utilisateurs` entre les deux applications. |
| **Newtonsoft.Json** | 13.0.4 | Serialisation/deserialisation JSON. Utilise pour la communication de donnees structurees et la persistance de configurations. |
| **BouncyCastle.Cryptography** | 2.6.2 | Bibliotheque cryptographique. Requise par MySql.Data pour etablir des connexions TLS/SSL vers le serveur MySQL. |

---

## Packages de support (dependances transitives)

| Package | Version | Role |
|---------|---------|------|
| **Google.Protobuf** | 3.32.0 | Serialisation Protocol Buffers — dependance du protocole MySQL X DevAPI. |
| **K4os.Compression.LZ4** | 1.3.8 | Compression LZ4 — utilisee par MySql.Data pour la compression du protocole. |
| **K4os.Compression.LZ4.Streams** | 1.3.8 | Streams LZ4 — extension de K4os.Compression.LZ4. |
| **K4os.Hash.xxHash** | 1.0.8 | Hash xxHash — verification d'integrite rapide pour le connecteur MySQL. |
| **ZstdSharp.Port** | 0.8.6 | Compression Zstandard — option de compression alternative pour MySQL. |

---

## Packages systeme (backports .NET)

Ces packages apportent des fonctionnalites de .NET Core/5+ vers .NET Framework 4.8.1 :

| Package | Version | Role |
|---------|---------|------|
| **System.Buffers** | 4.6.1 | `ArrayPool<T>` — gestion efficace des buffers memoire. |
| **System.Memory** | 4.6.3 | `Span<T>`, `Memory<T>` — acces memoire zero-copie. |
| **System.Numerics.Vectors** | 4.6.1 | Types SIMD vectoriels. |
| **System.IO.Pipelines** | 5.0.2 | I/O haute performance par pipelines — utilise par le connecteur MySQL. |
| **System.Threading.Tasks.Extensions** | 4.5.4 | `ValueTask<T>` — support async ameliore. |
| **System.Runtime.CompilerServices.Unsafe** | 6.1.2 | Operations memoire unsafe — infrastructure pour System.Memory. |
| **Microsoft.Bcl.AsyncInterfaces** | 5.0.0 | `IAsyncEnumerable<T>`, `IAsyncDisposable` — interfaces async modernes. |
| **System.Configuration.ConfigurationManager** | 8.0.0 | Acces aux fichiers App.config — requis par MySql.Data. |

---

## References framework .NET

| Assembly | Usage |
|----------|-------|
| `System` | Types de base |
| `System.Core` | LINQ, expressions |
| `System.Data` | ADO.NET |
| `System.Drawing` | GDI+ (UI WinForms) |
| `System.Windows.Forms` | Framework UI |
| `System.Xml` | Parsing XML |
| `System.Xml.Linq` | LINQ to XML |
| `System.Net.Http` | HttpClient |
| `System.Numerics` | BigInteger, Complex |
| `System.Configuration` | ConfigurationManager |
| `System.Transactions` | Transactions distribuees |
| `System.Management` | WMI |
| `System.Data.DataSetExtensions` | Extensions DataSet |
| `System.Deployment` | ClickOnce |
| `Microsoft.CSharp` | Binding dynamique |

---

## Securite des dependances

| Aspect | Statut |
|--------|--------|
| BCrypt pour les mots de passe | OK — jamais de hash maison |
| Connexion DB via TLS (BouncyCastle) | OK — chiffrement en transit |
| Pas de dependance CDN cote client | OK — application desktop standalone |
| Versions fixes dans packages.config | OK — reproductibilite des builds |

---

## Mise a jour

Pour mettre a jour les packages :
```
# Depuis la console NuGet Package Manager dans Visual Studio
Update-Package MySql.Data
Update-Package BCrypt.Net-Next
```

Verifier la compatibilite .NET Framework 4.8.1 avant toute mise a jour majeure.

---

## Communaute graphify

C20
