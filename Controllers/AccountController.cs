using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

public class AccountController(UserService _userService) : Controller
{
    public IActionResult Login()
    {
        var viewModel = new LoginViewModel();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewmodel)
    {

        if (!ModelState.IsValid) return View(viewmodel);
        var found = await _userService.Login(viewmodel);


        if (found.UserId == 0)
        {
            ViewBag.message = "No matches found.";
            return View();
        }
        else
        {
            List<Claim> claims = new List<Claim>() {
                    new Claim(ClaimTypes.NameIdentifier,found.UserId.ToString()),
                    new Claim(ClaimTypes.Name,found.FullName),
                    new Claim(ClaimTypes.Email,found.Email),
                    new Claim(ClaimTypes.Role,found.Type)
                };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties() { AllowRefresh = true }
                );

            return RedirectToAction("Index", "Home");
        }
    }

    public IActionResult Register()
    {
        var viewModel = new UserViewModel();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserViewModel viewmodel)
    {

        if (!ModelState.IsValid) return View(viewmodel);

        try
        {
            await _userService.Register(viewmodel);
            ViewBag.message = "Your account has been registered, please try logging in.";
            ViewBag.Class = "alert-success";
        }
        catch (Exception ex)
        {
            ViewBag.message = ex.Message;
            ViewBag.Class = "alert-danger";
        }

        return View();


    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

}
