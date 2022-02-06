using System;
using System.Collections.Generic;

namespace SampleService
{
    public class Factory : IDisposable
    {
        private bool disposed = false;
        public Factory()
        {

        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // already disposed
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // managed resource

            }

            // unmanaged resource

            disposed = true;
        }

        ~Factory()
        {
            Dispose(false);
        }

        public List<Sample> CreateSamples(uint num)
        {
            List<Sample> sl = new();
            for (int i = 1; i < num + 1; i++)
            {
                Sample sp = new Sample(EnumSampleStatus.IDLE, (uint)i);
                sl.Add(sp);
            }
            return sl;
        }

    }
    public class Sample
    {
        public uint ID { set; get; }
        public uint ProjectID { set; get; }
        public uint TaskID { set; get; }
        public EnumSampleStatus Status { set; get; }
        public Sample(EnumSampleStatus status, uint id)
        {
            Status = status;
            ID = id;
        }
    }
    public enum EnumSampleStatus
    {
        IDLE,
        PROCESSING,
        FAIL,
    }
}