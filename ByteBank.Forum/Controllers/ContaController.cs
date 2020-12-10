using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {
        public ActionResult Registrar()
        {
            return View();
        }

        //https://imasters.com.br/back-end/c-programacao-assincrona-async-e-await
        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            //detecta se o estado do modelo é valido ou não
            if(ModelState.IsValid)
            {
                //https://pt.stackoverflow.com/questions/163115/qual-a-diferen%C3%A7a-entre-criar-um-context-com-dbcontext-e-datacontext
                //utilizando dbContext passando a string connection
                var dbContext = new IdentityDbContext<UsuarioAplicacao>("DefaultConnection");

                //camada de abstração, fornece as informações do usuário para o identity
                var userStore = new UserStore<UsuarioAplicacao>(dbContext);

                //responsável por gerenciar os usuários
                var userManager = new UserManager<UsuarioAplicacao>(userStore);

                //criando de fato o usuario
                var novoUsuario = new UsuarioAplicacao();

                //associando os campos recebidos do IdentityUser
                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto; //campo vindo da classe UsuarioAplicacao

                //adicionado usuario utilizando userManager
                await userManager.CreateAsync(novoUsuario, modelo.Senha);

                //Apos incluir, redireciona para home
                return RedirectToAction("Index", "Home");
            }

            //algo deu errado
            return View(modelo);
        }
    }
}