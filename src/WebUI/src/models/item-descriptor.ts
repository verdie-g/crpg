export class ItemDescriptor {
  fields: [string, any][];
  modes: ItemMode[];
}

export class ItemMode {
  name: string;
  fields: [string, any][];
  flags: string[];
}
