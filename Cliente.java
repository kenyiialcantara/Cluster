package test2;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.*;
import java.net.Socket;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

class Cliente {
    private static  String SERVIDOR_IP;
    private static final int SERVIDOR_PUERTO = 3000;

    private static double[][] funcion;
    private double a;
    private double b;
    private int segmentos;
    private int threads;

    public Cliente(String ip) {
        this.funcion = null;
        this.a = 0;
        this.b = 0;
        this.segmentos = 0;
        this.SERVIDOR_IP = ip;
        this.threads = 1;
    }

    public void start() {
        try {
            Socket socket = new Socket(SERVIDOR_IP, SERVIDOR_PUERTO);
            System.out.println("Conexión lista con el servidor");
            ///////// Temporalmente en des-huso
            //DataInputStream input = new DataInputStream(socket.getInputStream());
            PrintWriter output = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);
            ///////
            BufferedReader in = new BufferedReader(new InputStreamReader(socket.getInputStream()));

            // Esperar a que el servidor mande la funcion

            //String taskJSONString = input.readUTF();
            String taskJSONString = in.readLine();
            JSONObject taskJSON = new JSONObject(taskJSONString);
            // Mostrar que le mando el servidor
            System.out.println("Servidor manda: " +taskJSON);
            parseTaskJSON(taskJSON);

            // Integrando concurrentemente
            double resultado = integrateRange(this.a, this.b, this.segmentos, this.threads);
            System.out.println("Nodo responde: " + resultado);
            // Enviar resultado al servidor
            output.println("{\"result\":"+resultado+"}");

            socket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private void parseTaskJSON(JSONObject taskJSON) {
        JSONArray functionJSON = taskJSON.getJSONArray("function");
        int numCoefficients = functionJSON.length();
        funcion = new double[numCoefficients][2];

        for (int i = 0; i < numCoefficients; i++) {
            JSONArray coefficientJSON = functionJSON.getJSONArray(i);
            double coefficient = coefficientJSON.getDouble(0);
            double exponent = coefficientJSON.getDouble(1);
            funcion[i][0] = coefficient;
            funcion[i][1] = exponent;
        }

        a = taskJSON.getDouble("a");
        b = taskJSON.getDouble("b");
        segmentos = taskJSON.getInt("segmentos");
        threads = taskJSON.getInt("threads");
    }


    public static double integrateRange(double a, double b, double segmentos, int nThreads) {
        //nThreads = Runtime.getRuntime().availableProcessors(); // Obtener el número de procesadores disponibles
        double step = (b - a) / segmentos; // Calcular el tamaño de cada segmento

        ExecutorService executor = Executors.newFixedThreadPool(nThreads);
        Future<Double>[] results = new Future[nThreads];

        // Dividir la integración en segmentos y asignarlos a los hilos
        for (int i = 0; i < nThreads; i++) {
            double start = a + i * step * segmentos / nThreads;
            double end = a + (i + 1) * step * segmentos / nThreads;
            results[i] = executor.submit(() -> integrateSegment(start, end, step));
        }

        // Obtener los resultados parciales de cada hilo y sumarlos
        double total = 0.0;
        for (int i = 0; i < nThreads; i++) {
            try {
                total += results[i].get();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }

        executor.shutdown(); // Apagar el ExecutorService

        return total;
    }

    private static double integrateSegment(double start, double end, double step) {
        double sum = 0.0;

        for (double x = start; x < end; x += step) {
            // Realizar el cálculo de la función en el segmento
            double y = polinomio(x);

            // Sumar el área del trapecio formado por "y" y "y + step"
            sum += (y + polinomio(x + step)) * step / 2.0;
        }

        return sum;
    }




    private static double polinomio(double x) {
        double resultado = 0;

        for (double[] coeficienteExponente : funcion) {
            double coeficiente = coeficienteExponente[0];
            double exponente = coeficienteExponente[1];
            resultado += coeficiente * Math.pow(x, exponente);
        }

        return resultado;
    }

    public static void main(String[] args) {
        String host;
        if(args.length ==0) host="llt" ; else  host = args[0];
        Cliente cliente = new Cliente(host);
        cliente.start();
    }
}