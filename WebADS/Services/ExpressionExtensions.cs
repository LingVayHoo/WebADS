using System.Linq.Expressions;

namespace WebADS.Services
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> NotNullOrEmpty<T>(this Expression<Func<T, string>> selector)
        {
            var parameter = selector.Parameters[0]; // Получаем параметр выражения (например, "a")
            var body = Expression.AndAlso(
                Expression.NotEqual(selector.Body, Expression.Constant(null)), // a.Field != null
                Expression.NotEqual(selector.Body, Expression.Constant(""))    // a.Field != ""
            );

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }

}
