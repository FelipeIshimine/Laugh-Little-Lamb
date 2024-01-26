using System;
using DataStructures.FibonacciHeap;

namespace DataStructures.PriorityQueue
{
    public class FibonacciPriorityQueue<TElement, TPriority> : IPriorityQueue<TElement, TPriority>
        where TPriority : IComparable<TPriority>
    {
        private readonly FibonacciHeap<TElement, TPriority> _heap;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="minPriority">Minimum value of the priority - to be used for comparing.</param>
        public FibonacciPriorityQueue(TPriority minPriority)
        {
            _heap = new FibonacciHeap<TElement, TPriority>(minPriority);
        }

        public void Insert(TElement item, TPriority priority) =>  _heap.Insert(new FibonacciHeapNode<TElement, TPriority>(item, priority));

        public TElement Top() => _heap.Min().Data;

        public TElement Pop() => _heap.RemoveMin().Data;
    }
}


