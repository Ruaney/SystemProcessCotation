# System Process Cotation

Monitoração de preços que envia alertas por email de quando comprar/vender ao atingir um limite.

O sistema é **assíncrono e orientado a eventos**: roda como um conjunto de *workers*
em segundo plano que se comunicam por um **barramento de mensagens (Redis Pub/Sub)**.

### features
+ arquitetura desacoplada com **.NET Generic Host** + 3 `BackgroundService`
+ **Redis Pub/Sub** como barramento de mensagens entre os estágios
+ **estado dos alertas no Redis** (preço/cooldown) — sem estado em memória
+ envio de email totalmente assíncrono (MailKit), sem bloquear o monitoramento
+ só envia um novo alerta se o preço mudar e fora do cooldown, evitando spam
+ encerramento gracioso (Ctrl+C / SIGTERM) gerenciado pelo host
+ pronto para rodar com **Docker Compose** (Redis + worker)

## Arquitetura

Três workers em um único processo, conectados pelo barramento Redis:

```
CotationWorker ──publish──▶ canal "cotations" ──▶ TradingWorker ──publish──▶ canal "alerts" ──▶ NotificationWorker
 (busca cotação)              (barramento Redis)     (decide compra/venda)      (barramento Redis)    (envia email / loga)
```

- **CotationWorker** (produtor): a cada intervalo busca a cotação e publica em `cotations`.
- **TradingWorker** (consumidor/produtor): decide compra/venda, aplica a deduplicação
  (preço mudou + cooldown) via Redis e publica os alertas em `alerts`.
- **NotificationWorker** (consumidor): envia o email; sem SMTP configurado, apenas registra o alerta.

## O sistema funcionando

Saída real de `docker compose up` (limiares de demonstração forçando um alerta de VENDA):

```text
info: CotationWorker[0]
      Monitorando PETR4 | venda >= R$ 1.00 | compra <= R$ 0.50 | intervalo 3000ms
info: TradingWorker[0]
      Inscrito no canal 'cotations', aguardando cotações...
info: NotificationWorker[0]
      Inscrito no canal 'alerts', aguardando alertas...
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: CotationWorker[0]
      Cotação PETR4: R$ 38.29
info: TradingService[0]
      VENDA: PETR4 R$ 38.29 (alvo: R$ 1.00)
info: TradingWorker[0]
      Alerta de Sell para PETR4 a R$ 38.29 → publicando em 'alerts'
warn: NotificationWorker[0]
      SMTP não configurado — alerta apenas registrado.
      Assunto: Alerta VENDA - PETR4 - R$ 38.29
      Alerta de Venda - PETR4

      Preço atual: R$ 38.29
      Preço de referência configurado: R$ 1.00
      Recomendação: Venda PETR4
      Horário: 25/06/2026 03:56:56
info: CotationWorker[0]
      Cotação PETR4: R$ 38.29
info: TradingService[0]
      VENDA: PETR4 R$ 38.29 (alvo: R$ 1.00)   ← preço repetido: NÃO gera novo alerta (dedup no Redis)
```

## Como executar

### Com Docker Compose (recomendado)

Sobe o Redis e o worker juntos:

```bash
docker compose up --build
```

O ativo e os preços são passados em `docker-compose.yml` (`command: ["PETR4", "1.00", "0.50"]`).

### Localmente (precisa de um Redis em `localhost:6379`)

```bash
docker run -p 6379:6379 redis:7-alpine   # em outro terminal
dotnet run --project SystemProcessCotation PETR4 22.67 22.59
```

Sem argumentos, o ativo e os preços são lidos da seção `Trading` em `appsettings.json`.

### Gerar executável

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Configuração

- **Redis**: `Redis:ConnectionString` em `appsettings.json` ou a variável de ambiente
  `Redis__ConnectionString` (o compose já aponta para `redis:6379`).
- **SMTP (opcional)**: renomeie `.env.example` para `.env` e preencha os campos
  (`HOST`, `PORT`, `USERNAME`, `PASSWORD`, `FROM`, `TO`). Sem SMTP, os alertas
  são apenas registrados em log.
