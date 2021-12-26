using Crpg.Application.Items.Models;

namespace Crpg.Application.Common.Interfaces;

internal interface IItemsSource
{
    Task<IEnumerable<ItemCreation>> LoadItems();
}
