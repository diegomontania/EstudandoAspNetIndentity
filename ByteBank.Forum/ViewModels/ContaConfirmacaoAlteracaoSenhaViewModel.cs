using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ByteBank.Forum.ViewModels
{
    //classe criada com o objetivo de pegar as informações do front pela ActionResult de GET e passar esses valores
    //para ActionResult de POST, fazendo assim a troca de senha do usuario
    public class ContaConfirmacaoAlteracaoSenhaViewModel
    {
        //HiddenInput : oculta os campos no front para que o usuario não tenha acesso
        [HiddenInput(DisplayValue = false)]
        public string UsuarioId { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NovaSenha { get; set; }
    }
}