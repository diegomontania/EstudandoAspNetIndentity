using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class SenhaValidador : IIdentityValidator<string>
    {
        public int TamanhoRequerido { get; set; }
        public bool ObrigatorioCaracteresEspeciais { get; set; }
        public bool ObrigatorioLetraMinuscula { get; set; }
        public bool ObrigatorioLetraMaiuscula { get; set; }
        public bool ObrigatorioDigitos { get; set; }

        public async Task<IdentityResult> ValidateAsync(string item)
        {
            //cria lista com erros da senha
            var erros = new List<string>();

            if (!VerificaTamanhoRequerido(item))
                erros.Add($"A senha deve conter no mínimo {TamanhoRequerido} caracteres.");

            if (ObrigatorioCaracteresEspeciais && !VerificaCaracteresEspeciais(item))
                erros.Add("A senha deve conter caracteres especiais!");

            if (ObrigatorioLetraMinuscula && !VerificaObrigatorioLetraMinuscula(item))
                erros.Add("A senha deve conter caracteres minusculos!");

            if (ObrigatorioLetraMaiuscula && !VerificaObrigatorioLetraMaiuscula(item))
                erros.Add("A senha deve conter caracteres maiusculos!");

            if (ObrigatorioDigitos && !VerificaObrigatorioDigitos(item))
                erros.Add("A senha deve conter números!");

            //se existir algum erro na lista
            if (erros.Any())
                return IdentityResult.Failed(erros.ToArray());
            else
                return IdentityResult.Success;
        }

        //quando um metodo tem apenas uma instrução, pode utilizar o '=>' e removendo o 'return' para deixar o código mais simples
        private bool VerificaTamanhoRequerido(string senha)
            //? é um operador de 'null propagator', significa que se o objeto for nulo, vai retornar false
            //se nao for nulo irá fazer a comparação com o tamanho minimo requerido
            => senha?.Length >= TamanhoRequerido;

        private bool VerificaCaracteresEspeciais(string senha)
            => Regex.IsMatch(senha, @"[~`!@#$%^&*()+=|\\{}':;.,<>/?[\]""_-]");

        private bool VerificaObrigatorioLetraMinuscula(string senha)
            => senha.Any(char.IsLower);

        private bool VerificaObrigatorioLetraMaiuscula(string senha)
            => senha.Any(char.IsUpper);

        private bool VerificaObrigatorioDigitos(string senha)
            => senha.Any(char.IsNumber);
    }
}