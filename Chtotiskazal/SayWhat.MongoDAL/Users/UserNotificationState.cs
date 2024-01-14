using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SayWhat.MongoDAL.Users;

[BsonIgnoreExtraElements]
public class UserNotificationState
{
    /// <summary>
    /// Время последней нотификации по поводу цели в день
    /// </summary>
    [BsonElement("lgsn")]
    public DateTime? LastGoalStreakMessage { get; set; }
    /// <summary>
    /// Время какой либо последней нотификации
    /// </summary>
    [BsonElement("lsn")]
    public DateTime? LastNotification { get; set; }
    /// <summary>
    /// Включены ли какие либо нотификации для пользователя
    /// </summary>
    [BsonElement("on")]
    public bool NotificationEnabled { get; set; }
    /// <summary>
    /// Ошибка последней нотификации
    /// </summary>
    [BsonElement("err")]
    public string LastNotificationError { get; set; }
    /// <summary>
    /// Будильник "Напомни мне через н минут"
    /// </summary>
    [BsonElement("snz")]
    public DateTime? ScheduledGoalStreakNotification { get; set; }

    public void OnGoalStreakMessage()
    {
        LastGoalStreakMessage = LastNotification = DateTime.Now;
        ScheduledGoalStreakNotification = null;
    }
}