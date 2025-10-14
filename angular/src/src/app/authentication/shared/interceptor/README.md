# JWT Interceptor

Este interceptor é responsável por adicionar automaticamente o token JWT de autenticação em todas as requisições HTTP.

## Funcionamento

1. **Verificação de URLs**: O interceptor verifica se a URL da requisição deve ser ignorada (login, register, refresh-token)
2. **Obtenção do Token**: Busca o token JWT armazenado no localStorage através do AuthService
3. **Validação do Token**: Verifica se o token existe e se não está expirado
4. **Adição do Header**: Adiciona o header `Authorization: Bearer {token}` na requisição

## URLs Ignoradas

- `/login` - Endpoint de login
- `/register` - Endpoint de registro
- `/refresh-token` - Endpoint de renovação de token

## Configuração

O interceptor está configurado no `app.config.ts`:

```typescript
provideHttpClient(withFetch(), withInterceptors([jwtInterceptor]))
```

## Uso

O interceptor funciona automaticamente. Todas as requisições HTTP feitas através do HttpClient terão o token JWT adicionado automaticamente, exceto para as URLs ignoradas.

## Exemplo

```typescript
// Esta requisição terá o token JWT adicionado automaticamente
this.http.get('/api/users').subscribe(...)

// Esta requisição NÃO terá o token JWT (URL ignorada)
this.http.post('/api/login', loginData).subscribe(...)
```
