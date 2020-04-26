﻿using SGNWebAppCli.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGNWebAppCli.Services
{
    public interface IUserService
    {
        public Task<User> LoginAsync(User user);
        public Task<User> RefreshTokenAsync(RefreshRequest refreshRequest);

    }
}
