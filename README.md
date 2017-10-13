# AspWebGen
ASP.NET Web Form generation framework

## Введение
Данный проект предназначен для упрощения разработки ПО, при использовании ASP.NET WebForms, MS SQL и компонент Ext.Net.
Разработан и используется на нескольких проектах компании.

Выполняется проектирование БД в PowerDesigner, на выходе получаются скрипты для БД и генерируются исходные коды 
для работы с таблицами и представлениями. 
Формируются разные поля в соответствии с типами колонок и ссылками на другие таблицы/представления.

Для PowerDesigner 16.5 разработано расширение, которое добавляет таблицам, представлениям, колонкам и другим объектам 
дополнительные атрибуты, на основании этого генерируются исходные коды.
Для реализации логики используются partial методы, внедренные в нужные места генерируемого кода.
Справочные таблицы обычно не содержат дополнительной логики.

Пример приложения: https://GitHub.com/NATKazakhstan/AspWebGenSample

Wiki: https://github.com/NATKazakhstan/AspWebGen/wiki

## Getting Started in Visual Studio

Необходимые шаги:
* Установить Visual Studio 2017 с компонентами "ASP.NET и разработка Web-приложений", "Инструменты LINQ to SQL" (вкладка отдельные компоненты)
* Клонировать репозиторий [AspWebGen](https://github.com/NATKazakhstan/AspWebGen.git) в отдельную папку решения, например, AspWebGen.
* Скомпилировать Core.sln во Visual Studio 2017.
* Скомпилировать Tools.sln во Visual Studio 2017.
* Создать проект "Веб приложение ASP.NET (.NET Framework)" - .NET Framework 4.0 или выше.

В созданном проекте:
* Добавить ссылки на все проекты из Core.sln.
* Добавить ссылку на Microsoft.JScript. 
* Установить nuget пакет Ext.NET 2.5.3.1 (совместимость с более старшей версией 3, 4 не производилась). 
* Добавить модель LINQ to SQL с названием файла "DB.dbml".
* Скопировать из [примера приложения](https://GitHub.com/NATKazakhstan/AspWebGenSample) файлы EmptyPage.aspx, MainPage.aspx, IFrameSite.Master, AutoCompleteHandler.ashx, DataSourceViewHandler.ashx, LinqToJavaScriptHandler.ashx.
* Скопировать из примера приложения настройки файла Web.Config, разделы: configSections, assemblyBinding, Nat.SqlDbInitializer, Nat.Initializer, Nat.WebReportManager.

После генерации исходных кодов из PowerDesigner включить новые файлы в проект, за исключением файлов из папок с названием, начинающимся с символа "_", они являются шаблонами. 

Для использования файлов с расширением ".SDBML", которые обновляют модель данных DB.dbml в соответствии с генерированной моделью из PowerDesigner:
* В контекстном меню нажать "открыть с помощью"
* В открывшемся окне нажать "добавить"
* Указать программу AspWebGen\BuildTools\SyncDbmlByScript\SyncDbmlByScript.exe, нажать ОК
* Нажать "По умолчанию", ОК

После внесения изменений в модель и генерации исходных кодов для актуализации модели данных DB.dbml достаточно запустить файл с расширением ".SDBML" либо в папке с проектом на все таблицы/представления, либо с именем нужной таблицы/представления.

## Getting Started in PowerDesigner (PD)
Расширения из директории "Extended Model Definitions" для PowerDesigner необходимо скопировать в 
"C:\Program Files (x86)\Sybase\PowerDesigner 16\Resource Files\Extended Model Definitions\" 
(предварительно PD закрыть, запускать от имени администратора)

После можно создать модель "Physical Data Model" (PDM) и добавить расширение "UserControlsGenerator" - 
в меню Model/Extensions нажать "Attach an Extension", поставить галочку для "UserControlsGenerator". 
Создать Package, указать Code и Comment, в нем создать таблицу, указать Code и Comment.
Добавить необходимые колонки с указанием Code, Comment, DataType, Mandatory, ...
У таблицы на вкладке "Генерация UserControls" выбрать одно из обязательных полей в атрибуте NameColumn.

Обязательно добавить колонку RowVersion, тип данных timestamp, 
на вкладке "Генерация UserControls" убрать галочки GridViewVisible, DetailsViewVisible, FilterVisible, Логирование поля.

У таблицы обычно и по умолчанию используется колонка id - идентификатор, инкрементируется базой данных, т.е. identity, и обычно не отображается.

Code используется как название поля, проекта, класса и др. Comment и Name - названия для пользователей.

Для генерации можно пользоваться стандартным вызовом из меню - Tools/Extended Generation, выбрать Package, указать путь к папке решения и OK.
Или же в контекстном меню у таблицы или Package "Генерировать UserControls", указать путь к папке решения и ОК
