#import <Foundation/Foundation.h>
#import <libproc.h>
#import <mach/mach.h>
#import <sys/time.h>
#import <unistd.h>

extern "C"
{
    double TheFallAcceptanceProcessUptimeSeconds()
    {
        struct proc_bsdinfo processInfo = {};
        const int result = proc_pidinfo(
            getpid(),
            PROC_PIDTBSDINFO,
            0,
            &processInfo,
            sizeof(processInfo));
        if (result != sizeof(processInfo))
        {
            return -1.0;
        }

        struct timeval now = {};
        if (gettimeofday(&now, nullptr) != 0)
        {
            return -1.0;
        }

        const double startedAt =
            (double)processInfo.pbi_start_tvsec
            + ((double)processInfo.pbi_start_tvusec / 1000000.0);
        const double currentTime =
            (double)now.tv_sec
            + ((double)now.tv_usec / 1000000.0);
        return MAX(0.0, currentTime - startedAt);
    }

    uint64_t TheFallAcceptancePhysicalFootprintBytes()
    {
        task_vm_info_data_t taskInfo = {};
        mach_msg_type_number_t count = TASK_VM_INFO_COUNT;
        const kern_return_t result = task_info(
            mach_task_self(),
            TASK_VM_INFO,
            reinterpret_cast<task_info_t>(&taskInfo),
            &count);
        return result == KERN_SUCCESS ? taskInfo.phys_footprint : 0;
    }

    int32_t TheFallAcceptanceThermalState()
    {
        if (@available(iOS 11.0, *))
        {
            return (int32_t)[NSProcessInfo processInfo].thermalState;
        }

        return -1;
    }
}
