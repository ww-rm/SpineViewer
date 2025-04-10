using SpineViewer.Exporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
{
    public interface IImplementationKey<TKey>
    {
        TKey ImplementationKey { get; }
    }

    /// <summary>
    /// 可以使用反射查找基类关联的所有实现类
    /// </summary>
    /// <typeparam name="TBase">所有实现类的基类型</typeparam>
    /// <typeparam name="TAttr">实现类类型属性标记类型</typeparam>
    /// /// <typeparam name="TKey">实现类类型标记类型</typeparam>
    public abstract class ImplementationResolver<TBase, TAttr, TKey> where TAttr : Attribute, IImplementationKey<TKey>
    {
        /// <summary>
        /// 实现类型缓存
        /// </summary>
        private static readonly Dictionary<TKey, Type> ImplementationTypes = new();

        static ImplementationResolver()
        {
            var baseType = typeof(TBase);
            var impTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => baseType.IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var type in impTypes)
            {
                foreach (var attr in type.GetCustomAttributes<TAttr>())
                {
                    var key = attr.ImplementationKey;
                    if (ImplementationTypes.ContainsKey(key))
                        throw new InvalidOperationException($"Multiple implementations found for key: {key}");
                    ImplementationTypes[key] = type;
                }
            }
            NLog.LogManager.GetCurrentClassLogger().Debug("Found implementations for {}: {}", baseType, string.Join(", ", ImplementationTypes.Keys));
        }

        /// <summary>
        /// 判断某种类型是否实现
        /// </summary>
        public static bool HasImplementation(TKey key) => ImplementationTypes.ContainsKey(key);

        /// <summary>
        /// 根据实现类键和参数创建实例
        /// </summary>
        /// <param name="impKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected static TBase New(TKey impKey, object?[] args)
        {
            if (!ImplementationTypes.TryGetValue(impKey, out var type))
                throw new NotImplementedException($"Not implemented type for {typeof(TBase)}: {impKey}");
            return (TBase)Activator.CreateInstance(type, args);
        }
    }
}
