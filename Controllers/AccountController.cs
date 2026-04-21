using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

public class AccountController(
    UserService _userService,
    CartService _cartService,
    ProductService _productService) : Controller
{
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            TempData["SuccessMessage"] = "El usuario ya se encuentra logueado.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new LoginViewModel();
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel viewmodel)
    {

        if (!ModelState.IsValid) return View(viewmodel);
        var found = await _userService.Login(viewmodel);


        if (found.UserId is 0)
        {
            TempData["message"] = "Email o Contraseña incorrectos.";
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

            await MergeSessionCartAsync(found.UserId);

            if (string.Equals(found.Type, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("CreateAdmin");
            }

            return RedirectToAction("Index", "Home");
        }
    }

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            TempData["SuccessMessage"] = "El usuario ya se encuentra logueado.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new UserViewModel
        {
            Type = "Client"
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserViewModel viewmodel)
    {
        viewmodel.Type = "Client";
        ModelState.Remove(nameof(UserViewModel.Type));

        if (!ModelState.IsValid) return View(viewmodel);

        try
        {
            await _userService.Register(viewmodel);
            TempData["SuccessMessage"] = "Tu cuenta ha sido registrada! Ingresa email y contraseña para acceder";
            return RedirectToAction("Login");
        }
        catch (InvalidOperationException)
        {
            TempData["ErrorMessage"] = "El email ya se encuentra registrado";
            return RedirectToAction("Register");
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "No se pudo registrar la cuenta. Intenta nuevamente";
            return RedirectToAction("Register");
        }
    }

    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var userId = GetAuthenticatedUserId();
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        var userModel = await _userService.GetProfileForEditAsync(userId.Value);
        return View("EditProfile", userModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditProfile(EditProfileViewModel viewmodel)
    {
        var userId = GetAuthenticatedUserId();
        
        if (userId == null)
        {
            return RedirectToAction("Login");
        }

        if (viewmodel.UserId != userId.Value)
        {
            return RedirectToAction("EditProfile");
        }

        if (!ModelState.IsValid) return View("EditProfile", viewmodel);

        try
        {
            var updatedUser = await _userService.UpdateProfileAsync(viewmodel);
            await RefreshSignInAsync(updatedUser);
            TempData["SuccessMessage"] = "Tus datos fueron actualizados.";
            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View("EditProfile", viewmodel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "No se pudieron actualizar los datos. Intenta nuevamente";
            return View("EditProfile", viewmodel);
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult CreateAdmin()
    {
        var viewModel = new UserViewModel
        {
            Type = "Admin"
        };

        SetAdminCreationViewData();
        return View("Register", viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateAdmin(UserViewModel viewmodel)
    {
        viewmodel.Type = "Admin";
        ModelState.Remove(nameof(UserViewModel.Type));

        if (!ModelState.IsValid)
        {
            SetAdminCreationViewData();
            return View("Register", viewmodel);
        }

        try
        {
            await _userService.Register(viewmodel);
            return RedirectToAction("AdminCreated");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            SetAdminCreationViewData();
            return View("Register", viewmodel);
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "No se pudo registrar la cuenta. Intenta nuevamente";
            SetAdminCreationViewData();
            return View("Register", viewmodel);
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    public IActionResult AdminCreated()
    {
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task MergeSessionCartAsync(int userId)
    {
        var sessionCart = HttpContext.Session.Get<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
        
        if (!sessionCart.Any())
        {
            return;
        }

        foreach (var sessionItem in sessionCart)
        {
            var product = await _productService.GetByIdAsync(sessionItem.ProductId);
            
            if (product == null)
            {
                continue;
            }

            var currentQuantity = await _cartService.GetQuantityByUserAndProductAsync(userId, sessionItem.ProductId);
            var totalQuantity = currentQuantity + sessionItem.Quantity;
            var adjustedQuantity = sessionItem.Quantity;

            if (totalQuantity > product.Stock)
            {
                adjustedQuantity = product.Stock - currentQuantity;
                if (adjustedQuantity <= 0)
                {
                    continue;
                }
            }

            var productViewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                ImageName = product.ImageName
            };

            await _cartService.AddOrIncrementAsync(userId, productViewModel, adjustedQuantity);
        }

        HttpContext.Session.Remove("Cart");
    }

    private int? GetAuthenticatedUserId()
    {
        if (HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var userId) ? userId : null;
    }

    private async Task RefreshSignInAsync(UserViewModel user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Type)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties { AllowRefresh = true });
    }

    private void SetAdminCreationViewData()
    {
        ViewData["FormTitle"] = "Crear Admin";
        ViewData["FormAction"] = "/Account/CreateAdmin";
        ViewData["ShowLogoutButton"] = true;
    }

}
