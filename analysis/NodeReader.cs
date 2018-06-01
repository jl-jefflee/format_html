using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html.analysis
{

    /* 读取文档的整个节点，
     * 纠正重复属性（覆盖相同的属性），
     * 未加引号属性添加引号，如果block中包含单引号，则添加双引号，如果block中包含双引号，添加单引号，默认添加双引号
     * 属性值统一变成双引号（值中包含双引号的除外）
     * 
     * 匹配属性名称值，
     * 当包含游离block时，如果block包含特殊字符（除字母数字下划线中线以外），则并入前一个属性值中，否则当做单独的属性，名称和值相同，
     * 等号前后都可能包含游离的block作为属性
     * 当有多个属性名时，忽略只取最后一个，
     * */

    public class NodeReader
    {
        public String Node { get; private set; }
        public NodeType NodeType { get; private set; }
        public String NodeName { get; private set; }


        private TokenReader tokenReader = null;
        private Dictionary<String, String> attrs = new Dictionary<string, string>();


        //忽略节点前缀
        private Boolean ignorePrefix = false;

        public NodeReader(String html)
        {
            tokenReader = new TokenReader(html);
        }

        public NodeReader(String html, Boolean ignorePrefix)
        {
            tokenReader = new TokenReader(html);
            this.ignorePrefix = ignorePrefix;
        }

        public Boolean Read()
        {
            Boolean readResult = false;
            attrs.Clear();

            while ((readResult = tokenReader.Read()))
            {
                if (tokenReader.TokenType == TokenType.DocDeclare)
                {
                    Node = tokenReader.Token;
                    NodeType = NodeType.DocDeclare;
                    NodeName = "";
                    break;
                }
                else if (tokenReader.TokenType == TokenType.Comment)
                {
                    Node = tokenReader.Token;
                    NodeType = NodeType.Comment;
                    NodeName = "";
                    break;
                }
                else if (tokenReader.TokenType == TokenType.Whitespace)
                {
                    Node = tokenReader.Token;
                    NodeType = NodeType.Whitespace;
                    NodeName = "";
                    break;
                }
                else if (tokenReader.TokenType == TokenType.Text)
                {
                    Node = tokenReader.Token;
                    NodeType = NodeType.Text;
                    NodeName = "";
                    break;
                }
                else if (tokenReader.TokenType == TokenType.Mark)
                {
                    Node = tokenReader.Token;
                    if (ignorePrefix)//移除前缀
                    {
                        int index = tokenReader.Token.LastIndexOf(':');
                        if (index != -1)
                        {
                            Node = tokenReader.Token.Substring(0, 1) + tokenReader.Token.Substring(index + 1);
                        }
                    }
                    
                    NodeType = NodeType.Node;
                    NodeName = Node.Substring(1);
                }
                else if (tokenReader.TokenType == TokenType.Block)
                {
                    addAttribu(tokenReader.Token);
                    /*
                    if (hasReadEquel)//已经读取到等号，次为潜在的属性值，全部入属性值堆栈
                    {
                        valueStack.Add(tokenReader.Token);
                    }
                    else//还没读取到等号，为潜在的属性名，入属性名堆栈
                    {
                        nameStack.Add(tokenReader.Token);
                    }*/
                }/*
                else if (tokenReader.TokenType == TokenType.Equal)//读取到等号以后表示下一个token将为属性值，为处理属性值被分割成多个token的情况，以后的属性值全部进入堆栈
                {
                    if (!hasReadEquel)//读取到等号以后读取的block全部为潜在的属性值，直到再次读取到等号或节点结束标记时进行属性配对
                    {
                        hasReadEquel = true;
                    }
                    else//进行属性配对
                    {
                        pairAttr(false);

                        nameStack.Clear();
                        if(valueStack.Count>0)
                        {
                            nameStack.Add(valueStack[valueStack.Count - 1]);
                        }
                        valueStack.Clear();
                    }
                }*/
                else if (tokenReader.TokenType == TokenType.MarkEnd)
                {
                    if (attrs.Count>0)//存在属性，把属性整合到节点字符串中
                    {
                        //pairAttr(true);
                        Node += " " + getAttrString();
                    }
                    Node += tokenReader.Token;
                    break;
                }
                else if (tokenReader.TokenType == TokenType.SelfCloseMarkEnd)
                {
                    if (attrs.Count > 0)//存在属性，把属性整合到节点字符串中
                    {
                        //pairAttr(true);
                        Node += " " + getAttrString();
                    }
                    //纠正即是闭合节点又是自结束节点的问题
                    //类似</img />
                    //这里当作自结束节点处理
                    if (Node.StartsWith("</"))
                    {
                        Node = "<" + Node.Substring(2);
                    }
                    Node +=" "+ tokenReader.Token;
                    NodeType = NodeType.SelfCloseNode;
                    break;
                }
                else if (tokenReader.TokenType == TokenType.CloseMark)
                {
                    Node = tokenReader.Token;
                    if (ignorePrefix)//移除前缀
                    {
                        int index = tokenReader.Token.LastIndexOf(':');
                        if (index != -1)
                        {
                            Node = tokenReader.Token.Substring(0, 2) + tokenReader.Token.Substring(index + 1);
                        }
                    }
                    
                    NodeType = NodeType.CloseNode;
                    NodeName = Node.Substring(2);
                }
                else
                {
                    throw new Exception("未知的token类型");
                }
            }


            return readResult;
        }


        private void addAttribu(String block)
        {
            //不包含=，但包含引号，抛弃
            //不包含=，添加=
            Boolean readYinhao = false;
            String text = "";
            String name = "";
            String value = "";
            for(int i=0;i<block.Length;i++)
            {
                Char c = block[i];
                if (c == '=')
                {
                    name = text;
                    value = block.Substring(i + 1);
                    break;
                }
                else if (c == '"' || c == '\'')
                {
                    readYinhao = true;
                }
                else
                {
                    text += c.ToString();
                }
            }
            if(String.IsNullOrEmpty(name)&&!readYinhao)
            {
                name = text;
                value = text;
            }

            //这里强行过滤掉一些符号
            //过滤掉name中不合法的字符
            if(!String.IsNullOrEmpty(name))
            {
                name = HtmlHelper.GetHtmlToken(name);
            }
            //过滤掉<
            value = value.Replace("<","");
            if(!String.IsNullOrEmpty(name)&&!String.IsNullOrEmpty(value))
            {
                removeSameAttr(name);
                attrs.Add(name, addYinhao(value));
            }
        }
        





        

        /// <summary>
        /// 为属性值添加引号，
        /// 如果字符串中包含单引号（开始和末尾的除外），则添加双引号，
        /// 如果字符串中包含双引号（开始和末尾的除外），则添加单引号，
        /// 如果字符串中既包含单引号又包含双引号（开始和末尾的除外），则删除双引号，
        /// 默认添加双引号，（值中包含双引号的除外）
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private String addYinhao(String value)
        {
            if(value.StartsWith("\'")||value.StartsWith("\""))
            {
                value = value.Substring(1);
            }
            if(value.EndsWith("\'")||value.EndsWith("\""))
            {
                value = value.Substring(0, value.Length - 1);
            }

            //单双引号都包含，删除双引号
            if(value.IndexOf('\"')!=-1&&value.IndexOf('\'')!=-1)
            {
                String str = "";
                foreach(var c in value)
                {
                    if(c!='\"')
                    {
                        str += c.ToString();
                    }
                }
                value = "\"" + str + "\"";
            }
            else if (value.IndexOf('\"') != -1)
            {
                value = "'" + value + "'";
            }
            else
            {
                value = "\"" + value + "\"";
            }

            return value;
        }

        /// <summary>
        /// 移除相同的属性
        /// </summary>
        /// <param name="attrName"></param>
        private void removeSameAttr(String attrName)
        {
            List<String> keys = attrs.Keys.ToList();
            foreach (String key in keys)
            {
                if (key == attrName)
                {
                    attrs.Remove(key);
                }
            }
        }


        /// <summary>
        /// 获取节点的所有属性的字符串
        /// </summary>
        /// <returns></returns>
        private String getAttrString()
        {
            String str = "";
            foreach (String key in attrs.Keys)
            {
                String attrName = key;
                //移除属性名前缀
                if (ignorePrefix)
                {
                    int index = key.LastIndexOf(':');
                    if (index != -1)
                    {
                        attrName = key.Substring(index + 1);
                    }
                }
                /* 不能在这里判断
                //属性名称不能以=开头
                if (attrName.StartsWith("="))
                {
                    attrName = attrName.Substring(1);
                }*/
                str += attrName + "=" + attrs[key] + " ";
            }
            if (!String.IsNullOrEmpty(str))
            {
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }

    }
}
