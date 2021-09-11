#nullable enable
namespace EasyContainer.Lib
{
    using System;
    using System.Threading.Tasks;

    public delegate Task AsyncEventHandler(object? sender, EventArgs e);

    public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);
}