using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace WikiPersonCalendar
{



    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "今日の日付";
            // Longフォーマットのカレンダー
            dateTimePicker2.Format = DateTimePickerFormat.Long;

            DisplayDayPerson();


        }
        string t = "";
        public void DisplayDayPerson()
        {
            string str = "https://ja.wikipedia.org/wiki/" + Convert.ToInt32(dateTimePicker2.Value.ToString("MM")).ToString()
               + "%E6%9C%88" + Convert.ToInt32(dateTimePicker2.Value.ToString("dd")).ToString() + "%E6%97%A5";
            label2.Text = str;

            string Tani_str = "";
            Uri webUri = new Uri(str);
            List<Person> Personlist = new List<Person>();

            WebClient client = new WebClient();
            try
            {
                //docがHtmlのコードを文書として保持します
                var doc = new HtmlAgilityPack.HtmlDocument();

                //webを通してHtmlのコードを取得します
                var web = new System.Net.WebClient();

                //HtmlをUTF-8で取得します。
                //特に指定しない場合、Htmlの文字コードによっては文字化けする場合があります
                web.Encoding = Encoding.UTF8;

                //web.DownloadString(URL)のURL先のHtmlを取得します。
                //今回の場合はテキストボックスに書かれているURLを入れています
                doc.LoadHtml(web.DownloadString(str));

                //取得したHtmlコードからどの部分を取得するのかをXPath形式で指定します
                //今回はブログの本文を指定しています
                string Tani_XPath = "//*[self::h2 or self::li]";

                //指定したXPathをもとに文を取得しています
                var Tani_Collection = doc.DocumentNode.SelectNodes(Tani_XPath);
                textBox1.Text = "";
               bool BirthFalg = false;
                //Tani_CollectionはListなので、一つずつ文を取り出しています
                //文はInnerTextで取り出すことができます
                Regex reg = new Regex("&..*;");
                foreach (var tmp in Tani_Collection)
                {
                    //忌日になったらデータ収集停止
                    if (tmp.InnerHtml.Contains("id=\"忌日\"")|| tmp.InnerHtml.Contains("id=\"命日\""))
                    {
                        BirthFalg = false;
                        break;
                    }

                    if (BirthFalg && !tmp.InnerHtml.Contains("thumb") && tmp.InnerText != "")
                    {
                        Person p = new Person();
                        t = tmp.InnerHtml;
                        if (tmp.InnerText.Contains("B.C.") )
                        {
                            p.birthYear = -Int32.Parse(tmp.InnerText.Substring(0, tmp.InnerText.IndexOf("年")).Replace("B.C.",""));
                        }
                        else if (tmp.InnerText.Contains("紀元前") && !tmp.InnerText.Contains("世紀"))
                        {
                            p.birthYear = -Int32.Parse(tmp.InnerText.Substring(0, tmp.InnerText.IndexOf("年")).Replace("紀元前", ""));
                        }
                        else if (!tmp.InnerText.Contains("不詳") && !tmp.InnerText.Contains("非公開") && !tmp.InnerText.Contains("生年") && !tmp.InnerText.Contains("世紀") && Int32.TryParse(tmp.InnerText.Substring(0, tmp.InnerText.IndexOf("年")), out int a)) 
                        {
                            p.birthYear = a;
                        }
                        if (p.birthYear != 10000 && (dateTimePicker2.Value.Year - p.birthYear) % 10 == 0)
                        {
                            if (tmp.InnerText.Contains("、")) {
                                p.name = tmp.InnerText.Substring(tmp.InnerText.IndexOf("-") + 1, tmp.InnerText.IndexOf("、") - tmp.InnerText.IndexOf("-") - 1);
                                p.name = reg.Replace(p.name, "");
                                Personlist.Add(p);
                                Tani_str += p.name + "　生誕" + (dateTimePicker2.Value.Year - p.birthYear).ToString() + "年" + "\r\n";

                            }
                        }
                    }
                    //誕生日のコーナーに来たらデータ収集開始
                    if (tmp.InnerHtml.Contains("id=\"誕生日\""))
                    {
                        BirthFalg = true;
                    }


                }

                textBox1.Text += Tani_str;
            }
            catch (Exception ex)
            {
                textBox1.Text += t;
            }
        }

        private class Person
        {
            public int birthYear;
            public string name;  
            
            public Person()
            {
                birthYear = 10000;
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            DisplayDayPerson();
        }
    }

}
