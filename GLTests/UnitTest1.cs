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

        [Theory, AutoData]
        public void OnGet_NullArg(IFixture fixture)
        {
            fixture.Customize(new AutoMoqCustomization());
            var context = fixture.Create<GreenLocatorDBContext>();

            fixture.Register((Mock<MainModel> m) => m.Object);
            var sut = fixture.Create<MainModel>();  // sut - system under test

            fixture.Register((Mock<AspNetUser> m) => m.Object);
            var user = fixture.Create<AspNetUser>();

            user.City = null;
            user.Street = null;
            user.House = null;

            Assert.Null(user.City);
            Assert.Null(user.Street);
            Assert.Null(user.House);
        }

        [Theory, AutoData]
        public void Extensions_Test(IFixture fixture)
        {
            fixture.Register((Mock<AspNetUser> m) => m.Object);
            var user = fixture.Create<AspNetUser>();

            user.City = null;
            user.Street = null;
            user.House = null;
            user.ShareStatus = null;
            user.ThingToShare = null;

            Assert.False(Extensions.CheckIfUsrNull(user));
            Assert.True(Extensions.CheckIfUsrFieldsNull(user));
            Assert.True(Extensions.CheckIfUsrStatusNull(user));

            user.City = "Vilnius";
            user.Street = "Didlaukio";
            user.House = 59;
            user.ShareStatus = 1;
            user.ThingToShare = 2;

            Assert.False(Extensions.CheckIfUsrFieldsNull(user));
            Assert.False(Extensions.CheckIfUsrStatusNull(user));

            user.City = "Vilnius";
            user.Street = "Didlaukio";
            user.House = null;
            user.ShareStatus = 1;
            user.ThingToShare = null;

            Assert.True(Extensions.CheckIfUsrFieldsNull(user));
            Assert.True(Extensions.CheckIfUsrStatusNull(user));


        }
    }
}