using System.Collections.Generic;
using System.Threading.Tasks;
using Crpg.Application.Items.Models;

namespace Crpg.Application.Common.Interfaces
{
    internal interface IItemsSource
    {
        Task<IEnumerable<ItemCreation>> LoadItems();
    }
}
