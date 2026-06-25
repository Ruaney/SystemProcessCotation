# System Process Cotation

Monitoração de preços que envia alertas por email de quando comprar/vender ao atingir um limite.

O sistema é **assíncrono e orientado a eventos**: roda como um conjunto de *workers*
em segundo plano que se comunicam por um **barramento de mensagens AWS (SNS + SQS)**.

### features
+ arquitetura desacoplada com **.NET Generic Host** + 3 `BackgroundService`
+ **AWS SNS + SQS** como barramento: tópicos SNS (fan-out) entregando em filas SQS (durável)
+ **estado dos alertas no Redis** (preço/cooldown) — sem estado em memória
+ envio de email totalmente assíncrono (MailKit), sem bloquear o monitoramento
+ só envia um novo alerta se o preço mudar e fora do cooldown, evitando spam
+ encerramento gracioso (Ctrl+C / SIGTERM) gerenciado pelo host
+ pronto para rodar com **Docker Compose** (LocalStack + Redis + worker) — sem conta AWS

## Arquitetura

Três workers em um único processo, conectados por SNS/SQS. Cada canal é um **tópico SNS**
com uma **fila SQS** inscrita: o produtor publica no tópico (`sns:Publish`) e o consumidor
faz long-poll na fila (`sqs:ReceiveMessage` + `DeleteMessage`).

```
CotationWorker ─publish▶ SNS "cotations" ─▶ SQS "cotations" ─▶ TradingWorker ─publish▶ SNS "alerts" ─▶ SQS "alerts" ─▶ NotificationWorker
 (busca cotação)             (tópico)            (fila)          (decide compra/venda)      (tópico)         (fila)        (envia email / loga)
```

- **CotationWorker** (produtor): a cada intervalo busca a cotação e publica no tópico `cotations`.
- **TradingWorker** (consumidor/produtor): consome a fila `cotations`, decide compra/venda,
  aplica a deduplicação (preço mudou + cooldown) via Redis e publica no tópico `alerts`.
- **NotificationWorker** (consumidor): consome a fila `alerts` e envia o email;
  sem SMTP configurado, apenas registra o alerta.

> O `SnsSqsInitializer` provisiona tópicos, filas e inscrições no startup (idempotente,
> com retry para o cold start do LocalStack) antes de qualquer worker publicar/consumir.

## O sistema funcionando

Saída real de `docker compose up` (limiares de demonstração forçando um alerta de VENDA):

```text
info: SnsSqsInitializer[0]
      Provisionando tópicos SNS e filas SQS...
info: SnsSqsInitializer[0]
      Barramento SNS/SQS pronto (canais: 'cotations', 'alerts')
info: CotationWorker[0]
      Monitorando PETR4 | venda >= R$ 1.00 | compra <= R$ 0.50 | intervalo 3000ms
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: SnsSqsEventBus[0]
      Consumindo a fila SQS 'cotations'
info: SnsSqsEventBus[0]
      Consumindo a fila SQS 'alerts'
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
      Horário: 25/06/2026 21:22:40
info: CotationWorker[0]
      Cotação PETR4: R$ 38.29
info: TradingService[0]
      VENDA: PETR4 R$ 38.29 (alvo: R$ 1.00)   ← preço repetido: NÃO gera novo alerta (dedup no Redis)
```

Tópicos, filas e inscrições criados (via `awslocal` no LocalStack):

```text
$ awslocal sns list-topics
  arn:aws:sns:us-east-1:000000000000:cotations
  arn:aws:sns:us-east-1:000000000000:alerts
$ awslocal sqs list-queues
  .../000000000000/cotations
  .../000000000000/alerts
$ awslocal sns list-subscriptions   # protocolo sqs em ambos os tópicos
```

## Como executar

### Com Docker Compose (recomendado)

Sobe LocalStack (SNS/SQS), Redis e o worker juntos — não precisa de conta AWS:

```bash
docker compose up --build
```

O ativo e os preços são passados em `docker-compose.yml` (`command: ["PETR4", "1.00", "0.50"]`).

### Localmente

É preciso ter SNS/SQS (LocalStack) e Redis acessíveis:

```bash
docker run -p 4566:4566 localstack/localstack:3   # SNS + SQS
docker run -p 6379:6379 redis:7-alpine            # estado dos alertas
dotnet run --project SystemProcessCotation PETR4 22.67 22.59
```

Sem argumentos, o ativo e os preços são lidos da seção `Trading` em `appsettings.json`.

### Gerar executável

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Configuração

- **AWS / SNS+SQS**: seção `Aws` em `appsettings.json` ou variáveis `Aws__*`.
  Com `Aws:ServiceUrl` definido (ex.: `http://localstack:4566`) usa LocalStack com
  credenciais de teste; deixe vazio para usar a **AWS real** (cadeia padrão de credenciais
  e `Aws:Region`).
- **Redis**: `Redis:ConnectionString` em `appsettings.json` ou `Redis__ConnectionString`
  (o compose já aponta para `redis:6379`).
- **SMTP (opcional)**: renomeie `.env.example` para `.env` e preencha os campos
  (`HOST`, `PORT`, `USERNAME`, `PASSWORD`, `FROM`, `TO`). Sem SMTP, os alertas
  são apenas registrados em log.
