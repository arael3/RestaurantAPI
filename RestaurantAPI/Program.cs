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


// --- II - dodanie zale¿noœci/serwisów w kontenerze dependecy injections ---

// START - uwierzytelnianie w oparciu o Json Web Token (JWT)
var authenticationSettings = new AuthenticationSettings(); // [1] utworznie obiektu klasy AuthenticationSettings
builder.Configuration.GetSection("Authentication").Bind(authenticationSettings); // [2] powi¹zanie obiektu klasy AuthenticationSettings z konfiguracj¹ zdefiniowan¹ w pliku appsettings.json w sekcji Authentication
builder.Services.AddSingleton(authenticationSettings); // [3] wstrzykniêcie konfiguracji z pliku appsettings.json
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
        cfg.SaveToken = true;  // dany token powinien zostaæ zapisany po stronie serwera do celów uwierzytelniania
        cfg.TokenValidationParameters = new TokenValidationParameters  // parametry walidacji, umo¿liwiajace sprawdzenie czy token wys³any przez klienta jest zgodny z tym co wie serwer
        {
            ValidIssuer = authenticationSettings.JwtIssuer,  // ustalenie issuer - czyli wydawca danego tokenu
            ValidAudience = authenticationSettings.JwtIssuer,  // audience - czyli jakie podmioty mog¹ u¿ywaæ tokenu, w tym przypadku jest to ta sama wartoœæ co dla ValidIssuer, poniewa¿ bêdziemy generowaæ token w obrêbie naszej aplikacji i tylko tacy klienci bêd¹ dopuszczeni do uwierzytelniania
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)) // signing key - klucz prywatny wygenerowany na podstawie wartoœci authenticationSettings.JwtKey, czyli zapisanej w appsettings.json >> "Authentication" >> "JwtKey": "PRIVATE_KEY_DOTN_SHARE"
        };
    });

// CUSTOMOWE regu³y autoryzacji
// Rozbudowa autoryzacji w oparciu o w³asn¹ (customow¹) politykê
// poni¿sza polityka o nazwie "HasNationality" dzia³a w ten sposób, ¿e wymaga aby ¿¹danie (claim) o nazwie "Nationality" istnia³o w tokenie oraz aby wartoœæ jego wartoœæ by³a równa "Polish"
// Je¿eli claim o nazwie "Nationality" nie istnieje w tokenie, u¿ytkownik nie otrzyma autoryzacji do wykonania danej akcji.
// przyk³adowo claim "Nationality" nie jest dodawany w tokenie dla u¿ytkownika, który nie ma wype³nionego pola "Nationality"
// metoda RequireClaim() mo¿e przyjmowaæ wiele parametów, które spe³niaj¹ warunki np. RequireClaim("Nationality", "Polish", "Czech", "German")
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HasNationality", builder => builder.RequireClaim("Nationality", "Polish", "Czech"));
    options.AddPolicy("IsAdult", builder => builder.AddRequirements(new MinimumAgeRequirement(18)));
    options.AddPolicy("IsCreator", builder => builder.AddRequirements(new MinimumCreatedRestaurantsRequirement(2)));
});
builder.Services.AddScoped<IAuthorizationHandler, MinimumAgeRequirementHandler>();  // rejestracja serwisu odpowiadaj¹cego za obs³u¿enie autoryzacji w oparciu o w³asn¹ (customow¹) politykê o nazwie "IsAdult" ze zdefiniowanymi w metodzie AddRequirements() w³asnymi wymaganiami - w tym przypadku wymaganym minimalnym wiekiem u¿ytkownika - u¿ytkownik, którego wiek jest mniejszy od minimalnego, nie otrzyma autoryzacji do wykonania danej akcji
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
// Dodanie serwisu RestaurantSeeder poprzez metodê AddScoped(). Serwis RestaurantSeeder umo¿liwia za³adowanie do bazy danych seedów (rekordów), od razu po uruchomieniu aplikacji. Dzieje siê to porzez uruchomienie metody Seed(), która jest wywo³ywana poni¿ej. Aby mo¿na by³o odwo³aæ siê do metody Seed(), potrzebne jest utworzenie scope poprzez metodê CreateScope(), a nastêpnie z wykorzystaniem utworzonego scope odwo³anie siê do serwisu RestaurantSeeder poprzez metodê GetRequiredService().
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
builder.Services.AddHttpContextAccessor(); // rejestracja tego serwisu umo¿liwi wstrzykniêcie do klasy UserContextService referencji do obiektu typu IHttpContextAccessor, który jest wykorzystywany w customowej klasie UserContextService. Klasa UserContextService jest odpowiedzialna za udostêpnianie informacji o danym u¿ytkowniku na podstawie kontekstu Http

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



// --- III - rejestrowanie przep³ywów zapytania / ustawienie middleware  ---
app.UseResponseCaching(); // Odpowiada za mechanizm keszowania
app.UseStaticFiles();  // Dodajemy na pocz¹tku listy przep³ywów, aby aplikacja dla ka¿dego zapytania sprawdza³a, czy pod œcie¿k¹ do zapytania nie istnieje jakiœ plik
app.UseCors("FrontEndClient");

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<RestaurantSeeder>();
seeder.Seed();
app.UseMiddleware<RequestTimeMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication(); // odpowiada za to, ¿e ka¿dy request wys³any przez klienta api musi podlegaæ uwierzytelnianiu 
// app.UseAuthentication() nale¿y umieœciæ przed app.UseHttpsRedirection()
app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API");
});

app.UseAuthorization();
// app.UseAuthorization() nale¿y umieœciæ przed app.MapControllers()
app.MapControllers();

//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
app.Run(); 
