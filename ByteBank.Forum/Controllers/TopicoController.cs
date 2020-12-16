using ByteBank.Forum.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    public class TopicoController : Controller
    {
        //Authorize : responsável por habilitar a action apenas para quem estiver logado
        //(autorizado) dentro da aplicação
        [Authorize]
        public ActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Criar(TopicoCriarViewModel modelo)
        {
            return View();
        }
    }
}