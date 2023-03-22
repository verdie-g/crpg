import { type ItemFlat, ItemFieldFormat, ItemFieldCompareRule } from '@/models/item';

export enum AggregationView {
  Range = 'Range',
  Checkbox = 'Checkbox',
  Radio = 'Radio',
}

export type FiltersModel<T> = Partial<Record<keyof ItemFlat, T>>;
export interface Aggregation extends Omit<itemsjs.Aggregation, 'title'> {
  view: AggregationView;
  chosen_filters_on_top?: boolean; // TODO: field is missing in the @types/itemsjs@2.1.1
  description?: string;
  title?: string;
  format?: ItemFieldFormat;
  hidden?: boolean; // don't display in the table
  compareRule?: ItemFieldCompareRule; // for compare mode
  width?: number;
}

export type AggregationConfig = Partial<Record<keyof ItemFlat, Aggregation>>;

export type SortingConfig = Record<string, itemsjs.Sorting<ItemFlat>>;
