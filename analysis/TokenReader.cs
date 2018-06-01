using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace magic.html.analysis
{
    /* 读出所有标识符
     * 读出的文本部分如果是空白则标记为whitespace
     * 当读取到属性时，引号中的属性中的特殊字符被忽略
     * 文本中的特殊字符不被忽略，除非是脚本中的文本
     * 读取脚本中的文本时，直到读取到结束标记，否则一直读取
     * 
     * 特别注意：关于属性中引号的处理，引号将成对的读取，读取到结束引号时引号处理完成。
     * 直到读取到空格或明显的标记符，否则block将一直读取，除非有成对的引号分割
     * */

    public class TokenReader
    {
        public String Token { get; private set; }
        public TokenType TokenType { get; private set; }


        private String source = null;
        private int readIndex = 0;
        private String text = "";
        
        /// <summary>
        /// 开始的引号，没读取到时为0
        /// </summary>
        private YinHaoType yinhaoStart = 0;

        

        /// <summary>
        /// 是否为脚本节点
        /// </summary>
        private Boolean scriptNode = false;

        //一个临时变量
        private int index = 0;

        public TokenReader(String html)
        {
            this.source = html;
        }

        public Boolean Read()
        {
            if (readIndex >= source.Length)//已经读取到结尾
            {
                return false;
            }

            //文本内容可以包含特殊字符，所以先处理
            //检查上一个token是否为标记结尾，如果是读取文本内容
            if (Token != null && (Token == "/>" || Token == ">"||TokenType==TokenType.DocDeclare||TokenType==TokenType.Comment))
            {
                for (; readIndex < source.Length; readIndex++)
                {
                    if (scriptNode)//读取脚本的文本，一直读取到结束节点</script>
                    {
                        if (source[readIndex] == '<'&&nextIsScriptEndNode(readIndex))
                        {
                            break;
                        }
                        text += source[readIndex].ToString();
                    }
                    else
                    {
                        if (source[readIndex] == '<')
                        {
                            break;
                        }
                        text += source[readIndex].ToString();
                    }
                }
            }


            if (text.Length > 0 && HtmlHelper.IsWhitespace(text))
            {
                Token = text;
                TokenType = analysis.TokenType.Whitespace;
            }
            else if (text.Length > 0)
            {
                Token = text;
                TokenType = analysis.TokenType.Text;
            }
            else//text为空，继续读取
            {
                //默认类型为block
                TokenType = analysis.TokenType.Block;

                Boolean end = false;
                for (; readIndex < source.Length; readIndex++)
                {
                    switch (source[readIndex])
                    {
                        case '\r':
                        case '\n':
                        case '\t':
                        case ' ':
                            if (text == "<" || text == "</" || text == "/")//< nodename,</ nodename,/ >过滤这3种情况的空格
                            {
                                continue;
                            }
                            else if (String.IsNullOrEmpty(text))//还没开始读取
                            {
                                continue;
                            }
                            else if (yinhaoStart!=0)//已经读取到引号，继续读取
                            {
                                text += source[readIndex].ToString();
                                continue;
                            }
                            else//停止读取
                            {
                                //判断是否为脚本开始节点，注意脚本结束节点的情况
                                //这里区别脚本和文本是因为结束标记的不同，（文本以<结束，脚本以</script>结束）
                                scriptNode = isScriptNode();
                                end = true;
                            }
                            break;
                        case '<'://可能结束读取的标记
                            if (!String.IsNullOrEmpty(text)&&yinhaoStart==0)//已经读取了一些文本并且没有读取引号，结束读取
                            {
                                end = true;
                            }
                            else
                            {
                                text += "<";
                            }
                            break;/* 等号不在单独读取
                        case '='://结束读取的标记，除非包括在引号中
                            if (String.IsNullOrEmpty(text))
                            {
                                //还没开始读取，等号作为单独的标记被读取
                                text = "=";
                                TokenType = analysis.TokenType.Equal;
                                readIndex++;
                                end = true;
                            }
                            else if (yinhaoStart)//在引号中当作一般字符读取
                            {
                                text += "=";
                            }
                            else
                            {
                                end = true;
                            }
                            break;*/
                        case '\'':
                            text += source[readIndex].ToString();
                            //如果已经读取了一个开始引号，并且开始引号是单引号，结束读取
                            if(yinhaoStart==YinHaoType.Single)
                            {
                                readIndex++;//下次不再读取该字符
                                end = true;
                            }
                            else if (yinhaoStart == 0)
                            {
                                yinhaoStart = YinHaoType.Single;
                            }
                     
                            break;
                        case '\"':
                            text += source[readIndex].ToString();
                            //如果已经读取了一个开始引号，并且开始引号是双引号，结束读取
                            if (yinhaoStart == YinHaoType.Double)
                            {
                                readIndex++;
                                end = true;
                            }
                            else if (yinhaoStart == 0)
                            {
                                yinhaoStart = YinHaoType.Double;
                            }
                           
                            break;
                        case '/'://可能的3种情况”</nodename“，”/>“，”Block文本“。
                            if (text.StartsWith("<") && text.Length > 1)//这里表示应该结束读取，已经读取的字符为mark
                            {
                                end = true;
                            }
                            else if (!String.IsNullOrEmpty(text) && yinhaoStart==0 &&
                                nextNotWhitespaceChar(readIndex + 1) == '>')//不是第一个字符并且不在引号中并且读到了"/>"，结束读取
                            {
                                end = true;
                            }
                            else
                            {
                                text += "/";
                            }

                            break;
                        case '>'://token读取可能结束
                            if (text == "/")//结束读取
                            {
                                text += ">";
                                readIndex++;
                                TokenType = analysis.TokenType.SelfCloseMarkEnd;
                                end = true;
                            }
                            else if (String.IsNullOrEmpty(text))//结束读取
                            {
                                text = ">";
                                readIndex++;
                                TokenType = analysis.TokenType.MarkEnd;
                                end = true;
                            }
                            else if(yinhaoStart!=0)//已经读取到引号，不管读取到什么字符，继续读取
                            {
                                text += ">";
                            }
                            else//读取token结束，这次读取的为mark
                            {
                                //判断是否为脚本，不要误判为脚本节点的结束节点
                                //这里区别脚本和文本是因为结束标记的不同，（文本以<结束，脚本以</script>结束）
                                scriptNode = isScriptNode();
                                end = true;
                            }
                            break;
                        case '!':
                            if (text == "<")//为文档声明或注释
                            {
                                //向后读2个字符
                                text += "!" + source[++readIndex].ToString() + source[++readIndex].ToString();
                                readIndex++;
                                if (text=="<!--")//为注释
                                {
                                    index = source.IndexOf("-->",readIndex);
                                    text += source.Substring(readIndex, index - readIndex + 3);
                                    readIndex = index + 3;
                                    TokenType = analysis.TokenType.Comment;
                                }
                                else//为文档声明
                                {
                                    index = source.IndexOf('>', readIndex);
                                    text += source.Substring(readIndex, index - readIndex + 1);
                                    readIndex = index + 1;
                                    TokenType = analysis.TokenType.DocDeclare;
                                }
                                end = true;//停止读取
                            }
                            else
                            {
                                text += "!";
                            }
                            break;
                        default:
                            text += source[readIndex].ToString();
                            break;
                    }

                    if (end)
                    {
                        break;
                    }
                }


                if (text.StartsWith("</"))
                {
                    TokenType = analysis.TokenType.CloseMark;
                }
                else if (text.StartsWith("<!"))
                {
                }
                else if (text.StartsWith("<"))
                {
                    TokenType = analysis.TokenType.Mark;
                }
                Token = text;
                

            }
            text = "";
            yinhaoStart = 0;

            return true;
        }

        /// <summary>
        /// 获取下一个不是空白的字符
        /// </summary>
        /// <param name="index">开始判断的索引</param>
        /// <returns></returns>
        private Char nextNotWhitespaceChar(int index)
        {
            Char c=default(Char);
            while(index<source.Length)
            {
                if(!HtmlHelper.IsWhitespace(source[index]))
                {
                    c = source[index];
                    break;
                }
                index++;
            }
            return c;
        }
        /// <summary>
        /// 判断后续字符串是不是脚本结束节点"&lt;/script>
        /// 全局读取索引不变
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Boolean nextIsScriptEndNode(int index)
        {
            Boolean flag = false;
            //读取步数，</script>
            int step = 1;
            while (index < source.Length)
            {
                if (HtmlHelper.IsWhitespace(source[index]))
                {
                    index++;
                    continue;
                }

                if (step == 1 && source[index] == '<')
                {
                    step++;
                }
                else if (step == 2 && source[index] == '/')
                {
                    step++;
                }
                else if (step == 3 && source[index].ToString().ToLower() == "s")
                {
                    step++;
                }
                else if (step == 4 && source[index].ToString().ToLower() == "c")
                {
                    step++;
                }
                else if (step == 5 && source[index].ToString().ToLower() == "r")
                {
                    step++;
                }
                else if (step == 6 && source[index].ToString().ToLower() == "i")
                {
                    step++;
                }
                else if (step == 7 && source[index].ToString().ToLower() == "p")
                {
                    step++;
                }
                else if (step == 8 && source[index].ToString().ToLower() == "t") 
                {
                    step++;
                }
                else if (step == 9 && source[index].ToString().ToLower() == ">")
                {
                    flag = true;
                    break;
                }
                else
                {
                    break;
                }

                index++;
            }

            return flag;
        }

        /// <summary>
        /// 判断当前（text）读取的是否为脚本开始节点
        /// 没有判断空格是因为空格已经过滤掉
        /// </summary>
        /// <returns></returns>
        private Boolean isScriptNode()
        {
            if (!text.StartsWith("</") && text.StartsWith("<"))//读取的一定是开始节点
            {
                //使用EndsWith时会把noscript节点当成script节点
                //if (text.Trim().ToLower().EndsWith("script"))
                if (text.Trim().ToLower().Substring(1)=="script")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 表示引号类型
        /// </summary>
        public enum YinHaoType
        {
            Single=1,
            Double=2
        }

    }
}
