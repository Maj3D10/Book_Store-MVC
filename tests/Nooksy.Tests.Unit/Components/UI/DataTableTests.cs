using Bunit;
using FluentAssertions;
using Xunit;
using Nooksy.Client.Components.UI;

namespace Nooksy.Tests.Unit.Components.UI;

public class DataTableTests : TestContext
{
    private List<string> GetSampleItems() => new() { "Apple", "Banana", "Cherry", "Date", "Elderberry" };

    [Fact]
    public void Renders_CorrectNumberOfRows()
    {
        var items = GetSampleItems();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        var rows = cut.FindAll("tbody tr");
        rows.Count.Should().Be(items.Count);
    }

    [Fact]
    public void EmptyItems_ShowsNoRecordsMessage()
    {
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, Enumerable.Empty<string>())
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        cut.Markup.Should().Contain("No matching records found");
    }

    [Fact]
    public void SearchQuery_FiltersRows()
    {
        var items = GetSampleItems();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.SearchMatch, (item, query) => item.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        // Type in search box
        cut.Find("input").Change("Apple");

        var rows = cut.FindAll("tbody tr");
        rows.Count.Should().Be(1);
    }

    [Fact]
    public void SearchQuery_ResetsToFirstPage()
    {
        var items = Enumerable.Range(1, 25).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, 10)
            .Add(c => c.SearchMatch, (item, query) => item.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        // Go to page 2
        var pageButtons = cut.FindAll(".page-item:not(.disabled) button");
        if (pageButtons.Count >= 2)
        {
            pageButtons[1].Click();
        }

        // Now search - should reset to page 1
        cut.Find("input").Change("Item 1");

        var startEntry = cut.Markup;
        startEntry.Should().Contain("Showing 1 to");
    }

    [Fact]
    public void Pagination_ShowsOnlyPageSizeRowsOnFirstPage()
    {
        var items = Enumerable.Range(1, 25).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, 10)
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        var rows = cut.FindAll("tbody tr");
        rows.Count.Should().Be(10);
    }

    [Fact]
    public void PreviousPage_DisabledOnFirstPage()
    {
        var items = Enumerable.Range(1, 25).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, 10)
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        var prevButton = cut.FindAll(".page-item button")[0];
        prevButton.ParentElement.ClassList.Should().Contain("disabled");
    }

    [Fact]
    public void NextPage_DisabledOnLastPage()
    {
        var items = Enumerable.Range(1, 5).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, 10)
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        var nextButton = cut.FindAll(".page-item button")[^1];
        nextButton.ParentElement.ClassList.Should().Contain("disabled");
    }

    [Fact]
    public void ShowsCorrectEntryCountFooter()
    {
        var items = Enumerable.Range(1, 25).Select(i => $"Item {i}").ToList();
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, items)
            .Add(c => c.PageSize, 10)
            .Add(c => c.HeaderTemplate, "<th>Name</th>")
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        cut.Markup.Should().Contain("Showing 1 to 10 of 25 entries");
    }

    [Fact]
    public void HeaderTemplate_RendersInThead()
    {
        var cut = RenderComponent<DataTable<string>>(p => p
            .Add(c => c.Items, GetSampleItems())
            .Add(c => c.HeaderTemplate, (builder) =>
            {
                builder.OpenElement(0, "th");
                builder.AddContent(1, "Name");
                builder.CloseElement();
            })
            .Add(c => c.RowTemplate, (item) => (builder) =>
            {
                builder.OpenElement(0, "td");
                builder.AddContent(1, item);
                builder.CloseElement();
            }));

        var header = cut.Find("thead");
        header.InnerHtml.Should().Contain("Name");
    }
}
