using Send_Block_Сustom_BlockChain_Logic.Services;

var builder = WebApplication.CreateBuilder(args);

// Додайте служби до контейнера.
builder.Services.AddControllersWithViews();

// Зареєструйте блокчейн-сервіси як синглтон для підтримки стану між запитами.
builder.Services.AddSingleton<NodeRegistryService>();

var app = builder.Build();

// Налаштуйте конвеєр HTTP-запитів.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Значення HSTS за замовчуванням становить 30 днів. Можливо, ви захочете змінити це для виробничих сценаріїв, див. https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BlockChain}/{action=Index}/{id?}");

app.Run();