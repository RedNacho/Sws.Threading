using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Sws.Threading.Extensions;
using Sws.Nindapter.Extensions;

namespace Sws.Threading.Ninject
{

    public interface IContainer
    {
        int Value { get; set; }
    }

    public class UnsafeContainer : IContainer
    {
        public int Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel();

            kernel.Bind<IContainer>().ThroughDecorator(c => c.ThreadSafeProxy()).To<UnsafeContainer>();

            var container = kernel.Get<IContainer>();
        }
    }
}
