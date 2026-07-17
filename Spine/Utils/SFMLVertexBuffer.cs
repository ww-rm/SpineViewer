using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Utils
{
    /// <summary>
    /// <see cref="VertexBuffer"/> 类的包装类
    /// </summary>
    public class SFMLVertexBuffer: Drawable, IDisposable
    {
        private int _vertexCount = 0;
        private Vertex[] _vertexArray;
        private readonly List<int> _vertexArrayOffsets = []; // [a, b, c, ...] -> [0:a) + [a:b) + [c:...), 用于记录顶点缓存区的分段点
        private readonly List<RenderStates> _vertexArrayStates = []; // 顶点分段的绘制状态信息

        private bool _isDirty = false;
        private readonly VertexBuffer _vertexBuffer;

        /// <summary>
        /// <inheritdoc cref="VertexBuffer(uint, PrimitiveType, VertexBuffer.UsageSpecifier)"/>
        /// </summary>
        public SFMLVertexBuffer(uint vertexCount, PrimitiveType primitiveType, VertexBuffer.UsageSpecifier usageType)
        {
            _vertexArray = new Vertex[vertexCount];
            _vertexBuffer = new(vertexCount, primitiveType, usageType);
        }

        /// <summary>
        /// 当前实际顶点数
        /// </summary>
        public int VertexCount { get => _vertexCount; }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            _vertexCount = 0;
            _vertexArrayOffsets.Clear();
            _vertexArrayStates.Clear();
        }

        /// <summary>
        /// 添加一个顶点
        /// </summary>
        public void AddVertex(Vertex vt)
        {
            // 按 1.5 倍自动扩容
            if (_vertexCount >= _vertexArray.Length)
            { 
                var vertexArray = new Vertex[_vertexCount + (_vertexCount >> 1) + 1];
                _vertexArray.CopyTo(vertexArray, 0);
                _vertexArray = vertexArray;
            }
            _vertexArray[_vertexCount++] = vt;
            _isDirty = true;
        }

        /// <summary>
        /// 添加一个渲染状态, 从添加的位置开始的顶点到上一个渲染状态为止的顶点 (不含) 将使用该渲染状态进行渲染
        /// </summary>
        public void AddStates(RenderStates states)
        {
            _vertexArrayStates.Add(states);
            _vertexArrayOffsets.Add(_vertexCount);
        }

        /// <summary>
        /// <inheritdoc cref="Drawable.Draw(RenderTarget, RenderStates)"/>
        /// <para>参数提供的 states 将会作用在最后一段未指定状态的顶点集合上</para>
        /// </summary>
        public void Draw(RenderTarget target, RenderStates states)
        {
            if (_isDirty)
            {
                _vertexBuffer.Update(_vertexArray, (uint)_vertexCount, 0);
                _isDirty = false;
            }

            // 分段 draw
            uint start = 0;
            uint count;
            for (int i = 0, n = _vertexArrayOffsets.Count; i < n; i++)
            {
                var end = (uint)_vertexArrayOffsets[i];
                count = end - start;
                if (count > 0)
                    _vertexBuffer.Draw(target, start, count, _vertexArrayStates[i]);
                start = end;
            }

            count = (uint)_vertexCount - start;
            if (count > 0)
                _vertexBuffer.Draw(target, start, count, states);
        }

        #region IDisposable 接口实现

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _vertexBuffer.Dispose();
            }
            _disposed = true;
        }

        ~SFMLVertexBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            if (_disposed)
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}
