# TRPEFontGen

## 环境要求

* 运行环境: .NET 10.0
* 操作系统: Windows

## 快速使用

1. 配置环境：确保本地已安装目标字体（默认：微软雅黑）。
2. 自定义配置：根据需求修改 `GenFont()` 中的参数：
   * 更换字体：修改 `new Font("字体名称", 大小, 样式)`。
   * 更改输出名：修改 `Writer.Write("文件名", ...)`。

3. 运行程序：执行后，将在输出目录生成对应的字体文件。

## 代码示例

```csharp
// 设置字体：名称、大小、样式
var font = new Font("微软雅黑", 15, FontStyle.Regular);

// 执行生成
var fontFile = Generator.Generate(font, chars.ToArray());

// 写入文件
Writer.Write("Death_Text", fontFile);

```