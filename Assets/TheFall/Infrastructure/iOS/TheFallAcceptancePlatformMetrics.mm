#import <Foundation/Foundation.h>
#import <mach/mach.h>
#import <mach/mach_time.h>

static const uint64_t TheFallAcceptanceNativeLoadTime = mach_continuous_time();

extern "C"
{
    double TheFallAcceptanceProcessUptimeSeconds()
    {
        mach_timebase_info_data_t timebase = {};
        mach_timebase_info(&timebase);
        const uint64_t elapsed = mach_continuous_time() - TheFallAcceptanceNativeLoadTime;
        const double nanoseconds =
            (double)elapsed
            * (double)timebase.numer
            / (double)timebase.denom;
        return nanoseconds / 1000000000.0;
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
