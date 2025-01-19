using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using static GetDataFromUrlInBrowserFormsApp.Form1;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace GetDataFromUrlInBrowserFormsApp
{
    public partial class Form1 : Form
    {

        public class JobListing
        {
            public string CompanyName { get; set; }
            public string JobTitle { get; set; }
            public string Location { get; set; }
            public string TimeAgo { get; set; }
            public string JobContent { get; set; }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            // appsettings
            var keyWord = configuration.GetSection("AppSettings")["keyWord"];
            var postFix = configuration.GetSection("AppSettings")["postFix"];
            var siteLink = configuration.GetSection("AppSettings")["siteUrl"];

            string filter = configuration.GetSection("AppSettings")["filter"];
            string siteUrl = siteLink + keyWord + postFix + filter  ;

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(siteUrl);

            await Task.Delay(8000); // Delay 

            var htmlContent1 = webBrowser1.Document.Body.OuterHtml;
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlContent1);

            var articles = doc.DocumentNode.Descendants("article").ToList();
            var jobListings = new List<JobListing>();

            var counter = 0; //for debugging


            foreach (var article in articles)
            {
                var jobListing = new JobListing();
                counter++;

                // Location (Standort) extrahieren
                var locationNode = article.Descendants("span")
                                           .FirstOrDefault(x => x.Attributes.Contains("data-at") && x.Attributes["data-at"].Value == "job-item-location");
                if (locationNode != null)
                {
                    jobListing.Location = locationNode.InnerText.Trim();
                }

                var jobTitleNode = article.Descendants("div")
                                .FirstOrDefault(x => x.HasClass("res-nehv70")); 
                                            
                if (jobTitleNode != null)
                {
                    jobListing.JobTitle = jobTitleNode.InnerText.Trim();

                }
                    // Company Name extrahieren
                    var companyNameNode = article.Descendants("span")
                                             .FirstOrDefault(x => x.Attributes.Contains("data-at") && x.Attributes["data-at"].Value == "job-item-company-name");
                if (companyNameNode != null)
                {
                    jobListing.CompanyName = companyNameNode.InnerText.Trim();
                }

               // timeStamp
                var timeNode = article.Descendants("time")
                                      .FirstOrDefault();
                if (timeNode != null)
                {
                    jobListing.TimeAgo = timeNode.InnerText.Trim();
                }


                if (!string.IsNullOrEmpty(jobListing.Location) &&
                    !string.IsNullOrEmpty(jobListing.CompanyName) &&
                    !string.IsNullOrEmpty(jobListing.TimeAgo))
                {
                    jobListings.Add(jobListing);
                }
            }

            // Ausgabe der extrahierten Daten
            foreach (var job in jobListings)
            {
                Console.WriteLine($"Firma: {job.CompanyName}");
                Console.WriteLine($"Standort: {job.Location}");
                Console.WriteLine($"Zeit: {job.TimeAgo}"); 
                Console.WriteLine("----------------------------------------------------");
            }

            var t1 = jobListings;
            var t2 = counter;
            string filePath;
            bool fileExists;
            WriteToCsvFile(jobListings, out filePath, out fileExists);
            return;
  
        }

        private static void WriteToCsvFile(List<JobListing> jobListings, out string filePath, out bool fileExists)
        {
            filePath = "jobs_.csv";
            fileExists = File.Exists(filePath);
            using (var writer = new StreamWriter(filePath, append: true))
            {
                if (!fileExists)
                {
                    writer.WriteLine("Job Title,Company,Location,Time Ago");
                }

                foreach (var jobListing in jobListings)
                {
                    writer.WriteLine($"{jobListing.JobTitle},{jobListing.CompanyName},{jobListing.Location},{jobListing.TimeAgo}");
                }
            }

            Console.WriteLine("CSV file has been successfully created.");
        }

        private void SortBtn_Click(object sender, EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate(UrlTextbox.Text);

            
        }
    }
}
