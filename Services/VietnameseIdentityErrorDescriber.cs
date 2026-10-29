using Microsoft.AspNetCore.Identity;

namespace AppleStore.Services;

/// <summary>
/// Dịch các thông báo lỗi của ASP.NET Identity sang tiếng Việt,
/// để người dùng không gặp thông báo tiếng Anh trên giao diện tiếng Việt.
/// </summary>
public class VietnameseIdentityErrorDescriber : IdentityErrorDescriber
{
    private static IdentityError Error(string code, string description) => new()
    {
        Code = code,
        Description = description
    };

    public override IdentityError DefaultError() =>
        Error(nameof(DefaultError), "Đã có lỗi xảy ra, vui lòng thử lại.");

    public override IdentityError ConcurrencyFailure() =>
        Error(nameof(ConcurrencyFailure), "Dữ liệu vừa được thay đổi bởi người khác, vui lòng thử lại.");

    public override IdentityError PasswordMismatch() =>
        Error(nameof(PasswordMismatch), "Mật khẩu xác nhận không khớp.");

    public override IdentityError InvalidToken() =>
        Error(nameof(InvalidToken), "Mật mã không hợp lệ hoặc đã hết hạn.");

    public override IdentityError RecoveryCodeRedemptionFailed() =>
        Error(nameof(RecoveryCodeRedemptionFailed), "Không thể sử dụng mã khôi phục, vui lòng thử lại.");

    public override IdentityError LoginAlreadyAssociated() =>
        Error(nameof(LoginAlreadyAssociated), "Tài khoản này đã được liên kết với một người dùng khác.");

    public override IdentityError InvalidUserName(string userName) =>
        Error(nameof(InvalidUserName), $"Tên đăng nhập '{userName}' không hợp lệ.");

    public override IdentityError InvalidEmail(string email) =>
        Error(nameof(InvalidEmail), "Email không hợp lệ.");

    public override IdentityError DuplicateUserName(string userName) =>
        Error(nameof(DuplicateUserName), $"Tên đăng nhập '{userName}' đã tồn tại.");

    public override IdentityError DuplicateEmail(string email) =>
        Error(nameof(DuplicateEmail), $"Email '{email}' đã được sử dụng.");

    public override IdentityError UserAlreadyHasPassword() =>
        Error(nameof(UserAlreadyHasPassword), "Người dùng đã có mật khẩu.");

    public override IdentityError UserLockoutNotEnabled() =>
        Error(nameof(UserLockoutNotEnabled), "Chưa bật khóa tài khoản cho người dùng này.");

    public override IdentityError PasswordTooShort(int length) =>
        Error(nameof(PasswordTooShort), $"Mật khẩu phải có ít nhất {length} ký tự.");

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
        Error(nameof(PasswordRequiresUniqueChars), $"Mật khẩu phải chứa ít nhất {uniqueChars} ký tự khác nhau.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Error(nameof(PasswordRequiresNonAlphanumeric), "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (!@#$...).");

    public override IdentityError PasswordRequiresDigit() =>
        Error(nameof(PasswordRequiresDigit), "Mật khẩu phải chứa ít nhất một chữ số (0-9).");

    public override IdentityError PasswordRequiresLower() =>
        Error(nameof(PasswordRequiresLower), "Mật khẩu phải chứa ít nhất một chữ thường (a-z).");

    public override IdentityError PasswordRequiresUpper() =>
        Error(nameof(PasswordRequiresUpper), "Mật khẩu phải chứa ít nhất một chữ hoa (A-Z).");
}
