using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html.analysis
{
    public enum NodeType
    {
        /// <summary>
        /// 形如&lt;!DOCTYPE
        /// </summary>
        DocDeclare = 1,

        /// <summary>
        /// 形如&lt;!--   -->
        /// </summary>
        Comment,

        /// <summary>
        /// 标记间的空格
        /// </summary>
        Whitespace,

        /// <summary>
        /// 文本
        /// </summary>
        Text,

        /// <summary>
        /// 形如&lt;node atr="abc">
        /// </summary>
        Node,

        /// <summary>
        /// 形如&lt;node atr="abc" />
        /// </summary>
        SelfCloseNode,

        /// <summary>
        /// 形如&lt;/node>
        /// </summary>
        CloseNode
    }
}
