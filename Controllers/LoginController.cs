using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebMySQL.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {

                return Json(new { Msg = "Usuário já logado!" });
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Logar(string username, string senha, bool manterlogado)
        {
            MySqlConnection mySqlConnection = new MySqlConnection("Server=localhost;DataBase=WebMySql;Uid=root;Pwd=root");
            await mySqlConnection.OpenAsync();

            MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
            mySqlCommand.CommandText = $"SELECT * FROM usuario WHERE Username = '{username}'AND Senha = '{senha}' ";

            MySqlDataReader reader = mySqlCommand.ExecuteReader();


            if (await reader.ReadAsync())
            {
                int id = reader.GetInt32(0);
                string nome = reader.GetString(1);

                List<Claim> direitosAcesso = new List<Claim>
                {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, nome)
                };


                var identity = new ClaimsIdentity(direitosAcesso, "Identity.Login");
                var userPrincipal = new ClaimsPrincipal(new[] { identity });

                await HttpContext.SignInAsync(userPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = manterlogado,
                        ExpiresUtc = DateTime.Now.AddHours(1)
                    }); ;



                return Json(new { Msg = "Usuário Logado com sucesso!" });

            }
            return Json(new { Msg = "Usuário não encontrado verifique suas credenciais!" });
        }

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Login");
        }
    }
}
