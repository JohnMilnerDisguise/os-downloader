
using System;

namespace SGet
{
    public enum OSListRecordStatus
    {
        //JM: OS not in library and not required
        Not_In_Library,

        //JM: OS not in library bug flagged for download
        To_Be_Added,

        //JM: Failed Validation so should be excluded from all download functionality
        FAILED_VALIDATION,

        // Client object is created and set to default values
        Initialized,

        // Client is waiting for the file to begin downloading
        Waiting,

        // Client is downloading data from the server
        Downloading,

        // Client releases used resources, so the download can be paused
        Pausing,

        // Download is paused
        Paused,

        // Download is added to the queue
        Queued,

        // Client releases used resources, so the download can be deleted
        Deleting,

        // Download is deleted
        Deleted,

        // Download is completed
        Completed,

        // There was an error during download
        Error
    }

    public enum OSListRecordAction
    {
        None,

        //JM: When OSListRecordStatus is Not_In_Library but user selects to Download It
        Add_To_Library,

        //JM: When OSListRecordStatus is Not_In_Library but user selects to Download It
        Do_Not_Add
    }

    public static class OSListRecordEnumUtils
    {
        public static OSListRecordAction getDefaultActionFromOSListRecordStatus(OSListRecordStatus status)
        {
            if (status == OSListRecordStatus.Not_In_Library)
            {
                return OSListRecordAction.Do_Not_Add;
            }
            if (status == OSListRecordStatus.To_Be_Added)
            {
                return OSListRecordAction.Add_To_Library;
            }
            return OSListRecordAction.None;
        }

        public static OSListRecordStatus? getNewOSListRecordStatusFromActionOption(OSListRecordAction action, OSListRecordStatus status)
        {
            if (status == OSListRecordStatus.Not_In_Library)
            {
                if (action == OSListRecordAction.Add_To_Library)
                {
                    return OSListRecordStatus.To_Be_Added;
                }
            }

            if (status == OSListRecordStatus.To_Be_Added)
            {
                if (action == OSListRecordAction.Do_Not_Add )
                {
                    return OSListRecordStatus.Not_In_Library;
                }
            }

            return null;
        }

        public static OSListRecordAction[] getActionOptionsFromOSListRecordStatus(OSListRecordStatus status)
        {
            if (status == OSListRecordStatus.Not_In_Library || status == OSListRecordStatus.To_Be_Added)
            {
                return new OSListRecordAction[] { OSListRecordAction.Do_Not_Add, OSListRecordAction.Add_To_Library };
            }
            else
            {
                return new OSListRecordAction[] { OSListRecordAction.None };
            }
        }

        public static string getActionDescriptionFromActionEnum(OSListRecordAction actionEnum)
        {
            if (actionEnum == OSListRecordAction.None)
            {
                return "";
            }
            else
            {
                return actionEnum.ToString().Replace('_', ' ');
            }
        }

        public static OSListRecordAction getActionEnumFromActionDescription(string actionDescription)
        {
            if (actionDescription == null || actionDescription.Equals(""))
            {
                return OSListRecordAction.None;
            }
            else
            {
                return (OSListRecordAction)Enum.Parse(typeof(OSListRecordAction), actionDescription.Replace(' ', '_'), true);
            }
        }

    }
}
