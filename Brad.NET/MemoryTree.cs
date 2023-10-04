using System.Runtime.InteropServices;

namespace Brad;

public unsafe class MemoryTree
{
    public GarbageCollector gc { get; set; } // replace with real GC object
    public List<Node> roots { get; set; } = new();

    public MemoryTree(GarbageCollector gc)
    {
        this.gc = gc;
    }

    public Node CreateRoot<T>(T *init, int length, byte isRef = 0) where T: unmanaged
    {
        if(isRef == 1)
        {
            Node n = new Node(null, sizeof(byte *), isRef, this, null);
            n.AppendReference((byte **)init, sizeof(T) * length);

            roots.Add(n);
            return n;
        } else {
            Node n = new Node((byte *)init, sizeof(T) * length, isRef, this, null);
            roots.Add(n);
            return n;
        }
    }

    // structs are of predefined size... inline buffers MUST also have a predefined size
    public Node CreateRefStruct(int byteSize) 
    {
        byte **stackSpace = (byte **)gc.StackAlloc<byte>(sizeof(byte *));
        
        Node n = new Node(null, sizeof(byte *), 1, this, null);
        n.AppendReference(stackSpace, byteSize);

        return n;
    }
}

public unsafe class Node
{
    public MemoryTree owner { get; set; }
    public Node? parent { get; set; }
    public byte *ptr { get; set; }
    public int length { get; set; }
    public List<Node> children { get; set; } = new();
    public byte containsPointer { get; set; }
    public byte mark { get; set; }

    public Node(byte *p, int l, byte isPtr, MemoryTree o, Node? par = null)
    {
        owner = o;
        parent = par;
        ptr = p;
        length = l;
        containsPointer = isPtr;
        mark = 0;
    }

    public Node GetEmptyNode(int length  = 0)
    {
        return new Node(null, length, 0, owner, this);
    }

    public void AppendChild(Node n)
    {
        if(containsPointer == 1) { throw new Exception("The given Node is a pointer and therefore cannot have multiple children!"); }

        byte *nptr = owner.gc.StackAlloc<byte>(length + n.length);
        n.ptr = &nptr[length];

        for(int i = 0; i < length; i++)
        {
            nptr[i] = ptr[i];
        }

        ptr = nptr;
        length = length + n.length;

        children.Add(n);
    }

    public void AppendReference(byte **_base, int refsize)
    {
        if(children.Count > 0) { throw new Exception("The given Node is a pointer and therefore cannot have multiple children!"); }

        ptr = (byte *)_base;
        *_base  = owner.gc.Alloc<byte>(refsize);

        children.Add(new Node(*_base, refsize, 0, owner, this));
    }

    public void AsRefStructAppendReference(int refsize)
    {
        if(parent == null) { throw new Exception("Arbitrary stack cannot contain pointers! This will lead to memory leaks! Use the new StructAlloc object! [Rainbow/Rain users: this should never happen!]"); }

        this.containsPointer = 1;

        byte *nptr = owner.gc.Alloc<byte>(length + sizeof(byte *));
        byte *location = &nptr[length];

        for(int i = 0; i < length; i++)
        {
            nptr[i] = ptr[i];
        }

        ptr = nptr;
        length = length + refsize;

        byte **real = (byte **)location;
        *real = owner.gc.Alloc<byte>(refsize);

        Node newNode = GetEmptyNode();
        newNode.parent = this;
        newNode.ptr = *real;
        newNode.length = refsize;

        children.Add(newNode);
    }
}