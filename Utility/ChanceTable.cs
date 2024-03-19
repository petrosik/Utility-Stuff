namespace Petrosik
{
    namespace Utility
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Linq;
        using Petrosik.Enums;

        [Serializable]
        public class ChanceTable<T> : IEnumerable<ChancePart<T>>, IEnumerator<ChancePart<T>>
        {
            private List<ChancePart<T>> Table = new();
            public int Count => Table.Count;
            /// <summary>
            /// Average rarity of the table
            /// </summary>
            public Rarity TableAvrgRarity { get; private set; }
            /// <summary>
            /// Count of each rarity inside the table
            /// </summary>
            public Dictionary<Rarity, int> RaritysCount { get; private set; }

            public ChancePart<T> Current => Table[CurrentIndex];
            private int CurrentIndex = -1;
            object IEnumerator.Current => Current;

            internal ChanceTable()
            {
                InitRarsCount();
            }
            internal ChanceTable(Rarity Rarity, T Item)
            {
                InitRarsCount();
                Add(Rarity, Item);
            }

            /// <summary>
            /// Returns random item from the table based on its weighted value
            /// </summary>
            /// <param name="UseUnityRandom"></param>
            /// <returns></returns>
            public T GetItem(bool UseUnityRandom = false, bool RemoveAfterPull = false)
            {
                T result = (T)(object)null;
                if (Count == 1)
                {
                    return Table[0].Object;
                }
                else if (Count == 0)
                {
                    Console.WriteLine("Trying to get new random item while the table is empty");
                    return result;
                }

                float rnum = 0;
                if (UseUnityRandom)
                {
                    rnum = UnityEngine.Random.Range(1f, 100f);
                }
                else
                {
                    System.Random r = new();
                    rnum = (float)r.Next(1, 100) + (float)r.NextDouble();
                }

                var sorted = Table.OrderBy(num => Math.Abs(num.Chance - rnum));
                if (sorted.ElementAt(0).Chance == sorted.ElementAt(1).Chance)
                {
                    var multiple = sorted.Where(p => sorted.First().Chance == p.Chance);
                    if (UseUnityRandom)
                    {
                        result = multiple.ElementAt(UnityEngine.Random.Range(0, multiple.Count())).Object;
                    }
                    else
                    {
                        System.Random r = new();
                        result = multiple.ElementAt(r.Next(0, multiple.Count())).Object;
                    }
                }
                else
                {
                    result = sorted.First().Object;
                }
                if (RemoveAfterPull)
                    Table.RemoveAt(Table.FindIndex(t => t.Object.Equals(result)));
                return result;
            }
            /// <summary>
            /// Returns random item from the table that is insinde the range based on its weighted value
            /// </summary>
            /// <param name="UseUnityRandom"></param>
            /// <returns></returns>
            public T GetItemWithRarity(List<Rarity> Range, bool UseUnityRandom = false, bool RemoveAfterPull = false)
            {
                T result = (T)(object)null;
                if (Count == 1)
                {
                    return Table[0].Object;
                }
                else if (Count == 0)
                {
                    Console.WriteLine("Trying to get new random item while the table is empty");
                    return result;
                }

                float rnum;
                if (UseUnityRandom)
                {
                    rnum = UnityEngine.Random.Range(1f, 100f);
                }
                else
                {
                    System.Random r = new();
                    rnum = (float)r.Next(1, 100) + (float)r.NextDouble();
                }

                var sorted = Table.Where(it => Range.Contains(it.Rarity)).OrderBy(num => Math.Abs(num.Chance - rnum));
                if (sorted.ElementAt(0).Chance == sorted.ElementAt(1).Chance)
                {
                    var multiple = sorted.Where(p => sorted.First().Chance == p.Chance);
                    if (UseUnityRandom)
                    {
                        result = multiple.ElementAt(UnityEngine.Random.Range(0, multiple.Count())).Object;
                    }
                    else
                    {
                        System.Random r = new();
                        result = multiple.ElementAt(r.Next(0, multiple.Count())).Object;
                    }
                }
                else
                {
                    result = sorted.First().Object;
                }
                if (RemoveAfterPull)
                    Table.RemoveAt(Table.FindIndex(t => t.Object.Equals(result)));
                return result;
            }
            public bool Exists(T item)
            {
                return Table.Exists(p => p.Object.Equals(item));
            }
            /// <summary>
            /// Clears the table
            /// </summary>
            public void Clear()
            {
                Table.Clear();
                InitRarsCount();
                TableAvrgRarity = Rarity.None;
            }
            public void Add(Rarity Rarity, T item)
            {
                if (Rarity == Rarity.None)
                {
                    Console.WriteLine($"Adding item ({item}) with rarity none, this item will never be pulled");
                }
                Table.Add(new ChancePart<T>(Rarity, item));
                RaritysCount[Rarity]++;
                RecalcChancePerc();
            }
            public bool Remove(T item)
            {
                if (Table.Exists(p => p.Equals(item)))
                {
                    var it = Table.Find(p => p.Object.Equals(item));
                    RaritysCount[it.Rarity]--;
                    Table.Remove(it);
                    RecalcChancePerc();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public void RemoveAt(int i)
            {
                RaritysCount[Table[i].Rarity]--;
                Table.RemoveAt(i);
                RecalcChancePerc();
            }
            private void RecalcChancePerc()
            {
                //reassigning values to items
                var total = Table.Sum(p => (int)p.Rarity);
                foreach (var part in Table)
                {
                    part.Chance = ((float)part.Rarity / (float)total) * 100;
                }
                //recalculating average rarity
                if (Table.Where(p => p.Rarity != Rarity.None).Count() != 0)
                {
                    var avrg = Table.Sum(p => (int)p.Rarity) / Table.Where(p => p.Rarity != Rarity.None).Count();
                    var sorted = Petrosik.Utility.Utility.GetEnumTypes<Rarity>().OrderBy(num => Math.Abs((int)num - avrg));
                    TableAvrgRarity = sorted.First();
                }
            }
            private void InitRarsCount()
            {
                RaritysCount = new();
                foreach (var r in Petrosik.Utility.Utility.GetEnumTypes<Rarity>())
                {
                    RaritysCount.Add(r, 0);
                }
            }

            public IEnumerator<ChancePart<T>> GetEnumerator()
            {
                return this;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this;
            }

            public bool MoveNext()
            {
                CurrentIndex++;
                return CurrentIndex < Table.Count;
            }

            public void Reset()
            {
                CurrentIndex = -1;
            }

            public void Dispose()
            {
                //i hope i dont actually need this
            }
        }

        [Serializable]
        public class ChancePart<T>
        {
            public Rarity Rarity;
            public T Object;
            /// <summary>
            /// Perecentage for spawn rate
            /// </summary>
            public float Chance;

            internal ChancePart(Rarity Rarity, T Object)
            {
                this.Rarity = Rarity;
                this.Object = Object;
                Chance = 0;
            }

            public override string ToString()
            {
                return $"{Chance} - {Object}";
            }
            public override bool Equals(object obj)
            {
                return Object.Equals(obj);
            }
            public override int GetHashCode()
            {
                return Object.GetHashCode();
            }
        }
    }
}