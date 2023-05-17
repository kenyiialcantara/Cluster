import socket
from multiprocessing import Pool
import json

class Client:
    def __init__(self) -> None:
        self.host = 'localhost'
        self.port = 3000
        self.client_socket = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
        
    def polinomio(self,x, coeficientes_exponentes):
        resultado = 0
        for coeficiente, exponente in coeficientes_exponentes:
            resultado += coeficiente * (x ** exponente)
        return resultado
            
    def integrate_range(self,arra_coff_exp,start, end, num_steps):
        print('Legue 🇲🇨 ')
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
    
        
        print('Legue 🙂 ',ranges)
        with Pool(num_threads) as pool:
            print('Legue ap🛐 ')
            results = pool.starmap(self.integrate_range, ranges)
        
        return sum(results)
        
        

    def start(self):
        #conectando
        self.client_socket.connect((self.host,self.port))
        print('Esperando al que envie el servidor')
        #Resibiendo la respuesta del servidor
        data_json = self.client_socket.recv(1024).decode()
        print('El servidor envio:',data_json)
        data = json.loads(data_json)
        a = data['a']
        b = data['b']
        arra_coff_exp = data['function']
        print('Legue 🍔 ')
        # self.function = lambda x: sum([coeff * (x ** exp) for coeff, exp in arra_coff_exp])
        # print('Legue 👨‍🦲  ',self.function(8))
        result = self.integrate_parallel(arra_coff_exp,a,b,10000,4)
        print('Legue 📱 ',result)
        # #Enviando al servidor
        message = {'result':result}
        message_json = json.dumps(message)
        self.client_socket.sendall(message_json.encode())
        self.client_socket.close()
        
def main():
    current_client = Client()
    current_client.start()

if __name__=="__main__":
    main()