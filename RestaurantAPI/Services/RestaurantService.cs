using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
   public interface IRestaurantService
   {
        RestaurantDto GetById(int restaurantId);
        PageResult<RestaurantDto> GetAll(RestaurantQuery query);
        int Create(CreateRestaurantDto dto);
        void Delete(int restaurantId);
        void Update(int restaurantId, UpdateRestaurantDto dto);
   }

    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RestaurantService> _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public RestaurantService(RestaurantDbContext dbContext, IMapper mapper, ILogger<RestaurantService> logger, IAuthorizationService authorizationService, IUserContextService userContextService)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }

        public RestaurantDto GetById(int restaurantId)
        {
            var restaurant = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .FirstOrDefault(r => r.Id == restaurantId);
            
            if (restaurant is null)
                throw new NotFoundException("Restaurant not found");

            var restaurantDto = _mapper.Map<RestaurantDto>(restaurant);

            return restaurantDto;
        }

        // ####################  GETALL  ####################
        public PageResult<RestaurantDto> GetAll(RestaurantQuery query)
        {
            // Poniższa lista restauracji do wyświetlenia jest tworzona z uwględnieniem filtrów takich jak query.SearchPhrase, query.PageSize, query.PageNumber
            var baseQuery = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r => query.SearchPhrase == null || (r.Name.ToLower().Contains(query.SearchPhrase.ToLower()) || r.Description.ToLower().Contains(query.SearchPhrase.ToLower())));

            var totalCount = baseQuery.Count(); // Liczba wszystkich rezultatów spełniających kryteria filtru query.SearchPhrase

            if (!string.IsNullOrEmpty(query.SortBy))
            {
                // W poniższym słowniku kluczem (key) jest nazwa właściwości Restauracji, a wartością (value) właściwość Restauracji
                // nazwa właściwości Restauracji (jako string) ustalana jest przy użyciu wyrażenia nameof()
                // właściwość Restauracji ustalana jest przy użyciu wyrażenia lambda typu Expression<Func<Restaurant, object>>.
                // Na przykład, para klucz-wartość { nameof(Restaurant.Name), r => r.Name } oznacza, że kluczem jest ciąg znaków "Name", a wartością jest wyrażenie lambda, które zwraca właściwości Name obiektu r typu Restaurant.
                var columnsSelectors = new Dictionary<string, Expression<Func<Restaurant, object>>>
                {
                    { nameof(Restaurant.Name), r => r.Name },
                    { nameof(Restaurant.Description), r => r.Description },
                    { nameof(Restaurant.Category), r => r.Category }
                };

                var selectedColumn = columnsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC ?
                    baseQuery.OrderBy(selectedColumn)
                    : baseQuery.OrderByDescending(selectedColumn);
            }

            int x = 0;

            var restaurants = baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList();

            //var restaurantsPerPage = restaurants
            //    .Skip(query.PageSize * (query.PageNumber - 1))
            //    .Take(query.PageSize);


            // PageSize = 3, PageNumber = 3 >> strony do pominięcia = od PageSize*(PageNumber-1) następnie wypisz ilość stron zgodnie z wartością PageNumber
            //1 {...}
            //2 {...}
            //3 {...}
            //4 {...}
            //5 {...}
            //6 {...}
            //------ PageNumber: 3 --------
            //7 {...}
            //8 {...}
            //9 {...}
            //-----------------------------
            //10 {...}
            //11 {...}

            //Powyższy przykład oznacza, że jeśli klient chciałby wczytać wyniki ze strony nr 2 i ilość elementów na stronę byłaby ustawiona na 5, to algorytm powinien zaprezentować obiekty od 6 do 10


            // Zamiast ręcznego mapowania poszczególnych pól można skorzystać z paczki AutoMapper
            // poniżej przykład ręcznego mapowania pól, a jeszcze niżej przykład z użyciem AutoMapper
            //var restaurantsDtos = restaurants.Select(r => new RestaurantDto()
            //{
            //    Id = r.Id,
            //    Name = r.Name,
            //    Description = r.Description,
            //    Category = r.Category,
            //    HasDelivery = r.HasDelivery,
            //    City = r.Address.City,
            //    Street = r.Address.Street,
            //    PostalCode = r.Address.PostalCode
            //});

            if (restaurants is null)
            {
                return null;
            }

            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);            

            var pageResult = new PageResult<RestaurantDto>(restaurantsDtos, totalCount, query.PageSize, query.PageNumber);

            return pageResult;
        }

        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);

            // _userContextService opiera się o własną (customową) klasę UserContextService, która jest odpowiedzialna za udostępnianie informacji o danym użytkowniku na podstawie kontekstu Http. Stworzona klasa korzysta z IHttpContextAccessor, który umożliwia dostęp do kontekstu Http nawet poza kontrolerem
            restaurant.CreatedById = _userContextService.GetUserId; ;
            _dbContext.Restaurants.Add(restaurant);
            _dbContext.SaveChanges();

            return restaurant.Id;
        }

        // public void Delete(int restaurantId, ClaimsPrincipal user)  // Sposób na przeprowadzenie autoryzacji z wykorzystaniem perametru ClaimsPrincipal user.

        // Inny sposób (który jest tu wykorzystywany) opiera się o własną (customową) klasę UserContextService, która jest odpowiedzialna za udostępnianie informacji o danym użytkowniku na podstawie kontekstu Http. Stworzona klasa korzysta z IHttpContextAccessor, który umożliwia dostęp do kontekstu Http nawet poza kontrolerem
        public void Delete(int restaurantId)
        {
            _logger.LogInformation($"Restaurant with id {restaurantId} DELETE action invoked");

            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");

            var user = _userContextService.User;

            var authorizationResult = _authorizationService.AuthorizeAsync(user, restaurant, new ResourceOperationRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
                throw new ForbidException();

            _dbContext.Restaurants.Remove(restaurant);
            _dbContext.SaveChanges();

            _logger.LogInformation($"Restaurant with id {restaurantId} DELETE action complete");
        }

        //public void Update(int restaurantId, UpdateRestaurantDto dto, ClaimsPrincipal user) // Sposób na przeprowadzenie autoryzacji z wykorzystaniem perametru ClaimsPrincipal user.

        // Inny sposób (który jest tu wykorzystywany) opiera się o własną (customową) klasę UserContextService, która jest odpowiedzialna za udostępnianie informacji o danym użytkowniku na podstawie kontekstu Http. Stworzona klasa korzysta z IHttpContextAccessor, który umożliwia dostęp do kontekstu Http nawet poza kontrolerem
        public void Update(int restaurantId, UpdateRestaurantDto dto)
        {
            var restaurant = _dbContext
                .Restaurants
                .FirstOrDefault(r => r.Id == restaurantId);

            if (restaurant == null)
                throw new NotFoundException("Restaurant not found");

            var user = _userContextService.User;

            var authorizationResult = _authorizationService.AuthorizeAsync(user, restaurant, new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authorizationResult.Succeeded)
                throw new ForbidException();

            restaurant.Name = dto.Name;
            restaurant.Description = dto.Description;
            restaurant.HasDelivery = dto.HasDelivery;
            _dbContext.SaveChanges();
        }
    }
}
