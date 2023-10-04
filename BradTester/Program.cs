using Brad;

unsafe
{
    byte *bs = stackalloc byte[4096];
    
    BradStack stack = new BradStack(bs, 4096);
    GarbageCollector gc = new GarbageCollector(stack);
    
    while(true)
    {
        Console.WriteLine("got to start of loop");
        
        byte *mem = gc.StackAlloc<IntPtr>(1);
        Node stackNode = gc.tree.CreateRoot(mem, sizeof(int), 1);
        
        Console.WriteLine("got to root alloc");
        
        Console.WriteLine(stackNode.children.Count);
        byte *ptr = *(byte **)stackNode.ptr;
        
        Console.WriteLine("ptr edit");
        
        Console.WriteLine(*(int *)ptr);
        
        gc.Collect();
        Console.WriteLine("got to end of func");
    }
}