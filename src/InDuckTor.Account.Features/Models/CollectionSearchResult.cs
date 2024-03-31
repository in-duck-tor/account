namespace InDuckTor.Account.Features.Models;

/// <param name="Total">Сколько всего элементов подходит под выборку</param>
/// <param name="Items">Элементы текущеё под-выборки</param>
public record CollectionSearchResult<T>(int Total, List<T> Items);