using MetroFramework.Forms;
using Newtonsoft.Json.Linq;
using scrapingTemplateV51.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using System.Windows.Forms;
using CsvHelper;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Newtonsoft.Json;
using System.Net.Http;

namespace scrapingTemplateV51
{
    public partial class MainForm : MetroForm
    {
        public bool LogToUi = true;
        public bool LogToFile = true;
        string token = "";
        private readonly string _path = Application.StartupPath;
        private int _nbr;
        private int _total;
        private int _maxConcurrency;
        List<string> listTest = new List<string>();
        public HttpCaller HttpCaller = new HttpCaller();
        public MainForm()
        {
            InitializeComponent();
        }


        private async Task MainWork()
        {
            await Task.Delay(3000);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ServicePointManager.DefaultConnectionLimit = 65000;
            //Control.CheckForIllegalCrossThreadCalls = false;
            //this.FormBorderStyle = FormBorderStyle.FixedSingle;
            //this.MaximizeBox = false;
            // this.MinimizeBox = false;

            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Utility.CreateDb();
            Utility.LoadConfig();

            Utility.InitCntrl(this);
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), @"Unhandled Thread Exception");
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show((e.ExceptionObject as Exception)?.ToString(), @"Unhandled UI Exception");
        }
        #region UIFunctions
        public delegate void WriteToLogD(string s, Color c);
        public void WriteToLog(string s, Color c)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new WriteToLogD(WriteToLog), s, c);
                    return;
                }
                if (LogToUi)
                {
                    if (DebugT.Lines.Length > 5000)
                    {
                        DebugT.Text = "";
                    }
                    DebugT.SelectionStart = DebugT.Text.Length;
                    DebugT.SelectionColor = c;
                    DebugT.AppendText(DateTime.Now.ToString(Utility.SimpleDateFormat) + " : " + s + Environment.NewLine);
                }
                Console.WriteLine(DateTime.Now.ToString(Utility.SimpleDateFormat) + @" : " + s);
                if (LogToFile)
                {
                    File.AppendAllText(_path + "/data/log.txt", DateTime.Now.ToString(Utility.SimpleDateFormat) + @" : " + s + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void NormalLog(string s)
        {
            WriteToLog(s, Color.Black);
        }
        public void ErrorLog(string s)
        {
            WriteToLog(s, Color.Red);
        }
        public void SuccessLog(string s)
        {
            WriteToLog(s, Color.Green);
        }
        public void CommandLog(string s)
        {
            WriteToLog(s, Color.Blue);
        }

        public delegate void SetProgressD(int x);
        public void SetProgress(int x)
        {
            if (InvokeRequired)
            {
                Invoke(new SetProgressD(SetProgress), x);
                return;
            }
            if ((x <= 100))
            {
                ProgressB.Value = x;
            }
        }
        public delegate void DisplayD(string s);
        public void Display(string s)
        {
            if (InvokeRequired)
            {
                Invoke(new DisplayD(Display), s);
                return;
            }
            displayT.Text = s;
        }

        #endregion
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utility.Config = new Dictionary<string, string>();
            Utility.SaveCntrl(this);
            Utility.SaveConfig();
        }
        private async void startB_Click(object sender, EventArgs e)
        {

            // _maxConcurrency = (int)threadsI.Value;
            //we spin it in a new worker thread
            //Task.Run(MainWork);
            //we run mainWork on the UI thread
            await MainWork();

        }
        private void loadInputB_Click_1(object sender, EventArgs e)
        {

        }
        private void openInputB_Click_1(object sender, EventArgs e)
        {

        }
        private void openOutputB_Click_1(object sender, EventArgs e)
        {

        }
        private void loadOutputB_Click_1(object sender, EventArgs e)
        {

        }

        private async void startB_Click_1(object sender, EventArgs e)
        {
       
           // System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Display("Start Scan ");
            var listeFinalProducts = new List<Products>();
            File.WriteAllText("data.csv", "");
            await WriteHeaders();
            var listUrls = new List<string>();
            var listeProduct = new List<string>();
            var listeProductUrls = new List<string>();
          var respToken = await HttpCaller.GetDoc("https://planning.wandsworth.gov.uk/Northgate/PlanningExplorer/GeneralSearch.aspx");
            if (respToken.error!=null)
            {
                ErrorLog("error to get this url");
                return;
            }
            var viewstate = respToken.doc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATE']")?.GetAttributeValue("value", "");
            var viewstateGenerator = respToken.doc.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATEGENERATOR']")?.GetAttributeValue("value", "");
            var eventValidation = respToken.doc.DocumentNode.SelectSingleNode("//input[@id='__EVENTVALIDATION']")?.GetAttributeValue("value", "");

             
                var frmd01 = new List<KeyValuePair<string, string>>
                {

                    new KeyValuePair<string, string>("__VIEWSTATE", viewstate),
                     new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewstateGenerator),
                    new KeyValuePair<string, string>("__EVENTVALIDATION",eventValidation),//
                    new KeyValuePair<string, string>("txtApplicationNumber", ""),//
                    new KeyValuePair<string, string>("txtApplicantName", ""),
                    new KeyValuePair<string, string>("txtAgentName", ""),
                    new KeyValuePair<string, string>("cboStreetReferenceNumber", ""),
                    new KeyValuePair<string, string>("txtProposal", ""),
                    new KeyValuePair<string, string>("cboWardCode", ""),
                    new KeyValuePair<string, string>("cboParishCode", ""),
                    new KeyValuePair<string, string>("cboApplicationTypeCode", ""),
                    new KeyValuePair<string, string>("cboDevelopmentTypeCode", ""),
                    new KeyValuePair<string, string>("cboStatusCode", ""),
                    new KeyValuePair<string, string>("cboSelectDateValue", "DATE_VALID"),
                    new KeyValuePair<string, string>("cboMonths", "1"),
                    new KeyValuePair<string, string>("cboDays", "1"),
                    new KeyValuePair<string, string>("rbGroup", "rbRange"),
                    new KeyValuePair<string, string>("dateStart", "01/01/2020"),
                    new KeyValuePair<string, string>("dateEnd", "01/03/2020"),
                    new KeyValuePair<string, string>("edrDateSelection", ""),
                    new KeyValuePair<string, string>("csbtnSearch", "Search"),

                };
            var encodedItems = frmd01.Select(i => WebUtility.UrlEncode(i.Key) + "=" + WebUtility.UrlEncode(i.Value));
            var encodedContent = new StringContent(String.Join("&", encodedItems), null, "application/x-www-form-urlencoded");

            var client = new HttpClient();
            /*
             var msg = new HttpRequestMessage(HttpMethod.Post, "https://planning.wandsworth.gov.uk/Northgate/PlanningExplorer/GeneralSearch.aspx");
             msg.Content = new MyFormUrlEncodedContent(frmd01);
             //var respons = await client.PostAsync("https://planning.wandsworth.gov.uk/Northgate/PlanningExplorer/GeneralSearch.aspx", encodedContent);
             var respons = await client.SendAsync(msg);
             string html = await respons.Content.ReadAsStringAsync();
             */
            var response = await client.PostAsync("https://planning.wandsworth.gov.uk/Northgate/PlanningExplorer/GeneralSearch.aspx", encodedContent);
            string html = await response.Content.ReadAsStringAsync();


            var post0 = await HttpCaller.PostFormData("https://planning.wandsworth.gov.uk/Northgate/PlanningExplorer/GeneralSearch.aspx", frmd01);
                if (post0.error != null)
                {
                    ErrorLog("error post");
                    return;
                }

         
            
            Display("Scan completed");


        }

      public  async Task<(List<Products> liste, string error)> GetURLProducts((string javax,string token,int i)v)
        {
            var liste = new List<Products>();
            var frmd02 = new List<KeyValuePair<string, string>>
                {

                    new KeyValuePair<string, string>("javax.faces.ViewState",v.javax),
                     new KeyValuePair<string, string>("_id68:token", v.token),
                    new KeyValuePair<string, string>("_id68_SUBMIT","1"),//
                    new KeyValuePair<string, string>("_id68_SUBMIT", ""),//
                    new KeyValuePair<string, string>("_id68:scroll_2", ""),
                    new KeyValuePair<string, string>("_id68:scroll_1", $"idx{v.i}"),
                    new KeyValuePair<string, string>("_id68:_link_hidden_", ""),
                    new KeyValuePair<string, string>("_id68:_idcl: _id68", $"scroll_1idx{v.i}")

                };
            if (v.i==3)
            {
                Console.WriteLine();
            }
            var post1 = await HttpCaller.PostFormData("https://planning.broxbourne.gov.uk/Planning/lg/GFPlanningSearchResults.page", frmd02);
            if (post1.error != null)
            {
                return(null,"error to post page : "+v.i);
            }
            
            HtmlDocument doc2 = new HtmlDocument();
            var a2 = HttpUtility.HtmlDecode(post1.html);
            doc2.LoadHtml(a2);
            var javax2 = doc2.DocumentNode.SelectSingleNode("//input[@id='javax.faces.ViewState']")?.GetAttributeValue("value", "");
             v.token = doc2.DocumentNode.SelectSingleNode("//input[@id='_id68:token']")?.GetAttributeValue("value", "");

            var nodes = doc2.DocumentNode.SelectNodes("//td//input");
            if (nodes is null)
            {
                return (null, "error to get this page " + v.i);
            }
            int g = (v.i-1)*10;
            foreach (var node in nodes)
            {
                var value1 = node?.GetAttributeValue("value", "");
                var frmd03 = new List<KeyValuePair<string, string>>
                {

                    new KeyValuePair<string, string>("javax.faces.ViewState", javax2),
                     new KeyValuePair<string, string>("_id68:token", v.token),
                     new KeyValuePair<string, string>($"_id68:results:{g}:_id82", value1),
                    new KeyValuePair<string, string>("_id68_SUBMIT","1"),//
                    new KeyValuePair<string, string>("_id68_SUBMIT", ""),//
                    new KeyValuePair<string, string>("_id68:scroll_2", ""),
                    new KeyValuePair<string, string>("_id68:scroll_1", ""),
                    new KeyValuePair<string, string>("_id68:_link_hidden_", ""),
                    new KeyValuePair<string, string>("_id68:_idcl", ""),
                };
                var post3 = await HttpCaller.PostFormData("https://planning.broxbourne.gov.uk/Planning/lg/GFPlanningSearchResults.page", frmd03);
                if (post3.error != null)
                {
                    continue;
                }
                g++;
                HtmlDocument doc3 = new HtmlDocument();
                var a3 = HttpUtility.HtmlDecode(post3.html);
                doc3.LoadHtml(a3);

                var referenceNumber = "";
                var location = "";
                var proposal = "";
                var applicationType = "";
                var applicationStatus = "";
                var agentName = "";
                var caseOfficer = "";
                var applicantName = "";
                var receivedDate = "";
                var validDate = "";
                var decision = "";
                var decisionDate = "";
                var appealLodgedDate = "";
                var appealDecision = "";
                var appealDecisionDate = "";

             

                var rows = doc3.DocumentNode.SelectNodes("//tr[contains(@class,'planSResult')]/td[1]");
                if (rows is null)
                {
                    return (null, "rows is null from this page : " +v.i);
                }

                foreach (var row in rows)
                {

                    if (row.InnerText?.Trim() == "Reference Number")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        referenceNumber = value;
                    }
                    if (row.InnerText?.Trim() == "Location")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        location = value;
                    }
                    if (row.InnerText?.Trim() == "Proposal")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        proposal = value;
                    }
                    if (row.InnerText?.Trim() == "Applicant Name")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        applicantName = value;
                    }
                    if (row.InnerText?.Trim() == "Agent Name")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        agentName = value;
                    }
                    if (row.InnerText?.Trim() == "Application Type")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        applicationType = value;
                    }
                    if (row.InnerText?.Trim() == "Application Status")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        applicationStatus = value;
                    }
                    if (row.InnerText?.Trim() == "Case Officer")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        caseOfficer = value;
                    }
                    if (row.InnerText?.Trim() == "Received Date")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        receivedDate = value;
                    }
                    if (row.InnerText?.Trim() == "Valid Date")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        validDate = value;
                    }
                    if (row.InnerText?.Trim() == "Decision")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        decision = value;
                    }
                    if (row.InnerText?.Trim() == "Decision")
                    if (row.InnerText?.Trim() == "Decision Date")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                            decisionDate = value;
                    }
                    if (row.InnerText?.Trim() == "Appeal Lodged Date")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        appealLodgedDate = value;
                    }
                    if (row.InnerText?.Trim() == "Appeal Decision")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        appealDecision = value;
                    }
                    if (row.InnerText?.Trim() == "Appeal Decision Date")
                    {
                        var value = row.SelectSingleNode("./following-sibling::td")?.InnerText?.Trim();
                        appealDecisionDate = value;
                    }
                 
                }
                var produit = new Products
                {
                    AgentName = agentName,
                    AppealDecision = appealDecision,
                    AppealDecisionDate = appealDecisionDate,
                    AppealLodgedDate = appealLodgedDate,
                    ApplicantName = applicantName,
                    ApplicationStatus = applicationStatus,
                    ApplicationType = applicationType,
                    CaseOfficer = caseOfficer,
                    Decision = decision,
                    DecisionDate = decisionDate,
                    Location = location,
                    Proposal = proposal,
                    ReceivedDate = receivedDate,
                    ReferenceNumber = referenceNumber,
                    ValidDate = validDate



                };
                liste.Add(produit);
            }
            return (liste, null);
        }

       
        async Task WriteHeaders()
        {
            try
            {
                using (TextWriter tr = new StreamWriter("data.csv", true, Encoding.UTF8))
                {
                    var csv = new CsvWriter(tr);
                    csv.WriteHeader<Products>();
                    csv.NextRecord();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        async Task SaveIntoCsv(List<Products> liste)
        {
            try
            {
                using (TextWriter tr = new StreamWriter("data.csv", true, Encoding.UTF8))
                {
                    var csv = new CsvWriter(tr);
                    //csv.WriteHeader<Products>();
                    //csv.NextRecord();
                    csv.Configuration.HasHeaderRecord = false;
                    csv.WriteRecords(liste);




                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }


        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }
    }
}
