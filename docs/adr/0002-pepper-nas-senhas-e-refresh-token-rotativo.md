# Pepper nas senhas e refresh token rotativo guardado como hash

As senhas passam por um HMAC-SHA256 com um pepper antes do BCrypt (`BCryptPasswordHasher`), e o pepper fica fora do banco, como segredo do AppHost (`PasswordPepper`). Assim, um vazamento só do banco não basta para atacar as senhas por força bruta. O refresh token é opaco, guardado só como hash SHA-256 (`RefreshTokenHasher`) e trocado a cada uso: o token usado é revogado e aponta para o hash do seu substituto. Se um token já rotacionado for apresentado de novo, todas as sessões do usuário são revogadas (`RefreshTokenUseCase`), porque o reuso indica que o token foi roubado.

## Considered Options

- **Só BCrypt, sem pepper.** Rejeitado: o salt fica no próprio hash, então quem tem o banco tem tudo o que precisa para atacar as senhas offline.
- **Refresh token como JWT, sem estado no servidor.** Rejeitado: não daria para revogar um token específico, encerrar sessões nem detectar reuso.
- **Refresh token guardado em texto puro.** Rejeitado: quem lê o banco poderia usar os tokens ativos diretamente.

## Consequences

- Trocar o pepper invalida todas as senhas cadastradas, porque o hash guardado deixa de bater. Não há rotação de pepper; trocá-lo exige que todos os usuários redefinam a senha.
- O pepper e a chave do JWT precisam ser os mesmos em todas as instâncias da API e não podem se perder.
- O access token continua válido até expirar mesmo depois da revogação das sessões, porque não é checado contra o banco.
