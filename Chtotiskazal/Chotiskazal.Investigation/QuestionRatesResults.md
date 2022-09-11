Результаты анализа:

# Results:
0.73:
Ru Write Single Translation
Eng Write
Eng phrase substitute

0.82:
Ru phrase substitute
Ru Write

0.84
Ru Choose By Transcription
Ru trust single translation

0.87
Eng trust
Ru trust

0.90
Ru Choose Phrase
Eng Choose Phrase
Eng Choose word in phrase

0.93
RuChoose
Eng Choose
Eng is it right translation

0.96
Trans Choose



1: 1.сразу
2: 8/9час   (53 минуты)
3: 8/3 часа (~3 часа)
4: 8 часов
5: 1 День
6: 3 День
7: 9 День
8: 27 День
9: 81...


```
# время в часах, через которое нужно спросить
t(n) = if(n<=1) 0 else 24 * 3**(n-5)

# изменение скора в зависимости от дельты
delta(th) = if(th<=0.25) ∞ else 5 + log₃(th/24)
```

