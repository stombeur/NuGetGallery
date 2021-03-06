﻿using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGetGallery.FunctionTests.Helpers;
using NuGetGallery.FunctionalTests.TestBase;
using System.IO;

namespace NuGetGallery.FunctionalTests.ODataTests 
{
    [TestClass]
    public partial class V2FeedTest : GalleryTestBase
    {


        [TestMethod]
        public void ApiV2BaseUrlTest()
        {
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl);
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            //Just check for presence of defined tag.
            Assert.IsTrue(responseText.Contains(@"<atom:title>Packages</atom:title>"));
        }

        [TestMethod]
        public void ApiV2MetadataTest()
        {
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"$metadata");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            //Just check for presence of defined tag.
            //Assert.IsTrue(responseText.Contains(@"<EntityType Name=" + @"""" + "V2FeedPackage" +@"""" +  "m:HasStream=" + @"""" + "true" +@"""" + ">"));
            Assert.IsTrue(responseText.Contains(@"V2FeedPackage"));
        }

        [TestMethod]
        public void Top30PackagesFeedTest()
        {            
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/Search()?$filter=IsAbsoluteLatestVersion&$orderby=DownloadCount%20desc,Id&$skip=0&$top=30&searchTerm=''&targetFramework='net45'&includePrerelease=true");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            //Just check for the presence of predefined package which is expected in Top 30.
            Assert.IsTrue(responseText.Contains("jQuery"));          
        }

        [TestMethod]
        public void FindPackagesByIdTest()
        {
            string packageId = "TestV2FeedFindPackagesById" + "." + DateTime.Now.Ticks.ToString();
            AssertAndValidationHelper.UploadNewPackageAndVerify(packageId, "1.0.0");
            AssertAndValidationHelper.UploadNewPackageAndVerify(packageId, "2.0.0");
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/FindPackagesById()?id='" + packageId +"'");          
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            Assert.IsTrue(responseText.Contains(@"<id>"+ UrlHelper.V2FeedRootUrl + "Packages(Id='"+ packageId + "',Version='1.0.0')</id>"));
            Assert.IsTrue(responseText.Contains(@"<id>" + UrlHelper.V2FeedRootUrl + "Packages(Id='" + packageId + "',Version='2.0.0')</id>"));
           
        }

      

        /// <summary>
        ///     Regression test for #1052
        /// </summary>
        [TestMethod]
        public void GetUpdates1052RegressionTest()
        {
            // Use the same package name, but force the version to be unique.
            string packageName = "NuGetGallery.FunctionalTests.ODataTests.GetUpdates1052RegressionTest";
            string ticks = DateTime.Now.Ticks.ToString();
            string version1 = new System.Version(ticks.Substring(0, 6) + "." + ticks.Substring(6, 6) + "." + ticks.Substring(12, 6)).ToString();
            string version2 = new System.Version(Convert.ToInt32((ticks.Substring(0, 6)) + 1).ToString() + "." + ticks.Substring(6, 6) + "." + ticks.Substring(12, 6)).ToString();
            AssertAndValidationHelper.UploadNewPackageAndVerify(packageName, version1);
            AssertAndValidationHelper.UploadNewPackageAndVerify(packageName, version2);

            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/GetUpdates()?packageIds='NuGetGallery.FunctionalTests.ODataTests.GetUpdates1052RegressionTest%7CNuGetGallery.FunctionalTests.ODataTests.GetUpdates1052RegressionTest%7COwin%7CMicrosoft.Web.Infrastructure%7CMicrosoft.AspNet.Identity.Core%7CMicrosoft.AspNet.Identity.EntityFramework%7CMicrosoft.AspNet.Identity.Owin%7CMicrosoft.AspNet.Web.Optimization%7CRespond%7CWebGrease%7CjQuery%7CjQuery.Validation%7CMicrosoft.Owin.Security.Twitter%7CMicrosoft.Owin.Security.OAuth%7CMicrosoft.Owin.Security.MicrosoftAccount%7CMicrosoft.Owin.Security.Google%7CMicrosoft.Owin.Security.Facebook%7CMicrosoft.Owin.Security.Cookies%7CMicrosoft.Owin%7CMicrosoft.Owin.Host.SystemWeb%7CMicrosoft.Owin.Security%7CModernizr%7CMicrosoft.jQuery.Unobtrusive.Validation%7CMicrosoft.AspNet.WebPages%7CMicrosoft.AspNet.Razor%7Cbootstrap%7CAntlr%7CMicrosoft.AspNet.Mvc%7CNewtonsoft.Json%7CEntityFramework'&versions='" + version1 + "%7C" + version2 + "%7C1.0%7C1.0.0.0%7C1.0.0%7C1.0.0%7C1.0.0%7C1.1.1%7C1.2.0%7C1.5.2%7C1.10.2%7C1.11.1%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.6.2%7C3.0.0%7C3.0.0%7C3.0.0%7C3.0.0%7C3.4.1.9004%7C5.0.0%7C5.0.6%7C6.0.0'&includePrerelease=false&includeAllVersions=false&targetFrameworks='net45'&versionConstraints='%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C'");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();

            // Verify at least one package is in the output.
            Assert.IsTrue(responseText.Contains(@"<title type=""text"">NuGetGallery.FunctionalTests.ODataTests.GetUpdates1052RegressionTest</title>"));
            Assert.IsTrue(responseText.Contains(@"<d:Version>" + version2 + "</d:Version><d:NormalizedVersion>" + version2 + "</d:NormalizedVersion>"));
        }

        /// <summary>
        ///     Regression test for #1199
        /// </summary>
        [TestMethod]
        public void GetUpdates1199RegressionTest()
        {
            // Use the same package name, but force the version to be unique.
            string packageName = "NuGetGallery.FunctionalTests.ODataTests.GetUpdates1199RegressionTest";
            string ticks = DateTime.Now.Ticks.ToString();
            string version1 = new System.Version(ticks.Substring(0, 6) + "." + ticks.Substring(6, 6) + "." + ticks.Substring(12, 6)).ToString();
            string version2 = new System.Version(Convert.ToInt32(ticks.Substring(0, 6) + 1).ToString() + "." + ticks.Substring(6, 6) + "." + ticks.Substring(12, 6)).ToString();
            string standardOutput = string.Empty;
            string standardError = string.Empty;
            string package1Location = PackageCreationHelper.CreatePackageWithTargetFramework(packageName, version1, "net45");
            int exitCode = CmdLineHelper.UploadPackage(package1Location, UrlHelper.V2FeedPushSourceUrl, out standardOutput, out standardError);
            Assert.IsTrue((exitCode == 0), "The package upload via Nuget.exe didnt suceed properly. Check the logs to see the process error and output stream.  Exit Code: " + exitCode + ". Error message: \"" + standardError + "\"");
            string package2Location = PackageCreationHelper.CreatePackageWithTargetFramework(packageName, version2, "net40");
            exitCode = CmdLineHelper.UploadPackage(package2Location, UrlHelper.V2FeedPushSourceUrl, out standardOutput, out standardError);
            Assert.IsTrue((exitCode == 0), "The package upload via Nuget.exe didnt suceed properly. Check the logs to see the process error and output stream.  Exit Code: " + exitCode + ". Error message: \"" + standardError + "\"");
            
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/GetUpdates()?packageIds='NuGetGallery.FunctionalTests.ODataTests.GetUpdates1199RegressionTest%7COwin%7CMicrosoft.Web.Infrastructure%7CMicrosoft.AspNet.Identity.Core%7CMicrosoft.AspNet.Identity.EntityFramework%7CMicrosoft.AspNet.Identity.Owin%7CMicrosoft.AspNet.Web.Optimization%7CRespond%7CWebGrease%7CjQuery%7CjQuery.Validation%7CMicrosoft.Owin.Security.Twitter%7CMicrosoft.Owin.Security.OAuth%7CMicrosoft.Owin.Security.MicrosoftAccount%7CMicrosoft.Owin.Security.Google%7CMicrosoft.Owin.Security.Facebook%7CMicrosoft.Owin.Security.Cookies%7CMicrosoft.Owin%7CMicrosoft.Owin.Host.SystemWeb%7CMicrosoft.Owin.Security%7CModernizr%7CMicrosoft.jQuery.Unobtrusive.Validation%7CMicrosoft.AspNet.WebPages%7CMicrosoft.AspNet.Razor%7Cbootstrap%7CAntlr%7CMicrosoft.AspNet.Mvc%7CNewtonsoft.Json%7CEntityFramework'&versions='" + version1 + "%7C1.0%7C1.0.0.0%7C1.0.0%7C1.0.0%7C1.0.0%7C1.1.1%7C1.2.0%7C1.5.2%7C1.10.2%7C1.11.1%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.6.2%7C3.0.0%7C3.0.0%7C3.0.0%7C3.0.0%7C3.4.1.9004%7C5.0.0%7C5.0.6%7C6.0.0'&includePrerelease=false&includeAllVersions=false&targetFrameworks='net45'&versionConstraints='%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C'");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();

            // Verify at least one package is in the output.
            Assert.IsTrue(responseText.Contains(@"<title type=""text"">NuGetGallery.FunctionalTests.ODataTests.GetUpdates1199RegressionTest</title>"));
            Assert.IsTrue(responseText.Contains(@"<d:Version>" + version2 + "</d:Version><d:NormalizedVersion>" + version2 + "</d:NormalizedVersion>"));
        }

        [TestMethod]
        public void GetUpdatesTest()
        {
            // Use the same package name, but force the version to be unique.
            string packageName = "NuGetGallery.FunctionalTests.Fluent.GetUpdatesTest";
            string ticks = DateTime.Now.Ticks.ToString();
            string version1 = new System.Version(ticks.Substring(0, 6) + "." + ticks.Substring(6, 6) + "." + ticks.Substring(12, 6)).ToString();
            string version2 = new System.Version(Convert.ToInt32((ticks.Substring(0, 6)) + 1).ToString()  + "." + ticks.Substring(6, 6) + "." + ticks.Substring(12, 6)).ToString();
            AssertAndValidationHelper.UploadNewPackageAndVerify(packageName, version1);
            AssertAndValidationHelper.UploadNewPackageAndVerify(packageName, version2);

            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"/GetUpdates()?packageIds='NuGetGallery.FunctionalTests.Fluent.GetUpdatesTest%7COwin%7CMicrosoft.Web.Infrastructure%7CMicrosoft.AspNet.Identity.Core%7CMicrosoft.AspNet.Identity.EntityFramework%7CMicrosoft.AspNet.Identity.Owin%7CMicrosoft.AspNet.Web.Optimization%7CRespond%7CWebGrease%7CjQuery%7CjQuery.Validation%7CMicrosoft.Owin.Security.Twitter%7CMicrosoft.Owin.Security.OAuth%7CMicrosoft.Owin.Security.MicrosoftAccount%7CMicrosoft.Owin.Security.Google%7CMicrosoft.Owin.Security.Facebook%7CMicrosoft.Owin.Security.Cookies%7CMicrosoft.Owin%7CMicrosoft.Owin.Host.SystemWeb%7CMicrosoft.Owin.Security%7CModernizr%7CMicrosoft.jQuery.Unobtrusive.Validation%7CMicrosoft.AspNet.WebPages%7CMicrosoft.AspNet.Razor%7Cbootstrap%7CAntlr%7CMicrosoft.AspNet.Mvc%7CNewtonsoft.Json%7CEntityFramework'&versions='" + version1 + "%7C1.0%7C1.0.0.0%7C1.0.0%7C1.0.0%7C1.0.0%7C1.1.1%7C1.2.0%7C1.5.2%7C1.10.2%7C1.11.1%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.0.0%7C2.6.2%7C3.0.0%7C3.0.0%7C3.0.0%7C3.0.0%7C3.4.1.9004%7C5.0.0%7C5.0.6%7C6.0.0'&includePrerelease=false&includeAllVersions=false&targetFrameworks='net45'&versionConstraints='%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C%7C'");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();

            // Verify version 2 is in the output.
            Assert.IsTrue(responseText.Contains(@"<title type=""text"">NuGetGallery.FunctionalTests.Fluent.GetUpdatesTest</title>"));
            Assert.IsTrue(responseText.Contains(@"<d:Version>" + version2 + "</d:Version><d:NormalizedVersion>" + version2 + "</d:NormalizedVersion>"));
        }

        /// <summary>
        ///     Double-checks whether feed and stats page rankings are the same.
        /// </summary>
        [TestMethod]
        public void PackageFeedSortingTest()
        {
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"stats/downloads/last6weeks/");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();

            // Grab the top 10 package names in the feed.
            string[] packageName = new string[10];
            responseText = packageName[0] = responseText.Substring(responseText.IndexOf(@"""PackageId"": """) + 14);
            packageName[0] = packageName[0].Substring(0, responseText.IndexOf(@""""));
            for (int i = 1; i < 10; i++)
            {
                responseText = packageName[i] = responseText.Substring(responseText.IndexOf(@"""PackageId"": """) + 14);
                packageName[i] = packageName[i].Substring(0, responseText.IndexOf(@""""));
                // Sometimes two versions of a single package appear in the top 10.  Stripping second and later instances for this test. 
                for (int j = 0; j < i; j++)
                {
                    if (packageName[j] == packageName[i])
                    {
                        packageName[i] = null;
                        i--;
                    }
                }


            }

            request = WebRequest.Create(UrlHelper.BaseUrl + @"stats/packageversions");
        
            // Get the response.          
            response = request.GetResponse();
            sr = new StreamReader(response.GetResponseStream());
            responseText = sr.ReadToEnd();
            for (int i = 1; i < 10; i++)
            {
                // Check to make sure the top 10 packages are in the same order as the feed.
                // We add angle brackets to prevent false failures due to duplicate package names in the page.
                Assert.IsTrue(responseText.IndexOf(">" + packageName[i - 1] + "<") < responseText.IndexOf(">" + packageName[i] + "<"), "Expected string " + packageName[i - 1] + " to come before " + packageName[i] + ".  Expected list is: " + packageName[0] + ", " + packageName[1] + ", " + packageName[2] + ", " + packageName[3] + ", " + packageName[4] + ", " + packageName[5] + ", " + packageName[6] + ", " + packageName[7] + ", " + packageName[8] + ", " + packageName[9]);
            }
        }

        /// <summary>
        ///     Double-checks whether expected fields exist in the packages feed.
        /// </summary>
        [TestMethod]
        public void PackageFeedSanityTest()
        {
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"stats/downloads/last6weeks/");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();

            string firstPackage = responseText.Substring(responseText.IndexOf("{"), responseText.IndexOf("}") - responseText.IndexOf("{"));

            Assert.IsTrue(firstPackage.Contains(@"""PackageId"": """), "Expected PackageId field is missing.");
            Assert.IsTrue(firstPackage.Contains(@"""PackageVersion"": """), "Expected PackageVersion field is missing.");
            Assert.IsTrue(firstPackage.Contains(@"""Gallery"": """), "Expected Gallery field is missing.");
            Assert.IsTrue(firstPackage.Contains(@"""PackageTitle"": """), "Expected PackageTitle field is missing.");
            Assert.IsTrue(firstPackage.Contains(@"""PackageIconUrl"": """), "Expected PackageIconUrl field is missing.");
            Assert.IsTrue(firstPackage.Contains(@"""Downloads"": "), "Expected PackageIconUrl field is missing.");
        }

        /// <summary>
        ///     Verify copunt querystring parameter in the Packages feed.
        /// </summary>
        [TestMethod]
        public void PackageFeedCountParameterTest()
        {
            WebRequest request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"stats/downloads/last6weeks/");
            // Get the response.          
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            string responseText = sr.ReadToEnd();
            string[] separators = new string[1] {"},"};
            int packageCount = responseText.Split(separators, StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.IsTrue(packageCount == 500, "Expected feed to contain 500 packages. Actual count: " + packageCount);

            request = WebRequest.Create(UrlHelper.V2FeedRootUrl + @"stats/downloads/last6weeks?count=5");
            // Get the response.          
            response = request.GetResponse();
            sr = new StreamReader(response.GetResponseStream());
            responseText = sr.ReadToEnd();

            packageCount = responseText.Split(separators, StringSplitOptions.RemoveEmptyEntries).Length;
            Assert.IsTrue(packageCount == 5, "Expected feed to contain 5 packages. Actual count: " + packageCount);
        }
    }
}
