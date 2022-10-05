using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenLocator.Models;

public class EnterInfoViewModel
{
    public string CityInput { get; set; }

    public string StreetInput { get; set; }

    public int HouseInput { get; set; }

}