#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <unistd.h>
#include <string.h>
#include <signal.h>
#include <sys/wait.h>
#include <pthread.h>
#include <unistd.h>
#include <time.h>

#define MAX 4096 
#define PORT 8081
#define MAX_CLIENTS 100

#define SA struct sockaddr 
#define _GNU_SOURCE

int clientscount =0;
int uid=1;

typedef struct {
    int uid;
    int cfd;
    struct sockaddr_in caddr;
    char name[30];
    int przeciwnikid;
    int przeciwnikcfd;
}cln;

cln *clients[MAX_CLIENTS];

pthread_mutex_t clients_mutex = PTHREAD_MUTEX_INITIALIZER;

//usuwa klienta gdy ten sie rozlaczy
void queue_remove(int uid){
    pthread_mutex_lock(&clients_mutex);
    for(int i=0;i<MAX_CLIENTS;++i){
        if(clients[i]){
            if(clients[i]->uid==uid){
                clientscount--;
                clients[i]=NULL;
                break;
            }            
        }
    }
    pthread_mutex_unlock(&clients_mutex);
}

void szukajPrzeciwnika(void *arg){
    cln *c = (cln*)arg;
    for(int i=0;i<clientscount;i++){
        if(c->przeciwnikcfd==0 && clients[i]->przeciwnikcfd==0 && c->cfd!=clients[i]->cfd){
            c->przeciwnikcfd = clients[i]->cfd;
            c->przeciwnikid = clients[i]->uid;
            clients[i]->przeciwnikcfd = c->cfd;
            clients[i]->przeciwnikid = c->uid;

            //wysylanie do dobranej pary wiadomosci o nicku przeciwnika
            write(c->cfd,clients[i]->name,sizeof(clients[i]->name));
            write(clients[i]->cfd,c->name,sizeof(c->name));
            //wysylanie do dobranej pary graczy wiadomosci o kolorze pionow
            srand(time(NULL));
            int random = rand()%2;
            printf("%d\n", random);

            if(random==0){
                write(c->cfd,"c",1);
                write(c->przeciwnikcfd,"z",1);
            }else{
                write(c->przeciwnikcfd,"c",1);
                write(c->cfd,"z",1);
            }

            break;
        }
    }
}


void *cthread(void *arg){
    int koniecGry =0;
    cln *c = (cln*)arg;
    clientscount++;
    char buff[MAX]; 
    bzero(buff, MAX); 
    read(c->cfd, buff, sizeof(buff)); 
    strcpy(c->name,buff);

    //szuka przeciwnika
    szukajPrzeciwnika(c);
        for(int i=0;i<clientscount;i++){
            printf("przed while: klient name: %s, klient uid: %d, klientcfd: %d, przeciwnikCFD: %d\n",clients[i]->name,clients[i]->uid,clients[i]->cfd,clients[i]->przeciwnikcfd);

        }
        printf("\n");
    while(1){
        char buff[MAX];
        char wiadomosc[MAX];
        bzero(wiadomosc, MAX);
        bzero(buff, MAX);

        if(koniecGry){
            break;
        }
            //odbieranie wiadomosci od klienta(zawiera rozmieszczenie poszczegolnych pionow)
        int receive = recv(c->cfd, buff, MAX, 0);
        if (receive > 0){
            if(strlen(buff) > 0){
                strcpy(wiadomosc, buff);
                    //wysylanie wiadomosci do przciwnika (rozmieszczenie pionow na planszy)
                write(c->przeciwnikcfd,wiadomosc,sizeof(wiadomosc));                   
            }
        }else{
            printf("error, klient sie rozlaczyl \n");    
            write(c->przeciwnikcfd,"q",sizeof("q"));

            koniecGry =1;
        }
    }

    //gdy klient sie rozlaczy, wysylana jest wiadomosc do drugiego gracza o koncu gry
    close(c->cfd);
    queue_remove(c->uid);

    free(c);

    pthread_detach(pthread_self());

    
    return NULL;
}

//dodanie gracza do tablicy clients
void addclient(cln *c){
	pthread_mutex_lock(&clients_mutex);
	for(int i=0; i < MAX_CLIENTS; ++i){
		if(!clients[i]){
			clients[i] = c;
			break;
		}
	}
	pthread_mutex_unlock(&clients_mutex);
}

int main() 
{ 
    pthread_t tid;
    socklen_t slt;
	int sockfd; 
	struct sockaddr_in servaddr; 

	sockfd = socket(AF_INET, SOCK_STREAM, 0); 
	bzero(&servaddr, sizeof(servaddr)); 

	servaddr.sin_family = AF_INET; 
	servaddr.sin_addr.s_addr = htonl(INADDR_ANY); 
	servaddr.sin_port = htons(PORT); 

	bind(sockfd, (SA*)&servaddr, sizeof(servaddr));
	listen(sockfd, 5);
	while (1) { 
        cln* c = (cln *)malloc(sizeof(cln));
        slt = sizeof(c->caddr);
        c->cfd = accept(sockfd, (struct sockaddr*)&c->caddr, &slt); 
        if((clientscount)==MAX_CLIENTS){
            printf("osiagnieto limit polaczen\n");
            close(c->cfd);
            continue;
        }
        c->przeciwnikcfd = 0;
        c->uid =uid;
        uid++;
        //printf("klient polaczony \n");
        addclient(c);
        pthread_create(&tid,NULL,cthread,(void*)c);
        
        
	}
    return EXIT_SUCCESS;
} 
