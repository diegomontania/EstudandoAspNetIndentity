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
                {
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao"); //Apos incluir, redireciona para home
                }
                else
                {
                    AdicionaErros(resultado);
                }
            }

            //algo deu errado
            return View(modelo);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            //armazena token do usuario
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            //cria link de confirmação do usuário
            var linkDeCallBack = Url.Action("ConfirmacaoEmail", "Conta", new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);

            await UserManager.SendEmailAsync(
                usuario.Id,
                "Fórum - Confirmação de email",
                $"Bem vindo ao fórum Bytebank, clique aqui para confirmar seu endereço de email! {linkDeCallBack}");
        }

        //criando Action de confirmação do email
        public async Task<ActionResult> ConfirmacaoEmailAsync(string usuarioId, string token)
        {
            //caso algum parametro seja nulo, retorne para view de erro
            if (usuarioId == null || token == null)
                return View("Error");

            //Confirma o email
            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            //redireciona para uma página após confirmação bem sucedida
            if (resultado.Succeeded)
                return RedirectToAction("Index", "Home");
            else
                return View("Error");
        }

        private void AdicionaErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }
    }
}