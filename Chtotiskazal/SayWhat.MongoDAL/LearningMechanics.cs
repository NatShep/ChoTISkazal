using System;
using SayWhat.MongoDAL.Words;

namespace SayWhat.MongoDAL;

public static class LearnMechanics {
    public static double CalculateAbsoluteScoreOnSuccessAnswer(double questionPassScore, UserWordModel model) {
        var delta = questionPassScore * GetTimeWeigh(model.LastQuestionAskedTimestamp);
        var absoluteScore = model.AbsoluteScore + delta;
        return absoluteScore;
    }

    public static double CalculateAbsoluteScoreOnFailedAnswer(double questionFailScore, UserWordModel model) {
        var delta = questionFailScore / GetTimeWeigh(model.LastQuestionAskedTimestamp);
        var absoluteScore = model.AbsoluteScore - delta;
        //Если вопрос был хорошо выучен, но при этом пользователь ошибся - накладываем дополнительный штраф
        if (absoluteScore > WordLeaningGlobalSettings.LearnedWordMinScore)
            absoluteScore -= 2;
        if (absoluteScore < 0)
            absoluteScore = 0;
        return absoluteScore;
    }

    /// <summary>
    /// Рассчитывает Вес давности заданного вапроса
    /// </summary>
    /// <param name="lastAskTime">Время последнего заданного вопроса</param>
    /// <returns>вес от 0.5 для только что заданного вопроса до 3 для давно заданного вопроса</returns>
    private static double GetTimeWeigh(DateTime? lastAskTime) {
        // If question was asked long time ago, then we add  more scores than if it was asked just right now

        // If it was asked 1 second ago, then it weight is  0.5
        // if it was asked 1 minute ago then it weight is 1
        // if it was asked 3 month ago - then it weight is 2  

        // Из нашего исследования известно что вероятность правильного ответа в зависимости от времени в секундах
        // апроксимируется по формуле:
        // P(T) = 96.62−0.933⋅ln(T)
        double P(double t) => 96.62 - 0.933 * Math.Log(t);
        // Для минуты это будет
        // P(60) = 92.8 
        // Для 3х месяцев это будет
        // P(7776000) = 77.81

        //Зная это, положим что
        //для одной минуты, мы хотим получить результат K = 1
        //А для 3х месяцев мы хотим получить результат K = 2

        // Аппроксимируем это линейно (так как это самый простой вариант):
        // K(T) = k1 + k2*P(T)

        // Получаем систему
        // k1+ k2*92.8 = 1
        // k1+ k2*81.81 = 2
        //
        // Решая ее получаем: 
        // k1≈-10.286 and k2≈0.1

        // Итоговая формула :
        // K(T) = 10.286-0.1*P(T)

        if (lastAskTime == null)
            return 1;
        var t = (DateTime.Now - lastAskTime.Value).TotalSeconds;
        if (t <= 0)
            t = 1;
        var result = 10.286 - 0.1 * P(t);
        if (result <= 0.5) return 0.5;
        if (result >= 3) return 3;
        return result;
    }
}