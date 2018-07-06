

namespace YHCJB.Util
{
    public static class DateTimeExtension
    {
        // formate yyyymm
        public static int StepMonth(int month, int step)
        {
            int cy = month / 100;
            int cm = month % 100;

            int months = cy * 12 + cm + step;
            cy = months / 12;
            cm = months % 12;
            if (cm == 0)
            {
                cy -= 1;
                cm = 12;
            }
            
            return cy * 100 + cm;
        }

        // formate yyyymm
        public static int NextMonth(int month)
        {
            return StepMonth(month, 1);
        }

        // formate yyyymm
        public static int PrevMonth(int month)
        {
            return StepMonth(month, -1);
        }
    }
}
