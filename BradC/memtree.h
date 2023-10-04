#include<stdlib.h>

#ifndef MEMTREE_H
#define MEMTREE_H


//typedef struct Node {};

typedef struct Node
{
    void *ptr;
    int bytesize;
    Node *parent;
    Node **children;
    unsigned char marked;
} Node;

typedef struct
{
    Node **roots;
    int rootsLen;
} MemTree;

#endif

// Node region

void IncreaseListSize(Node **ptr, int currentSize, int amount) {
    Node** n = (Node **)malloc(sizeof(Node *) * (currentSize + amount));

    for(int i = 0; i < currentSize; i++) {
        n[i] = ptr[i];
    }

    ptr = n;
}

//

// MemTree region

void AddRoot(MemTree *ptr) {
    IncreaseListSize(ptr->roots, ptr->rootsLen, 1);
    ptr->rootsLen++;
}

//