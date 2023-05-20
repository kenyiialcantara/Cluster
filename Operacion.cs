using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sockets.Cliente
{
    internal class Operacion
    {

        public static double evaluar(Function f, double x)
        {
            double sum = 0;
            foreach (double[] terms in f.function)
            {
                sum += terms[0] * Math.Pow(x, terms[1]);
            }
            return sum;
        }

        public static double integrarThreads(Function f, double min, double max, int steps)
        {
            double sum = 0, x;
            double step_size = (max - min) / steps;
            for (int i = 0; i < steps; ++i)
            {
                x = min + (i + 0.5) * step_size;
                sum += evaluar(f, x);
            }
            Console.WriteLine("- De " + min + " a " + max + ": " + sum * step_size);
            return sum * step_size;
        }

        public static double integrar(Function f)
        {
            double resultado = 0;
            double size = (double)(f.b - f.a) / f.threads;
            double[] sumas = new double[f.threads];
            object value = 0;

            Console.WriteLine("USANDO " + f.segmentos + " PASOS Y " + f.threads + " THREADS");

            Thread[] T = new Thread[f.threads];
            for (int i = 0; i < f.threads; ++i)
            {
                T[i] = new Thread((index) =>
                {
                    int i = (int)index;
                    sumas[i] = integrarThreads(f, f.a + i * size, f.a + (i + 1) * size, f.segmentos);
                });
                T[i].Start(i);
            }

            for (int i = 0; i < f.threads; i++)
            {
                T[i].Join();
            }

            for (int i = 0; i < f.threads; ++i)
            {
                //Console.WriteLine(sumas[i].ToString());
                resultado += sumas[i];

            }

            return resultado;
        }
    }
}
