## Отчет на 09.2.2023

* Всего вопросов:26364
* Правильных ответов: 90%

* Passed: 23976
* Failed: 2388

Total passed: 90%
|0: 88.4 |1: 87.5 |2: 89.2 |3: 89.1 |4: 90.1 |5: 89.9 |6: 91.5 |7: 92.4 |8: 93.3 |9: 95.2 |10: 95.1 |11: 95.3 |12: 95.3 |13: 96.4 |14: 95.5 |15: 95.8 |16: 96.0 |17: 96.7 |18: 99.1 |19: 100.0

## Разброс по времени:

Посмотрим зависимость вероятности правильного ответа от паузы между вопросами
и очками слова

```
                                        total   0-1     2-3     4-5     6-7     8-9     10-11  12+
00:00:00 - 00:00:10:[2526]              94      089     094     095     096     099     099     099
00:00:10 - 00:01:00:[10427]             93      088     091     095     096     096     098     098
00:01:00 - 00:06:00:[2150]              90      088     088     090     093     097     093     097
00:06:00 - 00:36:00:[384]               94      096     090     097     094     096     ---     ---
00:36:00 - 03:36:00:[880]               89      091     084     091     090     095     096     095
03:36:00 - 1.00:00:00:[1933]            88      088     087     088     090     094     095     094
1.00:00:00 - 5.00:00:00:[4366]          91      089     087     088     093     096     096     099
5.00:00:00 - 30.00:00:00:[1640]         86      085     086     075     082     088     089     095
30.00:00:00 - 180.00:00:00:[993]        82      085     077     064     082     078     088     084
180.00:00:00 - 3000.00:00:00:[611]      69      071     068     067     057     078     073     063
```

# Detailed time metrics
```
[avg sec]           [inteval]              [count]     [P]    
000005)         00:00:00 - 00:00:10:       [2526]      94.7    94.7    89.5    94.1    94.7    96.0    98.5    98.6    99.0
000020)         00:00:10 - 00:00:30:       [5885]      93.9    95.3    88.2    92.3    96.3    95.8    96.6    98.3    97.0
000035)         00:00:10 - 00:01:00:       [10427]     93.3    94.6    87.8    91.5    94.8    95.7    96.5    98.1    98.3
000120)         00:01:00 - 00:03:00:       [1898]      91.0    90.6    88.4    88.1    90.7    93.2    96.7    93.3    98.9
000360)         00:03:00 - 00:09:00:       [357]       89.6    88.8    90.5    87.8    85.7    ---     ---     95.2    ---
001170)         00:09:00 - 00:30:00:       [248]       94.8    96.3    97.0    91.8    97.3    95.7    ---     ---     ---
003600)         00:30:00 - 01:30:00:       [229]       88.6    86.7    92.6    81.3    91.3    ---     90.9    ---     ---
010800)         01:30:00 - 04:30:00:       [843]       89.7    89.8    88.6    86.5    90.1    91.2    96.4    94.6    97.5
029700)         04:30:00 - 12:00:00:       [566]       89.2    87.0    91.6    86.7    87.5    87.5    94.1    ---     ---
086400)         12:00:00 - 1.12:00:00:     [2316]      89.9    88.3    87.9    88.7    87.9    93.3    97.9    94.2    94.8
237600)         1.12:00:00 - 4.00:00:00:   [2729]      91.2    90.3    89.5    86.3    88.5    93.1    95.5    95.1    98.9
691200)         4.00:00:00 - 12.00:00:00:  [1605]      90.0    85.8    84.5    85.1    82.1    86.7    93.1    94.3    96.5
1814400)        12.00:00:00 - 30.00:00:00: [562]       79.7    74.4    85.3    85.0    65.1    77.3    79.7    ---     96.2
5184000)        30.00:00:00 - 90.00:00:00: [382]       81.2    72.3    91.3    75.0    59.2    80.8    81.3    93.1    ---
11664000)       90.00:00:00 - 180.00:00:00:[611]       83.3    75.0    82.7    77.0    70.0    84.2    72.1    86.7    86.5
```
* результирующая формула получается: P(Tsec) = 96.62−0.933⋅ln(Tsec)

# Question metrics without clean questions.
NAME                          passed in count | 0-1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-11 
-----------------------------------------------------------------------------------------
Eng write mising              (084% in 454)  :|  68 |  82 |  80 |  88 |  96 |  79
Eng phrase substitute         (085% in 1273) :|  86 |  79 |  84 |  87 |  90 |  94
Ru Write Single Translation   (085% in 678)  :|  75 |  80 |  89 |  87 |  93 |  94
Ru phrase substitute          (086% in 1251) :|  81 |  83 |  85 |  83 |  82 |  96
Eng Write                     (086% in 2740) :|  75 |  78 |  84 |  88 |  92 |  89
Assemble phrase               (086% in 369)  :|  77 |  84 |  86 |  84 |  92 |  92
Ru write mising               (086% in 407)  :|  69 |  80 |  89 |  83 |  88 |  95
Ru trust single translation   (087% in 1320) :|  74 |  88 |  87 |  93 |  92 |  96
Ru Choose Phrase              (088% in 1175) :|  83 |  89 |  88 |  89 |  92 |  98
Choose Eng By Transcription   (088% in 190)  :|  87 |  85 |  92 |  87 |  94 | 100
Ru trust                      (089% in 1148) :|  71 |  93 |  87 |  88 |  91 | 100
Eng trust                     (090% in 1302) :|  83 |  90 |  92 |  92 |  93 |  97
Choose Ru By Transcription    (091% in 1783) :|  83 |  88 |  89 |  92 |  95 |  95
Eng Choose Phrase             (092% in 1132) :|  90 |  91 |  95 |  93 |  95 |  93
Ru Write                      (092% in 1857) :|  75 |  87 |  91 |  91 |  96 |  96
Eng Choose word in phrase     (094% in 2352) :|  87 |  93 |  95 |  97 |  98 |  96
Eng Choose                    (095% in 2977) :|  92 |  95 |  96 |  99 |  98 |  98
Trans Choose                  (095% in 1658) :|  95 |  95 |  93 |  97 |  95 |  97
RuChoose                      (095% in 892)  :|  94 |  96 |  97 |  98 |  95 | 100
Eng is it right translation   (095% in 1406) :|  91 |  95 |  96 |  96 |  98 |  96


# Weighted Question metrics without clean questions.

RELATIVES-------------
Eng write mising              0454:   849
Ru Write Single Translation   0678:   866
Ru phrase substitute          1251:   866
Eng phrase substitute         1273:   872
Eng Write                     2740:   874
Ru Choose Phrase              1175:   897
Ru trust single translation   1320:   900
Ru trust                      1148:   912
Eng trust                     1302:   916
Choose Ru By Transcription    1783:   930
Eng Choose Phrase             1132:   932
Ru Write                      1857:   934
Eng Choose word in phrase     2352:   946
Eng Choose                    2977:   957
Trans Choose                  1658:   959
RuChoose                      0892:   961
Eng is it right translation   1406:   962

RELATIVES MISSED-------------
Eng write mising              0454:   155
Ru Write Single Translation   0678:   148
Eng phrase substitute         1273:   148
Eng Write                     2740:   144
Ru phrase substitute          1251:   142
Ru trust single translation   1320:   130
Ru Choose Phrase              1175:   113
Ru trust                      1148:   112
Eng trust                     1302:   096
Choose Ru By Transcription    1783:   084
Ru Write                      1857:   076
Eng Choose Phrase             1132:   075
Eng Choose word in phrase     2352:   058
Eng is it right translation   1406:   053
Eng Choose                    2977:   050
RuChoose                      0892:   046
Trans Choose                  1658:   041

SIMPLE RELATIVES-------------
Eng write mising              0454:   727
Eng Write                     2740:   732
Ru Write Single Translation   0678:   734
Eng phrase substitute         1273:   742
Ru trust single translation   1320:   787
Ru phrase substitute          1251:   802
Ru Write                      1857:   817
Ru trust                      1148:   826
Choose Ru By Transcription    1783:   837
Ru Choose Phrase              1175:   838
Eng trust                     1302:   869
Eng Choose word in phrase     2352:   891
Eng Choose Phrase             1132:   900
Eng Choose                    2977:   927
Eng is it right translation   1406:   938
Trans Choose                  1658:   939
RuChoose                      0892:   952

HARD RELATIVES-------------
Ru phrase substitute          1251:   847
Eng write mising              0454:   864
Ru Write Single Translation   0678:   870
Eng Write                     2740:   876
Eng phrase substitute         1273:   890
Eng trust                     1302:   901
Ru Choose Phrase              1175:   916
Ru trust                      1148:   917
Ru trust single translation   1320:   921
Ru Write                      1857:   923
Choose Ru By Transcription    1783:   934
Eng Choose Phrase             1132:   942
Eng Choose word in phrase     2352:   964
Trans Choose                  1658:   964
RuChoose                      0892:   965
Eng is it right translation   1406:   982
Eng Choose                    2977:   988


# Question metrics by time
Это типы вопросов, с учетом сложности врем

Eng write mising              (084% in 454)  :|  68 |  82 |  80 |  88 |  96 |  79
Eng phrase substitute         (085% in 1273) :|  86 |  79 |  84 |  87 |  90 |  94
Ru Write Single Translation   (085% in 678)  :|  75 |  80 |  89 |  87 |  93 |  94
Ru phrase substitute          (086% in 1251) :|  81 |  83 |  85 |  83 |  82 |  96
Eng Write                     (086% in 2740) :|  75 |  78 |  84 |  88 |  92 |  89
Assemble phrase               (086% in 369)  :|  77 |  84 |  86 |  84 |  92 |  92
Ru write mising               (086% in 407)  :|  69 |  80 |  89 |  83 |  88 |  95
Ru trust single translation   (087% in 1320) :|  74 |  88 |  87 |  93 |  92 |  96
Ru Choose Phrase              (088% in 1175) :|  83 |  89 |  88 |  89 |  92 |  98
Choose Eng By Transcription   (088% in 190)  :|  87 |  85 |  92 |  87 |  94 | 100
Ru trust                      (089% in 1148) :|  71 |  93 |  87 |  88 |  91 | 100
Eng trust                     (090% in 1302) :|  83 |  90 |  92 |  92 |  93 |  97
Choose Ru By Transcription    (091% in 1783) :|  83 |  88 |  89 |  92 |  95 |  95
Eng Choose Phrase             (092% in 1132) :|  90 |  91 |  95 |  93 |  95 |  93
Ru Write                      (092% in 1857) :|  75 |  87 |  91 |  91 |  96 |  96
Eng Choose word in phrase     (094% in 2352) :|  87 |  93 |  95 |  97 |  98 |  96
Eng Choose                    (095% in 2977) :|  92 |  95 |  96 |  99 |  98 |  98
Trans Choose                  (095% in 1658) :|  95 |  95 |  93 |  97 |  95 |  97
RuChoose                      (095% in 892)  :|  94 |  96 |  97 |  98 |  95 | 100
Eng is it right translation   (095% in 1406) :|  91 |  95 |  96 |  96 |  98 |  96



подкорректируем ее c целью разумности:

Сложность возьмем из вероятности ответа (не важно как-)). Точность прикинем на глаз - 
из зависимости выученность слова к вероятности ответа. Если оно монотонно растет - то точность 5, в противном случае - 1


если "x" это сложность, а "p" точность то выдмываю формулу штрафа: Sqrt(p/(5*x))
а формула бонуса : p*x/5

                             [сложность]  [точность]  [штраф]           [бонус]
Eng write mising                 2.16       3          0.52              1.296        
Eng phrase substitute            2.004      2          0.44              0.8016
Ru Write Single Translation      2.004      5          0.70              2.004
Ru phrase substitute             1.848      2          0.46              0.7392
Eng Write                        1.848      5          0.73              1.848
Ru Write                         1.848      5          0.73              1.848
Assemble phrase                  1.848      4          0.65              1.4784
Ru write mising                  1.848      3          0.56              1.1088
Ru trust single translation      1.692      4          0.68              1.3536
Ru Choose Phrase                 1.548      3          0.62              0.9288
Choose Eng By Transcription      1.548      3          0.62              0.9288
Choose Ru By Transcription       1.092      5          0.95              1.092
Eng Choose Phrase                0.936      2          0.65              0.3744
Eng Choose word in phrase        0.624      5          1.26              0.624
Eng Choose                       0.48       3          1.11              0.288     
Trans Choose                     0.48       2          0.91              0.192
RuChoose                         0.48       3          1.11              0.288
Ru trust                         0.48       2          0.91              0.192
Eng trust                        0.48       2          0.91              0.192
Eng is it right translation      0.48       4          1.29              0.384

Итоговые коэффициенты в боте выставлены примерно как в этой таблице.