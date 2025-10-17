import { PRIME_NG_MODULES } from "./primeng-imports";
import { HasPermissionDirective } from "./directives/has-permission.directive";

export { HasPermissionDirective };

export const SHARED_IMPORTS = [
  ...PRIME_NG_MODULES,
  HasPermissionDirective
];