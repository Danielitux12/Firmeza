using Firmeza.Application.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Admin.Controllers;

/// <summary>
/// Gestiona la autenticación de usuarios mediante cookies de Identity para el panel de administración.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // Muestra la vista del formulario de login.
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Si el usuario ya está autenticado como administrador, se le redirige al dashboard.
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Administrador"))
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // Procesa el inicio de sesión validando credenciales y que el usuario pertenezca al rol Administrador.
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // 1. Busca el usuario por correo electrónico.
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
            return View(model);
        }

        // 2. Verifica la contraseña.
        var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        if (!passwordValid)
        {
            ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
            return View(model);
        }

        // 3. Verifica que posea el rol "Administrador".
        var isAdmin = await _userManager.IsInRoleAsync(user, "Administrador");
        if (!isAdmin)
        {
            // Se bloquea el acceso y se cierra cualquier sesión por seguridad.
            await _signInManager.SignOutAsync();
            ModelState.AddModelError(string.Empty, "Acceso no autorizado: tu cuenta no posee el rol de Administrador necesario para ingresar a este panel.");
            return View(model);
        }

        // 4. Si es Administrador, inicia la sesión mediante cookie.
        await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // Cierra la sesión activa del usuario y redirige a la pantalla de login.
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    // Muestra una pantalla amigable cuando un usuario autenticado intenta acceder a un recurso sin permisos.
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
