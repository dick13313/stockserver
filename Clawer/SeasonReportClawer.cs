using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using StockServer.Const;
using StockServer.Models.DataBase;
using StockServer.Repository;

namespace StockServer.Clawer
{
    public class SeasonReportClawer
    {
        private SeasonReportRepository _seasonReportRepository;
        private readonly ILogger<SeasonReportClawer> _logger;
        private IHttpClientFactory _clientFactory;
        public SeasonReportClawer(SeasonReportRepository seasonReportRepository, ILogger<SeasonReportClawer> logger, IHttpClientFactory clientFactory)
        {
            _seasonReportRepository = seasonReportRepository;
            _logger = logger;
            _clientFactory = clientFactory;
        }
        public async Task ExecuteAsync(EnumModels.SeasonReportType seasonReportType, int year, int season)
        {
            try
            {
                if(_seasonReportRepository.IsExist(seasonReportType, year, season)) {
                    _logger.LogDebug($"{seasonReportType.ToString()}, {year}, {season} data is exist");
                    return;
                }
                _logger.LogInformation($"SeasonReportClawer Running, {seasonReportType.ToString()}, year = {year}, season = {season}");
                var html = await ReadSeasonReportHtmlByTWSEAsync(seasonReportType, year, season);
                var reports = ParseSeasonReport(html, year, season, seasonReportType);
                _seasonReportRepository.Insert(reports);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"SeasonReportClawer error\n{ex.Message}");
            }
        }

        public string GetSeasonReportUrl(EnumModels.SeasonReportType seasonReportType)
        {
            switch (seasonReportType)
            {
                case EnumModels.SeasonReportType.綜合損益表:
                    return "https://mops.twse.com.tw/mops/web/ajax_t163sb04";
                case EnumModels.SeasonReportType.資產負債表:
                    return "https://mops.twse.com.tw/mops/web/ajax_t163sb05";
                default:
                    throw new ArgumentException($"{seasonReportType.ToString()} Url 不存在");
            }
        }

        public async Task<string> ReadSeasonReportHtmlByTWSEAsync(EnumModels.SeasonReportType seasonReportType, int year, int season)
        {
            using (var client = _clientFactory.CreateClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var response = await client.PostAsync(
                    GetSeasonReportUrl(seasonReportType),
                    new StringContent($"encodeURIComponent=1&step=1&firstin=1&off=1&isQuery=Y&TYPEK=sii&year={year}&season={season}")
                );
                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new PlatformNotSupportedException($"目前無法爬取財報資料...，{response.StatusCode}，{result}");
                return result;
            }
        }

        public IEnumerable<SeasonReport> ParseSeasonReport(string html, int year, int season, EnumModels.SeasonReportType seasonReportType)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var tableNodes = doc.DocumentNode.SelectNodes("//table[@class=\"hasBorder\"]");
            foreach (var tableNode in tableNodes)
            {
                var trNodes = tableNode.SelectNodes("./tr");
                Dictionary<string, int> headers = new Dictionary<string, int>();
                foreach (var trNode in trNodes)
                {
                    //header
                    var thNodes = trNode.SelectNodes("./th");
                    if (thNodes != null)
                    {
                        int thIndex = 0;
                        foreach (var thNode in thNodes)
                        {
                            headers.Add(thNode.InnerText, thIndex++);
                            // System.Console.WriteLine(thNode.InnerText);
                        }
                    }
                    //data
                    var tdNodes = trNode.SelectNodes("./td");
                    if (tdNodes != null)
                    {
                        int tdIndex = 0;
                        foreach (var tdNode in tdNodes)
                        {
                            var report = new SeasonReport()
                            {
                                stock_id = tdNodes[headers["公司代號"]].InnerText,
                                year = year,
                                season = season,
                                item = headers.Keys.ElementAt(tdIndex),
                                value = tdNodes[tdIndex].InnerText,
                                seq = tdIndex + 1,
                                type = (int)seasonReportType
                            };
                            yield return report;
                            tdIndex++;
                        }
                    }
                }
            }
        }
    }
}