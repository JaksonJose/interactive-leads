import { Consultant } from "@core/models";

export class Token {
    token?: string;
    attemptsRemaining?: number;
    consultant?: Consultant;
}