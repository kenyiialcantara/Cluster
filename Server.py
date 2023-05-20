import socket
import numpy as np
import json
import threading
import time

class Server:
    def __init__(self) -> None:
        self.host = "localhost"
        # self.host = 'localhost'
        self.port = 3000
        self.serv_socket = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
        self.clients = {}
        # self.coefficientes_fun_pol = [[7,1],[8,2]] #a0 x + a1 x + a2 x**2 + ... + an x**n
        self.dominio = [0,0]
        # self.segmentos = 10000
        self.sumas_parciales = []
        self.threads_clients = []
        self.number_clients = 0
        self.inicio_time = 0
        
        
    def resibe_clientes(self):
        
        
        class ManagementClient:
           def __init__(self,client_socket,client_address) -> None:
               self.client_address = client_address
               self.client_socket = client_socket
               self.suma_parcial = 0
               
           def resibe_message_client(self):
               response_json = self.client_socket.recv(1024).decode('utf-8')
               print('Recibiendo del cliente {}:'.format(self.client_address[0]),response_json)
               response = json.loads(response_json)
               self.suma_parcial = response['result']
               
        
        id_available = 0
        while True:
            #Llega la conexion y acepta
            client_socket,client_address = self.serv_socket.accept()
            self.clients[id_available] = ManagementClient(client_socket,client_address)
            t = threading.Thread(target=self.clients[id_available].resibe_message_client)
            self.threads_clients.append(t)
            t.start()
            print('Nuevo cliente con ip {}'.format(client_address[0]))
            print('cantida de clientes:',len(self.clients))
            id_available +=1
            if len(self.clients)>=self.number_clients:
                print('Enviando a los clientes')
                self.inicio_time = time.time()
                self.envia_message_cliente()
                break
        
            
    
    
    def envia_message_cliente(self):
        
        domains = np.linspace(self.dominio[0],self.dominio[1],len(self.clients)+1)
        data = {"function":self.coefficientes_fun_pol,'a':0,"b":0}
        data['segmentos']=self.segmentos
        data['threads'] = self.threads
        i = 0
        for id in self.clients:
            data["a"] = domains[i]
            data['b'] = domains[i+1]
            data_json  = json.dumps(data)+"\n"
            print('Enviando al cliente con ip {}:'.format(self.clients[id].client_address[0]),data_json)
            #Enviando a todos los clientes
            self.clients[id].client_socket.sendall(data_json.encode())           
            i+=1

    
    def start(self):
        
        #Configurando el numero de clientes
        self.number_clients = int(input('Ingresa el numero de clientes:'))
     
        coeficientes = input("Ingrese los coeficientes separados por espacios: ").split()
        exponentes = input("Ingrese los exponentes separados por espacios: ").split()
    
        coeficientes_exponentes = []
    
        for coeficiente, exponente in zip(coeficientes, exponentes):
            coeficiente = float(coeficiente)
            exponente = int(exponente)
            coeficientes_exponentes.append([coeficiente, exponente])
        self.coefficientes_fun_pol = coeficientes_exponentes

        a = float(input("Ingresa a:")) 
        b = float(input("Ingresa b:")) 
        self.dominio = [a,b]
        self.segmentos = int(input("Ingresa numero de segmentos:")) 
        self.threads = int(input("Ingresa numero de threads que ejecutaran los clientes:")) 
        #Iniciando
        self.serv_socket.bind((self.host,self.port))
        #Experando conexion entrante
        self.serv_socket.listen()
        print('\nEl servidor esta escuchando en la ip {} y puerto {} ...'.format(self.host,self.port))
        self.resibe_clientes()
        
        #haciendo a que esperen q que todos terminen
        for thread in self.threads_clients:
            thread.join()
        
        suma_integral = 0    
        for id in self.clients:
            suma_integral = suma_integral + self.clients[id].suma_parcial
        
        end_time = time.time()

        print('Integral:',suma_integral)
        print("Tiempo de ejecucion:",end_time-self.inicio_time)
        self.serv_socket.close()
    
        
    
    
def main():
    current_server = Server()
    current_server.start()



if __name__=="__main__":
    main()








