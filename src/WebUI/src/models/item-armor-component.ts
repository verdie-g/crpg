import ArmorMaterialType from '@/models/armor-material-type';

export default interface ItemArmorComponent {
  headArmor: number;
  bodyArmor: number;
  armArmor: number;
  legArmor: number;
  materialType: ArmorMaterialType;
}
