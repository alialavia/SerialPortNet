#include<unistd.h>
#include<termios.h>

int main(int argc, char* argv[])
{
	if (argc != 2)
		return -1;
	
	int temp = VTIME;
	#include<stdio.h>	
	printf("%d", temp);
	return 0;
}
