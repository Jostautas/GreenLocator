using GreenLocator.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Moq;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoFixture.AutoMoq;
using GreenLocator.Pages;
using NuGet.Frameworks;
using System.Net.WebSockets;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace GLTests
{
    public class UnitTest1
    {
        [Fact]
        public void ConnectionToDBValid()
        {
            var context = new GreenLocatorDBContext();
            Assert.True(context.Database.CanConnect());
        }

        [Theory, AutoData]
        public void ConnectionToDB_valid(IFixture fixture)
        {
            fixture.Customize(new AutoMoqCustomization());
            var context = fixture.Create<GreenLocatorDBContext>();

            Assert.True(context.Database.CanConnect());
        }

        [Theory]
        [InlineData(null, null, null, true)]
        [InlineData("Vilnius", "Didlaukio", 47, false)]
        public void checkCheckIfCurrentUserArgsNull(string? City, string? Street, int? House, bool IsNull)
        {
            var user = new AspNetUser
            {
                City = City,
                Street = Street,
                House = House,
            };

            var MainModel = new MainModel(null!);

            Assert.Equal(IsNull, MainModel.checkIfCurrentUserArgsNull(user));
        }

        [Theory]
        [InlineData("Vilnius", "Jeruzales", 4, 1, 2)]
        [InlineData("Vilnius", "Gelezinio Vilko", 15, 0, 2)]
        [InlineData("Vilnius", "Visoriu sodu 1-oji", 78, 1, 0)]
        [InlineData("Vilnius", "Dariaus ir Gireno", 56, 2, 1)]
        [InlineData("Vilnius", "Didlaukio", 25, 0, 0)]
        [InlineData("Ukmerge", "Vytauto", 69, 1, 1)]
        [InlineData("Ukmerge", "Kauno", 55, 2, 2)]
        [InlineData("Naujoji Akmene", "J. Dalinkeviciaus", 37, 2, 0)]
        public void Extensions_TestWhenAllValuesNotNull(string City, string Street, int House, 
            int ShareStatus, int ThingToShare)
        {
            var user = new AspNetUser
            {
                City = City,
                Street = Street,
                House = House,
                ShareStatus = ShareStatus,
                ThingToShare = ThingToShare,
            };

            user.City = City;
            user.Street = Street;
            user.House = House;
            user.ShareStatus = ShareStatus;
            user.ThingToShare = ThingToShare;

            Assert.False(Extensions.CheckIfUsrFieldsNull(user));
            Assert.False(Extensions.CheckIfUsrStatusNull(user));
            Assert.False(Extensions.CheckIfUsrNull(user));
        }

        [Theory]
        [InlineData(null, null, null, null, null)]
        [InlineData("Vilnius", null, null, null, null)]
        [InlineData("Vilnius", "Visoriu sodu 1-oji", null, null, 0)]
        [InlineData(null, "Dariaus ir Gireno", null, null, null)]
        [InlineData(null, "Didlaukio", 25, 0, null)]
        [InlineData("Ukmerge", "Vytauto", null, 0, null)]
        [InlineData("Ukmerge", null, 55, 2, null)]
        [InlineData("Naujoji Akmene", null, 37, null, null)]
        public void Extensions_TestWhenSomeValuesAreNull(string? City, string? Street, int? House,
           int? ShareStatus, int? ThingToShare)
        {
            var user = new AspNetUser
            {
                City = City,
                Street = Street,
                House = House,
                ShareStatus = ShareStatus,
                ThingToShare = ThingToShare,
            };

             Assert.False(Extensions.CheckIfUsrNull(user));
             Assert.True(Extensions.CheckIfUsrFieldsNull(user));
             Assert.True(Extensions.CheckIfUsrStatusNull(user));
        }

        [Theory]
        [InlineData("Vilnius", "Jeruzales", 4)]
        [InlineData("Vilnius", "Gelezinio Vilko", 15)]
        [InlineData("Vilnius", "Visoriu sodu 1-oji", 78)]
        [InlineData("Vilnius", "Dariaus ir Gireno", 56)]
        [InlineData("Vilnius", "Didlaukio", 25)]
        [InlineData("Ukmerge", "Vytauto", 69)]
        [InlineData("Ukmerge", "Kauno", 55)]
        [InlineData("Naujoji Akmene", "J. Dalinkeviciaus", 37)]
        public void checkIfInputValidationTrue(string City, string Street, int House)
        {
            var user = new AspNetUser
            {
                City = City,
                Street = Street,
                House = House,
            };

            var enterInfoModel = new EnterInfoModel(null);

            Assert.True(enterInfoModel.InputValidation(user.City, user.Street, (int)user.House));
        }

        [Theory]
        [InlineData("", "Jeruzales", 4)]
        [InlineData("Vilnius", "", 15)]
        [InlineData("Vilnius", "Da", 56)]
        [InlineData("Vi", "Didlaukio", 25)]
        public void checkIfInputValidationFalse(string City, string Street, int? House)
        {
            var user = new AspNetUser
            {
                City = City,
                Street = Street,
                House = House,
            };

            var enterInfoModel = new EnterInfoModel(null);

            Assert.False(enterInfoModel.InputValidation(user.City, user.Street, (int)user.House));
        }

        [Theory]
        [InlineData("Vilnius", "didlaukio", 47, 1, 1, 0, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 1, 0, 0, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 0, 1, 0, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 0, 0, 0, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 2, 1, 0, 0)]
        [InlineData("Vilnius", "didlaukio", 48, 2, 2, 0, 0)]
        [InlineData("Vilnius", "didlaukijo", 47, 2, 2, 0, 0)]
        [InlineData("Vilnius", "didlaukijo", 47, 2, 2, 2, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 2, 2, 0, 1)]
        public void CheckNumOfMatchedPeople0(string? City, string? Street, int? House, int? shareStatus,
                                                int? thingToShare, int CurrentMatches, int Matches)
        {
            var optionsbuilder = new DbContextOptionsBuilder<GreenLocatorDBContext>();
            optionsbuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
            var context = new GreenLocatorDBContext(optionsbuilder.Options);

            var user1 = new AspNetUser { Id = "1", City = "Vilnius", Street = "didlaukio", House = 47, ShareStatus = 1, ThingToShare = 2 };
            var user2 = new AspNetUser { Id = "2", City = "Vilnius", Street = "didlaukio", House = 47, ShareStatus = 2, ThingToShare = 2 };
            context.Add(user1);
            context.Add(user2);
            context.SaveChanges();

            var user = new AspNetUser
            {
                City = City,
                Street = Street,
                House = House,
                ShareStatus = shareStatus,
                ThingToShare = thingToShare
            };

            object args = new object[2] { context, user };

            MainModel.currentNumberOfMatches = CurrentMatches;
            MainModel.NumOfMatchedPeople(args);

            Assert.Equal(Matches, MainModel.currentNumberOfMatches); 
        }

        [Theory]
        [InlineData("Vilnius", "Didlaukio", 47, "Main")]
        [InlineData("", "", 47, "EnterInfo")]
        [InlineData("Vilnius", "", 47, "EnterInfo")]
        [InlineData("", "Didlaukio", 47, "EnterInfo")]
        [InlineData("Naujoji Akmene", "J. Dalinkeviciaus", 37, "Main")]
        public void CheckEnterInfoRedirection(string CityInput, string StreetInput,
                    int HouseInput, string PageName)
        {
            var optionsbuilder = new DbContextOptionsBuilder<GreenLocatorDBContext>();
            optionsbuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

            GreenLocatorDBContext ctx = new(optionsbuilder.Options);
            AspNetUser user = new AspNetUser
            {
                Id = "1",
                City = null,
                Street = null,
                House = null
            };
            ctx.Add(user);
            ctx.SaveChanges();

            var enterInfoProperties = new EnterInfoViewModel
            {
                CityInput = CityInput,
                StreetInput = StreetInput,
                HouseInput = HouseInput
            };

            var sut = new EnterInfoModel(ctx);
            sut.EnterInfoViewModel = enterInfoProperties;
            var result = sut.GetInputAndRedirect(user);

            Assert.IsType<RedirectToPageResult>(result);

            var redirect = result as RedirectToPageResult;
            Assert.Equal(PageName, redirect!.PageName);
        }

        [Theory]
        [InlineData("Vilnius", "Didlaukio", 47, "Vilnius", "Didlaukio", 47)]
        [InlineData("", "", 47, (null), (null), (null))]
        [InlineData("", "J. Dalinkeviciaus", 37, (null), (null), (null))]
        [InlineData("Naujoji Akmene", "J. Dalinkeviciaus", 37, "Naujoji Akmene", "J. Dalinkeviciaus", 37)]
        public void CheckEnterInfoGetInput(string CityInput, string StreetInput,
                    int HouseInput, string ExpCity, string ExpStreet, int? ExpHouse)
        {
            var optionsbuilder = new DbContextOptionsBuilder<GreenLocatorDBContext>();
            optionsbuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

            GreenLocatorDBContext ctx = new(optionsbuilder.Options);
            AspNetUser user = new AspNetUser
            {
                Id = "1",
                City = null,
                Street = null,
                House = null
            };
            AspNetUser user1 = new AspNetUser
            {
                Id = "2",
                City = null,
                Street = null,
                House = null
            };
            ctx.Add(user);
            ctx.Add(user1);
            ctx.SaveChanges();

            var enterInfoProperties = new EnterInfoViewModel
            {
                CityInput = CityInput,
                StreetInput = StreetInput,
                HouseInput = HouseInput
            };

            AspNetUser[] userInfo = ctx.AspNetUsers.ToArray();

            Assert.Null(userInfo[0].City);
            Assert.Null(userInfo[0].Street);
            Assert.Null(userInfo[0].House);

            Assert.Null(userInfo[1].City);
            Assert.Null(userInfo[1].Street);
            Assert.Null(userInfo[1].House);

            var sut = new EnterInfoModel(ctx);
            sut.EnterInfoViewModel = enterInfoProperties;
            sut.GetInputAndRedirect(user);

            Assert.Equal(ExpCity, userInfo[0].City);
            Assert.Equal(ExpStreet, userInfo[0].Street);
            Assert.Equal(ExpHouse, userInfo[0].House);

            Assert.Null(userInfo[1].City);
            Assert.Null(userInfo[1].Street);
            Assert.Null(userInfo[1].House);
        }


        [Theory]
        [InlineData("Vilnius", "Didlaukio", 47, null, null, "Vilnius", "Didlaukio", 47, 0, 0, "EnterInfo")]
        [InlineData("Vilnius", "Didlaukio", 47, 1, 2, "Vilnius", "Didlaukio", 47, 1, 2, "EnterInfo")]
        public void CheckMainInitializeStatus(string CityInput, string StreetInput, int HouseInput,
            int? ShareStatusInput, int? ThingToShareInput, 
                    string ExpCity, string ExpStreet, int? ExpHouse,
                    int ExpShareStatus, int ExpThingToShare, string PageName)
        {
            var optionsbuilder = new DbContextOptionsBuilder<GreenLocatorDBContext>();
            optionsbuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

            GreenLocatorDBContext ctx = new(optionsbuilder.Options);
        
            AspNetUser user = new AspNetUser
            {
                Id = "1",
                City = null,
                Street = null,
                House = null,
                ThingToShare = null,
                ShareStatus = null
            };
            ctx.Add(user);
            ctx.SaveChanges();
           
            var sut = new MainModel(ctx);

            var result = sut.InitializeStatus(user);

            var redirect = result as RedirectToPageResult;
            Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal(PageName, redirect!.PageName); 
        }

        [Theory]
        [InlineData( null, null, 0, 0)]
        [InlineData( "Borrow", "Oven", 1, 2)]
        public void CheckGetInputAndChangeStatus(string ActionInput, string ApplianceInput,
                    int ExpAction, int ExpAppliance)
        {
            var optionsbuilder = new DbContextOptionsBuilder<GreenLocatorDBContext>();
            optionsbuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

            GreenLocatorDBContext ctx = new(optionsbuilder.Options);
            AspNetUser user = new AspNetUser
            {
                Id = "1",
                ThingToShare = null,
                ShareStatus = null
            };
            AspNetUser user1 = new AspNetUser
            {
                Id = "2",
                ThingToShare = null,
                ShareStatus = null
            };
            ctx.Add(user);
            ctx.Add(user1);
            ctx.SaveChanges();

            var mainProperties = new MainViewModel
            {
                ActionInput = ActionInput,
                ApplianceInput = ApplianceInput
            };

            AspNetUser[] userInfo = ctx.AspNetUsers.ToArray();

            Assert.Null(userInfo[0].City);
            Assert.Null(userInfo[0].Street);
            Assert.Null(userInfo[0].House);

            Assert.Null(userInfo[1].City);
            Assert.Null(userInfo[1].Street);
            Assert.Null(userInfo[1].House);

            var sut = new MainModel(ctx);
            sut.MainViewModel = mainProperties;
            sut.GetInputAndChangeStatus(user);

            Assert.Equal(ExpAction, userInfo[0].ShareStatus);
            Assert.Equal(ExpAppliance, userInfo[0].ThingToShare);

            Assert.Null(userInfo[1].City);
            Assert.Null(userInfo[1].Street);
            Assert.Null(userInfo[1].House);

        }

        [Theory]
        [InlineData("Vilnius", "didlaukio", 47, 1, 1, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 0, 1, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 0, 0, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 2, 1, 0)]
        [InlineData("Vilnius", "didlaukio", 48, 2, 2, 0)]
        [InlineData("Vilnius", "didlaukio", 47, 2, 2, 1)]
        [InlineData("Vilnius", "didlaukio", 47, 1, 2, 1)]
        public void CheckMessageToOptions(string? City, string? Street, int? House, int? shareStatus,
                                                int? thingToShare, int exp)
        {
            var optionsbuilder = new DbContextOptionsBuilder<GreenLocatorDBContext>();
            optionsbuilder.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
            var context = new GreenLocatorDBContext(optionsbuilder.Options);

            var user1 = new AspNetUser { Id = "1", UserName = "user1", City = "Vilnius", Street = "didlaukio", House = 47, ShareStatus = 1, ThingToShare = 2 };
            var user2 = new AspNetUser { Id = "2", UserName = "user2", City = "Vilnius", Street = "didlaukio", House = 47, ShareStatus = 0, ThingToShare = 0 };
            var user3 = new AspNetUser { Id = "3", UserName = "user3", City = "Vilnius", Street = "didlaukio", House = 47, ShareStatus = 2, ThingToShare = 2 };
            context.Add(user1);
            context.Add(user2);
            context.Add(user3);
            context.SaveChanges();

            var user = new AspNetUser
            {
                Id = "0",
                UserName = "user",
                City = City,
                Street = Street,
                House = House,
                ShareStatus = shareStatus,
                ThingToShare = thingToShare
            };

            var sut = new ChatModel(context);
            sut.generateMatchList(user);

            var list = sut.Options;
    
            Assert.Equal(exp, list.Count);
        }
    }
}