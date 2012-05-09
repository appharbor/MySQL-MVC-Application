﻿using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using PostgreSQLExample.Models;
using PostgreSQLExample.Mvc;
using StructureMap;
using NHConfig = NHibernate.Cfg;

namespace PostgreSQLExample
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
				"Default",
				"{controller}/{action}/{id}",
				new { controller = "Home", action = "Index", id = UrlParameter.Optional }
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
			var connectionString = ConfigurationManager.AppSettings["MYSQL_CONNECTION_STRING"];

			var autoMap = AutoMap.AssemblyOf<Entity>()
				.Where(t => typeof(Entity).IsAssignableFrom(t));

			return Fluently.Configure()
				.Database(
					MySQLConfiguration.Standard.ConnectionString(connectionString))
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