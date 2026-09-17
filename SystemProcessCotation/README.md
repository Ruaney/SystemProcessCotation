# System Process Cotation

![Demo](assets/demo.gif)

Monitoração de preços que envia alertas por email de quando comprar/vender ao atingir um limite.

### features extras:
+ mensagens mais detalhadas no console
+ sistema só envia um novo alerta se o preço mudar, evitando spam
+ intervalo para as verificações da cotação

### como executar
```bash
dotnet run -- --help
```

dotnet run **Ativo preçoVenda preçoCompra [intervaloMs] [cooldownSegundos]**

```bash
dotnet run PETR4 22.67 22.59
```

O ativo é normalizado para maiúsculas. Tickers brasileiros copiados do Yahoo, como `PETR4.SA`, e formatos de corretora/TradingView, como `B3:PETR4` ou `BMFBOVESPA:PETR4.SA`, também são aceitos e enviados ao Fundamentus como `PETR4`.

Para uma demonstração mais rápida, informe também o intervalo de consulta e o cooldown entre alertas:

```bash
dotnet run PETR4 22.67 22.59 1000 15
dotnet run PETR4 22.67 22.59 1second 1min
dotnet run B3:PETR4 22.67 22.59 5m 1h
dotnet run PETR4.SA 22.67 22.59 1segundo 1minuto
```

### Gerar executável
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### Executar o arquivo
```bash
bin\Release\net9.0\win-x64\publish\SystemProcessCotation.exe PETR4 35.00 30.00
``` 

### Executando no Windows através do powershell
```powershell
Set-ExecutionPolicy Unrestricted
``` 

### Executar com um script pronto
```powershell
.\script.ps1 PETR4 35.00 30.00
``` 

### Configuração

Configure o SMTP renomeando `.env.example` para `.env` e edite os campos.

As variáveis curtas (`HOST`, `PORT`, `USERNAME`, `PASSWORD`, `FROM`, `TO`, `ENABLE_SSL`) continuam funcionando. Também é possível usar aliases de deploy: `SMTP_HOST` ou `SMTP_SERVER`, `SMTP_PORT`, `SMTP_USERNAME`, `SMTP_USER`, `SMTP_USER_NAME` ou `SMTP_USER_EMAIL`, `SMTP_PASSWORD`, `SMTP_FROM`, `SMTP_FROM_ADDRESS` ou `SMTP_FROM_EMAIL`, `SMTP_TO`, `SMTP_TO_ADDRESS` ou `SMTP_TO_EMAIL`, `SMTP_ENABLE_SSL`, `SMTP_SSL`, `SMTP_USE_SSL`, além dos equivalentes `EMAIL_*` e `MAIL_*`.

Valores de SSL aceitam formas comuns como `true`, `false`, `on`, `off`, `enabled`, `disabled`, `sim`, `não`, `ligado`, `desligado`, `habilitado` e `desabilitado`.
