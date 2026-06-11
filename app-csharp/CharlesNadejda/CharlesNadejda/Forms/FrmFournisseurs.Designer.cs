// Designer.cs vidé — FrmFournisseurs hérite désormais de FrmListeBase<Fournisseur>
// qui construit tout le layout en code. Ce fichier est conservé car le .csproj
// le référence encore (DependentUpon FrmFournisseurs.cs).
//
// Le Designer VS ne peut pas ouvrir les classes héritant d'un Form générique
// (limitation connue) — pas de blocage car FrmListeBase gère tout en code.
