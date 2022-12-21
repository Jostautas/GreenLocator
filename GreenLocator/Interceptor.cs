using System;

using CodeCop;
using CodeCop.Core;
using CodeCop.Core.Contracts;

namespace GreenLocator;

public class Interceptor : ICopIntercept
{
    public void OnBeforeExecute(InterceptionContext context)
    {
        string filePath = @"C:\Error.txt";

        using StreamWriter writer = new(filePath, true);
        writer.WriteLine("-----------------------------------------------------------------------------");
        writer.WriteLine("Date : " + DateTimeOffset.UtcNow.ToString());
    }

    public void OnAfterExecute(InterceptionContext context)
    {

    }

}
