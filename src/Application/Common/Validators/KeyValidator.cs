using System;
using System.Linq.Expressions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Trpg.Application.Common.Validators
{
    public class KeyValidator<TModel, TEntity, TProperty> : AbstractValidator<TModel> where TEntity : class
    {
        public KeyValidator(DbSet<TEntity> entities, Expression<Func<TModel, TProperty>> expression,
            bool present = true)
        {
            var ruleBuilderOptions = RuleFor(expression)
                .MustAsync(async (key, cancellation) => (await entities.FindAsync(key) != null) == present);
            if (present)
                ruleBuilderOptions.WithMessage(cmd => $"The {nameof(TEntity)} doesn't exist");
            else
                ruleBuilderOptions.WithMessage(cmd => $"The {nameof(TEntity)} already exists");
        }
    }
}
