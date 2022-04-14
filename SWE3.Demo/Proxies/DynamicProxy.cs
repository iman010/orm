using Castle.DynamicProxy;
using Microsoft.CSharp.RuntimeBinder;
using SWE3.Demo.Extensions;
using System;
using System.Collections.Generic;

namespace SWE3.Demo.Proxies
{
    internal class DynamicProxy
    {
        private static readonly ProxyGenerator dynamicProxyGenerator = new ProxyGenerator();
        internal static Dictionary<Object, Type> typeMapper = new Dictionary<object, Type>();
        private class DynamicProxyInterceptor : IInterceptor
        {
            private readonly Func<String, Object> getPropertyCalled;
            public DynamicProxyInterceptor(Func<String, Object> getPropertyCalled)
            {
                this.getPropertyCalled = getPropertyCalled ?? throw new ArgumentNullException(nameof(getPropertyCalled));
            }

            public void Intercept(IInvocation invocation)
            {
                if (invocation.Method.Name.StartsWith("get_"))
                {
                    invocation.ReturnValue = this.getPropertyCalled?.Invoke(invocation.Method.GetPropertyName());
                }
                else
                {
                    invocation.Proceed();
                }
            }
        }
        public Object CreateDynamicProxy(Type type, Func<string, Object> getProperty)
        {
            var obj = dynamicProxyGenerator.CreateClassProxy(type, new DynamicProxyInterceptor(propertyName => getProperty(propertyName)));
            typeMapper.Add(obj, type);
            return obj;
        }
    }
}
