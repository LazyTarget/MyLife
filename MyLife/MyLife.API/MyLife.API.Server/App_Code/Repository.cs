using MyLife.Data.Context;
using ProcessLib.Data;

namespace MyLife.API.Server
{
    public class Repository
    {
        public MyLifeDataContext MyLife => new MyLifeDataContext();

        public ProcessDataContext Process => new ProcessDataContext();

    }
}