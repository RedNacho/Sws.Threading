using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Sws.Threading.Extensions;
using Sws.Nindapter.Extensions;

namespace Sws.Threading.Ninject
{

    public interface IContainer
    {
        void UpdateValues();
    }

    public class UnsafeContainer : IContainer
    {
        private int _value1, _value2;

        public void UpdateValues()
        {
            if (_value1 != _value2)
            {
                throw new Exception("Values don't match!  This method is not thread-safe!");
            }

            var value = _value1 + 1;

            _value1 = value;
            _value2 = value;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel();

            kernel.Bind<IContainer>().To<UnsafeContainer>().Named("Unsafe");

            kernel.Bind<IContainer>().ThroughDecorator(
                c => c.ConfigureThreadSafeProxy()
                    .ForMember(con => con.UpdateValues())
                    .Build()
            ).To<UnsafeContainer>().Named("Safe");

            var container = kernel.Get<IContainer>("Safe"); // Swap this for the unsafe one to see the problem.

            var usedVals = new List<int>();
            
            Parallel.Invoke(Enumerable.Range(0, 10).Select(value => (Action)(() => {

                for (int iteration = 0; iteration < 10000; iteration++)
                {
                    container.UpdateValues();
                }
            })).ToArray());

            Console.WriteLine("Everything OK.");

            Console.ReadKey();
        }
    }
}
