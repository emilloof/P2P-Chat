	
	; r16 och r19 är free to use 
	.def	num = r20 ;number 0 -  9 
	.def	key = r21  ; key pressed yes / no  

	;sätta stacken 
	ldi		r16, HIGH(RAMEND) 
	out		SPH, r16 
	ldi		r16,LOW(RAMEND) 
	out		SPL,r16 

	call	INIT 
	clr		num 


FOREVER:
	call	GET_KEY   ;hämtar key press om boolean key är pressad  

LOOP: 
	cpi		key,0 
	breq	FOREVER    ; until key pressed 
	out		PORTB,num    ; print digit 
	call	DELAY 
	inc		num			;num ++
	cpi		num, 10    ; num == 10? 
	brne	NOT_10		; no so jump 
	clr		num     ; was 10 


NOT_10: 
	call	GET_KEY 
	jmp		LOOP 

	;		Get_KEY, return Key i=0 if pressed
GET_KEY: 
	clr		key  ; clr nollst'ller registeret på key 
	sbic	PINC, 0 ; <------ skip over if not pressed   Kontrolera om porten C är noll 0 eller inte 
	dec		key 
	ret


	;			Init. Pinnar on C in , B3 - B0 out 
INIT: 
	clr		r16 
	out		DDRC,r16 ; PORTEN C ingång
	ldi		r16,$0F
	out		DDRB,r16  ; PORTEN B Utgång
	ret 

DELAY: 
	ldi		r18,3




