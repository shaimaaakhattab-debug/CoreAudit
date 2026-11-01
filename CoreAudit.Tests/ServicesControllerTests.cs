using CoreAudit.Controllers;
using CoreAudit.Models;
using CoreAudit.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CoreAudit.Tests;

public class ServicesControllerTests
{
    private static ServicesController BuildController() => new(new ContentService());

    [Fact]
    public void Details_returns_view_with_model_for_judicial_support()
    {
        var result = BuildController().Details("judicial-support");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ServiceItem>(view.Model);
        Assert.Equal("judicial-support", model.Slug);
    }

    [Fact]
    public void Details_slug_is_case_insensitive()
    {
        var result = BuildController().Details("JUDICIAL-Support");

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Details_returns_not_found_for_unknown_slug()
    {
        Assert.IsType<NotFoundResult>(BuildController().Details("unknown-slug"));
    }

    [Fact]
    public void Details_returns_not_found_for_service_without_details()
    {
        Assert.IsType<NotFoundResult>(BuildController().Details("external-audit"));
    }
}
