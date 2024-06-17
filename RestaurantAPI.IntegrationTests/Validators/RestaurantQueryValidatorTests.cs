using FluentValidation.TestHelper;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;

namespace RestaurantAPI.IntegrationTests
{
    public class RestaurantQueryValidatorTests
    {
        public static IEnumerable<object[]> ValidSampleData()
        { 
        var list = new List<RestaurantQuery>
            {
                new RestaurantQuery() { PageNumber = 1, PageSize = 10 } ,
                new RestaurantQuery() { PageNumber = 1, PageSize = 5 } 
            };
            return list.Select(x=> new object[] { x });
        }

        public static IEnumerable<object[]> InvalidSampleData()
        {
            var list = new List<RestaurantQuery>
            {
                new RestaurantQuery() { PageNumber = 0, PageSize = 10 } ,
                new RestaurantQuery() { PageNumber = 1, PageSize = 2 }
            };
            return list.Select(x => new object[] { x });
        }

        [Theory]
        [MemberData(nameof(ValidSampleData))]
        public void Validate_ValidModel_ReturnSuccess(RestaurantQuery model)
        { 
            var validator = new RestaurantQueryValidator();
            var result = validator.TestValidate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }
        [Theory]
        [MemberData(nameof(InvalidSampleData))]
        public void Validate_InvalidModel_ReturnSuccess(RestaurantQuery model)
        {
            var validator = new RestaurantQueryValidator();
            var result = validator.TestValidate(model);
            result.ShouldHaveAnyValidationError();
        }
    }
}
