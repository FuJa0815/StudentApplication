using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Web;
using Microsoft.AspNetCore.SignalR.Client;
using StudentApplication.Common.Attributes;
using StudentApplication.Common.Utils;

namespace StudentApplication.Client.HttpRepository;

public class RestData<T, TKey> : IEnumerable<T>, IDisposable, INotifyCollectionChanged
    where T : IWithId<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly List<T> _list = new();

    private readonly HttpClient _client;
    private readonly NotificationHub _hub;
    private readonly string _endpoint;
    
    private int _page = 0;
    private int _pageLength = 10;
    private string _sortBy = string.Empty;
    private bool _sortAscending = true;
    private string _query = string.Empty;
    private int _items = 0;

    public int Count => _items;
    public bool IsReadOnly => false;
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;


    public int Page
    {
        get => _page;
        set => SetPageAsync(value).Wait();
    }

    public async Task SetPageAsync(int page)
    {
        _page = page;
        await FetchAsync(page - 1);
        await FetchAsync(page);
        await FetchAsync(page + 1);
    }

    public async Task NextPage()
    {
        await SetPageAsync(Page + 1);
    }
    
    public async Task PrevPage()
    {
        if (Page == 0)
            return;
        await SetPageAsync(Page - 1);
    }

    public int PageLength
    {
        get => _pageLength;
        set => SetPageLengthAsync(value).Wait();
    }

    // Integer round up
    public int Pages => (Count - 1) / PageLength + 1;

    public async Task SetPageLengthAsync(int pageLength)
    {
        _pageLength = pageLength;
        await FetchAsync(Page - 1);
        await FetchAsync();
        await FetchAsync(Page + 1);
    }

    public string SortBy => _sortBy;

    public bool SortAscending => _sortAscending;

    public async Task SetSortAsync(string sortBy, bool ascending)
    {
        _sortBy = sortBy;
        _sortAscending = ascending;
        await Reload();
    }

    public string Query
    {
        get => _query;
        set => SetQueryAsync(value).Wait();
    }

    public async Task SetQueryAsync(string query)
    {
        _query = query;
        await Reload();
    }

    public T this[int index]
    {
        get
        {
            if (index > PageLength || index < 0)
                throw new IndexOutOfRangeException();
            return _list[Page * PageLength + index];
        }
    }
    
    public RestData(HttpClient client, NotificationHub notificationHub)
    {
        _hub = notificationHub;
        _client = client;
        _endpoint = typeof(T).GetCustomAttribute<RestEndpointAttribute>()?.Url ?? throw new ArgumentException("Generic type T must have a RestEndpoint attribute");
        notificationHub.HubConnection.On<T>($"{_endpoint}_update", ItemUpdated);
        notificationHub.HubConnection.On<T>($"{_endpoint}_create", async item => await ItemAdded(item));
        notificationHub.HubConnection.On<TKey>($"{_endpoint}_delete", async id => await ItemDeleted(id));
    }

    public async Task InitAsync()
    {
        await _hub.ConnectAsync();
        await Reload();
        await _hub.JoinGroupAsync(_endpoint);
    }

    private async Task ItemDeleted(TKey key)
    {
        var index = _list.FindIndex(i => i.Id.Equals(key));
        if (index == -1)
            return;

        var oldItem = Get(index);
        _list.RemoveAt(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
        var page = index / PageLength;
        await FetchAsync(page);
    }
    
    private void ItemUpdated(T item)
    {
        var index = _list.FindIndex(i => i.Id.Equals(item.Id));
        if (index == -1)
            return;
        var oldItem = _list[index];
        _list[index] = item;

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
    }
    
    private async Task ItemAdded(T item)
    {
        if (!MatchesQuery(item))
            return;

        for (var i = Page == 0 ? 0 : Page * PageLength - 1; i < (Page + 2) * PageLength; i++)
        {
            var currentItem = Get(i);
            if (currentItem == null)
            {
                // End of list reached
                _list.Insert(i, item);
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, i));
                return;
            }

            var compare = Compare(item, currentItem);

            // Before current page
            if (compare < 0 && i < Page * PageLength && Page > 0)
            {
                // Invalidate the previous page
                // We don't want to add the item to the previous page because that would shift the data viewed by the user
                await FetchAsync(Page - 1);
                return;
            }

            if (compare < 0)
            {
                _list.Insert(i, item);
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, i));
                return;
            }
        }
    }

    private bool MatchesQuery(T item)
    {
        if (string.IsNullOrWhiteSpace(Query))
            return true;

        return SearchableProperties.Select(p => p.GetValue(item)).Any(t => t?.ToString()?.ToLower().Contains(Query.ToLower()) ?? false);
    }

    private int Compare(T t1, T t2)
    {
        var property = typeof(T).GetProperty(string.IsNullOrEmpty(SortBy) ? "Id" : SortBy)!;
        var value = ((IComparable)property.GetValue(t1)!).CompareTo(property.GetValue(t2));
        return SortAscending ? value : -value;
    }
    
    private static IEnumerable<PropertyInfo> SearchableProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestSearchableAttribute>() != null);
    
    

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public IEnumerable<T> GetEnumerable()
    {
        return _list.AsEnumerable();
    }

    ~RestData()
    {
        Dispose();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public async void Dispose()
    {
        GC.SuppressFinalize(this);
        await _hub.LeaveGroupAsync(_endpoint);
    }

    private T? Get(int i)
    {
        return i >= _list.Count ? default : _list[i];
    }

    private async Task FetchAsync(int? page = null)
    {
        if (page < 0)
            return;
        
        var p = page ?? Page;

        if (Enumerable.Range(p * PageLength, PageLength).All(i => Get(i) != null))
            return;
        
        var uriBuilder = new UriBuilder(_client.BaseAddress!+_endpoint);
        var getQuery = HttpUtility.ParseQueryString(uriBuilder.Query);
        getQuery["page"] = p.ToString();
        getQuery["pageLength"] = PageLength.ToString();
        getQuery["sortBy"] = SortBy;
        getQuery["ascending"] = SortAscending.ToString();
        getQuery["query"] = Query;
        uriBuilder.Query = getQuery.ToString();
        var result = await ReadResultAsync<PaginationListResult<T>>(await _client.GetAsync(uriBuilder.ToString()));
        _items = result.TotalItems;
        for (var i = 0; i < result.Items.Count; i++)
        {
            if (Get(i+p*PageLength) == null)
                _list.Insert(i+p*PageLength, result.Items[i]);
            else
                _list[i + p * PageLength] = result.Items[i];
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, result.Items, p*PageLength));
    }
    
    [Pure]
    private async Task<TResp> ReadResultAsync<TResp>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        var obj = await JsonSerializer.DeserializeAsync<TResp>(stream);
        if (obj == null)
            throw new HttpRequestException("Could not deserialize JSON");
        return obj;
    }

    private async Task Reload()
    {
        _list.Clear();
        await FetchAsync(Page - 1);
        await FetchAsync();
        await FetchAsync(Page + 1);
    }

    public async Task Add(T item)
    {
        item.Id = await ReadResultAsync<TKey>(await _client.PostAsync(_endpoint, JsonContent.Create(item)));
    }

    public async Task Remove(TKey item)
    {
        var response = await _client.DeleteAsync($"{_endpoint}/{item}");
        response.EnsureSuccessStatusCode();
    }

    public async Task Update(T item)
    {
        var response = await _client.PutAsync($"{_endpoint}/{item.Id}", JsonContent.Create(item));
        response.EnsureSuccessStatusCode();
    }

}