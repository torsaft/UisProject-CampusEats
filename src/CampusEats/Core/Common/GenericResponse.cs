namespace CampusEats.Core.Common;

public record GenericResponse<T>
{
	public GenericResponse(string[] errors)
	{
		Errors = errors;
		Data = default!;
	}

	public GenericResponse(T result)
	{
		Data = result;
		Errors = Array.Empty<string>();
	}

	public T Data { get; private set; }
	public string[] Errors { get; private set; }
	public bool Success => Errors.Length == 0;

	public static implicit operator GenericResponse<T>(T result) => new(result);

	public static implicit operator GenericResponse<T>(string[] errors) => new(errors);
}
