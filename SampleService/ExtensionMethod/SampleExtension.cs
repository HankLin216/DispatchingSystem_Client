using DispatchingSystem_Client;

namespace SampleService.ExtensionMethod
{
    static public class SampleExtension
    {
        static public SampleInformation ToRefreshSampleInformationRequest(this Sample s)
        {
            //
            var si = new SampleInformation();

            // get the props
            var sampleProps = typeof(Sample).GetProperties();
            var sampleInfoProps = typeof(SampleInformation).GetProperties();
            foreach (var p in sampleProps)
            {
                // get value
                var v = p.GetValue(s);

                // get the prop corresponding to SampleInformation
                var siP = typeof(SampleInformation).GetProperty(p.Name);

                if (siP != null)
                {
                    // set value
                    siP.SetValue(si, v);
                }
            }
            return si;
        }
    }
}