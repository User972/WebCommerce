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

        var vm = BuildHomeViewModel(ebooks);
        return View(vm);
    }

    [Route("/band-7-formula")]
    [Route("/ielts-writing-mastery")]
    public async Task<IActionResult> BandSevenFormula(CancellationToken ct)
    {
        var flagship = await _db.Ebooks
            .FirstOrDefaultAsync(e => e.Id == Data.SeedData.FlagshipId, ct);

        var vm = BuildHomeViewModel(flagship is null ? new List<Ebook>() : new List<Ebook> { flagship });
        return View(vm);
    }

    private HomeViewModel BuildHomeViewModel(List<Ebook> ebooks)
    {
        var flagship = ebooks.FirstOrDefault(e => e.Id == Data.SeedData.FlagshipId);
        return new HomeViewModel
        {
            Flagship = flagship,
            // Secondary library = everything except the flagship (it has its own premium section).
            Ebooks = ebooks.Where(e => e.Id != Data.SeedData.FlagshipId).ToList(),
            PayPalClientId = _paypal.ClientId,
            PayPalCurrency = _paypal.Currency,
            PayPalEnabled = _paypal.IsConfigured,
            IdrPerUsd = _paypal.IdrPerUsd,
            PromoCode = _paypal.PromoCode
        };
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
