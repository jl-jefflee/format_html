using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html
{
    //良构化html文档
    //对属性名值配对，对未添加引号的属性值添加引号，去掉重复的属性值
    //添加结束节点
    //去掉没有开始的结束节点
    public class FormatHtml
    {

        private FormatSetting setting = null;
        private List<Fragment> stack = new List<Fragment>();
        private analysis.NodeReader reader = null;
        

        public FormatHtml(String html)
        {
            setting = new FormatSetting();

            reader = new analysis.NodeReader(html,setting.IgnorePreFix);
        }

        public FormatHtml(String html, FormatSetting setting)
        {
            this.setting = setting;
            reader = new analysis.NodeReader(html, setting.IgnorePreFix);
        }




        public String Format()
        {
            Boolean isScriptNode = false;
            while (reader.Read())
            {
                if (reader.NodeType == analysis.NodeType.DocDeclare)
                {
                    if (!setting.IgnoreDocDeclare)
                    {
                        stack.Add(new Fragment(reader.Node, FragmentType.DocDeclare, reader.NodeName));
                    }
                }
                else if (reader.NodeType == analysis.NodeType.Comment)
                {
                    if (!setting.IgnoreComment)
                    {
                        stack.Add(new Fragment(reader.Node, FragmentType.Comment, reader.NodeName));
                    }
                }
                else if (reader.NodeType == analysis.NodeType.Whitespace)
                {
                    if (!setting.IgnoreWhitespace)
                    {
                        stack.Add(new Fragment(reader.Node, FragmentType.Whitespace, reader.NodeName));
                    }
                }
                else if (reader.NodeType == analysis.NodeType.Text)
                {
                    if (setting.IgnoreScript && isScriptNode)
                    {
                        isScriptNode = false;
                    }
                    else
                    {
                        stack.Add(new Fragment(reader.Node, FragmentType.Text, reader.NodeName));
                    }
                }
                else if (reader.NodeType == analysis.NodeType.Node)
                {
                    String nodeName = reader.NodeName.Trim().ToLower();
                    String lastNodeName = stack.Count > 0 ? stack[stack.Count - 1].Name : "";
                    if(nodeName=="script"&&setting.IgnoreScript)
                    {
                        isScriptNode = true;
                    }
                    else if (nodeName == "tr" && setting.ValidateHtmlTable && lastNodeName != "table" && lastNodeName != "tbody" && lastNodeName != "tr")
                    {
                        //忽略该节点
                    }
                    else if (nodeName == "td" && setting.ValidateHtmlTable && lastNodeName != "tr" && lastNodeName != "td")
                    {
                        //忽略该节点
                    }
                    else 
                    {
                        isScriptNode = false;
                        Fragment fragment=new Fragment(reader.Node, FragmentType.Begin, reader.NodeName);
                        stack.Add(fragment);
                        lastNodeName = nodeName;
                    }
                   
                    
                }
                else if (reader.NodeType == analysis.NodeType.SelfCloseNode)
                {
                    if (!setting.IgnoreScript || reader.NodeName.Trim().ToLower() != "script")
                    {
                        Fragment fragment = new Fragment(reader.Node, FragmentType.Html, reader.NodeName);
                        stack.Add(fragment);
                    }

                    
                }
                else if (reader.NodeType == analysis.NodeType.CloseNode)
                {
                    if (!setting.IgnoreScript||reader.NodeName.Trim().ToLower()!="script")
                    {
                        Fragment fragment = new Fragment(reader.Node, FragmentType.End, reader.NodeName);
                        mergeNode(fragment);
                    }

                    
                }
                else
                {
                    throw new Exception("未知的节点类型");
                }
            }

            
            //为没有结束节点的节点添加结束节点
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                if (stack[i].Type == FragmentType.Begin)
                {
                    mergeNode(new Fragment("</"+stack[i].Name+">", FragmentType.End, stack[i].Name));
                }
            }
            /* 有时候会出现在文档后面附加文本的情况
            if (stack.Count == 1 && stack[0].Type == FragmentType.Html)
            {
                return stack[0].Text;
            }
            else
            {
                throw new Exception("良构化html失败");
            }*/
            if(stack.Count>0&&stack[0].Type==FragmentType.Html)
            {
                return stack[0].Text;
            }
            else
            {
                throw new Exception("良构化html失败");
            }
        }

        //当读取到闭合节点时，合并闭合节点为html元素
        //采用结束节点优先的原则，当发现没有结束节点的节点时，直接在节点后补充结束节点
        private void mergeNode(Fragment fragment)
        {
            //在栈中是否能找到与要合并的节点匹配的开始节点
            Boolean find = false;
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                if (stack[i].Type == FragmentType.Begin)
                {
                    if (stack[i].Name == fragment.Name)//合并
                    {
                        stack[i].Type = FragmentType.Html;
                        for (int j = i+1; j < stack.Count; j++)
                        {
                            stack[i].Text += stack[j].Text;
                        }
                        stack[i].Text += fragment.Text;
                        for (int j = stack.Count - 1; j > i; j--)
                        {
                            stack.RemoveAt(j);
                        }
                        break;
                    }
                    else//当前为开始节点，没有匹配的闭合节点，添加闭合节点
                    {
                        if (!find)
                        {
                            foreach (Fragment f in stack)
                            {
                                if (f.Name == fragment.Name&&f.Type==FragmentType.Begin)
                                {
                                    find = true;
                                }
                            }
                        }
                        if (find)//如果能找到，给当前节点添加结束节点
                        {
                            stack[i].Type = FragmentType.Html;
                            stack[i].Text += "</" + stack[i].Name + ">";
                        }
                        else//没有与要合并的结束节点匹配的开始节点，抛弃该结束节点
                        {
                            break;
                        }
                    }
                }
            }
        }








        
    }



    
}
