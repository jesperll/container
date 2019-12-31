﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Unity.Builder;
using Unity.Policy;
using Unity.Resolution;

namespace Unity.Processors
{
    public class FieldProcessor : MemberProcessor<FieldInfo, object>
    {
        #region Constructors

        public FieldProcessor(IPolicySet policySet)
            : base(policySet)
        {
        }

        #endregion


        #region Overrides

        protected override Type MemberType(FieldInfo info) => info.FieldType;

        protected override IEnumerable<FieldInfo> DeclaredMembers(Type type) => UnityDefaults.SupportedFields(type);

        #endregion


        #region Expression 

        protected override Expression GetResolverExpression(FieldInfo info)
        {
            var attribute = info.GetCustomAttribute(typeof(DependencyResolutionAttribute)) as DependencyResolutionAttribute
                                                                                           ?? DependencyAttribute.Instance;
            var resolver = attribute.GetResolver<BuilderContext>(info);

            return Expression.Assign(
                Expression.Field(Expression.Convert(BuilderContextExpression.Existing, info.DeclaringType), info),
                Expression.Convert(
                    Expression.Call(BuilderContextExpression.Context,
                        BuilderContextExpression.ResolveFieldMethod,
                        Expression.Constant(info, typeof(FieldInfo)),
                        Expression.Constant(attribute.Name, typeof(string)),
                        Expression.Constant(resolver, typeof(ResolveDelegate<BuilderContext>))),
                    info.FieldType));
        }

        protected override Expression GetResolverExpression(FieldInfo info, object? data)
        {
            var attribute = info.GetCustomAttribute(typeof(DependencyResolutionAttribute)) as DependencyResolutionAttribute
                                                                                           ?? DependencyAttribute.Instance;
            ResolveDelegate<BuilderContext>? resolver = data switch
            {
                IResolve policy                                 => policy.Resolve,
                IResolverFactory<FieldInfo> fieldFactory        => fieldFactory.GetResolver<BuilderContext>(info),
                IResolverFactory<Type> typeFactory              => typeFactory.GetResolver<BuilderContext>(info.FieldType),
                Type type when typeof(Type) != MemberType(info) => attribute.GetResolver<BuilderContext>(type),
                _                                               => null
            };

            if (null == resolver)
            {
                return Expression.Assign(
                    Expression.Field(Expression.Convert(BuilderContextExpression.Existing, info.DeclaringType), info),
                    Expression.Convert(
                        Expression.Call(BuilderContextExpression.Context,
                            BuilderContextExpression.OverrideFieldMethod,
                            Expression.Constant(info, typeof(FieldInfo)),
                            Expression.Constant(attribute.Name, typeof(string)),
                            Expression.Constant(data, typeof(object))),
                        info.FieldType));
            }
            else
            { 
                return Expression.Assign(
                    Expression.Field(Expression.Convert(BuilderContextExpression.Existing, info.DeclaringType), info),
                    Expression.Convert(
                        Expression.Call(BuilderContextExpression.Context,
                            BuilderContextExpression.ResolveFieldMethod,
                            Expression.Constant(info, typeof(FieldInfo)),
                            Expression.Constant(attribute.Name, typeof(string)),
                            Expression.Constant(resolver, typeof(ResolveDelegate<BuilderContext>))),
                        info.FieldType));
            }
        }

        #endregion


        #region Resolution

        protected override ResolveDelegate<BuilderContext> GetResolverDelegate(FieldInfo info)
        {
            var attribute = info.GetCustomAttribute(typeof(DependencyResolutionAttribute)) as DependencyResolutionAttribute
                                                                                           ?? DependencyAttribute.Instance;
            var resolver = attribute.GetResolver<BuilderContext>(info);

            return (ref BuilderContext context) =>
            {
                info.SetValue(context.Existing, context.Resolve(info, attribute.Name, resolver));
                return context.Existing;
            };
        }

        protected override ResolveDelegate<BuilderContext> GetResolverDelegate(FieldInfo info, object? data)
        {
            var attribute = info.GetCustomAttribute(typeof(DependencyResolutionAttribute)) as DependencyResolutionAttribute
                                                                                           ?? DependencyAttribute.Instance;
            ResolveDelegate<BuilderContext>? resolver = data switch
            {
                IResolve policy                                 => policy.Resolve,
                IResolverFactory<FieldInfo> fieldFactory        => fieldFactory.GetResolver<BuilderContext>(info),
                IResolverFactory<Type>typeFactory               => typeFactory.GetResolver<BuilderContext>(info.FieldType),
                Type type when typeof(Type) != MemberType(info) => attribute.GetResolver<BuilderContext>(type),
                _                                               => null
            };

            if (null == resolver)
            {
                return (ref BuilderContext context) =>
                {
                    info.SetValue(context.Existing, context.Override(info, attribute.Name, data));
                    return context.Existing;
                };
            }
            else
            { 
                return (ref BuilderContext context) =>
                {
                    info.SetValue(context.Existing, context.Resolve(info, attribute.Name, resolver));
                    return context.Existing;
                };
            }
        }

        #endregion
    }
}
