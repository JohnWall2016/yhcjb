#! "netcoreapp2.1"

using System.Threading;

/// <summary>
///   参保登记审核
/// </summary>
void Cbdjsh()
{
    Console.WriteLine("Cbdjsh");
    Thread.Sleep(5000);
}

/// <summary>
///   参保登记审核处理循环
/// </summary>
async Task CbdjshLoop(CancellationToken token, TimeSpan delay)
{
    try
    {
        while (true)
        {
            Console.WriteLine("开始批量审核参保登记数据");
            Cbdjsh();
            Console.WriteLine("结束批量审核参保登记数据");
            if (!token.IsCancellationRequested)
            {
                if (delay.TotalMinutes < 1)
                    Console.WriteLine($"延迟{delay.TotalSeconds}秒...");
                else
                    Console.WriteLine($"延迟{delay.TotalMinutes}分钟...");
            }
            await Task.Delay(delay, token);
        }
    }
    catch (OperationCanceledException ex)
    {
        Console.WriteLine("参保登记审核退出");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
}

/// <summary>
///   开始批量审核参保登记数据
/// </summary>
void startCbdjshLoop(TimeSpan delay)
{
    var source = new CancellationTokenSource();

    // Cancel just during delay not bussiness operation.
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        source.Cancel();
    };

    var task = CbdjshLoop(source.Token, delay);
    task.GetAwaiter().GetResult(); // block;
}

startCbdjshLoop(TimeSpan.FromSeconds(5));
