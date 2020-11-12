using JWTExemplo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTExemplo.Logic
{
    public interface IAccountLogic
    {
        public TokenModel GetAuthenticationToken(LoginModel loginModel);

        public TokenModel ActiveTokenUsingRefreshToken(TokenModel tokenModel);
    }
}
