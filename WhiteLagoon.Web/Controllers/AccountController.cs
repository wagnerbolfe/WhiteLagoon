using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(IUnitOfWork unitOfWork, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Login(string returnUrl=null)
        {

            returnUrl??= Url.Content("~/");

            LoginVm loginVm = new ()
            {
                RedirectUrl = returnUrl
            };

            return View(loginVm);
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm loginVm)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager
                    .PasswordSignInAsync(loginVm.Email, loginVm.Password, loginVm.RememberMe, lockoutOnFailure:false);
                
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(loginVm.Email);
                    if (await _userManager.IsInRoleAsync(user!, StaticDetail.RoleAdmin))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(loginVm.RedirectUrl))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return LocalRedirect(loginVm.RedirectUrl);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                }
            }

            return View(loginVm);
        }
    

        public IActionResult Register(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!_roleManager.RoleExistsAsync(StaticDetail.RoleAdmin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(StaticDetail.RoleAdmin)).Wait();
                _roleManager.CreateAsync(new IdentityRole(StaticDetail.RoleCustomer)).Wait();
            }

            RegisterVm registerVm = new ()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }),
                RedirectUrl = returnUrl 
            };

            return View(registerVm);
        }
        
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm registerVm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new()
                {
                    Name = registerVm.Name,
                    Email = registerVm.Email,
                    PhoneNumber = registerVm.PhoneNumber,
                    NormalizedEmail = registerVm.Email.ToUpper(),
                    EmailConfirmed = true,
                    UserName = registerVm.Email,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, registerVm.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(registerVm.Role))
                    {
                        await _userManager.AddToRoleAsync(user, registerVm.Role);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, StaticDetail.RoleCustomer);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (string.IsNullOrEmpty(registerVm.RedirectUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return LocalRedirect(registerVm.RedirectUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                } 
            }
            registerVm.RoleList = _roleManager.Roles.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name
            });

            return View(registerVm);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        
        public IActionResult AccessDenied()
        {
            return View();
        }
        
        public IActionResult ForgotPassword()
        {
            return null;
        }
    }
}
