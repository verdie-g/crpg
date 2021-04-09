export interface ItemDescriptor {
  fields: [string, any][];
  modes: ItemMode[];
}

export interface ItemMode {
  name: string;
  fields: [string, any][];
  flags: string[];
}
