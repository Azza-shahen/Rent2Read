namespace Rent2Read.Domain.Dtos;

/*DTO(Data Transfer Object) is a simple object for carrying data between layers.
It usually contains only the needed properties, not everything from the entity.*/
public record FilterationDto(
int Skip,
int PageSize,
string SearchValue,
string SortColumnIndex,
string SortColumn,
string SortColumnDirection
);

/*Class
Reference type.
Equality is by reference(memory address).
Usually mutable(you can change properties after creation).
Best for objects that have behavior, logic, or state changes.

Record
Also a reference type but designed for data objects.
Equality is by value (if all properties are the same → objects are equal).
Designed to be immutable(values don’t change after creation).
Best for data carriers like DTOs.
Provides auto-generated methods (Equals, GetHashCode, ToString).*/



