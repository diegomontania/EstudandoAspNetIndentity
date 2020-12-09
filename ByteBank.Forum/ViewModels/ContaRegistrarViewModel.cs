using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ByteBank.Forum.ViewModels
{
    public class ContaRegistrarViewModel
    {
        //Data anotations
        //Required : Campo obrigatório
        //Display : Utilizado para mostrar na ui
        //EmailAddress : Campo especifico de email
        //[DataType(DataType.Password)] : Campo de senha

        [Required]
        [Display(Name = "Nome de Usuário")]
        public string UserName { get; set; }
        [Required]
        [Display (Name = "Nome Completo")]
        public string NomeCompleto { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType (DataType.Password)]
        public string Senha { get; set; }
    }
}