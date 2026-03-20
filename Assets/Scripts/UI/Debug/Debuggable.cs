using System.Collections.Generic;

public interface Debuggable
{
    public string GetDebugText();
}

public interface MultipleDebuggable : Debuggable
{
    // this is for debuggables that have multiple lines of info, like the entities debug
    // it will return a list of strings, each string being a line of info
    public List<string> GetDebugLines();
}

public interface Debugger
{
    public void SetDebuggable(Debuggable debuggable);
}