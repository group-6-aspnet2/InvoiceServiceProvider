namespace Domain.Responses;

public class InvoiceResult<T> : InvoiceResult
{
    public T? Result { get; set; }
}

public class  InvoiceResult : ResponseResult
{
    
}
