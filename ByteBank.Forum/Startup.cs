using ByteBank.Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ByteBank.Forum.App_Start.Identity;

//Utilizando owin, para apenas utilizar os recursos necessários na aplicação. Sendo mais leve que o System.web
//adicionando atributo que define o tipo a classe de inicialização do 'Owin'
[assembly: OwinStartup(typeof(ByteBank.Forum.Startup))]

namespace ByteBank.Forum
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            //https://pt.stackoverflow.com/questions/163115/qual-a-diferen%C3%A7a-entre-criar-um-context-com-dbcontext-e-datacontext
            //criando DbContext - utilizando dbContext passando a string connection
            builder.CreatePerOwinContext<DbContext>(
                () => new IdentityDbContext<UsuarioAplicacao>("DefaultConnection"));

            //criando UserStore - camada de abstração, fornece as informações do usuário para o identity
            builder.CreatePerOwinContext<IUserStore<UsuarioAplicacao>>(
                (opcoes, contextoOwin) => 
                {
                    var dbContext = contextoOwin.Get<DbContext>();
                    return new UserStore<UsuarioAplicacao>(dbContext);
                });

            //criando UserManager - responsável por gerenciar os usuários
            builder.CreatePerOwinContext<UserManager<UsuarioAplicacao>>(
                (opcoes, contextoOwin) =>
                {
                    var userStore = contextoOwin.Get<IUserStore<UsuarioAplicacao>>();
                    var userManager = new UserManager<UsuarioAplicacao>(userStore);

                    //guarda o objeto responsável pela validação do usuário quando são incluídos
                    var userValidator = new UserValidator<UsuarioAplicacao>(userManager);
                    userValidator.RequireUniqueEmail = true;

                    //atribui a propriedade
                    userManager.UserValidator = userValidator;

                    //validacao de senha
                    userManager.PasswordValidator = new SenhaValidador() /*utilizando 'Property Initializer' para colocar os valores*/
                    {
                        TamanhoRequerido = 6,
                        ObrigatorioCaracteresEspeciais = true,
                        ObrigatorioDigitos = true,
                        ObrigatorioLetraMaiuscula = true,
                        ObrigatorioLetraMinuscula = true
                    };

                    //fazendo uso do novo serviço de email, para utilização da confirmação de email
                    userManager.EmailService = new EmailServico();

                    return userManager;
                });
        }
    }
}