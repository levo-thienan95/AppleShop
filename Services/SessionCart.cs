using System.Text.Json;
using AppleStore.Models;

namespace AppleStore.Services;

/// <summary>Giỏ hàng lưu trong Session (ISession) — không cần bảng database.</summary>
public static class SessionCart
{
    private const string Key = "ShoppingCart";

    public static List<CartItem> GetCart(this ISession session)
    {
        if (!session.TryGetValue(Key, out var json) || json is null)
            return new List<CartItem>();

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    public static void SaveCart(this ISession session, List<CartItem> cart)
    {
        session.Set(Key, JsonSerializer.SerializeToUtf8Bytes(cart));
    }

    public static void ClearCart(this ISession session)
    {
        session.Remove(Key);
    }

    public static int Count(this ISession session) => GetCart(session).Sum(i => i.Quantity);
}
