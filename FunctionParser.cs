using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Sockets
{
    internal class FunctionParser
    {
        String f;
        double a, b;
        int steps, threads;
        public FunctionParser(String f, double a, double b, int steps, int threads)
        {
            this.f = f;
            this.a = a;
            this.b = b;
            this.steps = steps;
            this.threads = threads;
        }

        public Function toObject()
        {
            List<double[]> list = new List<double[]>();
            double[] coef_exp= new double[2];
            String polinomio = this.f.Replace(" ", ""); 
            Function function = new Function();

            double result = 0;
            string[] terms = polinomio.Split('+', '-'); // Dividir en términos
            int cont = 0;

            foreach (string t in terms)
            {
                String term=t;
                
                if (string.IsNullOrWhiteSpace(term))
                    continue;
                if (polinomio.IndexOf(term, cont)!=0 && polinomio[polinomio.IndexOf(term, cont) -1].Equals('-'))
                {
                    term = String.Concat("-", term);
                    //Console.WriteLine("Negativo");
                }
                cont += term.Length;
                double coef;
                double exp = 0;
                
                if (term.Contains("x"))
                {
                    string[] parts = term.Split('x');

                    if (parts.Length == 1)
                    {
                        coef = 1;
                        if (!string.IsNullOrEmpty(parts[0]))
                            exp = 1;
                        
                    }
                    else
                    {
                        if (parts[0] == "-")
                            coef = -1;
                        else if (parts[0] == "")
                            coef = 1;
                        else
                            coef = double.Parse(parts[0]);

                        if (!string.IsNullOrEmpty(parts[1]))
                            exp = double.Parse(parts[1].Substring(1));
                        else
                            exp = 1;
                    }
                }
                else
                {
                    // El término es solo un coeficiente
                    coef = double.Parse(term);
                    
                }
                coef_exp[0] = coef;
                coef_exp[1] = exp;

                double[] e = { coef_exp[0], coef_exp[1] };
                //Console.WriteLine(coef + " " + exp);
                list.Add(e);

                //result += coef * Math.Pow(x, exp);
            }
            function.function = list;
            function.a = a;
            function.b = b;
            function.segmentos= steps;
            function.threads= threads;

            return function;
        }



        public double IntegracionRectangulos(String funcion, double a, double b, int n)
        {
            double h = (b - a) / n;
            double sum = 0.0;
            return 0;
        }
    }
}
