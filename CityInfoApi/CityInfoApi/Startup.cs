﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfoApi.Entities;
using CityInfoApi.Models;
using CityInfoApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;

namespace CityInfoApi
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddMvcOptions(o=>o.OutputFormatters.Add(
                new XmlDataContractSerializerOutputFormatter()));

            //if you  want to overwrite default json properties name 
            //    .AddJsonOptions(o =>
            //{
            //    if (o.SerializerSettings.ContractResolver != null)
            //    {
            //        var castedResolver = o.SerializerSettings.ContractResolver as DefaultContractResolver;

            //        castedResolver.NamingStrategy = null;

            //    }
            //});
#if Debug

            services.AddTransient<IMailService,LocalMailService>();
#else

            services.AddTransient<IMailService, CloudMailService>();
#endif
            services.AddScoped<ICityInfoRepository, CityInfoRepository>();
            var connectionString = Startup.Configuration["connectionStrings:cityInfoDBConnectionString"];

            services.AddDbContext<CityInfoContext>(c=>c.UseSqlServer(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env , ILoggerFactory loggerFactory , CityInfoContext cityInfoContext)
        {
           // loggerFactory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());

            loggerFactory.AddNLog();
            //loggerFactory.AddConsole();
            //loggerFactory.AddDebug();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            cityInfoContext.EnsureSeedDataForContext();
            app.UseStatusCodePages();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<City, CityWithOutPointsOfInterestDto>();
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<PointOfInterest, PointOfInterestDto>();
                cfg.CreateMap<PointOfInterestForCreationDto, PointOfInterest>();


            });

            app.UseMvc();
            
           
        }
    }
}
