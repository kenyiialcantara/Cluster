import socket
from multiprocessing import Pool
import json
def procesar_cadena(cadena):
    if cadena[0] == '{':
        return cadena
    else:
        return cadena[1:]
class Client:
    def __init__(self) -> None:
        self.host = '10.128.0.7'
        # self.host = 'localhost'
        self.port = 3000
        self.client_socket = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
        
    def polinomio(self,x, coeficientes_exponentes):
        resultado = 0
        for coeficiente, exponente in coeficientes_exponentes:
            resultado += coeficiente * (x ** exponente)
        return resultado
            
    def integrate_range(self,arra_coff_exp,start, end, num_steps):
        # Función que integra en un rango específico
        step_size = (end - start) / num_steps
        partial_sum = 0
        for i in range(num_steps):
            x = start + (i + 0.5) * step_size
            partial_sum += self.polinomio(x,arra_coff_exp)
        return partial_sum * step_size

    def integrate_parallel(self,arra_coff_exp,start, end, num_steps, num_threads):
        # Función que integra en paralelo utilizando hilos
        step_size = (end - start) / num_threads
        ranges = [(arra_coff_exp,start + i * step_size, start + (i + 1) * step_size, num_steps) for i in range(num_threads)]
        
        with Pool(num_threads) as pool:
            results = pool.starmap(self.integrate_range, ranges)
        
        return sum(results)

    def start(self):
        #conectando
        self.client_socket.connect((self.host,self.port))
        print('Esperando al que envie la funcion ...')

        #Resibiendo la respuesta del servidor
        data_json_aux = self.client_socket.recv(1024).decode()
        data_json = procesar_cadena(data_json_aux.strip())
        print('El servidor envio:',data_json)
        data = json.loads(data_json)
        a = data['a']
        b = data['b']
        arra_coff_exp = data['function']
        segmentos = data['segmentos']
        threads = data['threads']
        result = self.integrate_parallel(arra_coff_exp,a,b,segmentos,threads)
        
        #Enviando al servidor
        message = {"result":result}
        print('Enviando al servidor el resultado parcial:',message)
        message_json = json.dumps(message)
        self.client_socket.sendall((message_json.strip()).encode())
        self.client_socket.close()
        
def main():
    current_client = Client()
    current_client.start()

if __name__=="__main__":
    main()

