using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SirJoshua.Web.Data;
using SirJoshua.Web.Models;
using SirJoshua.Web.Options;
using SirJoshua.Web.ViewModels;

namespace SirJoshua.Web.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly PayPalOptions _paypal;

    public HomeController(AppDbContext db, IOptions<PayPalOptions> paypal)
    {
        _db = db;
        _paypal = paypal.Value;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var ebooks = await _db.Ebooks
            .OrderBy(e => e.SortOrder)
            .ToListAsync(ct);

        var vm = new HomeViewModel
        {
            Ebooks = ebooks,
            PayPalClientId = _paypal.ClientId,
            PayPalCurrency = _paypal.Currency,
            PayPalEnabled = _paypal.IsConfigured,
            IdrPerUsd = _paypal.IdrPerUsd,
            PromoCode = _paypal.PromoCode
        };
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
