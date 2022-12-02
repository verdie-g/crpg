interface ElementBound {
  x: number;
  y: number;
  width: number;
}

interface OpenedItem {
  id: string;
  userId: number;
  bound: ElementBound;
}

// shared state
const openedItems = ref<OpenedItem[]>([]);

export const useItemDetail = () => {
  const openItemDetail = (item: OpenedItem) => {
    if (!openedItems.value.some(oi => oi.id === item.id)) {
      openedItems.value.push(item);
    }
  };

  const closeItemDetail = (id: string) => {
    if (openedItems.value.some(oi => oi.id === id)) {
      openedItems.value = openedItems.value.filter(oi => oi.id !== id);
    }
  };

  const closeAll = () => {
    openedItems.value = [];
  };

  return {
    openedItems: readonly(openedItems),
    openItemDetail,
    closeItemDetail,
    closeAll,
  };
};
