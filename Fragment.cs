using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html
{
    //html片段
    internal class Fragment
    {
        public String Text { get; set; }
        public FragmentType Type { get; set; }
        public String Name { get; set; }

        public Fragment(String text, FragmentType type,String name)
        {
            this.Text = text;
            this.Type = type;
            this.Name = name;
        }

        /// <summary>
        /// 移除节点名称前缀
        /// </summary>
        [Obsolete("当属性值中包含冒号时该方法失败",true)]
        public void RemovePrefix()
        {
            int index=Text.LastIndexOf(':');
            if (index != -1)
            {
                Text = Text.Substring(0, 1) + Text.Substring(index + 1);
            }


            index = Name.LastIndexOf(':');
            if (index != -1)
            {
                Name = Name.Substring(index + 1);
            }
        }
    }
}
