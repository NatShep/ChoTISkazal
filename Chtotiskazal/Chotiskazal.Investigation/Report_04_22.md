## Отчет на 09.04.2022

* Всего вопросов:19200
* Правильных ответов: 92%

Passed: 17684
Failed: 1516
92%
|  90 |  88 |  90 |  89 |  91 |  92 |  93 |  93 |  94 |  96 |  96 |  97 |  96 |  97 |  97 |  98 |  96 |  97 |  98 | 100

## Разброс по времени:

Посмотрим зависимость вероятности правильного ответа от паузы между вопросами
и очками слова
```
                    total   0-1   2-3   4-5   6-7   8-9  10-11  12+
00:00:00 - 00:00:10:   95 | 090 | 094 | 094 | 097 | 098 | 100 | 098 
00:00:10 - 00:01:00:   93 | 088 | 091 | 095 | 096 | 096 | 099 | 098 
00:01:00 - 00:06:00:   92 | 091 | 089 | 091 | 095 | 095 | 095 | 100 
00:06:00 - 00:36:00:   94 | 098 | 091 | 095 | 093 | 095 | 100 | 100 
00:36:00 - 03:36:00:   88 | 093 | 084 | 086 | 085 | 092 | 092 | 100 
03:36:00 - 1d      :   89 | 090 | 087 | 090 | 090 | 095 | 090 | 100 
1d - 5d            :   91 | 088 | 087 | 088 | 092 | 095 | 095 | 095 
5d - 1m            :   88 | 082 | 080 | 079 | 087 | 092 | 090 | 098 
1m - 6m            :   85 | 086 | 075 | 061 | 085 | 068 | 093 | 089 
6m+                :   69 | 080 | 064 | 073 | 058 | 065 | 082 | 057 
```

## Разборс по вопросам

Для каких вопросов уровень сложности слишком простой, а для каких - слишком сложный?
Некоторые вопросы подразумевают дисперсию. Они могут быть требовательны к опечаткам или не очень понятны пользователю. Изучим детальнее это

Построим ранжировку - процент правильного ответа в зависимости от изученности ответа для каждого типа вопроса:

```
NAME                              passed in count   | 0-1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-11 | Оценка
-----------------------------------------------------------------------------------------
Eng phrase substitute               (083% in 464)  :| 100 |  75 |  91 |  79 |  86 |  94
Eng Write                           (083% in 896)  :|  64 |  77 |  81 |  91 |  89 |  95
Ru Write Single Translation         (085% in 574)  :|  75 |  80 |  90 |  88 |  93 |  97
Clean Eng phrase substitute         (087% in 543)  :|  64 |  82 |  83 |  92 |  96 |  97
Assemble phrase                     (087% in 306)  :|  85 |  86 |  88 |  85 |  90 |  91
Ru phrase substitute                (087% in 471)  :|  83 |  85 |  82 |  93 |  84 |  96
Clean Eng Write                     (088% in 1569) :| 100 |  80 |  87 |  87 |  94 |  90
Clean Ru phrase substitute          (089% in 524)  :|  83 |  83 |  93 |  93 |  87 |  96
Clean Ru trust single translation   (090% in 397)  :|  79 |  91 |  92 |  96 |  94 |  95
Ru trust single translation         (090% in 414)  :|  73 |  90 |  94 | 100 |  97 | 100
Ru Choose By Transcription          (091% in 593)  :|  80 |  90 |  91 |  92 |  96 |  97
Ru Write                            (091% in 532)  :| 100 |  86 |  93 |  90 |  96 |  97
Clean Eng trust                     (092% in 243)  :|  86 |  88 |  94 |  96 |  95 | 100
Ru trust                            (092% in 546)  :|  78 |  93 |  95 |  98 |  95 | 100
Ru Choose Phrase                    (092% in 436)  :|  89 |  92 | 100 |  93 | 100 |  93
Eng Choose Phrase                   (092% in 382)  :|  91 |  92 |  95 | 100 |  90 | 100
Clean Ru Choose Phrase              (092% in 432)  :| 100 |  91 |  90 |  90 |  91 | 100
Eng trust                           (093% in 721)  :|  86 |  93 |  96 |  96 |  98 |  96
Clean Ru Choose By Transcription    (093% in 786)  :|  80 |  88 |  90 |  94 |  96 |  94
Eng Choose                          (094% in 1457) :|  90 |  95 |  98 | 100 | 100 | 100
Clean Eng Choose Phrase             (094% in 446)  :| 100 |  91 |  96 |  92 |  95 | 100
Eng Choose word in phrase           (094% in 1064) :|  88 |  92 |  95 |  98 | 100 | 100
Clean Ru trust                      (094% in 208)  :|  58 |  97 | 100 |  87 |  94 | 100
Clean Ru Write                      (094% in 1014) :|  40 |  92 |  90 |  93 |  95 | 100
RuChoose                            (095% in 571)  :|  94 |  96 |  96 |  97 | 100 | 100
Eng is it right transltion          (095% in 616)  :|  92 |  95 |  97 |  97 | 100 | 100
Clean Eng Choose word in phrase     (096% in 858)  :|  84 |  95 |  94 |  98 |  97 |  98
Trans Choose                        (097% in 692)  :|  97 |  96 |  93 | 100 | 100 | 100
Clean Eng Choose                    (097% in 546)  :|  96 |  96 | 100 | 100 |  97 | 100
Clean Trans Choose                  (098% in 516)  :| 100 |  98 |  95 |  98 |  97 |  97
Clean Eng is it right transltion    (098% in 383)  :| 100 |  97 |  96 |  97 | 100 | 100
```

# Question metrics without clean questions
```
NAME                              passed in count   | 0-1 | 2-3 | 4-5 | 6-7 | 8-9 | 10-11 | Оценка
-----------------------------------------------------------------------------------------
Eng phrase substitute               (085% in 1007) :|  85 |  78 |  86 |  87 |  92 |  95
Ru Write Single Translation         (085% in 574)  :|  75 |  80 |  90 |  88 |  93 |  97
Eng Write                           (086% in 2465) :|  71 |  78 |  85 |  88 |  92 |  91
Assemble phrase                     (087% in 306)  :|  85 |  86 |  88 |  85 |  90 |  91
Ru phrase substitute                (088% in 995)  :|  83 |  84 |  88 |  93 |  86 |  96
Ru trust single translation         (090% in 811)  :|  76 |  90 |  93 |  98 |  95 |  97
Eng trust                           (092% in 964)  :|  86 |  92 |  95 |  96 |  97 |  97
Ru trust                            (092% in 754)  :|  76 |  94 |  96 |  94 |  95 | 100
Ru Choose Phrase                    (092% in 868)  :|  90 |  92 |  92 |  91 |  93 |  97
Ru Choose By Transcription          (092% in 1379) :|  80 |  89 |  91 |  93 |  96 |  95
Eng Choose Phrase                   (093% in 828)  :|  91 |  91 |  95 |  94 |  94 | 100
Ru Write                            (093% in 1546) :|  75 |  88 |  91 |  92 |  95 |  99
Eng Choose                          (095% in 2003) :|  91 |  95 |  99 | 100 |  98 | 100
Eng Choose word in phrase           (095% in 1922) :|  88 |  94 |  95 |  98 |  98 |  99
RuChoose                            (095% in 571)  :|  94 |  96 |  96 |  97 | 100 | 100
Eng is it right transltion          (096% in 999)  :|  92 |  96 |  96 |  97 | 100 | 100
Trans Choose                        (097% in 1208) :|  97 |  97 |  94 |  99 |  98 |  98
```
# Question metrics by time
```
-----------------------------------------------------------------------------------------
Aggregate                      92% ||  95 |  93 |  92 |  89 |  90 |  85 |  82 | --- | --- | ---
Eng phrase substitute          85% ||  88 |  89 |  91 |  72 |  85 |  56 |  11 | --- | --- | ---
Ru Write Single Translation    85% ||  98 |  87 |  89 |  78 |  82 |  78 | --- | --- | --- | ---
Eng Write                      86% ||  91 |  90 |  86 |  75 |  82 |  74 |  50 | --- | --- | ---
Assemble phrase                87% ||  96 |  90 |  70 |  88 |  84 |  83 | --- | --- | --- | ---
Ru phrase substitute           88% ||  94 |  90 |  89 |  83 |  84 |  87 |  58 | --- | --- | ---
Ru trust single translation    90% ||  92 |  90 |  90 |  83 |  91 |  66 | --- | --- | --- | ---
Ru Choose Phrase               92% ||  97 |  96 |  89 |  91 |  90 |  95 |  74 | --- | --- | ---
Ru trust                       92% ||  94 |  92 |  96 |  91 |  92 | 100 | --- | --- | --- | ---
Ru Choose By Transcription     92% ||  94 |  93 |  93 |  91 |  91 | 100 |  76 | --- | --- | ---
Eng trust                      92% ||  95 |  92 |  95 |  95 |  92 |  83 |  73 | --- | --- | ---
Ru Write                       93% ||  95 |  96 |  93 |  81 |  91 |  79 |  50 | --- | --- | ---
Eng Choose Phrase              93% ||  96 |  95 |  94 |  87 |  92 |  92 |  95 | --- | --- | ---
Eng Choose word in phrase      95% ||  99 |  97 |  97 |  93 |  91 |  89 |  91 | --- | --- | ---
Eng Choose                     95% ||  95 |  96 |  94 |  97 |  95 |  86 |  85 | --- | --- | ---
RuChoose                       95% || 100 |  93 |  98 |  97 |  94 |  60 |  97 | --- | --- | ---
Eng is it right transltion     96% ||  98 |  96 |  92 |  92 |  98 | 100 |  91 | --- | --- | ---
Trans Choose                   97% || 100 |  98 |  97 |  95 |  97 |  92 |  89 | --- | --- | ---
```
Вывод: данных достаточно для ранжировки вопросов. 

# Regression report

Посмотрим зависимость ожидаемого результата в зависимости от очков

Regression report:
*score   *size   batch   count   rsqr    val      slope
[0-5]   11082   100     109     0.37    93.68   -0.59
[0-5]   11082   200     055     0.45    93.33   -0.51
[0-5]   11082   300     036     0.49    93.15   -0.46
[1-5]   9641    300     032     0.55    94.08   -0.63
[1-6]   10918   300     036     0.60    94.30   -0.61
[2-6]   9524    300     031     0.70    94.88   -0.63
[2-7]   11009   300     036     0.64    95.32   -0.63
[2-8]   11920   300     039     0.64    95.49   -0.62
[7-10]  2505    162     015     0.49    97.29   -0.26
[0-10]  16349   300     054     0.49    94.73   -0.52
[0-20]  19107   300     063     0.52    95.08   -0.42

Посмотрим зависимость ожидаемого результата в зависимости от очков и типа вопроса
Цель : Ранжировать сложность вопросов в зависимости от изученности слова

*Name                          *score   *size   batch   count   rsqr    val      slope
Eng phrase substitute           [0-4]   0236    015     014     0.32    89.50   -1.77
Eng phrase substitute           [1-5]   0273    017     015     0.32    88.61   -1.35
Eng phrase substitute           [2-6]   0286    018     015     0.27    87.85   -1.21
Eng phrase substitute           [3-7]   0194    012     014     0.22    92.77   -1.05
Eng phrase substitute           [4-8]   0113    007     014     0.23    98.54   -1.39
Eng phrase substitute           [0-10]  0379    024     015     0.46    90.51   -1.25
Eng phrase substitute           [11-20] 0061    003     015     0.01    88.87   0.33
Eng phrase substitute           [0-20]  0461    029     015     0.17    89.27   -0.65

Eng Choose                      [0-4]   1151    074     015     0.05    95.04   -0.16
Eng Choose                      [1-5]   0817    053     015     0.15    97.76   -0.31
Eng Choose                      [2-6]   0604    039     015     0.11    98.30   -0.22
Eng Choose                      [3-7]   0277    017     015     0.10    99.55   -0.19
Eng Choose                      [4-8]   0184    011     015     0.09    100.42  -0.14
Eng Choose                      [0-10]  1381    089     015     0.01    95.36   -0.04
Eng Choose                      [11-20] 0055    003     013     Infinity        100.00  0.00
Eng Choose                      [0-20]  1454    094     015     0.00    95.51   -0.02
Eng Choose Phrase               [0-4]   0312    020     014     0.03    93.35   -0.25
Eng Choose Phrase               [1-5]   0223    014     014     0.09    96.01   -0.36
Eng Choose Phrase               [2-6]   0152    009     015     0.12    100.29  -0.70
Eng Choose Phrase               [3-7]   0051    003     012     0.00    96.16   -0.04
Eng Choose Phrase               [4-8]   0041    002     013     NaN     100.00  0.00
Eng Choose Phrase               [0-10]  0363    023     015     0.13    95.33   -0.38
Eng Choose Phrase               [11-20] 0015    000     015     0.06    104.36  -1.24
Eng Choose Phrase               [0-20]  0382    024     015     0.12    95.57   -0.37
Eng trust                       [0-4]   0454    029     015     0.13    87.90   0.59
Eng trust                       [1-5]   0426    027     015     0.04    90.23   0.31
Eng trust                       [2-6]   0384    024     015     0.02    95.40   -0.16
Eng trust                       [3-7]   0223    014     014     0.44    102.11  -0.62
Eng trust                       [4-8]   0144    009     014     0.52    102.27  -0.72
Eng trust                       [0-10]  0648    042     015     0.13    91.00   0.39
Eng trust                       [11-20] 0051    003     012     0.12    101.74  -0.52
Eng trust                       [0-20]  0717    046     015     0.00    93.14   0.01
Eng Choose word in phrase       [0-4]   0610    039     015     0.53    100.43  -1.17
Eng Choose word in phrase       [1-5]   0623    040     015     0.49    101.13  -1.23
Eng Choose word in phrase       [2-6]   0588    038     015     0.66    99.88   -0.88
Eng Choose word in phrase       [3-7]   0346    022     015     0.33    101.20  -0.79
Eng Choose word in phrase       [4-8]   0259    016     015     0.27    100.67  -0.55
Eng Choose word in phrase       [0-10]  0942    061     015     0.60    99.78   -0.75
Eng Choose word in phrase       [11-20] 0088    005     014     Infinity        100.00  -0.00
Eng Choose word in phrase       [0-20]  1058    068     015     0.54    99.64   -0.65
Eng Write                       [0-4]   0409    026     015     0.73    92.79   -3.07
Eng Write                       [1-5]   0503    032     015     0.77    92.16   -2.65
Eng Write                       [2-6]   0577    037     015     0.60    92.05   -2.29
Eng Write                       [3-7]   0418    027     014     0.46    96.29   -2.03
Eng Write                       [4-8]   0294    019     014     0.44    96.16   -1.44
Eng Write                       [0-10]  0777    050     015     0.47    91.76   -1.64
Eng Write                       [11-20] 0095    006     013     0.22    101.64  -0.51
Eng Write                       [0-20]  0894    058     015     0.42    91.80   -1.31
RuChoose                        [0-4]   0476    030     015     0.01    94.44   0.10
RuChoose                        [1-5]   0282    018     014     0.18    90.63   0.58
RuChoose                        [2-6]   0195    012     015     0.00    96.87   -0.05
RuChoose                        [3-7]   0076    004     015     0.00    97.11   0.03
RuChoose                        [4-8]   0060    003     015     0.00    96.84   -0.02
RuChoose                        [0-10]  0550    035     015     0.03    94.61   0.14
RuChoose                        [11-20] 0014    000     014     Infinity        100.00  0.00
RuChoose                        [0-20]  0569    036     015     0.02    94.93   0.11
Trans Choose                    [0-4]   0516    033     015     0.21    99.75   -0.32
Trans Choose                    [1-5]   0414    026     015     0.27    100.38  -0.44
Trans Choose                    [2-6]   0309    020     014     0.17    99.38   -0.37
Trans Choose                    [3-7]   0156    010     014     0.35    102.06  -0.75
Trans Choose                    [4-8]   0096    006     013     0.18    101.10  -0.49
Trans Choose                    [0-10]  0632    041     015     0.35    100.52  -0.41
Trans Choose                    [11-20] 0040    002     013     Infinity        100.00  0.00
Trans Choose                    [0-20]  0692    044     015     0.26    99.99   -0.31
Ru trust                        [0-4]   0317    020     015     0.02    89.24   -0.20
Ru trust                        [1-5]   0301    019     015     0.23    96.66   -0.74
Ru trust                        [2-6]   0281    018     014     0.06    96.75   -0.36
Ru trust                        [3-7]   0179    011     014     0.05    99.74   -0.23
Ru trust                        [4-8]   0118    007     014     0.15    100.97  -0.28
Ru trust                        [0-10]  0480    031     015     0.04    90.94   0.19
Ru trust                        [11-20] 0052    003     013     NaN     100.00  0.00
Ru trust                        [0-20]  0544    035     015     0.03    93.06   -0.14
Ru phrase substitute            [0-4]   0243    015     015     0.21    90.85   -0.86
Ru phrase substitute            [1-5]   0283    018     014     0.30    92.23   -1.21
Ru phrase substitute            [2-6]   0292    018     015     0.24    93.04   -1.19
Ru phrase substitute            [3-7]   0185    012     014     0.40    96.28   -1.30
Ru phrase substitute            [4-8]   0124    008     013     0.32    98.70   -1.75
Ru phrase substitute            [0-10]  0400    025     015     0.52    92.77   -1.09
Ru phrase substitute            [11-20] 0050    003     012     0.02    96.50   -0.36
Ru phrase substitute            [0-20]  0468    030     015     0.16    91.98   -0.72
Ru Choose Phrase                [0-4]   0353    022     015     0.06    95.59   -0.34
Ru Choose Phrase                [1-5]   0262    017     014     0.18    99.56   -0.69
Ru Choose Phrase                [2-6]   0177    011     014     0.22    104.33  -1.03
Ru Choose Phrase                [3-7]   0061    003     015     0.22    103.17  -0.86
Ru Choose Phrase                [4-8]   0036    002     012     0.32    104.77  -1.06
Ru Choose Phrase                [0-10]  0405    026     015     0.26    98.82   -0.70
Ru Choose Phrase                [11-20] 0020    001     010     Infinity        100.00  -0.00
Ru Choose Phrase                [0-20]  0435    028     015     0.21    98.19   -0.55
Assemble phrase                 [0-4]   0130    008     014     0.17    95.76   -1.91
Assemble phrase                 [1-5]   0154    010     014     0.19    93.88   -1.27
Assemble phrase                 [2-6]   0166    010     015     0.10    91.78   -0.84
Assemble phrase                 [3-7]   0114    007     014     0.27    94.73   -1.32
Assemble phrase                 [4-8]   0091    005     015     0.11    94.09   -1.02
Assemble phrase                 [0-10]  0254    016     014     0.09    91.83   -0.70
Assemble phrase                 [11-20] 0038    002     012     0.12    101.55  -0.91
Assemble phrase                 [0-20]  0306    019     015     0.24    93.65   -0.85
Ru Choose By Transcription      [0-4]   0232    015     014     0.00    89.08   -0.05
Ru Choose By Transcription      [1-5]   0241    015     015     0.11    93.19   -0.61
Ru Choose By Transcription      [2-6]   0276    017     015     0.21    95.18   -0.77
Ru Choose By Transcription      [3-7]   0278    018     014     0.32    94.86   -0.88
Ru Choose By Transcription      [4-8]   0185    012     014     0.01    93.28   -0.12
Ru Choose By Transcription      [0-10]  0472    030     015     0.07    92.42   -0.27
Ru Choose By Transcription      [11-20] 0095    006     013     0.04    95.96   0.20
Ru Choose By Transcription      [0-20]  0588    038     015     0.00    92.19   -0.01
Eng is it right transltion      [0-4]   0452    029     015     0.09    92.33   0.24
Eng is it right transltion      [1-5]   0350    022     015     0.24    90.99   0.43
Eng is it right transltion      [2-6]   0263    017     014     0.05    93.70   0.32
Eng is it right transltion      [3-7]   0123    007     015     0.08    93.12   0.45
Eng is it right transltion      [4-8]   0083    005     013     0.01    96.58   0.13
Eng is it right transltion      [0-10]  0555    036     015     0.10    93.29   0.22
Eng is it right transltion      [11-20] 0045    002     015     NaN     100.00  0.00
Eng is it right transltion      [0-20]  0616    040     015     0.14    93.59   0.26
Ru Write                        [0-4]   0231    015     014     0.30    93.39   -1.27
Ru Write                        [1-5]   0276    017     015     0.29    95.02   -1.24
Ru Write                        [2-6]   0303    019     015     0.68    95.63   -1.41
Ru Write                        [3-7]   0186    012     014     0.30    98.26   -1.15
Ru Write                        [4-8]   0143    009     014     0.43    102.80  -1.35
Ru Write                        [0-10]  0434    028     014     0.37    96.42   -1.04
Ru Write                        [11-20] 0073    004     014     0.14    101.25  -0.40
Ru Write                        [0-20]  0530    034     015     0.43    96.52   -0.77
Ru Write Single Translation     [0-4]   0282    018     014     0.17    90.52   -1.40
Ru Write Single Translation     [1-5]   0322    020     015     0.25    92.55   -1.57
Ru Write Single Translation     [2-6]   0300    019     015     0.50    97.52   -2.05
Ru Write Single Translation     [3-7]   0223    014     014     0.32    97.84   -1.46
Ru Write Single Translation     [4-8]   0150    009     015     0.15    94.30   -0.69
Ru Write Single Translation     [0-10]  0477    030     015     0.34    91.85   -1.07
Ru Write Single Translation     [11-20] 0077    005     012     0.05    98.21   -0.32
Ru Write Single Translation     [0-20]  0572    037     015     0.47    92.72   -0.94
Ru trust single translation     [0-4]   0219    014     014     0.24    89.64   -1.22
Ru trust single translation     [1-5]   0211    013     015     0.17    90.39   -0.70
Ru trust single translation     [2-6]   0197    012     015     0.54    99.48   -1.28
Ru trust single translation     [3-7]   0135    008     015     0.29    101.26  -0.89
Ru trust single translation     [4-8]   0092    005     015     0.03    99.14   -0.21
Ru trust single translation     [0-10]  0346    022     015     0.06    90.41   -0.24
Ru trust single translation     [11-20] 0055    003     013     Infinity        100.00  0.00
Ru trust single translation     [0-20]  0411    026     015     0.01    91.18   -0.09

Возьмем агрегированное по вопросам (сортировка по значению)
*Name                          *score   *size   batch   count   rsqr    val      slope
Eng phrase substitute           [0-20]  0461    029     015     0.17    89.27   -0.65
Eng Write                       [0-20]  0894    058     015     0.42    91.80   -1.31
Ru trust single translation     [0-20]  0411    026     015     0.01    91.18   -0.09
Ru phrase substitute            [0-20]  0468    030     015     0.16    91.98   -0.72
Ru Choose By Transcription      [0-20]  0588    038     015     0.00    92.19   -0.01
Ru Write Single Translation     [0-20]  0572    037     015     0.47    92.72   -0.94
Ru trust                        [0-20]  0544    035     015     0.03    93.06   -0.14
Eng trust                       [0-20]  0717    046     015     0.00    93.14   0.01
Eng is it right transltion      [0-20]  0616    040     015     0.14    93.59   0.26
Assemble phrase                 [0-20]  0306    019     015     0.24    93.65   -0.85
RuChoose                        [0-20]  0569    036     015     0.02    94.93   0.11
Eng Choose                      [0-20]  1454    094     015     0.00    95.51   -0.02
Eng Choose Phrase               [0-20]  0382    024     015     0.12    95.57   -0.37
Ru Write                        [0-20]  0530    034     015     0.43    96.52   -0.77
Ru Choose Phrase                [0-20]  0435    028     015     0.21    98.19   -0.55
Eng Choose word in phrase       [0-20]  1058    068     015     0.54    99.64   -0.65
Trans Choose                    [0-20]  0692    044     015     0.26    99.99   -0.31

Возьмем агрегированное по вопросам (сортировка по уклону)
*Name                          *score   *size   batch   count   rsqr    val      slope
Eng Write                       [0-20]  0894    058     015     0.42    91.80   -1.31
Ru Write Single Translation     [0-20]  0572    037     015     0.47    92.72   -0.94
Assemble phrase                 [0-20]  0306    019     015     0.24    93.65   -0.85
Ru Write                        [0-20]  0530    034     015     0.43    96.52   -0.77
Ru phrase substitute            [0-20]  0468    030     015     0.16    91.98   -0.72
Eng phrase substitute           [0-20]  0461    029     015     0.17    89.27   -0.65
Eng Choose word in phrase       [0-20]  1058    068     015     0.54    99.64   -0.65
Ru Choose Phrase                [0-20]  0435    028     015     0.21    98.19   -0.55
Eng Choose Phrase               [0-20]  0382    024     015     0.12    95.57   -0.37
Trans Choose                    [0-20]  0692    044     015     0.26    99.99   -0.31
Ru trust                        [0-20]  0544    035     015     0.03    93.06   -0.14
Ru trust single translation     [0-20]  0411    026     015     0.01    91.18   -0.09
Eng Choose                      [0-20]  1454    094     015     0.00    95.51   -0.02
Ru Choose By Transcription      [0-20]  0588    038     015     0.00    92.19   -0.01
Eng trust                       [0-20]  0717    046     015     0.00    93.14   0.01
RuChoose                        [0-20]  0569    036     015     0.02    94.93   0.11
Eng is it right transltion      [0-20]  0616    040     015     0.14    93.59   0.26
