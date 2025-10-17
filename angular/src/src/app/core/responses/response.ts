import { Message } from "./message";

export class Response<T> {
    data?: T;
    items?: T[];
    messages?: Message[];
}