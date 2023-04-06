using FamilyHubs.ServiceDirectory.Shared.Enums;

namespace PluginBase;

public static class StringToEnum
{
    public static T Convert<T>(string str) where T : struct
    {
        Enum.TryParse<T>(str, true, out T result2);
        return result2;
    }

    public static AttendingAccessType ConvertAttendingAccessType(string str)
    {
        if (Enum.TryParse<AttendingAccessType>(str, true, out AttendingAccessType result))
        {
            return result;
        }

        return AttendingAccessType.NotSet;
    }

    public static AttendingType ConvertAttendingType(string str)
    {
        if (Enum.TryParse<AttendingType>(str, true, out AttendingType result))
        {
            return result;
        }

        return AttendingType.NotSet;
    }

    public static DeliverableType ConvertDeliverableType(string str)
    {
        if (Enum.TryParse<DeliverableType>(str, true, out DeliverableType result))
        {
            return result;
        }

        return DeliverableType.NotSet;
    }

    public static ServiceStatusType ConvertServiceStatusType(string str)
    {
        if (Enum.TryParse<ServiceStatusType>(str, true, out ServiceStatusType result))
        {
            return result;
        }

        return ServiceStatusType.NotSet;
    }

    public static EligibilityType ConvertEligibilityType(string str)
    {
        if (Enum.TryParse<EligibilityType>(str, true, out EligibilityType result))
        {
            return result;
        }

        return EligibilityType.NotSet;
    }
}
