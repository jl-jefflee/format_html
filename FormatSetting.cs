using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html
{
    /// <summary>
    /// 格式化html配置
    /// </summary>
    public class FormatSetting
    {
        public Boolean IgnoreDocDeclare { get; set; }
        public Boolean IgnoreComment { get; set; }
        public Boolean IgnoreScript { get; set; }
        public Boolean IgnoreWhitespace { get; set; }
        public Boolean IgnorePreFix { get; set; }
        /// <summary>
        /// 验证table标签，tr的父标签必须为table或tbody，td的父标签必须为tr
        /// </summary>
        public Boolean ValidateHtmlTable { get; set; }

        public FormatSetting()
        {
            IgnoreDocDeclare = true;
            IgnoreComment = true;
            IgnoreScript = true;
            IgnoreWhitespace = true;
            IgnorePreFix = true;
            ValidateHtmlTable = true;
        }
    }
}
