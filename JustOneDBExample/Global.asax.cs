﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using JustOneDBExample.Models;
using JustOneDBExample.Mvc;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using StructureMap;
using NHConfig = NHibernate.Cfg;

namespace JustOneDBExample
{
	public class MvcApplication : System.Web.HttpApplication
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
			);
		}

		protected void Application_Start()
		{
			ControllerBuilder.Current.SetControllerFactory(new StructureMapControllerFactory());

			ObjectFactory.Initialize(x =>
			{
				x.For<ISessionFactory>()
					.Singleton()
					.Use(CreateSessionFactory());

				x.For<ISession>()
					.HttpContextScoped()
					.Use(context => context.GetInstance<ISessionFactory>().OpenSession());
			});

			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}

		protected void Application_EndRequest()
		{
			ObjectFactory.ReleaseAndDisposeAllHttpScopedObjects();
		}

		protected ISessionFactory CreateSessionFactory()
		{
			var uriString = ConfigurationManager.AppSettings["JUSTONEDB_DBI_URL"];
			var uri = new Uri(uriString);
			var connectionString = new SqlConnectionStringBuilder
			{
				DataSource = uri.Host,
				InitialCatalog = uri.AbsolutePath.Trim('/'),
				UserID = uri.UserInfo.Split(':').First(),
				Password = uri.UserInfo.Split(':').Last(),
			}.ConnectionString;

			var autoMap = AutoMap.AssemblyOf<Entity>()
				.Where(t => typeof(Entity).IsAssignableFrom(t));

			return Fluently.Configure()
				.Database(
					PostgreSQLConfiguration.Standard.ConnectionString(connectionString))
				.Mappings(m => m.AutoMappings.Add(autoMap))
				.ExposeConfiguration(TreatConfiguration)
				.BuildSessionFactory();
		}

		protected virtual void TreatConfiguration(NHConfig.Configuration configuration)
		{
			var update = new SchemaUpdate(configuration);
			update.Execute(false, true);
		}
	}
}