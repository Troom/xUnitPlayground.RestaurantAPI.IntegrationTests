using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;

namespace RestaurantAPI.IntegrationTests.Validators
{
    public class RegisterUserDtoValidatorTests
    {
        private readonly RestaurantDbContext _dbContext;

        public RegisterUserDtoValidatorTests() {
            var builder = new DbContextOptionsBuilder<RestaurantDbContext>();
            builder.UseInMemoryDatabase("RegisterUserDtoValidatorTests");
            _dbContext = new RestaurantDbContext(builder.Options);
            DataSeed(); 
        }
        public void DataSeed() { 
        var users = new List<User>
            {
                new User
                {
                    Email = "test@test.pl"
                }
            };
            _dbContext.Users.AddRange(users);
            _dbContext.SaveChanges();
        }

        [Fact]
        public void Validate_ValidModel_ReturnSuccess()
        {
            var model = new RegisterUserDto
            {
                Email = "john@doe.com",
                Password = "testtest",
                ConfirmPassword = "testtest"
            };
            var validator = new RegisterUserDtoValidator(_dbContext);
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }



        public static IEnumerable<object[]> InvalidSampleData()
        {
            var list = new List<RegisterUserDto>
            {
             new RegisterUserDto()
                {
                
                    Email = "test@test.pl",
                    Password = "testtest",
                    ConfirmPassword = "testtest"
                },
             new RegisterUserDto()
                {
                    Email = "john@doe.com",
                    Password = "test",
                    ConfirmPassword = "test"
                },
            };
            return list.Select(x => new object[] { x });
        }

        [Theory]
        [MemberData(nameof(InvalidSampleData))]
        public void Validate_InvalidModel_ReturnFail(RegisterUserDto userDto)
        {
            var model = new RegisterUserDto
            {
                Email = "john@doe.com",
                Password = "test",
                ConfirmPassword = "test"
            };
            var validator = new RegisterUserDtoValidator(_dbContext);
            var result = validator.TestValidate(userDto);
            result.ShouldHaveAnyValidationError();
        }
    }
}
