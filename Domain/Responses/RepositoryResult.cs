namespace Domain.Responses;

public class RepositoryResult<T> : RepositoryResult
{
    public T? Result { get; set; }
}

public class RepositoryResult : ResponseResult
{

}
