# Android İmza Yapılandırması

Yayın APK/AAB imzası `AbxPilot.Android.csproj` içinde ortam değişkenlerinden okunur.
Keystore dosyası ve parolası **repoya girmez**; kullanıcı kendi makinesinde üretir.

## 1. Keystore Üretimi

```powershell
keytool -genkeypair -v `
  -keystore abxpilot-release.keystore `
  -alias abxpilot `
  -keyalg RSA -keysize 2048 -validity 10000 `
  -storetype PKCS12
```

Keytool parola sorar; dosyayı `tmp/` dışında, repo kapsamı dışındaki güvenli bir yere koy
(ör. `%USERPROFILE%\.abxpilot\abxpilot-release.keystore`). `trash/` veya git'e ASLA girmez.

## 2. Ortam Değişkenleri

Derlemeden önce ayarla (PowerShell 5.1, kalıcı için `setx`, oturumluk için `$env:`):

```powershell
$env:ANDROID_SIGNING_KEY_STORE = "$env:USERPROFILE\.abxpilot\abxpilot-release.keystore"
$env:ANDROID_SIGNING_KEY_ALIAS = "abxpilot"
$env:ANDROID_SIGNING_KEY_PASS  = "<anahtar parolası>"
$env:ANDROID_SIGNING_STORE_PASS = "<keystore parolası>"
```

`.csproj`, bu dört değişkeni MSBuild'in tanıdığı `AndroidSigningKeyStore`,
`AndroidSigningKeyAlias`, `AndroidSigningKeyPass`, `AndroidSigningStorePass`
özelliklerine eşler; yalnız hepsi boş değilse `AndroidKeyStore=true` olur:

```xml
<AndroidSigningKeyStore Condition="'$(AndroidSigningKeyStore)' == '' and '$(ANDROID_SIGNING_KEY_STORE)' != ''">$(ANDROID_SIGNING_KEY_STORE)</AndroidSigningKeyStore>
```

## 3. Yayın Derlemesi

```powershell
dotnet publish src\AbxPilot.Android\AbxPilot.Android.csproj -c Release -f net10.0-android
```

Ortam değişkenleri boşsa derleme **imzasız** (veya varsayılan debug anahtarıyla) başarıyla
tamamlanır — CI ve ilk kurulum bu yüzden bozulmaz. Değişkenler doluysa çıktı `abxpilot-release.keystore`
ile imzalanır.

## 4. Parolayı Asla

- `.csproj`, `appsettings`, commit mesajı veya `docs/` içine parola yazma.
- Keystore dosyasını repoya ekleme; kaybedilirse Play Store güncellemesi imkânsız hale gelir,
  yedeğini repo dışında iki yerde tut.
