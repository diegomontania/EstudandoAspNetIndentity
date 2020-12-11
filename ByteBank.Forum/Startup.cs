﻿using ByteBank.Forum.Models;
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
                    return new UserManager<UsuarioAplicacao>(userStore);
                });
        }
    }
}