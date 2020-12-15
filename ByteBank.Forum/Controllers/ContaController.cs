using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {
        //responsavel por conter os valores retornados pelo contexto do owin
        private UserManager<UsuarioAplicacao> _userManager { get; set; }
        public UserManager<UsuarioAplicacao> UserManager 
        {
            get 
            {
                //se não houver valor, recupere do contexto do owin
                if (_userManager == null)
                {
                    var contextoOwin = HttpContext.GetOwinContext();
                    _userManager = contextoOwin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }

                return _userManager;
            }
            set 
            {
                _userManager = value;
            }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        //https://imasters.com.br/back-end/c-programacao-assincrona-async-e-await
        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            //detecta se o estado do modelo é valido ou não
            if (ModelState.IsValid)
            {
                //criando de fato o usuario
                var novoUsuario = new UsuarioAplicacao();

                //associando os campos recebidos do IdentityUser
                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto; //campo vindo da classe UsuarioAplicacao

                var resultado = await UserManager.CreateAsync(novoUsuario, modelo.Senha);

                //verifica o resultado da ação
                if (resultado.Succeeded)
                    return RedirectToAction("Index", "Home"); //Apos incluir, redireciona para home
                else
                    AdicionaErros(resultado);
            }

            //algo deu errado
            return View(modelo);
        }

        private void AdicionaErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }

        private void VerificaSeUsuarioExiste()
        {

        }
    }
}