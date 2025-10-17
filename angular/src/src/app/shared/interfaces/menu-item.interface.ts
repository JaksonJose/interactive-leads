export interface MenuItemInterface {
  label: string;
  icon: string;
  route?: string;
  expanded?: boolean;
  permissions: string[];
  children?: MenuItemInterface[];
}
