# Requisitos — Conformidade LGPD

> O "o quê" e o "porquê". Independente de stack. Esta seção é a fonte da verdade da feature.

## Contexto
O Bora pro Jogo coleta dados pessoais de torcedores (nome, e-mail, telefone opcional) para
cadastro de conta, alertas de notificação e ingressos digitais. A Lei Geral de Proteção de
Dados (Lei 13.709/2018) exige base legal para o tratamento, transparência sobre o uso e
garantia dos direitos do titular (acesso, correção, exclusão e portabilidade). Esta feature
implementa consentimento explícito no cadastro, uma Política de Privacidade pública, os
direitos do titular na área logada e a minimização de dados nos formulários.

## User stories
- Como **torcedor**, quero **ler a Política de Privacidade e consentir explicitamente no cadastro**,
  para **saber como meus dados serão usados antes de criar a conta**.
- Como **torcedor logado**, quero **baixar todos os dados vinculados à minha conta em JSON**,
  para **exercer meus direitos de acesso e portabilidade**.
- Como **torcedor logado**, quero **excluir minha conta confirmando a senha**, para
  **exercer meu direito de exclusão sem depender de contato manual**.
- Como **organizador (admin)**, quero **que os endpoints administrativos exijam autorização**,
  para **que dados e conteúdo do hub não sejam manipulados por anônimos**.

## Critérios de aceite (testáveis)
- [x] Dado o formulário de cadastro do site (`/conta/registrar`), quando o torcedor NÃO marca
      "Li e aceito a Política de Privacidade", então o cadastro é recusado com erro de validação.
- [x] Dado o cadastro via API (`POST /api/auth/registrar`), quando `aceitePrivacidade` não é
      `true`, então a API responde 400 com mensagem clara.
- [x] Dado um cadastro aceito (site ou API), então `ConsentimentoLgpdEm` (UTC) e
      `ConsentimentoVersao` (ex.: "v1") ficam persistidos no usuário.
- [x] Dado qualquer visitante, quando acessa `/privacidade`, então vê a política em pt-BR cobrindo:
      dados coletados, finalidade, base legal, compartilhamento, retenção, direitos do titular e
      canal de contato (contato@hubesporteslages.sc). Há link no rodapé de todas as páginas.
- [x] Dado um torcedor logado, quando acessa `GET /conta/meus-dados`, então baixa um JSON
      (camelCase) com: dados da conta (nome, e-mail, datas/versão de consentimento), inscrições
      de alerta (por e-mail) e ingressos. Interações de torcida (votos, mural, equipes favoritas)
      NÃO entram — usam identificador anônimo não vinculável à conta (documentado na política).
- [x] Dado um torcedor logado, quando confirma a exclusão com a senha correta em
      `POST /conta/excluir`, então: inscrições com o e-mail da conta são apagadas, ingressos são
      anonimizados (nome do comprador vira "Titular removido"; registros contábeis mantidos),
      o usuário Identity é excluído e a sessão é encerrada.
- [x] Dado senha incorreta na exclusão, então nada é apagado e uma mensagem de erro é exibida.
- [x] Dado um usuário Admin que é o ÚNICO admin do sistema, quando tenta se excluir, então a
      operação é bloqueada com mensagem explicativa.
- [x] Dado o formulário de inscrição de alertas, então o telefone é opcional e há nota curta de
      privacidade com link para `/privacidade`.
- [x] Dado um requisitante anônimo, quando chama os endpoints de administração da torcida
      (`/api/eventos/{id}/torcida/...` de escrita administrativa), então recebe 401/403 —
      a classe exige `[Authorize(Roles = "Admin")]`.
- [x] Dado um requisitante anônimo, quando chama `POST/DELETE /api/favoritos/equipes/{id}`,
      então recebe 401; logado (cookie do site ou JWT), o fluxo do `torcida.js` segue funcionando.

## Fora de escopo
- Confirmação de e-mail (double opt-in) no cadastro.
- Anonimização/expurgo das interações de torcida (já são anônimas por construção — TorcedorId
  aleatório de navegador, sem vínculo com a conta).
- Registro de auditoria de consentimento com histórico de versões (guardamos apenas a última).
- Encarregado (DPO) formal e relatório de impacto (RIPD) — fora do escopo do produto demo.
