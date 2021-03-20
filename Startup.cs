using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Coravel;
using StockServer.Schedule;
using StockServer.Repository;
using StockServer.Clawer;
using Slack.Webhooks;
using System.Text;
using Microsoft.Extensions.Logging;
using StockServer.Service;
using AutoMapper;

namespace StockServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<SqlConnection>(db => new SqlConnection(
                    Configuration.GetConnectionString("DefaultConnection")));
                    
            services.AddScheduler();
            services.AddHttpClient();
            services.AddHttpClient<StockHolderDailyClawer>("StockHolderDailyClawer", client => {
                client.BaseAddress = new System.Uri("https://bsr.twse.com.tw/");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.116 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-us,en;q=0.5");
                client.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                // client.HttpClientHandler = new System.Net.Http.HttpClientHandler() { CookieContainer = cookieContainer }
                // var response = await client.GetAsync("http://bsr.twse.com.tw/bshtm/bsMenu.aspx");
            });
            services.AddScoped<StockRepository>();
            services.AddScoped<StockInfoRepository>();
            services.AddScoped<StockApiRepository>();
            services.AddScoped<SeasonReportRepository>();
            services.AddScoped<MonthReportRepository>();
            services.AddScoped<StockHolderRepository>();
            services.AddScoped<OptionLegalRepository>();
            services.AddScoped<OptionDailyRepository>();
            services.AddScoped<StockDividendRepository>();
            services.AddScoped<RevenueRepository>();
            services.AddScoped<FundamentalDailyRepository>();
            
            //services.AddSingleton<SlackClient>(new SlackClient(Configuration["Slack:WebhookUrl"]));
            services.AddScoped<SeasonReportClawer>();
            services.AddScoped<MonthReportClawer>();
            services.AddScoped<StockHolderClawer>();
            services.AddScoped<OptionLegalClawer>();
            services.AddScoped<OptionDailyClawer>();
            services.AddScoped<StockHolderDailyClawer>();
            services.AddScoped<StockInfoClawer>();
            services.AddScoped<StockDividendClawer>();
            services.AddScoped<FundamentalDailyClawer>();
            services.AddSingleton<ClawerFcatory>(new ClawerFcatory(services));

            services.AddScoped<StockPickingService>();
            
            services.AddTransient<StockPriceCrawlerSchedule>();
            services.AddTransient<SeasonReportClawerSchedule>();
            services.AddTransient<MonthReportClawerSchedule>();
            services.AddTransient<StockHolderClawerSchedule>();
            services.AddTransient<OptionLegalClawerSchedule>();
            services.AddTransient<OptionDailyClawerSchedule>();
            services.AddTransient<StockDividendClawerSchedule>();
            services.AddTransient<FundamentalDailyClawerSchedule>();
            
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                options.SerializerSettings.DateFormatString = "yyyy/MM/dd HH:mm:ss";
            });
            services.AddOpenApiDocument();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOpenApi(); 
            app.UseSwaggerUi3();    // serve Swagger UI
            // app.UseReDoc(config =>  // serve ReDoc UI
            // {
            //     // 這裡的 Path 用來設定 ReDoc UI 的路由 (網址路徑) (一定要以 / 斜線開頭)
            //     config.Path = "/swagger";
            // });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            //排程
            app.ApplicationServices.UseScheduler(scheduler =>
            {
                scheduler
                    .Schedule<StockPriceCrawlerSchedule>()
                    .DailyAt(7, 11);
                //scheduler
                //    .Schedule<SeasonReportClawerSchedule>()
                //    .Cron("2 18 14 * *"); //分鐘 小時 日 月 星期 (UTC時間)
                //scheduler
                //    .Schedule<MonthReportClawerSchedule>()
                //    .Cron("30 8 10 * *");
                //    // .DailyAt(8, 30);
                //scheduler
                //    .Schedule<StockHolderClawerSchedule>()   
                //    .DailyAt(18, 1) //UTC hour 執行1次
                //    .Saturday();     
                //scheduler
                //    .Schedule<OptionLegalClawerSchedule>()
                //    .DailyAt(7, 15);
                //scheduler
                //    .Schedule<OptionDailyClawerSchedule>()
                //    .DailyAt(7, 31);
                //scheduler
                //    .Schedule<StockDividendClawerSchedule>()
                //    .DailyAt(8, 5)
                //    .Saturday();
                //scheduler
                //    .Schedule<FundamentalDailyClawerSchedule>()
                //    .DailyAt(12, 41);
                    
                    
            }).OnError(ex => {
                logger.LogError(ex.Message);
            });

        }
    }
}
