using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using RestaurantAPI;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Middleware;
using RestaurantAPI.Models;
using RestaurantAPI.Models.Validators;
using RestaurantAPI.Services;
using System.Reflection;
using System.Text;


// --- I - utworzenie aplikacji/webhosta---
var builder = WebApplication.CreateBuilder(args);


// --- II - dodanie zale�no�ci/serwis�w w kontenerze dependecy injections ---

// START - uwierzytelnianie w oparciu o Json Web Token (JWT)
var authenticationSettings = new AuthenticationSettings(); // [1] utworznie obiektu klasy AuthenticationSettings
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings); // [2] powi�zanie obiektu klasy AuthenticationSettings z konfiguracj� zdefiniowan� w pliku appsettings.json w sekcji Authentication
builder.Services.AddSingleton(authenticationSettings); // [3] wstrzykni�cie konfiguracji z pliku appsettings.json
builder.Services
    .AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = "Bearer";
        option.DefaultScheme = "Bearer";
        option.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;  // nie wymuszamy na kliencie logowania przez https
        cfg.SaveToken = true;  // dany token powinien zosta� zapisany po stronie serwera do cel�w uwierzytelniania
        cfg.TokenValidationParameters = new TokenValidationParameters  // parametry walidacji, umo�liwiajace sprawdzenie czy token wys�any przez klienta jest zgodny z tym co wie serwer
        {
            ValidIssuer = authenticationSettings.JwtIssuer,  // ustalenie issuer - czyli wydawca danego tokenu
            ValidAudience = authenticationSettings.JwtIssuer,  // audience - czyli jakie podmioty mog� u�ywa� tokenu, w tym przypadku jest to ta sama warto�� co dla ValidIssuer, poniewa� b�dziemy generowa� token w obr�bie naszej aplikacji i tylko tacy klienci b�d� dopuszczeni do uwierzytelniania
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)) // signing key - klucz prywatny wygenerowany na podstawie warto�ci authenticationSettings.JwtKey, czyli zapisanej w appsettings.json >> "Authentication" >> "JwtKey": "PRIVATE_KEY_DOTN_SHARE"
        };
    });

// CUSTOMOWE regu�y autoryzacji
// Rozbudowa autoryzacji w oparciu o w�asn� (customow�) polityk�
// poni�sza polityka o nazwie "HasNationality" dzia�a w ten spos�b, �e wymaga aby ��danie (claim) o nazwie "Nationality" istnia�o w tokenie oraz aby warto�� jego warto�� by�a r�wna "Polish"
// Je�eli claim o nazwie "Nationality" nie istnieje w tokenie, u�ytkownik nie otrzyma autoryzacji do wykonania danej akcji.
// przyk�adowo claim "Nationality" nie jest dodawany w tokenie dla u�ytkownika, kt�ry nie ma wype�nionego pola "Nationality"
// metoda RequireClaim() mo�e przyjmowa� wiele paramet�w, kt�re spe�niaj� warunki np. RequireClaim("Nationality", "Polish", "Czech", "German")
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality", "Polish", "Czech"));
    options.AddPolicy("IsAdult", builder => builder.AddRequirements(new MinimumAgeRequirement(18)));
    options.AddPolicy("IsCreator", builder => builder.AddRequirements(new MinimumCreatedRestaurantsRequirement(2)));
});
builder.Services.AddScoped<IAuthorizationHandler, MinimumAgeRequirementHandler>();  // rejestracja serwisu odpowiadaj�cego za obs�u�enie autoryzacji w oparciu o w�asn� (customow�) polityk� o nazwie "IsAdult" ze zdefiniowanymi w metodzie AddRequirements() w�asnymi wymaganiami - w tym przypadku wymaganym minimalnym wiekiem u�ytkownika - u�ytkownik, kt�rego wiek jest mniejszy od minimalnego, nie otrzyma autoryzacji do wykonania danej akcji
builder.Services.AddScoped<IAuthorizationHandler, ResourceOperationRequirementHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MinimumCreatedRestaurantsRequirementHandler>();
// END - uwierzytelnianie w oparciu o Json Web Token (JWT)

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddDbContext<RestaurantDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RestaurantDbConnection")));
// Dodanie serwisu RestaurantSeeder poprzez metod� AddScoped(). Serwis RestaurantSeeder umo�liwia za�adowanie do bazy danych seed�w (rekord�w), od razu po uruchomieniu aplikacji. Dzieje si� to porzez uruchomienie metody Seed(), kt�ra jest wywo�ywana poni�ej. Aby mo�na by�o odwo�a� si� do metody Seed(), potrzebne jest utworzenie scope poprzez metod� CreateScope(), a nast�pnie z wykorzystaniem utworzonego scope odwo�anie si� do serwisu RestaurantSeeder poprzez metod� GetRequiredService().
builder.Services.AddScoped<RestaurantSeeder>();
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<RequestTimeMiddleware>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IValidator<RegisterUserDto>, RegisterUserDtoValidator>();
builder.Services.AddScoped<IValidator<RestaurantQuery>, RestaurantQueryValidator>();

builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddHttpContextAccessor(); // rejestracja tego serwisu umo�liwi wstrzykni�cie do klasy UserContextService referencji do obiektu typu IHttpContextAccessor, kt�ry jest wykorzystywany w customowej klasie UserContextService. Klasa UserContextService jest odpowiedzialna za udost�pnianie informacji o danym u�ytkowniku na podstawie kontekstu Http

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FronEndClient", policyBuilder =>

        policyBuilder.AllowAnyMethod()
            .AllowAnyHeader()
            .WithOrigins(builder.Configuration["AllowedOrigins"])
    );
});

// zbudowanie aplikacji
var app = builder.Build();



// --- III - rejestrowanie przep�yw�w zapytania / ustawienie middleware  ---
app.UseResponseCaching(); // Odpowiada za mechanizm keszowania
app.UseStaticFiles();  // Dodajemy na pocz�tku listy przep�yw�w, aby aplikacja dla ka�dego zapytania sprawdza�a, czy pod �cie�k� do zapytania nie istnieje jaki� plik
app.UseCors("FrontEndClient");

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();
seeder.Seed();
app.UseMiddleware<RequestTimeMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication(); // odpowiada za to, �e ka�dy request wys�any przez klienta api musi podlega� uwierzytelnianiu 
// app.UseAuthentication() nale�y umie�ci� przed app.UseHttpsRedirection()
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API");
});

app.UseAuthorization();
// app.UseAuthorization() nale�y umie�ci� przed app.MapControllers()
app.MapControllers();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
app.Run();
