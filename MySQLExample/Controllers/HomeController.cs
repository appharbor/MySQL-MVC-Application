using System.Web.Mvc;
using MySQLExample.Models;
using NHibernate;
using NHibernate.Linq;

namespace MySQLExample.Controllers
{
	public class HomeController : Controller
	{
		private ISession _session;

		public HomeController(ISession session)
		{
			_session = session;
		}

		public ActionResult Index()
		{
			return View(_session.Query<Thingy>());
		}

		public ActionResult New()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Create(Thingy thing)
		{
			_session.SaveOrUpdate(thing);
			return RedirectToAction("Index");
		}
	}
}
