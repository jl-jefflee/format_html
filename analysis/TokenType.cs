using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html.analysis
{

    /* 文档中最小的单元片段类型
     * 约定：
     *  1. <!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
     *     文档类型声明作为一个token来处理，以"<!"开始，直到遇到">"时结束，如果期间遇到了其它明显的开始符号如"<"将自动补全文档声明的结束符
     *  2. <!---->注释，由"<!--"开始，直到遇到"-->"结束，如果一直没有结束默认持续到结尾
     *  3. Mark：形如<node,标记的开始，"<"和name之间可以包含空格，将被过滤掉，读到空格时表示该token读取结束
     *  4. CloseMark：形如</node，关闭标记
     *  5. MarkEnd：形如>,标记的结束符
     *  6. SelfCloseMarkEnd：形如/>，自关闭标记的结束符
     *  7. Whitespace：标记之间的空格，">"和"<"之间若没有文本即视为标记之间的空格
     *  8. Block：属性名或属性值，引号引起的部分视为一个整体，引号中可以包含一些特殊字符；
     *     属性名和属性值将一起读取，碰到引号时将一直读取直到引号闭合。
     *     连接到一起的文本将一直读取，除非有成对的引号分割，如href="/html/dongman/2015/0324/29580.html"class="img"将从class处分割
     *  10.Text：文本，><之间，从">"开始一直读取直到遇到"<"结束
     * 
     * */

    public enum TokenType
    {
        /// <summary>
        /// 形如"&lt;!DOCTYPE...>"
        /// </summary>
        DocDeclare=1,

        /// <summary>
        /// 形如"&lt;!--   -->"
        /// </summary>
        Comment,

        /// <summary>
        /// 形如"&lt;nodename"
        /// </summary>
        Mark,

        /// <summary>
        /// 形如"&lt;/nodename"
        /// </summary>
        CloseMark,

        /// <summary>
        /// 形如">"
        /// </summary>
        MarkEnd,

        /// <summary>
        /// 形如"/>"
        /// </summary>
        SelfCloseMarkEnd,

        /// <summary>
        /// 标记之间的空格,">"和"&lt;"之间若没有文本即视为标记之间的空格
        /// </summary>
        Whitespace,

        /// <summary>
        /// 属性名或属性值
        /// </summary>
        Block,


        /// <summary>
        /// =，属性名和属性值之间的
        /// </summary>
        [Obsolete("弃用，不在单独读取等号")]
        Equal,

        /// <summary>
        /// ">"和"&lt;"之间的所有文本
        /// </summary>
        Text
        
    }
}
