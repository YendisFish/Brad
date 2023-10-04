using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Brad;

class Program {
    public static void Main(string[] args) {
        unsafe {
            byte *bs = stackalloc byte[4096];

            BradStack stack = new BradStack(bs, 4096);
            GarbageCollector gc = new GarbageCollector(stack);

            while(true) {
                byte *mem = gc.StackAlloc<IntPtr>(1);
                Node stackNode = gc.tree.CreateRoot(mem, sizeof(int), 1);

                Console.WriteLine(stackNode.children.Count);
                byte *ptr = *(byte **)stackNode.ptr;

                Console.WriteLine(*(int*) ptr);

                gc.Collect();
            }
        }
    }
}
