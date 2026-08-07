namespace nitevault.Dto;
public record UserDTO(Guid Id, string Email, string Name, DateTime? CreatedAt, bool Active);