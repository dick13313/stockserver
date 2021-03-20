using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace StockServer.Clawer
{
    public class StockHolderDailyClawer
    {
        private IHttpClientFactory _httpClientFactory;
        private HttpClient _client; 
        public StockHolderDailyClawer(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _client = _httpClientFactory.CreateClient("StockHolderDailyClawer");
        }

        public async Task ExecuteAsync()
        {
            var html = await GetHtml();
            Regex regex1 = new Regex("VIEWSTATE\"\\s+value=\"(.*?)\"", RegexOptions.Multiline);
            Regex regex2 = new Regex("EVENTVALIDATION\"\\s+value=\"(.*?)\"", RegexOptions.Multiline);

            var viewstate = regex1.Match(html).Groups[1].Value;
            var eventvalidation = regex2.Match(html).Groups[1].Value;
            // var capCode = OCR(await _client.GetStreamAsync(GetImageUrl(html)));
            // var base64 = await ToBase64String(GetImageUrl(html));
            // var capCode = await OCRParseAsync(base64);
            await GetAll(viewstate, eventvalidation, "capCode");
        }

        public async Task<string> GetHtml()
        {
            var _httpClient = _httpClientFactory.CreateClient("StockHolderDailyClawer");
            var response = await _httpClient.GetAsync("/bshtm/bsMenu.aspx");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task GetAll(string viewstate, string eventvalidation, string capCode)
        {
            Dictionary<string, string> dataContent = new Dictionary<string, string>() {
                { "__EVENTTARGET", "" },
                { "__EVENTARGUMENT", "" },
                { "__LASTFOCUS", "" },
                { "__VIEWSTATE", viewstate },
                { "__EVENTVALIDATION", eventvalidation },
                { "RadioButton_Normal", "RadioButton_Normal" },
                { "TextBox_Stkno", "1101" },
                { "CaptchaControl1", capCode },
                { "btnOK", "%E6%9F%A5%E8%A9%A2" }
            };

            var response = await _client.PostAsync("/bshtm/bsMenu.aspx", new FormUrlEncodedContent(dataContent));
            var html = await response.Content.ReadAsStringAsync();

            var res = await _client.GetAsync("/bshtm/bsContent.aspx");
            var result = await res.Content.ReadAsStringAsync();
        }

        public string GetImageUrl(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var imgNodes = doc.DocumentNode.SelectNodes("//div/img");
            return "https://bsr.twse.com.tw/bshtm/" + imgNodes[0].Attributes["src"].Value;
        }

        public async Task<string> ToBase64String(string imgUrl)
        {
            var bytes = await _client.GetByteArrayAsync(imgUrl);
            return Convert.ToBase64String(bytes, 0, bytes.Length);
        }

        public async Task<string> OCRParseAsync (string base64) {
            string KEY = "dec286437e88957";
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("apikey", KEY);
            var response = await _client.PostAsync(
                $"https://api.ocr.space/parse/image", 
                new FormUrlEncodedContent(new [] {
                    new KeyValuePair<string,string>("apikey", KEY),
                    new KeyValuePair<string,string>("base64Image", "data:image/jpeg;base64," + base64),
                    new KeyValuePair<string,string>("filetype", "JPG"),
                })
            );
            var result = await response.Content.ReadAsStringAsync();

            var parseModel = JsonSerializer.Deserialize<ParseModel>(result);
            if(parseModel.OCRExitCode == 1 && parseModel.ParsedResults[0].ParsedText != "")
                return parseModel.ParsedResults[0].ParsedText;
            else 
                throw new Exception("parse error");
        }

        public class ParseModel
        {
            public List<ParsedResults> ParsedResults { get; set; }
            public int OCRExitCode { get; set; }
        } 

        public class ParsedResults
        {
            public string ParsedText { get; set; }
        }
    }
}