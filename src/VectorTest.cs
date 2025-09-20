using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace RayTracer
//{
//    public class VectorTest
//    {
//        private static void Main(string[] args)
//        {
//            Vector3 v1 = new Vector3(3.0, 4.0, 0.0);
//            Vector3 v2 = new Vector3(3.0, 4.0, 5.0);

//            // To String
//            Console.WriteLine(v1.ToString());
//            Console.WriteLine(v2.ToString());
//            Console.WriteLine("------------------------");
//            // Length Calculations
//            Console.WriteLine(v1.ToString() + ": LengthSq: "+ v1.LengthSq());
//            Console.WriteLine(v1.ToString() + ": Length: "+ v1.Length());
//            Console.WriteLine();
//            Console.WriteLine(v2.ToString() + ": LengthSq: " + v2.LengthSq());
//            Console.WriteLine(v2.ToString() + ": Length: " + v2.Length());
//            Console.WriteLine("------------------------");
//            // Normalization
//            Console.WriteLine(v1.ToString() + ": Normalized: " + v1.Normalized());
//            Console.WriteLine();
//            Console.WriteLine(v2.ToString() + ": Normalized: " + v2.Normalized());
//            Console.WriteLine("------------------------");
//            // Dot product
//            Console.WriteLine(v1.ToString() + " . " + v2.ToString() + " = " + v1.Dot(v2));
//            Console.WriteLine("------------------------");
//            // Cross product combos
//            Console.WriteLine(v1.ToString() + " x " + v2.ToString() + " = " + v1.Cross(v2));
//            Console.WriteLine(v2.ToString() + " x " + v1.ToString() + " = " + v2.Cross(v1));
//            Console.WriteLine(v1.ToString() + " x " + v1.ToString() + " = " + v1.Cross(v1));
//            Console.WriteLine("------------------------");
//            // Addition
//            Console.WriteLine(v1.ToString() + " + " + v2.ToString() + " = " + v1+v2);
//            Console.WriteLine("------------------------");
//            // Negation
//            Console.WriteLine(v1.ToString() + " Negated: " + (-v1));
//            Console.WriteLine("------------------------");
//            // Subtraction
//            Console.WriteLine(v1.ToString() + " - " + v2.ToString() + " = " + (v1-v2));
//            Console.WriteLine("------------------------");
//            // Scalar mult.
//            Console.WriteLine("8" + " * " + v1.ToString() + " = " + (8 * v1));
//            Console.WriteLine("------------------------");
//            // Scalar mult backwards
//            Console.WriteLine(v1.ToString() + " * " + "8" + " = " + (v1 * 8));
//            Console.WriteLine("------------------------");
//            // Division
//            Console.WriteLine(v1.ToString() + " / " + "4" + " = " + (v1/4));
//            Console.WriteLine("------------------------");
//        }
//    }
//}
