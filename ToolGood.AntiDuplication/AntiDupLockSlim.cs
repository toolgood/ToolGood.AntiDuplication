using System.Threading;

namespace ToolGood.AntiDuplication
{
    class AntiDupLockSlim : ReaderWriterLockSlim
    {
        public int UseCount;
    }

}
