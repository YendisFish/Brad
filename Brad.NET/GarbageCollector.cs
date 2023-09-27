using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Brad;

public unsafe class GarbageCollector
{
    public MemoryTree tree { get; set; }
    public List<IntPtr> allocated { get; set; } = new();
    public BradStack stack { get; set; }

    public GarbageCollector(BradStack stack)
    {
        this.stack = stack;
        tree = new(this);
    }

    public T * Alloc<T>(int length) where T: unmanaged
    {
        IntPtr iptr = Marshal.AllocHGlobal(sizeof(T) * length);
        allocated.Add(iptr);

        return (T *)iptr;
    }

    public byte * StackAlloc<T>(int size) where T: unmanaged
    {
        byte * mem = stack.StackAlloc(sizeof(T) * size);
        tree.CreateRoot((T *)mem, size);

        return mem;
    }

    public void Collect()
    {
        Mark(); Sweep();
    }

    public void Mark()
    {
        for(int i = 0; i < tree.roots.Count; i++)
        {
            if(stack.Contains(tree.roots[i].ptr))
            {
                tree.roots[i].mark = 1;
            } else {
                tree.roots.Remove(tree.roots[i]);   // we can just remove this beacuse it means its already been popped off the stack
            }

            if(tree.roots[i].containsPointer == 1)
            {
                RecursiveMark(tree.roots[i]);
            }
        }
    }

    public void RecursiveMark(Node parent)
    {
        if(!(parent.containsPointer == 1)) { throw new Exception("Tried to dereference non pointer type! [This should NEVER happen]"); }

        for(int i = 0; i < parent.children.Count; i++)
        {
            parent.children[i].mark = 1;
            if(parent.children[i].containsPointer == 1)
            {
                RecursiveMark(parent.children[i]);
            }
        }
    }

    public void Sweep()
    {
        for(int i = 0; i < tree.roots.Count; i++)
        {
            RecursiveSweep(tree.roots[i]);
            tree.roots[i].mark = 0;
        }
    }

    public void RecursiveSweep(Node parent)
    {
        if(parent.mark == 0)
        {
            Marshal.FreeHGlobal((IntPtr)parent.ptr);
        }

        for(int i = 0; i < parent.children.Count; i++)
        {
            if(parent.children[i].mark == 1)
            {
                if(parent.children[i].containsPointer == 1)
                {
                    RecursiveSweep(parent.children[i]);
                }

                parent.children[i].mark = 0;
            } else {
                Marshal.FreeHGlobal((IntPtr)parent.children[i].ptr);
                parent.children.Remove(parent.children[i]);
            }
        }
    }
}