using System.Diagnostics;
using SixLabors.Fonts;
using Spectre.Console;
using TRPEFontGen;

namespace FontGeneratorCLI;

internal static class Program
{
    public static void Main()
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]程序运行过程中发生致命错误:[ /]");
            AnsiConsole.WriteException(ex);
        }
        Console.ReadLine();
    }

    private static void Run()
    {
        AnsiConsole.Write(
            new FigletText("Font Generator CLI")
                .Color(Color.Orange1));

        var fontSourceMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]请选择字体来源类型:[/]")
                .AddChoices("系统安装的字体", "本地字体文件 (.ttf/.otf)"));

        Font? font = null;
        
        while (font == null)
        {
            try
            {
                if (fontSourceMode == "系统安装的字体")
                {
                    var fontName = AnsiConsole.Ask<string>("请输入[green]系统字体名称[/] (如 Microsoft YaHei):", "Microsoft YaHei");
                    var size = AnsiConsole.Ask("请输入[green]字体大小[/]:", 15f);
                    font = SystemFonts.CreateFont(fontName, size, FontStyle.Regular);
                }
                else
                {
                    var fontPath = AnsiConsole.Ask<string>("请输入[green]字体文件完整路径[/]:");
                    if (!File.Exists(fontPath))
                    {
                        AnsiConsole.MarkupLine($"[red]错误: 文件不存在 -> {fontPath}[/]");
                        continue;
                    }
                    var size = AnsiConsole.Ask("请输入[green]字体大小[/]:", 15f);
                    var collection = new FontCollection();
                    var family = collection.Add(fontPath);
                    font = family.CreateFont(size, FontStyle.Regular);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]加载字体失败: {ex.Message}[/]");
                if (!AnsiConsole.Confirm("是否重试?")) return;
            }
        }
        
        var charOptions = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("[yellow]请选择要包含的字符集 (空格选中/取消):[/]")
                .Required()
                .AddChoices("基础 ASCII (0x0020-0x007E)", "常用中文字符 (0x4E00-0x9FA5)", "全角符号 (0xFF01-0xFF5E)", "中文常用3500字", "常用1000字", "从外部 TXT 读取", "从字体导出 TXT 读取", "自定义 Hex 范围", "仅导出字符集到 TXT"));

        var chars = new HashSet<char>();

        if (charOptions.Contains("基础 ASCII (0x0020-0x007E)")) CharSet.AddRange(chars, CharSet.BasicASCIIRange);
        if (charOptions.Contains("常用中文字符 (0x4E00-0x9FA5)")) CharSet.AddRange(chars, CharSet.CJKUnifiedRange);
        if (charOptions.Contains("全角符号 (0xFF01-0xFF5E)")) CharSet.AddRange(chars, CharSet.FullWidthRange);
        if (charOptions.Contains("中文常用3500字")) CharSet.AddString(chars, CharSet.CommonUseChinese);
        if (charOptions.Contains("常用1000字")) CharSet.AddString(chars, CharSet.CommonUse1000);

        if (charOptions.Contains("从外部 TXT 读取"))
        {
            while (true)
            {
                var txtPath = AnsiConsole.Ask<string>("请输入 [blue]TXT 文件路径[/] (输入 [yellow]q[/] 结束读取):", "q");
        
                if (txtPath.Trim().Equals("q", StringComparison.CurrentCultureIgnoreCase)) break;

                try
                {
                    if (File.Exists(txtPath))
                    {
                        var text = File.ReadAllText(txtPath);
                        var countBefore = chars.Count;
                        CharSet.AddString(chars, text);
                
                        AnsiConsole.MarkupLine($"[green]成功导入![/] 新增了 [yellow]{chars.Count - countBefore}[/] 个字符。");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]错误: 文件不存在 -> {txtPath}[/]");
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]读取失败: {ex.Message}[/]");
                }
            }
        }

        if (charOptions.Contains("从字体导出 TXT 读取"))
        {
            while (true)
            {
                var fontTxtPath = AnsiConsole.Ask<string>("请输入 [blue]字体导出 TXT 文件路径[/] (输入 [yellow]q[/] 结束读取):", "q");
        
                if (fontTxtPath.Trim().Equals("q", StringComparison.CurrentCultureIgnoreCase)) break;

                try
                {
                    if (File.Exists(fontTxtPath))
                    {
                        var readChars = TRPEFontGen.FontFile.ReadCharsFromMetaData(fontTxtPath);
                        if (readChars != null && readChars.Length > 0)
                        {
                            var countBefore = chars.Count;
                            CharSet.AddString(chars, new string(readChars));
                            AnsiConsole.MarkupLine($"[green]成功导入![/] 从字体元数据中读取了 [yellow]{chars.Count - countBefore}[/] 个字符。");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]错误: 无法读取字体元数据文件或文件格式不正确。[/]");
                        }
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[red]错误: 文件不存在 -> {fontTxtPath}[/]");
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]读取失败: {ex.Message}[/]");
                }
            }
        }

        if (charOptions.Contains("自定义 Hex 范围"))
        {
            while (true)
            {
                var rangeStr = AnsiConsole.Ask<string>("请输入 Hex 范围 (如 0x4E00-0x4E10, 输入 [yellow]q[/] 结束):", "q");
        
                if (rangeStr.Trim().Equals("q", StringComparison.CurrentCultureIgnoreCase)) break;

                try
                {
                    var parts = rangeStr.Split('-');
                    if (parts.Length == 2)
                    {
                        var start = Convert.ToInt32(parts[0].Trim(), 16);
                        var end = Convert.ToInt32(parts[1].Trim(), 16);
                
                        var countBefore = chars.Count;
                        CharSet.AddRange(chars, start, end);
                        AnsiConsole.MarkupLine($"[green]已添加范围![/] 该区间内新增了 [yellow]{chars.Count - countBefore}[/] 个唯一字符。");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]格式错误。正确格式示例: 0x0021-0x007E[/]");
                    }
                }
                catch
                {
                    AnsiConsole.MarkupLine("[red]解析失败，请确保输入的是有效的 16 进制数。[/]");
                }
            }
        }

        if (charOptions.Contains("仅导出字符集到 TXT"))
        {
            if (chars.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]错误: 未选择或未识别到任何字符，程序退出。[/]");
                return;
            }
            var exportPath = AnsiConsole.Ask<string>("请输入 [blue]导出 TXT 文件路径[/]:", "chars_export.txt");
            var sb = new System.Text.StringBuilder();
            foreach (var c in chars)
            {
                sb.Append(c);
            }
            File.WriteAllText(exportPath, sb.ToString());
            AnsiConsole.MarkupLine($"[green]已导出 [yellow]{chars.Count}[/] 个字符到: {exportPath}[/]");
            return;
        }

        if (chars.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]错误: 未选择或未识别到任何字符，程序退出。[/]");
            return;
        }
        
        
        var outputName = AnsiConsole.Ask<string>("请输入[green]输出文件夹/字体名[/]:", "Death_Text");
        
        AnsiConsole.Write(new Rule("[yellow]排版参数微调[/]").RuleStyle("grey").LeftJustified());
        
        var spacing = AnsiConsole.Ask("请输入[green]字符额外间距 (spacing)[/]:", 0f);
        var minusKerning = AnsiConsole.Ask("请输入[green]负字间距 (minusKerning)[/]:", 5);
        var lineSpacing = AnsiConsole.Ask("请输入[green]行间距 (lineSpacing)[/]:", 24);
        var latinMargin = AnsiConsole.Ask("请输入[green]拉丁字母补偿 (latinMargin)[/]:", 0.5f);
        var defaultCharInput = AnsiConsole.Ask<string>("请输入[green]默认缺省字符 (defaultChar)[/]:", "*");
        var defaultChar = string.IsNullOrEmpty(defaultCharInput) ? '*' : defaultCharInput[0];

        AnsiConsole.WriteLine();
        var sw = Stopwatch.StartNew();

        try
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[bold yellow]正在生成贴图中...[/]", _ =>
                {
                    var fontFile = Generator.Generate(
                        font, 
                        chars.ToArray(), 
                        outputName,
                        spacing: spacing,
                        minusKerning: minusKerning,
                        lineSpacing: lineSpacing,
                        latinMargin: latinMargin,
                        defaultChar: defaultChar
                    );
                    var savePath = Path.Combine("font", outputName);
                    
                    if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);

                    fontFile.SaveMetaData(savePath);
                    fontFile.SaveTextures(savePath);
                });

            sw.Stop();

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("项目");
            table.AddColumn("结果");
            table.AddRow("字符总数", $"[blue]{chars.Count}[/]");
            table.AddRow("耗时", $"[yellow]{sw.Elapsed.TotalSeconds:F3}s[/]");
            table.AddRow("保存位置", $"[link]font/{outputName}[/]");

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine("[bold green]✨ 字体生成任务已完成！[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]生成过程中出现错误，可能是由于 IO 权限或内存不足。[/]");
            AnsiConsole.WriteException(ex);
        }
    }
}