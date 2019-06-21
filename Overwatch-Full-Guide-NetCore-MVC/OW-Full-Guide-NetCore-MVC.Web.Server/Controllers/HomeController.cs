using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace OW_Full_Guide_NetCore_MVC.Web.Server.Controllers
{
    /// <summary>
    /// Manages the standard web server pages
    /// </summary>
    public class HomeController : Controller
    {
        #region Protected Members

        /// <summary>
        /// The scoped Application context
        /// </summary>
        protected ApplicationDbContext mContext;

        /// <summary>
        /// The Manager for handling user creation, deletion, searching, roles etc...
        /// </summary>
        protected UserManager<ApplicationUser> mUserManager;

        /// <summary>
        /// The Manager for handling signing in and out for our users
        /// </summary>
        protected SignInManager<ApplicationUser> mSignInManager;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context">The injected context</param>
        public HomeController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager)
        {
            mContext = context;
            mUserManager = userManager;
            mSignInManager = signInManager;
        }

        #endregion

        public IActionResult Index()
        {
            // Make sure we have the database
            mContext.Database.EnsureCreated();

            // If we have no settings already...
            if (!mContext.Settings.Any())
            {
                // Add a new setting
                mContext.Settings.Add(new SettingsDataModel
                {
                    Name = "BackgroundColor",
                    Value = "Red"
                });

                // Check to show the new setting is currently only local and not in the database
                var settingsLocally = mContext.Settings.Local.Count();
                var settingsDatabase = mContext.Settings.Count();
                var firstLocal = mContext.Settings.Local.FirstOrDefault();
                var firstDatabase = mContext.Settings.FirstOrDefault();

                // Commit setting to database
                mContext.SaveChanges();

                // Recheck to show its now in local and the actual database
                settingsLocally = mContext.Settings.Local.Count();
                settingsDatabase = mContext.Settings.Count();
                firstLocal = mContext.Settings.Local.FirstOrDefault();
                firstDatabase = mContext.Settings.FirstOrDefault();
            }

            return View();
        }

        /// <summary>
        /// Create a Single User
        /// </summary>
        /// <returns></returns>
        [Route("create")]
        public async Task<IActionResult> CreateUserAsync()
        {
            var result = await mUserManager.CreateAsync(new ApplicationUser
            {
                UserName = "Uponia",
                Email = "Adam.r.thain@gmail.com"
            }, "password");
            
            // If the result of the login is successful
            // else it fails
            if(result.Succeeded)
                return Content("User was created", "text/html");

            return Content("User creation failed","text/html");
        } 

        /// <summary>
        /// private area. No peeking 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [Route("private")]
        public IActionResult Private()
        {
            return Content($"This is a private area.welcome {HttpContext.User.Identity.Name}", "text/html");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("logout")]
        public async Task<IActionResult> SignOutAsync()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Content("done");
        }

        /// <summary>
        /// An auto-login page for testing
        /// </summary>
        /// <param name="returnUrl"> The URL to return if successfully logged in </param>
        /// <returns></returns>
        [Route("login")]
        public async Task<IActionResult> LoginAsync(string returnUrl)
        {
            //Sign Out Previous Sessions
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Sign user in with the valid credentials
            var result = await mSignInManager.PasswordSignInAsync("Uponia","password", true,false);

            // If we have no
            if (result.Succeeded)
            {
                // If we have no return URL
                if (string.IsNullOrEmpty(returnUrl))
                    // Go to home
                    return RedirectToAction(nameof(Index));

                // Otherwise, go to the return url
                return Redirect(returnUrl);
            }
               
            return Content("Failed to login","text/html");
        }
    }
}