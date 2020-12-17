using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System;
using Microsoft.Owin.Security;

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

        //responsavel por conter os valores do signInManager
        private SignInManager<UsuarioAplicacao, string> _signInManager { get; set; }
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                //se não houver valor, recupere do contexto do owin
                if (_signInManager == null)
                {
                    var contextoOwin = HttpContext.GetOwinContext();
                    _signInManager = contextoOwin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                }

                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        //responsavel pelo logoff do usuario
        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                //recuperando 'AuthenticationManager' que vem por padrão no owin com identity
                var contextoOwin = Request.GetOwinContext();
                return contextoOwin.Authentication;
            }
        }

        //retorna view Registrar
        [HttpGet]
        public ActionResult Registrar()
        {
            return View();
        }
        [HttpPost] 
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            //https://imasters.com.br/back-end/c-programacao-assincrona-async-e-await

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
            var linkDeCallBack = Url.Action("RecebeConfirmacaoEmail", "Conta", new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);

            //envia o email de fato
            await UserManager.SendEmailAsync(
                usuario.Id,
                "Fórum - Confirmação de email",
                $"Bem vindo ao fórum Bytebank, clique aqui para confirmar seu endereço de email! {linkDeCallBack}");
        }

        //Action de confirmação do email
        public async Task<ActionResult> RecebeConfirmacaoEmail(string usuarioId, string token)
        {
            //caso algum parametro seja nulo, retorne para view de erro
            if (usuarioId == null || token == null)
                return View("Error");

            //Confirma o email
            var resultado = await UserManager.ConfirmEmailAsync(usuarioId, token);

            //redireciona para uma página após confirmação bem sucedida
            if (resultado.Succeeded)
                return RedirectToAction("SucessoConfirmacao", "Conta");
            else
                return View("Error");
        }

        //retorna view SucessoConfirmacao
        [HttpGet]
        public async Task<ActionResult> SucessoConfirmacao()
        {
            return View();
        }

        //retorna view Login
        [HttpGet]
        public async Task<ActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                //recebe o usuario pelo email
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                //se usuário não existir
                if (usuario == null)
                    SenhaOuUsuariosInvalidos();

                //gerenciador responsável pelas operações de logar e deslogar usuários                             
                var signInResultado =
                    await SignInManager.PasswordSignInAsync(
                        usuario.UserName, /*nome do usuario*/
                        modelo.Senha,     /*senha vinda do formulario*/
                        isPersistent: modelo.ContinuarLogado, /*habilita o 'continuar logado'*/
                        shouldLockout: true);  /*habilita o 'lockout' se o usuario fizer muitas tentativas de login*/

                //resultado da tentativa de login
                switch (signInResultado)
                {
                    //se tiver sucesso no login
                    case SignInStatus.Success:
                        if (usuario.EmailConfirmed)
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);               /*desloga o usuario*/
                            ModelState.AddModelError("", "Por favor, confirme o seu email antes do primeiro acesso!"); /*escreve na tela*/
                        }
                        break;

                    //se usuario acertou a senha após varias tentativas erradas
                    case SignInStatus.LockedOut:
                        var senhaCorreta = await UserManager.CheckPasswordAsync(usuario, modelo.Senha);
                        if (senhaCorreta)
                            ModelState.AddModelError("", "Sua conta está bloqueada temporariamente! Tente novamente daqui a 15 minutos!"); /*escreve na tela*/
                        else
                            return SenhaOuUsuariosInvalidos();
                        break;

                    default:
                        return SenhaOuUsuariosInvalidos();
                }
            }

            //algo errado aconteceu
            return View(modelo);
        }

        //Action de Logoff
        [HttpPost] /*utilizando http post pois, o http get não altera estado*/
        public ActionResult Logoff()
        {
            //Passando o tipo de autenticação utilizada no 'Startup.cs'
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //retorna view EsqueciSenha
        [HttpGet]
        public ActionResult EsqueciSenha()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);

                //se o usuario já existir cadastrado envie o email de fato, se não apenas redirecione para a view
                if (usuario != null)
                {
                    //criando token de reset
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);

                    //cria o email
                    var linkDeCallBack = Url.Action("ConfirmacaoAlteracaoDeSenha", "Conta", new { usuarioId = usuario.Id, token = token }, Request.Url.Scheme);
                    
                    //envia o email
                    await UserManager.SendEmailAsync(
                        usuario.Id,
                        "Fórum - Alteração de senha",
                        $"Clique no link abaixo para alterar a sua senha : {linkDeCallBack}");   
                }

                //mostra a view
                return View("EmailAlteracaoSenhaEnviado");
            }

            return View();
        }

        [HttpGet]
        public ActionResult ConfirmacaoAlteracaoDeSenha(string usuarioId, string token)
        {
            //recebe os campos do usuario e passa para a ActionResult de post
            var modelo = new ContaConfirmacaoAlteracaoSenhaViewModel
            {
                UsuarioId = usuarioId,
                Token = token,
            };

            return View(modelo);
        }
        [HttpPost]
        public ActionResult ConfirmacaoAlteracaoSenha(ContaConfirmacaoAlteracaoSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                //verifica o token recebido
                //verifica id do usuario
                //mudar a senha
            }

            return View();
        }

        private ActionResult SenhaOuUsuariosInvalidos()
        {
            ModelState.AddModelError("", "Credenciais inválidas!");
            return View("Login");
        }

        private void AdicionaErros(IdentityResult resultado)
        {
            foreach (var erro in resultado.Errors)
                ModelState.AddModelError("", erro);
        }
    }
}