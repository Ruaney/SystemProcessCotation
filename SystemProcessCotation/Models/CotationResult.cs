public class CotationResult{
     public string Symbol {get;set;} = string.Empty;
     public double Price {get;set;}
     public DateTime Timestamp {get;set;} = DateTime.Now;
     public bool IsValid => Price > 0 && !string.IsNullOrEmpty(Symbol);
}