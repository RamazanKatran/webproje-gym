# Migration Uygulama Talimatları

## Sorun
`ApplicationUserId` kolonu veritabanında yok. Migration dosyası oluşturuldu ama veritabanına uygulanmadı.

## Çözüm

### Visual Studio Package Manager Console'da:

```powershell
Update-Database
```

### Veya Terminal'de (EF Core Tools yüklüyse):

```bash
dotnet ef database update
```

## Kontrol

Migration uygulandıktan sonra, SQL Server Management Studio veya başka bir araçla `Trainers` tablosunda `ApplicationUserId` kolonunun olup olmadığını kontrol edin.

