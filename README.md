# File-Split-n-Join
Поиск данных в файле.
Поиск возможен по **byte**-данным, а для текстовых данных можно искать по тексту или регулярному выражению.
За анализ данных отвечают так называемые поисковые юниты: `MatchUnitBytes`, `MatchUnitText` и `MatchUnitRegexp`
При помощи поисковых юнитов есть возможность задействовать фильтр на поток чтения данных.
Таким образом поисковые юниты предварительно из потока будут выщечащть "мусор", а механизм поиска и анализа данных будет использовать уже "модифицированный поток данных".
Модификации подтвергается не файл, а поток в процессе чтения.
Фильтр может не только удалять данные на лету, но и заменять их на свои. 

Эта библиотека, например, нашла применение в парсинге логов.
Юниты в фильтре меняли на лету часто повторяющееся длинное техническое имя сервера на короткое и понятное, а так же удаляли часть технических строковых данных, которые были излишними для дальнейших манипуляций.
Одна особенность логов asp .net core (by linux), что в каждую строку там дабовляются последовательность символов в качестве маркера статуса сообщения.

Исходные данные:
```
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]: #033[40m#033[37mdbug#033[39m#033[22m#033[49m: Microsoft.AspNetCore.Server.Kestrel[1]
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]:       Connection id "0HLQ6660KEG7O" started.
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]: #033[40m#033[32minfo#033[39m#033[22m#033[49m: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]:       Request starting HTTP/1.1 GET http://localhost:5000/ping
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]: #033[40m#033[32minfo#033[39m#033[22m#033[49m: Microsoft.EntityFrameworkCore.Infrastructure[10403]
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]:       Entity Framework Core 2.2.6-servicing-10079 initialized 'AppDbContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: SensitiveDataLoggingEnabled
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]: #033[40m#033[37mdbug#033[39m#033[22m#033[49m: Microsoft.EntityFrameworkCore.Database.Connection[20000]
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]:       Opening connection to database 'my-db' on server '127.0.0.1'.
Oct  1 06:25:23 765331-vds-sserv333999 dotnet[7307]: #033[40m#033[37mdbug#033[39m#033[22m#033[49m: Microsoft.EntityFrameworkCore.Database.Connection[20001]
```

Настройка фильтра:
```c#
AdapterFileScanner LogFileScanner = new AdapterFileScanner();
LogFileScanner.OpenFile("/var/log/syslog");
/*
Следует размещать в начало фильтра именно `MatchUnitBytes` т.к. они самые "быстрые". Последующим фильтрам тогда не потребуется чекать эти данные
Текстовые (и, тем более, regexp) юниты преобразовывают читаемый поток в строку после каждого прочитаного байта.
*/
LogFileScanner.FileFilteredReadStream.Scanner.AddMatchUnit(new MatchUnitBytes(AdapterFileReader.EncodingMode.GetBytes("#033[39m#033[22m#033[49m"), System.Array.Empty<byte>()));
LogFileScanner.FileFilteredReadStream.Scanner.AddMatchUnit(new MatchUnitBytes(AdapterFileReader.EncodingMode.GetBytes("#033[40m#033[1m#033[33m"), System.Array.Empty<byte>()));
LogFileScanner.FileFilteredReadStream.Scanner.AddMatchUnit(new MatchUnitBytes(AdapterFileReader.EncodingMode.GetBytes("#033[40m#033[37m"), System.Array.Empty<byte>()));
LogFileScanner.FileFilteredReadStream.Scanner.AddMatchUnit(new MatchUnitBytes(AdapterFileReader.EncodingMode.GetBytes("#033[40m#033[32m"), System.Array.Empty<byte>()));
//
Regex PrefixRegex = new Regex(@"^[^\]]+\]:\s", RegexOptions.Compiled);
LogFileScanner.FileFilteredReadStream.Scanner.AddMatchUnit(new MatchUnitRegexp(PrefixRegex, 22, Array.Empty<byte>()));
```

после фильтра поток для сканера/парсера принимал такой вид:
```
dbug: Microsoft.AspNetCore.Server.Kestrel[1]
      Connection id "0HLQ6660KEG7O" started.
minfo: Microsoft.AspNetCore.Hosting.Internal.WebHost[1]
      Request starting HTTP/1.1 GET http://localhost:5000/ping
minfo: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core 2.2.6-servicing-10079 initialized 'AppDbContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: SensitiveDataLoggingEnabled
dbug: Microsoft.EntityFrameworkCore.Database.Connection[20000]
      Opening connection to database 'my-db' on server '127.0.0.1'.
dbug: Microsoft.EntityFrameworkCore.Database.Connection[20001]
```
далее парсеру оставалось эти данные сегментировать и записать в хранилище (например в базу данных)

Таким образмо, благодаря фильтрам, служба сканирования/парсинга получала поток вдвое меньше по размеру и и втрое читабельнее по содержанию.

\
P.S.
Данная библитека задействована в [WPF Win App. File-Split-n-Join](https://github.com/badhitman/File-Split-n-Join-WPF)
