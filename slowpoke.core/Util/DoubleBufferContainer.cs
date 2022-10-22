namespace slowpoke.core.Util;

public class DoubleBufferContainer<T> where T: new()
{
    public T[] Buffers { get; } = new T[2] { new(), new() };

    public T Current => UseFrontBuffer ? FrontBuffer : BackBuffer;

    public bool UseFrontBuffer { get; private set; }
    
    public T FrontBuffer => Buffers[0];

    public T BackBuffer => Buffers[1];


    public void Swap()
    {
        UseFrontBuffer = !UseFrontBuffer;
    }
}