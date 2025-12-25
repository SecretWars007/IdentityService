using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.UseCases.Users
{
    public class RegisterUserUseCase
    {
        public RegisterUserRequest Request { get; }

        public RegisterUserUseCase(RegisterUserRequest request)
        {
            Request = request;
        }
    }
}
