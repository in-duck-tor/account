namespace InDuckTor.Account.WebApi.Models;

/// <param name="Total">Сколько всего элементов подходит под выборку</param>
/// <param name="Items">Элементы текущеё под-выборки</param>
public class CollectionSearchResult<T>(int Total, List<T> Items);