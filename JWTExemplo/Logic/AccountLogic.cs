﻿using JWTExemplo.Data.Entities;
using JWTExemplo.Models;
using JWTExemplo.Shared;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JWTExemplo.Data.Context;
using System.Security.Cryptography;

namespace JWTExemplo.Logic
{
    public class AccountLogic : IAccountLogic
    {

        private readonly TokenSettings _tokenSettings;

        private readonly AuthDBContext _authDBContext;

        public AccountLogic(IOptions<TokenSettings> tokenSettings, AuthDBContext authDBContext)
        {
            _tokenSettings = tokenSettings.Value;
            _authDBContext = authDBContext;
        }

        //private List<User> Users = new List<User>
        //{
        //    new User
        //    {
        //        Id = 1,
        //        FirstName = "Naveen",
        //        LastName = "Bommidi",
        //        Email = "naveen@gmail.com",
        //        Password = "1234",
        //        PhoneNumber = "888889999"
        //    },
        //    new User
        //    {
        //        Id = 2,
        //        FirstName = "Hemanth",
        //        LastName = "Kumar",
        //        Email = "hemanth@gmail.com",
        //        Password = "abcd",
        //        PhoneNumber = "222229999"
        //    }
        //};
        public TokenModel GetAuthenticationToken(LoginModel loginModel)
        {
            //User currentUser = Users.Where(_ => _.Email.ToLower() == loginModel.Email.ToLower() && _.Password ==
            //loginModel.Password).FirstOrDefault();

            User currentUser = _authDBContext.User.Where(_ => _.Email.ToLower() == loginModel.Email.ToLower() && _.Password ==
           loginModel.Password).FirstOrDefault();

            if (currentUser != null)
            {
                var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
                var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

                var userCliams = new Claim[]
                {
                    new Claim("email",currentUser.Email),
                    new Claim("phone", currentUser.PhoneNumber),
                };

                var jwtToken = new JwtSecurityToken(
                    issuer: _tokenSettings.Issuer,
                    audience: _tokenSettings.Audience,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: credentials,
                    claims:userCliams
                    );

                string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                string refreshToken = GetRefreshToken();

                currentUser.RefreshToken = refreshToken;
                _authDBContext.SaveChanges();


                return new TokenModel
                {
                    Token = token,
                    RefreshToken = refreshToken
                };

            }

            return null;
        }
        private string GetRefreshToken()
        {
            var key = new Byte[32];

            using (var refreshTokenGenerator = RandomNumberGenerator.Create())
            {
                refreshTokenGenerator.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        public TokenModel ActiveTokenUsingRefreshToken(TokenModel tokenModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(tokenModel.Token,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _tokenSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _tokenSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key)),
                    ValidateLifetime = true

                }, out SecurityToken validateToken);

            var jwtToken = validateToken as JwtSecurityToken;

            if(jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
            {
                return null;
            }

            var email = claimsPrincipal.Claims.Where(_ => _.Type == ClaimTypes.Email).Select(_ => _.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var currentUser = _authDBContext.User.Where(_ => _.Email == email && _.RefreshToken == tokenModel.RefreshToken).FirstOrDefault();
            if(currentUser == null)
            {
                return null;
            }

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var newJwtToken = new JwtSecurityToken(
                issuer: _tokenSettings.Issuer,
                audience: _tokenSettings.Audience,
                expires: DateTime.Now.AddMinutes(20),
                signingCredentials: credentials,
                claims: jwtToken.Claims
                );

            string token = new JwtSecurityTokenHandler().WriteToken(newJwtToken);
            string refreshToken = GetRefreshToken();

            currentUser.RefreshToken = refreshToken;

            _authDBContext.SaveChanges();


            return new TokenModel
            {
                Token = token,
                RefreshToken = refreshToken
            };

        }


    }

}
