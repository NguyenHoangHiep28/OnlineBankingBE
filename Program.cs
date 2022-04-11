using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace OnlineBankingAPI
{
    public class Program
    {
        static ISavingFinalizationService _service;

        //public Program(OnlineBankingDBContext onlineBankingDB)
        //{
        //    _onlineBankingDB = onlineBankingDB;
        //}
        public static void Main(string[] args)
        {

            var optionsBuilder = new DbContextOptionsBuilder<OnlineBankingDBContext>();

            optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=OnlineBankingDB;Trusted_Connection=True;");
            OnlineBankingDBContext dbContext = new(optionsBuilder.Options);

            _service = new SavingFinalizationService(dbContext, new OTPService());
            Timer aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 5000;
            aTimer.Enabled = true;

            CreateHostBuilder(args).Build().Run();
           
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                       .UseUrls("https://localhost:5001");
                });
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _service.FinalizeSaving();
        }
    }
}
