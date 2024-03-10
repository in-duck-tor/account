namespace InDuckTor.Account.Domain;

// todo Подумать как гибко поддерживать все виды счётов
public static class КодыБалансовыхСчётов
{
    public static class ПрочиеСчёта
    {
        public const int КодПервогоПорядка = 408;
        public const int ФизическиеЛица = 40817;
    }

    public static class КредитыФизическимЛицам
    {
        public const int КодПервогоПорядка = 455;
        public const int ДоМесяца = 45502;
        public const int До3Месяцев = 45503;
        public const int ДоПолуГода = 45504;
        public const int ДоГода = 45505;
        public const int До3Лет = 45506;
        public const int Более3Лет = 45507;
        public const int ДоВостребования = 45508;
    }
}