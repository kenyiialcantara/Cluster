package test2;

import java.io.*;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.ArrayList;
import java.util.List;
import java.util.Scanner;
import java.util.concurrent.CountDownLatch;

import org.json.JSONArray;
import org.json.JSONObject;

public class Servidor {
    private static final int PUERTO = 3000;

    private double[][] funcion;
    private double a;
    private double b;
    private int segmentos;
    private int threads;
    private List<Double> sumasParciales;
    private int nodos;
    List<double[]> intervalosList;
    List<Thread> ThreadList;
    private CountDownLatch startSignal = new CountDownLatch(1);


    public Servidor() {
        this.funcion = new double[][] {{7, 1}, {8, 2}};
        this.a = 5;
        this.b = 10;
        this.segmentos = 10000;
        this.threads=10;
        this.sumasParciales = new ArrayList<>();
        this.nodos = 3;
        this.ThreadList = new ArrayList<>();
    }

    public void start() {
        try {
            ServerSocket serverSocket = new ServerSocket(PUERTO);
            System.out.println("El servidor está escuchando en el puerto " + PUERTO + "...");
            intervalosList = this.generarIntervalos(this.a,this.b, this.nodos);
            int idActual = 0;
            while (idActual < nodos) {

                Socket socket = serverSocket.accept();
                System.out.println("Nuevo cliente conectado: " + socket.getInetAddress());
                System.out.println("Pasando ID: " + idActual);
                int finalIdActual = idActual;
                Thread thread = new Thread(() -> handleClient(socket, finalIdActual));
                thread.start();
                this.ThreadList.add(thread);

                idActual++;
            }
            while(sumasParciales.size()<nodos){
                try {
                    Thread.sleep(1000); // Espera 1 segundo

                } catch (InterruptedException e) {

                }
            }

            serverSocket.close();

            double sumaIntegral = 0;
            for (double sumaParcial : sumasParciales) {
                sumaIntegral += sumaParcial;
            }

            System.out.println("Integral: " + sumaIntegral);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private void handleClient(Socket socket, int clientID) {
        try {
            BufferedReader input = new BufferedReader(new InputStreamReader(socket.getInputStream()));
            PrintWriter output = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);

            try {
                startSignal.await();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            System.out.println("Enviar a cliente");
            // Enviar tarea al cliente
            System.out.println("Creando tarea para ID: " + clientID);
            JSONObject taskJSON = createTaskJSON(clientID);
            output.println(taskJSON);

            // Recibir resultado parcial del cliente
            String sumaParcialString = input.readLine();
            JSONObject sumaParcialJSON = new JSONObject(sumaParcialString);
            double resultadoJSON = sumaParcialJSON.getDouble("result");
            sumasParciales.add(resultadoJSON);

            socket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private JSONObject createTaskJSON(int clientID) {
        JSONObject taskJSON = new JSONObject();
        JSONArray functionJSON = new JSONArray();

        for (double[] coeficienteExponente : funcion) {
            JSONArray coefficientJSON = new JSONArray();
            coefficientJSON.put(coeficienteExponente[0]);
            coefficientJSON.put(coeficienteExponente[1]);
            functionJSON.put(coefficientJSON);
        }

        taskJSON.put("function", functionJSON);
        System.out.println("A punto de usar ID: "+clientID);
       /* taskJSON.put("a", a);
        taskJSON.put("b", b);*/
        taskJSON.put("a", this.intervalosList.get(clientID)[0]);
        taskJSON.put("b", this.intervalosList.get(clientID)[1]);
        taskJSON.put("segmentos", segmentos);
        taskJSON.put("threads", threads);

        return taskJSON;
    }
    private static List<double[]> generarIntervalos(double a, double b, int nodos) {
        List<double[]> intervalos = new ArrayList<>();

        double delta = (b - a) / nodos;
        double x0 = a;

        for (int i = 0; i < nodos; i++) {
            double x1 = x0 + delta;
            intervalos.add(new double[]{x0, x1});
            x0 = x1;
        }

        return intervalos;
    }


    public static void main(String[] args) {
        Servidor servidor = new Servidor();
        Scanner scanner = new Scanner(System.in);
        String input = "";
        while(true){
            System.out.print("Ingrese numero de nodos: ");
            input = scanner.nextLine();
            try{
                servidor.nodos = Integer.parseInt(input);
                break;
            }
            catch(Exception e){
                System.out.println("Ingresa un número entero");
            }
        }

        new Thread(()-> servidor.start()).start();

        // Esperar hasta que se ingrese "INICIAR" por teclado


        while (true){
            input = scanner.nextLine();
            System.out.println(input);

            if (input.equals("INICIAR")) {
                break;

            }

        }
        while(true){
            System.out.print("Ingrese numero de threads por cliente: ");
            input = scanner.nextLine();
            try{
                servidor.threads = Integer.parseInt(input);
                break;
            }
            catch(Exception e){
                System.out.println("Ingresa un número entero");
            }
        }

        servidor.startSignal.countDown();
    }
}