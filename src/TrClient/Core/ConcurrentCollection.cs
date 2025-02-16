using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrClient.Core
{
    class ConcurrentCollection<T> : IEnumerable<T>
    {
        private readonly ConcurrentDictionary<int, T> items = new();
        private T[] cachedItems = [];
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < cachedItems.Length; i++) {
                yield return cachedItems[i];
            }
        }
        public ReadOnlySpan<T> GetSockets() {
            return cachedItems;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public bool TryAdd(int id, T socket) {
            if (items.TryAdd(id, socket)) {
                cachedItems = [.. items.Values];
                return true;
            }
            return false;
        }
        public bool TryRemove(int id) {
            if (items.TryRemove(id, out _)) {
                cachedItems = [.. items.Values];
                return true;
            }
            return false;
        }
    }
}
