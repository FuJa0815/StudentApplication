using Microsoft.AspNetCore.Components;
using MudBlazor;
using StudentApplication.Client.HttpRepository;
using StudentApplication.Common.Utils;

namespace StudentApplication.Client.Shared;

public class MyMudTable<T, TKey> : MudTable<T>
    where T : IWithId<TKey>
    where TKey : IEquatable<TKey>
{
    
    private int _lastCurrentPage;
    public override int GetFilteredItemsCount()
    {
        // Really hacky way of making CurrentPage two way bindable
        if (_lastCurrentPage != CurrentPage)
        {
            CurrentPageChanged.InvokeAsync(CurrentPage);
            _lastCurrentPage = CurrentPage;
        }
        return TotalItems;
    }

    [Parameter]
    public EventCallback<int> CurrentPageChanged { get; set; }

    protected override int NumPages => ((RestData<T, TKey>)Items).Pages;
}