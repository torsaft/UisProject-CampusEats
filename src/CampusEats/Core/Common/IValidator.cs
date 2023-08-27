namespace CampusEats.Core.Common;

public interface IValidator<T>
{
	(bool IsValid, string Error) IsValid(T item);
}
