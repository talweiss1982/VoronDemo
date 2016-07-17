using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voron;
using Voron.Util.Conversion;

namespace VoronDemo
{
    public class VoronDemo : IDisposable
    {
        private readonly StorageEnvironment _storageEnvironment;

        public VoronDemo(StorageEnvironmentOptions seo)
        {
            _storageEnvironment = new StorageEnvironment(seo);
            using (var tx = _storageEnvironment.NewTransaction(TransactionFlags.ReadWrite))
            {
                _storageEnvironment.CreateTree(tx, "Users"); // user id -> user value
                _storageEnvironment.CreateTree(tx, "IX_UsersByLastLogin"); // last login -> user ids, plural

                tx.Commit();
            }
        }

        public Tuple<DateTime, bool> GetLastLogin(string userId)
        {
            using (var tx = _storageEnvironment.NewTransaction(TransactionFlags.Read))
            {
                var users = tx.ReadTree("Users");
                var read = users.Read(userId);
                if (read == null)
                    return null;

                var parts = read.Reader.ToStringValue().Split('|');
                return Tuple.Create(DateTime.Parse(parts[0]), bool.Parse(parts[1]));
            }
        }

        // all users logged in between dates, the logged in date 
        // seek, limit, iteration over multi tree
        public IEnumerable<string> GetUsersWhoLogedInBetweenTimes(DateTime from, DateTime to, int limit = int.MaxValue)
        {
            using (var tx = _storageEnvironment.NewTransaction(TransactionFlags.Read))
            {
                var lastLoginIndex = _storageEnvironment.CreateTree(tx, "IX_UsersByLastLogin");
                using (var it = lastLoginIndex.Iterate())
                {
                    //We could not find a value that is bigger than the 'from' time  
                    if (it.Seek(new Slice(EndianBitConverter.Big.GetBytes(from.Ticks))) == false)
                        yield break;
                    var currKey = it.CurrentKey.CreateReader().ReadBigEndianInt64();
                    //the next key after 'from' is bigger than the 'to' range, nothing to return
                    if (currKey > to.Ticks)
                        yield break;
                    int found = 0;
                    do
                    {
                        using (var iterator = lastLoginIndex.MultiRead(it.CurrentKey))
                        {
                            iterator.Seek(Slice.BeforeAllKeys);
                            do
                            {
                                if (found++ >= limit)
                                    break;
                                yield return iterator.CurrentKey.CreateReader().ToStringValue();
                            } while (iterator.MoveNext());
                        }
                        //no more keys
                        if (it.MoveNext() == false)
                            yield break;
                        currKey = it.CurrentKey.CreateReader().ReadBigEndianInt64();
                    } while (currKey <= to.Ticks);
                }
            }
        }
        // excerise: add the last login success status

        public void UserLoggedIn(string userId, DateTime time, bool successful)
        {
            using (var tx = _storageEnvironment.NewTransaction(TransactionFlags.ReadWrite))
            {
                var users = _storageEnvironment.CreateTree(tx, "Users");
                users.Add(userId, time.ToString("o") + "|" + successful);
                var lastLoginIndex = _storageEnvironment.CreateTree(tx, "IX_UsersByLastLogin");

                lastLoginIndex.MultiAdd(new Slice(EndianBitConverter.Big.GetBytes(time.Ticks)),
                    userId);

                tx.Commit();
            }
        }

        public void Dispose()
        {
            _storageEnvironment.Dispose();
        }
    }
}
