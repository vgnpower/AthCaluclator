using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace AthCalculator
{
    public partial class Form1 : Form
    {
        List<Coin> lsCoins = new List<Coin>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public HtmlAgilityPack.HtmlDocument Send_Request(string url)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            var webRequest = HttpWebRequest.Create(url);
            Stream stream = webRequest.GetResponse().GetResponseStream();
            doc.Load(stream);
            stream.Close();
            
            return doc;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lsCoins.Clear();

            label1.Text = CercaSingolaMoneta(textBox1.Text).ToString();
        }

        public void PopolaDataGrid()
        {
            var bindingList = new BindingList<Coin>(lsCoins);
            var source = new BindingSource(bindingList, null);
            dataGridView1.DataSource = source;
        }

        public double CercaSingolaMoneta(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = Send_Request(url);
            string testDivSelector = "//div[@id='historical-data']//tbody//tr[@class='text-right']";
            HtmlNodeCollection nodo = doc.DocumentNode.SelectNodes(testDivSelector);
            double ath = 0;

            //cerca ogni entry
            if (nodo != null)
            {
                foreach (HtmlNode node in nodo)
                {
                    //prende dalla entry solo all time high
                    HtmlNodeCollection nodoPerAth = doc.DocumentNode.SelectNodes(testDivSelector + "//td");
                    double currentPrice = double.Parse(nodoPerAth[2].InnerText, CultureInfo.InvariantCulture);

                    if (currentPrice > ath)
                        ath = currentPrice;

                    node.Remove();
                }
            }
            return ath;
        }

        public void CercaTutteCoins(string url)
        {
            HtmlAgilityPack.HtmlDocument doc = Send_Request(url);
            string testDivSelector = "//table[@id='currencies-all']//tbody//tr";
            HtmlNodeCollection nodi = doc.DocumentNode.SelectNodes(testDivSelector);

            //cerca ogni entry
            if (nodi != null)
            {
                foreach (HtmlNode nodo in nodi)
                {
                    Coin record = new Coin();
                    //prende dalla entry solo all time high
                    record.Name = nodo.SelectSingleNode("//td[@class='no-wrap currency-name']//a[@class='currency-name-container link-secondary']").InnerText;
                    string urlMoneteCorrente = "https://coinmarketcap.com" + nodo.SelectSingleNode("//td[@class='no-wrap currency-name']//a[@class='currency-name-container link-secondary']").Attributes["href"].Value;
                    //c'è una chiamata al metodo per ottenere lo ath di ogni singola moneta analizzata.
                    record.Ath = CercaSingolaMoneta(urlMoneteCorrente + "historical-data/" + SetDateFilter(dateTP_inizio.Text, dateTP_fine.Text));
                    record.CurrentPrice = double.Parse(nodo.SelectSingleNode("//td[@class='no-wrap text-right']//a[@class='price']").InnerText.Replace("$", ""), CultureInfo.InvariantCulture);
                    record.perc_drop = Math.Round(((record.Ath - record.CurrentPrice) / record.Ath) * 100);
                    record.perc_to_ath = Math.Round(((record.Ath / record.CurrentPrice) - 1) * 100);
                    lsCoins.Add(record);

                    nodo.Remove();
                    if (lsCoins.Count > nUpDwn_MaxCoin.Value)
                        break;
                }
            }
        }

        public string SetDateFilter(string inizio, string fine)
        {
            return "?start=" + inizio.Replace("/", "") + "&end=" + fine.Replace("/", "");
        }

        private void btn_AllCrypto_Click(object sender, EventArgs e)
        {
            lsCoins.Clear();
            CercaTutteCoins("https://coinmarketcap.com/all/views/all/");
            PopolaDataGrid();
        }

        private void dateTP_inizio_ValueChanged(object sender, EventArgs e)
        {
            DateTime check;

            if(DateTime.TryParse(dateTP_inizio.Text, out check) && check > DateTime.Now)   
                dateTP_inizio.Text = dateTP_fine.Text;
        }
    }
}
