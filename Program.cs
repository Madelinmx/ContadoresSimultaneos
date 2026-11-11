using System;
using System.Collections.Generic;
using System.Threading;

namespace ContadoresSimultaneos
{
    class Program
    {
        
        class ContadorHilo
        {
            public int Id { get; }
            public int Valor => _valor; 
            private int _valor;
            public bool EnEjecucion { get; private set; }

            private Thread _hilo;
            private CancellationTokenSource _cts;

            public ContadorHilo(int id)
            {
                Id = id;
                _valor = 0;
                EnEjecucion = false;
            }

            
            public void Iniciar()
            {
                if (EnEjecucion)
                {
                    Console.WriteLine($"\n[!] El Contador {Id} ya se encuentra en ejecución.");
                    return;
                }

                _cts = new CancellationTokenSource();
                EnEjecucion = true;

                _hilo = new Thread(() => EjecutarContador(_cts.Token));
                _hilo.Start();

                Console.WriteLine($"\n[+] Contador {Id} iniciado en paralelo.");
            }

            private void EjecutarContador(CancellationToken token)
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        Thread.Sleep(1000); 
                        Interlocked.Increment(ref _valor); 
                    }
                }
                catch (ThreadInterruptedException)
                {
                    
                }
                finally
                {
                    EnEjecucion = false;
                }
            }

            
            public void Detener()
            {
                if (!EnEjecucion)
                {
                    Console.WriteLine($"\n[!] El Contador {Id} ya está detenido.");
                    return;
                }

                _cts?.Cancel();
                _hilo?.Join(); 
                EnEjecucion = false;

                Console.WriteLine($"\n[-] Contador {Id} detenido. Valor final: {_valor}");
            }
        }

        static void Main(string[] args)
        {
            
            Dictionary<int, ContadorHilo> contadores = new Dictionary<int, ContadorHilo>
            {
                { 1, new ContadorHilo(1) },
                { 2, new ContadorHilo(2) },
                { 3, new ContadorHilo(3) }
            };

            bool salir = false;

            
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("     CONTADORES SIMULTÁNEOS CON HILOS   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Iniciar un contador");
                Console.WriteLine("2. Detener un contador");
                Console.WriteLine("3. Mostrar el estado actual de los contadores");
                Console.WriteLine("4. Salir del programa (Detiene todos los hilos)");
                Console.Write("\nSeleccione una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        Console.Write("Ingrese el ID del contador a iniciar (1, 2 o 3): ");
                        if (int.TryParse(Console.ReadLine(), out int idInicio) && contadores.ContainsKey(idInicio))
                        {
                            contadores[idInicio].Iniciar();
                        }
                        else
                        {
                            Console.WriteLine("\n[X] ID de contador no válido.");
                        }
                        break;

                    case "2":
                        Console.Write("Ingrese el ID del contador a detener (1, 2 o 3): ");
                        if (int.TryParse(Console.ReadLine(), out int idDetener) && contadores.ContainsKey(idDetener))
                        {
                            contadores[idDetener].Detener();
                        }
                        else
                        {
                            Console.WriteLine("\n[X] ID de contador no válido.");
                        }
                        break;

                    case "3":
                        Console.WriteLine("\n--- ESTADO ACTUAL DE LOS CONTADORES ---");
                        foreach (var kvp in contadores)
                        {
                            string estado = kvp.Value.EnEjecucion ? "En ejecución (🟢)" : "Detenido (🔴)";
                            Console.WriteLine($"Contador {kvp.Key} -> Valor: {kvp.Value.Valor} | Estado: {estado}");
                        }
                        break;

                    case "4":
                        Console.WriteLine("\nDeteniendo de manera segura todos los hilos activos...");
                        foreach (var kvp in contadores)
                        {
                            if (kvp.Value.EnEjecucion)
                            {
                                kvp.Value.Detener();
                            }
                        }
                        Console.WriteLine("¡Programa finalizado con éxito!");
                        salir = true;
                        break;

                    default:
                        Console.WriteLine("\n[X] Opción no válida. Intente de nuevo.");
                        break;
                }

                if (!salir)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }
            }
        }
    }
}