# AspWebGen
ASP.NET Web Form generation framework

## Введение
Данный проект предназначен для упрощения разработки ПО, при иcпользовании ASP.NET WebForms, MS SQL и компонент Ext.Net.
Разработан и используется на нескольких проектах компании.

Выполняется проектирование БД в PowerDesigner, на выходе получаются скрипты для БД и генерируются исходные коды 
для работы с таблицами и представлениями. 
Формируются разные поля в соответствии с типами колонок и ссылками на другие таблицы/представления.

Для PowerDesigner разработано расширение, которое добавляет таблицам, представлениям, колонкам и другим объектам 
дополнительные атрибуты, на основании этого генерируются исходные коды.
Для реализации логики используются partial методы, внедренные в нужные места генерируемого кода.
Справочные таблицы обычно не содержат дополнительной логики.

Пример приложения: https://GitHub.com/NATKazakhstan/AspWebGenSample

## Getting Started in Visual Studio

Создать проект "Веб приложение ASP.NET (.NET Framework)" - .NET Framework 4.0 или выше. 

Добавить ссылки на проекты или скомпилированные библиотеки AspWebGen. 
Добавить ссылку на Microsoft.JScript. 
Установить nuget пакет Ext.NET 2.5.3.1 (совместимость с более старшей версией 3, 4 не производилась). 
Добавить файл "DB.dbml" - модель LINQ to SQL.
Скопировать из примера приложения файлы EmptyPage.aspx, MainPage.aspx, IFrameSite.Master, 
AutoCompleteHandler.ashx, DataSourceViewHandler.ashx, LinqToJavaScriptHandler.ashx.
Скопировать настройки файла Web.Config из примера, 
разделы: configSections, assemblyBinding, Nat.SqlDbInitializer, Nat.Initializer, Nat.WebReportManager.

После генерации исходных кодов из PD включить новые файлы в проект. 

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
