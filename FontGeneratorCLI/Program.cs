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
                .AddChoices("基础 ASCII (0x0020-0x007E)", "常用中文字符 (0x4E00-0x9FA5)", "全角符号 (0xFF01-0xFF5E)", "从外部 TXT 读取", "自定义 Hex 范围"));

        var chars = new HashSet<char>();

        if (charOptions.Contains("基础 ASCII (0x0020-0x007E)")) AddRange(chars, 0x0020, 0x007E);
        if (charOptions.Contains("常用中文字符 (0x4E00-0x9FA5)")) AddRange(chars, 0x4E00, 0x9FA5);
        if (charOptions.Contains("全角符号 (0xFF01-0xFF5E)")) AddRange(chars, 0xFF01, 0xFF5E);

        if (charOptions.Contains("从外部 TXT 读取"))
        {
            var txtPath = AnsiConsole.Ask<string>("请输入 [blue]TXT 文件路径[/]:");
            try
            {
                if (File.Exists(txtPath))
                {
                    var text = File.ReadAllText(txtPath);
                    foreach (var c in text.Where(c => !char.IsControl(c))) chars.Add(c);
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]警告: TXT 文件不存在，已跳过。[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]读取 TXT 失败: {ex.Message}[/]");
            }
        }

        if (charOptions.Contains("自定义 Hex 范围"))
        {
            var rangeStr = AnsiConsole.Ask<string>("请输入范围 (格式如 0x4E00-0x4E10):");
            try
            {
                var parts = rangeStr.Split('-');
                if (parts.Length == 2)
                    AddRange(chars, Convert.ToInt32(parts[0].Trim(), 16), Convert.ToInt32(parts[1].Trim(), 16));
            }
            catch
            {
                AnsiConsole.MarkupLine("[red]Hex 格式输入错误，示例: 0x0021-0x007E[/]");
            }
        }

        if (chars.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]错误: 未选择或未识别到任何字符，程序退出。[/]");
            return;
        }
        
        var outputName = AnsiConsole.Ask<string>("请输入[green]输出文件夹/字体名[/]:", "Death_Text");

        AnsiConsole.WriteLine();
        var sw = Stopwatch.StartNew();

        try
        {
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("[bold yellow]正在生成贴图中...[/]", _ =>
                {
                    var fontFile = Generator.Generate(font, chars.ToArray(), outputName);
                    var savePath = Path.Combine("font", outputName);

                    // 确保目录存在
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

    private static void AddRange(HashSet<char> chars, int start, int end)
    {
        var actualStart = Math.Max(0, start);
        var actualEnd = Math.Min(char.MaxValue, end);
        
        for (var i = actualStart; i <= actualEnd; i++) chars.Add((char)i);
    }
}