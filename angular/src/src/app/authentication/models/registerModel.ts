import { Company } from "@core/models";

export class RegisterModel {
  id: number = 0;
  identityId: string = '';
  companies = new Array<Company>();
  email: string = '';
  enabled: boolean = true;
  fullName: string = '';
  nickName: string = '';
  phoneNumber: string = '';
  roles: string = '';
  userName: string = '';
}
