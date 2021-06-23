using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Models;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace BugTracker.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly SignInManager<BTUser> _signInManager;
        private readonly IBTImageService _bTImageService;
        private readonly IConfiguration _configuration;

        public IndexModel(
            UserManager<BTUser> userManager,
            SignInManager<BTUser> signInManager,
            IBTImageService bTImageService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _bTImageService = bTImageService;
            _configuration = configuration;
        }

        public string Username { get; set; }

        public string CurrentImage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Display Name")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            public string DisplayName { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Update Profile Image")]
            public IFormFile NewImage { get; set; }
        }

        private async Task LoadAsync(BTUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            if(user.AvatarFileData is not null)
            {
                 _bTImageService.DecodeImage(user.AvatarFileData, user.AvatarContentType);
            }
            else
            {
                CurrentImage = "//i.ibb.co/Yd6P9rF/default-profile-picture.jpg";
            }
                

            Input = new InputModel
            {
                DisplayName = user.DisplayName,
                PhoneNumber = phoneNumber

            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            var hasChanged = false;

            //Store the new displayName if it has changed
            if (user.DisplayName != Input.DisplayName)
            {
                //store the new name
                user.DisplayName = Input.DisplayName;
                hasChanged = true;
            }

            if (Input.NewImage is not null)
            {
                user.AvatarFileData = await _bTImageService.EncodeFileAsync(Input.NewImage);
                hasChanged = true;

                user.AvatarContentType = _bTImageService.ContentType(Input.NewImage);
                hasChanged = true;
            }

            if (hasChanged == true)
            {
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
