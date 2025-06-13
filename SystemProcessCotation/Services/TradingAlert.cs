public enum AlertType{
    Buy, Sell
}

public class TradingAlert{
    public AlertType Type {get;set;}
    public string Symbol {get;set;} = string.Empty;
    public double CurrentPrice {get;set;}
    public double TargetPrice {get;set;}
    public DateTime Timestamp {get;set;} = DateTime.Now;
    
    public string GetMessage(){
        var action = Type == AlertType.Buy? "Comprar" : "Vender";
        return $"Alerta de {action} - {Symbol}\n\n"+
        $"Preço atual: R$ {CurrentPrice:F2}\n"+
        $"Preço de referência: R$ {TargetPrice:F2}\n"+
        $"Recomendação: {action} {Symbol} \n"+
        $"Horário: {Timestamp:dd/MM/yyyy HH:mm:ss}";
    }

    public string GetSubject(){
        var action = Type == AlertType.Buy? "COMPRA": "VENDA";
        return $"Alerta {action} - {Symbol} - R$ {CurrentPrice:F2}";
    }
}