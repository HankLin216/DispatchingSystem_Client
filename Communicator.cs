using System;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using Grpc.Net.Client;
using SampleService;
using DispatchingSystem_Client;
using SampleService.ExtensionMethod;

namespace CommunicateProvider
{
    public class Communicator : IDisposable
    {
        private Timer refreshSampleInfo2ServerTimer = new(10 * 1000);
        public bool printStateOfRefreshSampleInfo2ServerTimer = true;
        private bool disposed = false;
        private GrpcChannel grpcChannel;
        public Communicator()
        {
            setGrpcChannel();
            setTimer();
        }

        private void setGrpcChannel()
        {
            // // 1. 自動安裝憑證
            // string pjRootPath = Environment.CurrentDirectory;
            // string caCrtPath = Path.Combine(pjRootPath, "crt", "rootCA.crt");
            // X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            // store.Open(OpenFlags.ReadWrite);
            // store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(caCrtPath)));
            // store.Close();

            // // 2. 不確認伺服器憑證
            // var httpClientHandler = new HttpClientHandler();
            // httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            // var grpcChannelOptions = new GrpcChannelOptions()
            // {
            //     HttpHandler = httpClientHandler
            // };

            string pjRootPath = Environment.CurrentDirectory;
            string caCrtPath = Path.Combine(pjRootPath, "crt", "rootCA.crt");
            var rootCertificate = new X509Certificate2(File.ReadAllBytes(caCrtPath));
            var rootCertificates = new X509Certificate2Collection(rootCertificate);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = CreateCustomRootValidator(rootCertificates);

            var grpcChannelOptions = new GrpcChannelOptions()
            {
                HttpHandler = httpClientHandler
            };

            grpcChannel = GrpcChannel.ForAddress("https://localhost:8081", grpcChannelOptions);
        }
        private void setTimer()
        {
            // send sample info periodically
            refreshSampleInfo2ServerTimer.Elapsed += refreshSampleInfo2Server;
            refreshSampleInfo2ServerTimer.Start();
        }

        private async void refreshSampleInfo2Server(object source, ElapsedEventArgs args)
        {
            // get the current samples from repository
            var sampleInfos = Repository.samplesList.ConvertAll(r => r.ToRefreshSampleInformationRequest());

            // get the grpc client
            var client = new SampleController.SampleControllerClient(grpcChannel);

            // call rpc
            RefreshSampleInformationRequest req = new RefreshSampleInformationRequest();
            req.SampleInfo.AddRange(sampleInfos);

            var res = await client.RefreshSampleInformationAsync(req);

            // echo
            if (printStateOfRefreshSampleInfo2ServerTimer)
            {
                Console.WriteLine($"[Info] Send Message to Server Success, sending timestamp: {args.SignalTime}");
            }
        }

        private Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool> CreateCustomRootValidator(X509Certificate2Collection trustedRoots, X509Certificate2Collection intermediates = null)
        {
            RemoteCertificateValidationCallback callback = CreateCustomRootRemoteValidator(trustedRoots, intermediates);
            return (message, serverCert, chain, errors) => callback(null, serverCert, chain, errors);
        }
        private RemoteCertificateValidationCallback CreateCustomRootRemoteValidator(X509Certificate2Collection trustedRoots, X509Certificate2Collection intermediates = null)
        {
            if (trustedRoots == null)
                throw new ArgumentNullException(nameof(trustedRoots));
            if (trustedRoots.Count == 0)
                throw new ArgumentException("No trusted roots were provided", nameof(trustedRoots));

            // Let's avoid complex state and/or race conditions by making copies of these collections.
            // Then the delegates should be safe for parallel invocation (provided they are given distinct inputs, which they are).
            X509Certificate2Collection roots = new X509Certificate2Collection(trustedRoots);
            X509Certificate2Collection intermeds = null;

            if (intermediates != null)
            {
                intermeds = new X509Certificate2Collection(intermediates);
            }

            intermediates = null;
            trustedRoots = null;

            return (sender, serverCert, chain, errors) =>
            {
                // Missing cert or the destination hostname wasn't valid for the cert.
                if ((errors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0)
                {
                    return false;
                }

                for (int i = 1; i < chain.ChainElements.Count; i++)
                {
                    chain.ChainPolicy.ExtraStore.Add(chain.ChainElements[i].Certificate);
                }

                if (intermeds != null)
                {
                    chain.ChainPolicy.ExtraStore.AddRange(intermeds);
                }

                chain.ChainPolicy.CustomTrustStore.Clear();
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.AddRange(roots);
                return chain.Build((X509Certificate2)serverCert);
            };
        }


        ~Communicator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // managed resource
                refreshSampleInfo2ServerTimer.Dispose();
                refreshSampleInfo2ServerTimer = null;
            }
            // unmanaged resource

            disposed = true;
        }
    }
}