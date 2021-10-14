﻿## Отчет на 01.10.2021

* Всего вопросов:4440
* Правильных ответов: 90%


* Общая вероятность правильного ответа в зависимости от степни изученности слова:

|  89 |  89 |  88 |  87 |  90 |  92 |  93 |  95 |  91 |  97 |  94 | 100 |  95 |  95 | 100 | 100 |

* Расброс по скорингу:

```
     allMetrics.GroupBy(a => (int) a.ScoreBefore).Select(b => new {score = b.Key, count = b.Count()}).OrderBy(b=>b.score).ToList()
  {Count = 16}
    [0]: {{ score = 0, count = 543 }}
    [1]: {{ score = 1, count = 405 }}
    [2]: {{ score = 2, count = 1119 }}
    [3]: {{ score = 3, count = 672 }}
    [4]: {{ score = 4, count = 426 }}
    [5]: {{ score = 5, count = 408 }}
    [6]: {{ score = 6, count = 421 }}
    [7]: {{ score = 7, count = 278 }}
    [8]: {{ score = 8, count = 250 }}
    [9]: {{ score = 9, count = 132 }}
    [10]: {{ score = 10, count = 104 }}
    [11]: {{ score = 11, count = 67 }}
    [12]: {{ score = 12, count = 42 }}
    [13]: {{ score = 13, count = 14 }}
```

## Разброс по времени:

Посмотрим зависимость вероятности правильного ответа от паузы между вопросами
и очками слова
```
                    total   0-1   2-3   4-5   6-7   8-9  10-11  12+
00:00:00 - 00:00:10:   93 | 088 | 094 | 092 | 098 | 097 | 100 | 100 
00:00:10 - 00:01:00:   92 | 089 | 089 | 095 | 097 | 093 | 100 | 100 
00:01:00 - 00:06:00:   91 | 091 | 088 | 089 | 094 | 096 | 100 | 100 
00:06:00 - 00:36:00:   93 | 095 | 088 | 094 | 100 | 093 | 100 | --- 
00:36:00 - 03:36:00:   86 | 092 | 084 | 076 | 085 | 088 | --- | --- 
03:36:00 - 1d      :   89 | 089 | 088 | 090 | 090 | 093 | 100 | 100 
1d - 5d            :   89 | 089 | 083 | 087 | 093 | 096 | 100 | --- 
5d - 1m            :   91 | 083 | 087 | 092 | 092 | 091 | 100 | --- 
1m - 6m            :   81 | 090 | 068 | 050 | 083 | 050 | 090 | 100 
6m+                :   72 | 077 | 069 | 083 | 054 | 076 | 081 | 057 
```
 
Выводы: 
1) 10-11 балов действительно значит что слово заучено (в рамках одного месяца)
2) 12+ балов значит что слово заучено навсегда   
3) Корреляция видна, но выборка маловата. Для колонки 2-3 
- видно красивое падение. Именно в 2-3 выборка самая большая (примерно в два раза больше чем в остальных)

## Разборс по вопросам

Для каких вопросов уровень сложности слишком простой, а для каких - слишком сложный?
Некоторые вопросы подразумевают дисперсию. Они могут быть требовательны к опечаткам или не очень понятны пользователю. Изучим детальнее это

Построим ранжировку - процент правильного ответа в зависимости от изученности ответа для каждого типа вопроса:
```
NAME                              passed in count | 0-1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-11 | Оценка
-----------------------------------------------------------------------------------------
Clean Ru trust single translation   (076% in 043) :|  65 |  88 | 100 | --- |  33 | 100     // 3 Вероятно тест сложнее чем предполагалось  
Assemble phrase                     (077% in 061) :|  75 |  76 |  75 |  80 |  80 |  66     // ДИСПЕРСИЯ  
Eng Write                           (078% in 374) :|  70 |  71 |  84 |  86 |  86 |  91     // 10 Тест сложнее чем предполагалось
Eng phrase substitute               (079% in 101) :| 100 |  64 |  94 | 100 | 100 |  83
Clean Ru phrase substitute          (084% in 093) :| --- |  76 |  90 | 100 |  83 |  75
Clean Eng phrase substitute         (085% in 120) :| --- |  83 |  73 |  88 | 100 | 100     // 7
Clean Ru Write                      (086% in 061) :| --- |  75 |  75 |  90 |  87 | 100     // 9
Ru phrase substitute                (087% in 091) :| 100 |  84 |  90 |  91 |  77 | 100
Clean Eng Write                     (088% in 661) :| --- |  81 |  88 |  92 |  94 |  97     // 6 с небольшой дисперсией
Ru Write                            (088% in 042) :| --- |  71 |  85 | 100 | 100 | 100     // 5
Clean Ru Choose Phrase              (089% in 084) :| 100 |  90 |  90 |  78 |  90 | 100     // ДИСПЕРСИЯ
Ru trust                            (090% in 187) :|  84 |  89 |  94 | 100 | 100 | 100     // 3
Eng trust                           (091% in 335) :|  87 |  93 |  92 |  96 |  93 |  85     // ДИСПЕРСИЯ
Clean Eng trust                     (091% in 144) :|  80 |  89 |  94 |  94 | 100 | ---     // 3
Eng is it right transltion          (091% in 108) :|  89 |  93 | 100 |  85 | 100 | ---     // 1
Ru Write Single Translation         (091% in 035) :|  83 |  75 | 100 | 100 | 100 | 100     // 4
Eng Choose                          (092% in 505) :|  89 |  93 | 100 | 100 | 100 | ---     // 2
Ru Choose Phrase                    (092% in 107) :|  88 |  94 | 100 | 100 | 100 | ---     // 2
Eng Choose Phrase                   (092% in 101) :|  87 |  96 | 100 | 100 | --- | ---     // 2
Clean Ru trust                      (092% in 054) :|  80 |  95 | 100 |  80 | --- | ---     // 2
Clean Eng is it right transltion    (092% in 050) :| --- |  89 |  87 |  90 | 100 | 100     // 6
Clean Eng Choose Phrase             (093% in 108) :| --- |  87 |  93 | 100 | 100 | 100     // 3
Clean Ru Choose By Transcription    (093% in 104) :| --- |  87 |  90 |  97 |  93 | 100     // 4
Ru trust single translation         (093% in 029) :| 100 |  85 | 100 | --- |  80 | 100     // ДИСПЕРСИЯ
RuChoose                            (094% in 186) :|  94 |  95 | 100 |  66 | 100 | ---     // 1
Ru Choose By Transcription          (094% in 084) :|  83 |  96 |  90 | 100 | 100 | 100     // 2.5
Eng Choose word in phrase           (095% in 274) :|  91 |  96 |  95 | 100 | 100 | 100     // 1
Clean Eng Choose                    (097% in 199) :|  96 |  96 | 100 | 100 |  92 | ---
Clean Trans Choose                  (097% in 141) :| 100 |  96 | 100 | 100 |  90 | 100
Clean Eng Choose word in phrase     (097% in 194) :| 100 |  98 |  95 |  97 | 100 | 100  
Trans Choose                        (098% in 214) :|  98 |  96 | 100 | 100 | 100 | 100     // 1
```
Средний показатель оказался - 90%. Пока возьмем его за основу.


Выводы:

1) Данных пока мало. Нужно где то в 3 раза больше (12к+ сэмплов)

2) Нужно поднять сложность для:
```
Clean Ru trust single translation    2    ClearScreenQuestionDecorator
Assemble phrase                      2,3  AssemblePhraseQuestion
Eng Write                            2,6  EngWriteQuestion
Eng phrase substitute                2    EngPhraseSubstituteQuestion
```
3) Нужно опустить сложность для:
```
   Trans Choose                         1,6  TranscriptionChooseQuestion
   Clean Eng Choose word in phrase      2,3  ClearScreenQuestionDecorator
   Clean Trans Choose                   2,6  ClearScreenQuestionDecorator
   Clean Eng Choose                     2,3  ClearScreenQuestionDecorator
   Eng Choose word in phrase            2    EngChooseWordInPhraseQuestion
```
3) Ввести понятие дисперсии - понижающее влияние теста на скоринг

Дисперсионные тесты:
```
Ru trust single translation
Eng trust
Clean Ru Choose Phrase
Assemble phrase
```

4) Время последнего вопроса влияет больше чем сложность вопроса (!!!)