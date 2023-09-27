using System.Runtime.InteropServices;

namespace Brad;

public unsafe struct BradStack
{
    public byte *stack { get; set; }
    public static int GlobalPos { get; set; } = 0;
    public int pos => GlobalPos;
    public int size { get; set; }

    List<KeyValuePair<int, int>> segments { get; set; }

    public BradStack(byte *ptr, int size)
    {
        stack = ptr;
        this.size = size;
        segments = new();
    }

    public byte * Peek(int index)
    {
        //Span<byte> ret = new Span<byte>(&stack[segments[index].Value], segments[index].Value);        should we switch to spans?
        return &stack[segments[index].Value];
    }

    public byte * StackAlloc(int size)
    {
        byte *current = &stack[pos];
        segments.Add(new KeyValuePair<int, int>(pos, size));

        return current;
    }

    public bool Contains(byte *_ref)
    {
        for(int i = 0; i < segments.Count; i++)
        {
            if(Peek(i) == _ref)
            {
                return true;
            }
        }

        return false;
    }
}