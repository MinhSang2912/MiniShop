namespace MiniShop.Core.Exceptions;

/// <summary>Vi phạm quy tắc nghiệp vụ (ví dụ hủy đơn đã thanh toán) → 409.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>Không tìm thấy tài nguyên → 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}