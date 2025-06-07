using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Front_end.Models;
using Front_end.Models.ViewModels;
using Front_end.Services;
using Front_end.Models.DTOs;
using System.Diagnostics.Eventing.Reader;

namespace Front_end.Controllers;

public class HomeController : Controller
{
    private readonly IApiService _apiService;

    public HomeController(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(
        string? searchName,
        DateTime? startDate,
        DateTime? endDate,
        int page = 1,
        int pageSize = 9 // for example, 9 items per page
    )
    {
        var viewModel = new EventListViewModel
        {
            SearchName = searchName,
            StartDate = startDate,
            EndDate = endDate
        };

        EventSearchResponse searchResult;

        if (string.IsNullOrEmpty(searchName) && !startDate.HasValue && !endDate.HasValue)
        {
            searchResult = await _apiService.SearchEventsAsync(null, null, null, page, pageSize);
        }
        else
        {
            searchResult = await _apiService.SearchEventsAsync(searchName, startDate, endDate, page, pageSize);
        }

        viewModel.Events = searchResult.Events.Select(e => new EventDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            VenueId = e.VenueId
        }).ToList();

        viewModel.CurrentPage = searchResult.Page;
        viewModel.PageSize = searchResult.PageSize;
        viewModel.TotalPages = searchResult.TotalPages;
        viewModel.HasNextPage = searchResult.HasNextPage;
        viewModel.HasPreviousPage = searchResult.HasPreviousPage;

        return View(viewModel);
    }
}
