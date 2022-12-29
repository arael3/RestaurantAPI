using FluentValidation;
using RestaurantAPI.Entities;

namespace RestaurantAPI.Models.Validators
{
    public class RestaurantQueryValidator : AbstractValidator<RestaurantQuery>
    {
        private int[] allowedPageSizes = new []{ 5, 10, 15 };
        private string[] allowedSortByColumnName = new []{ nameof(Restaurant.Name), nameof(Restaurant.Category), nameof(Restaurant.Description) };
        public RestaurantQueryValidator()
        {
            RuleFor(r => r.PageNumber).GreaterThanOrEqualTo(1);

            RuleFor(r => r.PageSize).Custom((value, contex) =>
            {
                if(!allowedPageSizes.Contains(value))
                {
                    contex.AddFailure("PageSize", $"PageSize must in [{string.Join(", ", allowedPageSizes)}]");
                }
            });

            RuleFor(r => r.SortBy)
                .Must(value => string.IsNullOrEmpty(value) || allowedSortByColumnName.Contains(value))
                .WithMessage($"SortBy is optional or must be in [{string.Join(", ", allowedSortByColumnName)}]");
        }
    }
}
