using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace OSDownloader.Models
{
    public class DriveSpaceInfo
    {
        public string DriveLetter { get; set; }
        public double TotalSizeGB { get; set; }
        public double UsedSpaceGB { get; set; }
        public double FreeSpaceGB { get; set; }
        public string DriveFormat { get; set; }
        public string DriveType { get; set; }

        public static DriveSpaceInfo GetDetailedDriveInfo(string directoryPath)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                DriveInfo driveInfo = new DriveInfo(directoryInfo.Root.FullName);

                if (!driveInfo.IsReady)
                {
                    return null;
                }

                return new DriveSpaceInfo
                {
                    DriveLetter = driveInfo.Name,
                    TotalSizeGB = Math.Round(driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0), 2),
                    UsedSpaceGB = Math.Round((driveInfo.TotalSize - driveInfo.TotalFreeSpace) / (1024.0 * 1024.0 * 1024.0), 2),
                    FreeSpaceGB = Math.Round(driveInfo.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0), 2),
                    DriveFormat = driveInfo.DriveFormat,
                    DriveType = driveInfo.DriveType.ToString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting drive info: {ex.Message}");
                return null;
            }
        }

        public static string GetEXEPathIfRunningFromUSBDrive()
        {
            try
            {
                // Get the executable's directory path
                string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
                string rootPath = Path.GetPathRoot(exePath);

                // Get drive information
                DriveInfo driveInfo = new DriveInfo(rootPath);

                // First check - basic drive type (may not detect all USB devices)
                if (driveInfo.DriveType == System.IO.DriveType.Removable || driveInfo.DriveType == System.IO.DriveType.Network)
                {
                    return exePath;
                }

                // Second check - more thorough WMI query to detect USB drives
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'"))
                {
                    foreach (var drive in searcher.Get())
                    {
                        using (var partitionSearcher = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{drive["DeviceID"]}'}} " +
                            "WHERE AssocClass=Win32_DiskDriveToDiskPartition"))
                        {
                            foreach (var partition in partitionSearcher.Get())
                            {
                                using (var logicalSearcher = new ManagementObjectSearcher(
                                    $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} " +
                                    "WHERE AssocClass=Win32_LogicalDiskToPartition"))
                                {
                                    foreach (var logicalDisk in logicalSearcher.Get())
                                    {
                                        string diskRoot = logicalDisk["DeviceID"].ToString();
                                        if (rootPath.StartsWith(diskRoot, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return exePath;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return null; // Not a USB drive
            }
            catch (Exception ex)
            {
                // Log error if needed
                Console.WriteLine($"Error checking drive type: {ex.Message}");
                return null;
            }
        }
    }
}


