using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Utils
{
    /// <summary>
    /// 提供关联的实现标识
    /// </summary>
    /// <typeparam name="TKey">标记类型</typeparam>
    public interface IImplementationKeyAttribute<TKey>
    {
        /// <summary>
        /// 实现类类型标记
        /// </summary>
        TKey ImplementationKey { get; }
    }

    /// <summary>
    /// 可以使用反射查找基类关联的所有实现类
    /// </summary>
    /// <typeparam name="TBase">所有实现类的基类型</typeparam>
    /// <typeparam name="TAttr">实现类类型属性标记类型</typeparam>
    /// <typeparam name="TKey">实现类类型标记类型</typeparam>
    public abstract class ImplementationResolver<TBase, TAttr, TKey>
        where TAttr : Attribute, IImplementationKeyAttribute<TKey>
        where TKey : notnull
    {
        /// <summary>
        /// 实现类型缓存
        /// </summary>
        private static readonly Dictionary<TKey, Type> _implementationTypes = [];

        static ImplementationResolver()
        {
            var baseType = typeof(TBase);
            var impTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in impTypes)
            {
                foreach (var attr in type.GetCustomAttributes<TAttr>())
                {
                    var key = attr.ImplementationKey;
                    if (_implementationTypes.ContainsKey(key))
                        throw new InvalidOperationException($"Multiple implementations found for key: {key}");
                    _implementationTypes[key] = type;
                }
            }
        }

        /// <summary>
        /// 判断某种类型是否实现
        /// </summary>
        public static bool HasImplementation(TKey key) => _implementationTypes.ContainsKey(key);

        /// <summary>
        /// 根据实现类键和参数创建实例
        /// </summary>
        /// <param name="impKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected static TBase CreateInstance(TKey impKey, params object?[]? args)
        {
            if (!_implementationTypes.TryGetValue(impKey, out var type))
                throw new NotImplementedException($"Not implemented type for {typeof(TBase)}: {impKey}");
            return (TBase)Activator.CreateInstance(type, args);
        }
    }
}
