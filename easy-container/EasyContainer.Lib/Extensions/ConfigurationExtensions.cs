// namespace EasyContainer.Lib.Extensions
// {
//     using System;
//     using System.Threading.Tasks;
//     using Microsoft.Extensions.Configuration;
//     using Microsoft.Extensions.Primitives;
//
//     public static class ConfigurationExtensions
//     {
//         public static void OnChange(this IConfiguration config, Func<Task> onChange, Func<Exception, Task> onError,
//             TimeSpan? pollInterval = null)
//         {
//             _onChange += onChange;
//             _onError += onError;
//             // IConfiguration's change detection is based on FileSystemWatcher, which will fire multiple change
//             // events for each change - Microsoft's code is buggy in that it doesn't bother to debounce/dedupe
//             // https://github.com/aspnet/AspNetCore/issues/2542
//             var debouncer = new Debouncer(pollInterval);
//
//             ChangeToken.OnChange<object>(config.GetReloadToken,
//                 _ => debouncer.DebouceAsync(), null);
//         }
//     }
// }

