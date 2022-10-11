using GreenLocator.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenLocator.Pages;

public class EnterInfoModel : PageModel
{
    [BindProperty]
    public EnterInfoViewModel EnterInfoViewModel { get; set; } = null!;

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        try
        {
            using (var context = new GreenLocatorDBContext())
            {
                if (User.Identity == null)
                {
                    return RedirectToPage("Error");
                }

                var userList = from usr in context.AspNetUsers
                               select usr;

                AspNetUser current = userList.First(x => x.UserName == User.Identity.Name);

                current.City = EnterInfoViewModel.CityInput ?? throw new ArgumentNullException();
                current.Street = EnterInfoViewModel.StreetInput ?? throw new ArgumentNullException();
                current.House = EnterInfoViewModel.HouseInput;

                if (current.CheckIfUsrFieldsNull())
                {
                    throw new ArgumentNullException();
                }

                context.SaveChanges();

                return RedirectToPage("Main");
            }

        }
        catch (InvalidOperationException)
        {
            return RedirectToPage("EnterInfo");
        }
        catch (FormatException)
        {
            return RedirectToPage("EnterInfo");
        }
        catch (ArgumentNullException)
        {
            return RedirectToPage("EnterInfo");
        }
        catch (Exception)
        {
            return RedirectToPage("Error");
        }

    }
    public void OnGet()
    {
    }

}