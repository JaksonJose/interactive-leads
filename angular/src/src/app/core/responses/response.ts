import { Message } from "./message";

export class Response<T> {
    data?: T;
    items?: T[];
    isSuccessful?: boolean;
    messages?: Message[];
}