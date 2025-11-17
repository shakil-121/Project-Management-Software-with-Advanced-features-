using FastPMS.Models.Domain;
using FastPMS.Models.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FastPMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>Login(LoginViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure:false); 

            if(result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            } 

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt"); 

            return View(model);

        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var debugMessages = new List<string>();
            debugMessages.Add("🔧 DEBUG: Registration Started");

            // Show ALL field values
            debugMessages.Add($"📋 FIELD VALUES:");
            debugMessages.Add($"- Name: '{model.Name}'");
            debugMessages.Add($"- Email: '{model.Email}'");
            debugMessages.Add($"- Password: SET");
            debugMessages.Add($"- ConfirmPassword: SET");
            debugMessages.Add($"- Role: '{model.Role}'");
            debugMessages.Add($"- Department: '{model.Department}'");

            var currentUser = await userManager.GetUserAsync(User);
            var isSuperAdmin = currentUser != null && currentUser.Role == "SuperAdmin";
            debugMessages.Add($"- IsSuperAdmin: {isSuperAdmin}");

            // 🔥 FIX: COMPLETELY CLEAR AND REVALIDATE MODEL STATE
            ModelState.Clear(); // Complete clear

            // Manually re-validate required fields
            if (string.IsNullOrEmpty(model.Name))
                ModelState.AddModelError("Name", "Name is required.");

            if (string.IsNullOrEmpty(model.Email))
                ModelState.AddModelError("Email", "Email is required.");
            else if (!new EmailAddressAttribute().IsValid(model.Email))
                ModelState.AddModelError("Email", "Invalid email format.");

            if (string.IsNullOrEmpty(model.Password))
                ModelState.AddModelError("Password", "Password is required.");
            else if (model.Password.Length < 8)
                ModelState.AddModelError("Password", "Password must be at least 8 characters.");

            if (string.IsNullOrEmpty(model.ConfirmPassword))
                ModelState.AddModelError("ConfirmPassword", "Confirm Password is required.");
            else if (model.Password != model.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");

            debugMessages.Add("✅ Manually re-validated all fields");

            // Check validation again
            if (!ModelState.IsValid)
            {
                debugMessages.Add("❌ VALIDATION FAILED AFTER MANUAL CHECK:");

                int errorCount = 0;
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        errorCount++;
                        debugMessages.Add($"🚨 ERROR #{errorCount}: {key} - {error.ErrorMessage}");
                    }
                }

                debugMessages.Add($"📊 Total errors: {errorCount}");
                ViewBag.DebugMessages = debugMessages;
                return View(model);
            }

            debugMessages.Add("✅ ALL VALIDATIONS PASSED - Proceeding with user creation");

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                debugMessages.Add("❌ User already exists");
                ViewBag.DebugMessages = debugMessages;
                ModelState.AddModelError("", "User with this email already exists.");
                return View(model);
            }

            // Create user
            var user = new Users
            {
                FullName = model.Name,
                UserName = model.Email,
                NormalizedUserName = model.Email.ToUpper(),
                Email = model.Email,
                NormalizedEmail = model.Email.ToUpper(),
                Role = model.Role,
                Department = model.Department ?? ""
            };

            debugMessages.Add($"Creating user: {model.Email} with Role: {model.Role}");

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                debugMessages.Add("🎉 USER CREATED SUCCESSFULLY!");

                // Handle role
                var roleExists = await roleManager.RoleExistsAsync(model.Role);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(model.Role));
                    debugMessages.Add($"✅ Created new role: {model.Role}");
                }

                await userManager.AddToRoleAsync(user, model.Role);
                debugMessages.Add($"✅ Role assigned: {model.Role}");

                ViewBag.DebugMessages = debugMessages;

                if (isSuperAdmin)
                {
                    TempData["Success"] = $"Admin account created successfully!";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return RedirectToAction("Login", "Account", new { message = "Registration successful! Please login." });
                }
            }
            else
            {
                debugMessages.Add("❌ User creation failed");
                foreach (var error in result.Errors)
                {
                    debugMessages.Add($"Error: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
                ViewBag.DebugMessages = debugMessages;
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        { 
        if(!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await userManager.FindByNameAsync(model.Email);
            if(user==null)
            {
                ModelState.AddModelError(string.Empty, "user not found");
                return View(model);
            }
            // Here you would typically generate a token and send it via email
            // For simplicity, we will just redirect to ResetPassword action
            return RedirectToAction("ChangePassword","Account", new { username = user.UserName });

        }
        [HttpGet]
        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail","Account");
            }

            return View(new ChangePasswordViewModel { Email = username });
        
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(model);
            }

            var user = await userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found!");
                return View(model);
            }

            var result = await userManager.RemovePasswordAsync(user);
            if (result.Succeeded)
            {
                result = await userManager.AddPasswordAsync(user, model.NewPassword);
                return RedirectToAction("Login", "Account");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }




    }
}
