using System.Collections;
using System.Collections.Specialized;
using System.Net.Http.Json;
using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.SignalR.Client;
using StudentApplication.Common.Attributes;
using StudentApplication.Common.Utils;

namespace StudentApplication.Client.HttpRepository;

/// <summary>
///   A <see cref="IEnumerable{T}"/> continuously communicating with the REST server, listening for updates via SignalR,
///   automatically fetching new data when required and informing the server about changes.
/// </summary>
public class RestData<T, TKey> : IEnumerable<T>, IDisposable, INotifyCollectionChanged
    where T : IModel<TKey>
    where TKey : IEquatable<TKey>
{
    // Internal cache
    private readonly List<T> _list = new();

    private readonly HttpClient _client;
    private readonly NotificationHub _hub;
    private readonly string _endpoint;
    private readonly ILogger _logger;
    
    private int _page = 0;
    private int _pageLength = 10;
    private string _sortBy = string.Empty;
    private bool _sortAscending = true;
    private string _query = string.Empty;
    private int _items = 0;

    /// <summary>
    ///   Total amount of items available on the server.
    /// </summary>
    public int Count => _items;
    
    /// <summary>
    ///   Event that gets fired upon changes to the list
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    ///   Currently active list
    /// </summary>
    public int Page
    {
        get => _page;
        set => SetPageAsync(value).Wait();
    }

    public async Task SetPageAsync(int page)
    {
        _page = page;
        _logger.LogDebug($"Went to page {page}");
        await FetchAsync(page - 1);
        await FetchAsync(page);
        await FetchAsync(page + 1);
    }

    /// <summary>
    ///   Items per page
    /// </summary>
    public int PageLength
    {
        get => _pageLength;
        set => SetPageLengthAsync(value).Wait();
    }

    public async Task SetPageLengthAsync(int pageLength)
    {
        _pageLength = pageLength;
        _logger.LogDebug($"Set page length to {pageLength}");
        await FetchAsync(Page - 1);
        await FetchAsync();
        await FetchAsync(Page + 1);
    }
    
    /// <summary>
    ///   Total amount of pages
    /// </summary>
    // Integer round up
    public int Pages => (Count - 1) / PageLength + 1;

    /// <summary>
    ///   Sort by this attribute name. Must be tagged with <see cref="RestSearchableAttribute"/>
    /// </summary>
    public string SortBy => _sortBy;

    public bool SortAscending => _sortAscending;

    public async Task SetSortAsync(string sortBy, bool ascending)
    {
        _sortBy = sortBy;
        _sortAscending = ascending;
        _logger.LogDebug($"Sorting by {sortBy} {(ascending?"ascending":"descending")}");
        await Reload();
    }

    /// <summary>
    ///   Current search query. Debounce before setting this.
    /// </summary>
    public string Query
    {
        get => _query;
        set => SetQueryAsync(value).Wait();
    }

    public async Task SetQueryAsync(string query)
    {
        _query = query;
        _logger.LogDebug($"Searching for {query}");
        await Reload();
    }
    
    public RestData(HttpClient client, NotificationHub notificationHub, ILogger<RestData<T, TKey>> logger)
    {
        _hub = notificationHub;
        _client = client;
        _endpoint = typeof(T).GetCustomAttribute<RestEndpointAttribute>()?.Url ?? throw new ArgumentException("Generic type T must have a RestEndpoint attribute");
        _logger = logger;
        
        notificationHub.HubConnection.On<T>($"{_endpoint}_update", ItemUpdated);
        notificationHub.HubConnection.On<T>($"{_endpoint}_create", async item => await ItemAdded(item));
        notificationHub.HubConnection.On<TKey>($"{_endpoint}_delete", async id => await ItemDeleted(id));
    }

    /// <summary>
    ///   Call this method upon creation!
    ///   Initialized connections and fetches initial data.
    /// </summary>
    public async Task InitAsync()
    {
        await _hub.ConnectAsync();
        await Reload();
        await _hub.JoinGroupAsync(_endpoint);
        _logger.LogDebug("Initialized");
    }

    /// <summary>
    ///   The server informed me that an item was deleted!
    /// </summary>
    private async Task ItemDeleted(TKey key)
    {
        _logger.LogInformation($"Item with id {key} deleted");
        _items--;
        var index = _list.FindIndex(i => i.Id.Equals(key));
        if (index == -1)
            return;

        // Item found in cache
        var oldItem = Get(index);
        _list.RemoveAt(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
        var page = index / PageLength;
        await FetchAsync(page);
    }
    
    /// <summary>
    ///   The server informed me that an item was updated!
    /// </summary>
    private void ItemUpdated(T item)
    {
        _logger.LogInformation($"Item with id {item.Id} updated");
        var index = _list.FindIndex(i => i.Id.Equals(item.Id));
        if (index == -1)
            return;
        
        // Item found in cache
        var oldItem = _list[index];
        _list[index] = item;

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
    }
    
    /// <summary>
    ///   The server informed me that an item was added!
    /// </summary>
    private async Task ItemAdded(T item)
    {
        _logger.LogInformation($"Item with id {item.Id} added");
        _items++;
        // Check if the added item matches the current search term
        if (!MatchesQuery(item))
            return;

        // Compare the item with the last item of the previous page until the next page
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

            if (compare < 0 && i < Page * PageLength && Page > 0)
            {
                // Item is before the current page
                // We don't want to add the item to the previous page because that would shift the data viewed by the user
                
                // Instead, we just reload the previous page
                await FetchAsync(Page - 1);
                return;
            }

            if (compare < 0)
            {
                // We found a comfortable slot for the item
                _list.Insert(i, item);
                CollectionChanged?.Invoke(this,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, i));
                return;
            }
        }
    }

    /// <summary>
    ///   Does the item match the current search term?
    /// </summary>
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
    
    /// <summary>
    ///   All searchable properties of T
    /// </summary>
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

    /// <summary>
    ///   We leave the notification group upon disposing. Goodbye!
    /// </summary>
    public async void Dispose()
    {
        GC.SuppressFinalize(this);
        await _hub.LeaveGroupAsync(_endpoint);
        _logger.LogInformation($"Connection closed");
    }

    /// <summary>
    ///   Helper that gets an item at an index or returns default
    /// </summary>
    private T? Get(int i)
    {
        if (i < 0) return default;
        return i >= _list.Count ? default : _list[i];
    }

    /// <summary>
    ///   Fetches a page from the server utilizing previously defined properties.
    /// </summary>
    /// <param name="pageToLoad">The page to be fetched. Defaults to the current page</param>
    private async Task FetchAsync(int? pageToLoad = null)
    {
        if (pageToLoad < 0)
            return;
        
        var page = pageToLoad ?? Page;
        var firstItemIndex = page * PageLength;

        // Do not fetch if we already have all info
        if (Enumerable.Range(firstItemIndex, PageLength).All(i => Get(i) != null))
        {
            _logger.LogInformation($"Not loading page {page} because we have all data here");
            return;
        }
        _logger.LogInformation($"Loading page {page}, sort by {SortBy}, filtered by {Query}");
        
        var uriBuilder = new UriBuilder(_client.BaseAddress!+_endpoint);
        var getQuery = HttpUtility.ParseQueryString(uriBuilder.Query);
        getQuery["page"] = page.ToString();
        getQuery["pageLength"] = PageLength.ToString();
        getQuery["sortBy"] = SortBy;
        getQuery["ascending"] = SortAscending.ToString();
        getQuery["query"] = Query;
        uriBuilder.Query = getQuery.ToString();
        var result = await JsonUtils.ReadResultAsync<PaginationListResult<T>>(await _client.GetAsync(uriBuilder.ToString()));
        _items = result.TotalItems;
        for (var i = 0; i < result.Items.Count; i++)
        {
            // Update or insert
            if (Get(firstItemIndex + i) == null)
                _list.Insert(firstItemIndex + i, result.Items[i]);
            else
                _list[firstItemIndex + i] = result.Items[i];
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, result.Items, page*PageLength));
    }
    
    /// <summary>
    ///   Fetches the current, previous and next page.
    /// </summary>
    private async Task Reload()
    {
        _list.Clear();
        await FetchAsync(Page - 1);
        await FetchAsync();
        await FetchAsync(Page + 1);
    }

    /// <summary>
    ///   Add an item to the list. Others will be informed.
    /// </summary>
    public async Task Add(T item)
    {
        _logger.LogInformation($"Added new item locally");
        item.Id = await JsonUtils.ReadResultAsync<TKey>(await _client.PostAsync(_endpoint, JsonContent.Create(item)));
    }

    /// <summary>
    ///   Removes an item from the list. Others will be informed.
    /// </summary>
    public async Task<bool> Remove(TKey key)
    {
        _logger.LogInformation($"Removed item with id {key} locally");
        return (await _client.DeleteAsync($"{_endpoint}/{key}")).IsSuccessStatusCode;
    }

    /// <summary>
    ///   Updates an item in the list. Others will be informed.
    /// </summary>
    public async Task<bool> Update(T item)
    {
        _logger.LogInformation($"Updated item with id {item.Id} locally");
        return (await _client.PutAsync($"{_endpoint}", JsonContent.Create(item))).IsSuccessStatusCode;
    }
}