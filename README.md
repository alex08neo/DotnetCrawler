# DotnetCrawler
DotnetCrawler is a straightforward, lightweight web crawling/scrapying library for Entity Framework Core output based on dotnet core.
This library designed like other strong crawler libraries like WebMagic and Scrapy but for enabling extandable your custom requirements. 

## Getting Started

This project intented for providing EF.Core database insert but it has very scale architecture in order to implement your custom scenarios. So the main design of architecture is very common for web crawler/scrapying frameworks, you can see below image.

![alt text](https://raw.githubusercontent.com/dotnetcore/DotnetSpider/master/images/DESIGN.jpg)

As per above image, in this library created project structures including DotnetCrawler.Request-Downloader-Processor-Pipeline projects. 

### Usage

You can use this library using DotnetCrawler class with builder pattern in your console applications;

```
 var crawler = new DotnetCrawler<Catalog>()
                                 .AddRequest(new DotnetCrawlerRequest { Url = "https://www.ebay.com/b/Apple-iPhone/9355/bn_319682", Regex = @".*itm/.+", TimeOut = 5000 })
                                 .AddDownloader(new DotnetCrawlerDownloader { DownloderType = DotnetCrawlerDownloaderType.FromMemory })
                                 .AddProcessor(new DotnetCrawlerProcessor<Catalog> { })
                                 .AddPipeline(new DotnetCrawlerPipeline<Catalog> { });

await crawler.Crawle();
```

Catalog is a generic type of DotnetCrawler and also generated by EF.Core scaffolding command in .Data project. DotnetCrawler.Data project installed EF.Core nuget pagkages and run this command in Package Manager Console.
```
Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=Microsoft.eShopOnWeb.CatalogDb;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
```
By this command DotnetCrawler.Data project created Model folder and all entities in eShopOnWeb Microsoft's example.
After that need to configure your entity class with custom crawler attributes;
```
[DotnetCrawlerEntity(XPath = "//*[@id='LeftSummaryPanel']/div[1]")]
public partial class Catalog : IEntity
{
    public int Id { get; set; }
    [DotnetCrawlerField(Expression = "1", SelectorType = SelectorType.FixedValue)]
    public int CatalogBrandId { get; set; }
    [DotnetCrawlerField(Expression = "1", SelectorType = SelectorType.FixedValue)]
    public int CatalogTypeId { get; set; }        
    public string Description { get; set; }
    [DotnetCrawlerField(Expression = "//*[@id='itemTitle']/text()", SelectorType = SelectorType.XPath)]
    public string Name { get; set; }
    public string PictureUri { get; set; }
    public decimal Price { get; set; }

    public virtual CatalogBrand CatalogBrand { get; set; }
    public virtual CatalogType CatalogType { get; set; }
}
```
With this code, basically, crawler requested given url and try to find given attributes which defined xpath addresses for target web url.

Whe run this code, first create your request which url want to consume in your Request object. In these request object you can also provide Regex expression in order to filter your web urls. The process runs as define Request - set Downloader - set Processor - run Pipeline.

### Example of eShopOnWeb Microsoft Project Usage

This library also include example project which name is DotnetCrawler.Sample. Basically, in this sample project, implementing Microsoft eShopOnWeb repository. You can find this repo [here](https://github.com/dotnet-architecture/eShopOnWeb)
So in this example repo, implemented e-commerce project, it has "Catalog" table when you generate with EF.Core code-first approach.
Passing "Catalog" table as a generic type in DotnetCrawler class.
```
var crawler = new DotnetCrawler<Catalog>()
```
### DotnetWebCrawler.Request

DotnetWebCrawler's example use this Catalog table and load this table by crawling [eBay website](https://www.ebay.com/b/Apple-iPhone/9355/bn_319682). Also aplying Regex filter for crawling only phone web pages in eBay. (Regex = @".*itm/.+")

All these parameters defined into DotnetCrawlerRequest class;
```
 .AddRequest(new DotnetCrawlerRequest { Url = "https://www.ebay.com/b/Apple-iPhone/9355/bn_319682", Regex = @".*itm/.+", TimeOut = 5000 })
```
### DotnetWebCrawler.Downloader
DotnetWebCrawler's example use downloader which provide to use System.Web and download targeted url with various types. You can find different types of usage as below code block;

```
.AddDownloader(new DotnetCrawlerDownloader { DownloderType = DotnetCrawlerDownloaderType.FromFile, DownloadPath = @"C:\DotnetCrawlercrawler\" })

.AddDownloader(new DotnetCrawlerDownloader { DownloderType = DotnetCrawlerDownloaderType.FromMemory })

.AddDownloader(new DotnetCrawlerDownloader { DownloderType = DotnetCrawlerDownloaderType.FromWeb })

```
Definitions of these types explains;

```
/// <summary>
    /// Type of the downloaders when crawler download source web
    /// </summary>
    public enum DotnetCrawlerDownloaderType
    {
        /// <summary>
        /// Download to local file
        /// </summary>
        FromFile,
        /// <summary>
        /// Without downloading to local file, download temp and directly use
        /// </summary>
        FromMemory,
        /// <summary>
        /// Read direct from web
        /// </summary>
        FromWeb
    }
```
### DotnetWebCrawler.Processor

DotnetWebCrawler's example use processor which provide to use HtmlAgilityPack and process html pages as per intented xpaths.
```
.AddProcessor(new DotnetCrawlerProcessor<Catalog> { })
```
DotnetWebCrawler has default processor which name is DotnetCrawlerProcessor.cs. You can pass EF.Core entity if you provide attributes and derived from IEntity.cs.
In addition you can create your own processor in order to apply your custom scenarios.

### DotnetWebCrawler.Pipeline

DotnetWebCrawler's example use pipeline which provide to use EF.Core and insert records into EF.Core database.
```
.AddPipeline(new DotnetCrawlerPipeline<Catalog> { });
```
DotnetWebCrawler has default pipeline which name is DotnetCrawlerPipeline.cs. You can pass EF.Core entity if you provide attributes and derived from IEntity.cs
In addition you can create your own pipeline in order to apply your custom scenarios.

## Requirements

Before run the program you must configure your EF.Core entities. First need to scaffolding and after that you should devired from IEntity and should configure XPath definitions with attributes on entity property fields.
First;
```
Scaffold-DbContext "Server=(localdb)\mssqllocaldb;Database=Microsoft.eShopOnWeb.CatalogDb;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
```
Second; Configure your table OnModelCreating method, change id generator strategy and change MaxLength to 200.
```
protected override void OnModelCreating(ModelBuilder modelBuilder)
 {   
     modelBuilder.Entity<Catalog>(entity =>
     {
         entity.HasIndex(e => e.CatalogBrandId);

         entity.HasIndex(e => e.CatalogTypeId);

         //entity.Property(e => e.Id).ValueGeneratedNever();
         entity.Property(e => e.Id).ForSqlServerUseSequenceHiLo("catalog_hilo").IsRequired();

         entity.Property(e => e.Name)
             .IsRequired()
             .HasMaxLength(200);
             
    .....             
```
At last, In example of eBay crawler usage Catalog table as below way;
```
[DotnetCrawlerEntity(XPath = "//*[@id='LeftSummaryPanel']/div[1]")]
public partial class Catalog : IEntity
{
   public int Id { get; set; }
   [DotnetCrawlerField(Expression = "1", SelectorType = SelectorType.FixedValue)]
   public int CatalogBrandId { get; set; }
   [DotnetCrawlerField(Expression = "1", SelectorType = SelectorType.FixedValue)]
   public int CatalogTypeId { get; set; }        
   public string Description { get; set; }
   [DotnetCrawlerField(Expression = "//*[@id='itemTitle']/text()", SelectorType = SelectorType.XPath)]
   public string Name { get; set; }
   public string PictureUri { get; set; }
   public decimal Price { get; set; }

   public virtual CatalogBrand CatalogBrand { get; set; }
   public virtual CatalogType CatalogType { get; set; }
}
```

## Background of DotnetCrawler

Development environments;

* Visual Studio 2017
* .Net Core 2.2 or later
* EF Core 2.2 or later

## Next Releases

This program only imported EF.Core and using default downloader-processor-pipeline classes. And this program only solve spesific problem of customer requirements. So it will evolve a product and extent with new features as listed below;

* Extend with different database providers. 
* Extend for different downloader-processor-pipeline implementations which requested with different aproaches.
* Use with hangfire, quartz schedular frameworks in order to schedule and use async functions.


## Authors

* **Mehmet Ozkaya** - *Initial work* - [mehmetozkaya](https://github.com/mehmetozkaya)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Buy Me a Coffee

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
