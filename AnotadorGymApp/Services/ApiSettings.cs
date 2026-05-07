using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotadorGymApp.Services
{
    public static class ApiSettings
    {
        public const string BaseUrl =
       "https://anotadorgym-api.azurewebsites.net/api";

        public const string LoginEndpoint =
            "/Auth/login/invitado";

        public const string EejerciciosEndpoint =
            "/Ejercicio/todos";

        public const string RutinasEndpoint =
            "/Rutina";
    }
}
