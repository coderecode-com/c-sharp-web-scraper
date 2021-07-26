using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CsvHelper;
using System.IO;
using System.Globalization;
namespace scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://books.toscrape.com/catalogue/category/books/travel_2/index.html";
            var links = GetBookLinks(url);
            List<Book> books = GetBooks(links);
            Export(books);
            // Console.Write(doc.DocumentNode.InnerHtml);
        }

        private static void Export(List<Book> books)
        {
            using (var writer = new StreamWriter("./books.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(books);
            }

        }

        private static List<Book> GetBooks(List<string> links)
        {
            var books = new List<Book>();
            foreach (var link in links)
            {
                var doc = GetDocument(link);
                var book = new Book();
                book.Title = doc.DocumentNode.SelectSingleNode("//h1").InnerText;
                var xpath = "//*[@class=\"col-sm-6 product_main\"]/*[@class=\"price_color\"]";
                var price_raw = doc.DocumentNode.SelectSingleNode(xpath).InnerText;
                book.Price = ExtractPrice(price_raw);
                books.Add(book);
            }
            return books;
        }

        static double ExtractPrice(string raw)
        {
            var reg = new Regex(@"[\d\.,]+", RegexOptions.Compiled);
            var m = reg.Match(raw);
            if (!m.Success)
                return 0;
            return Convert.ToDouble(m.Value);
        }
        static List<string> GetBookLinks(string url)
        {
            var doc = GetDocument(url);
            var linkNodes = doc.DocumentNode.SelectNodes("//h3/a");

            var baseUri = new Uri(url);
            var links = new List<string>();
            foreach (var node in linkNodes)
            {
                var link = node.Attributes["href"].Value;
                link = new Uri(baseUri, link).AbsoluteUri;
                links.Add(link);
            }
            return links;
        }

        static HtmlDocument GetDocument(string url)
        {
            var web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }
    }
}
