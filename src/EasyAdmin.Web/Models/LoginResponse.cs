﻿namespace EasyAdmin.Web.Models;

public class LoginResponse
{
    /// <summary>
    /// 令牌Token
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// 刷新Token
    /// </summary>
    //public string RefreshToken { get; set; }
}