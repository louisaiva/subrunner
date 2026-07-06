using UnityEngine;

public interface Hacker
{
    // this interface is for capables that can carry a laptop and or interact with computers
    // it then has a device
    public Laptop Laptop { get; set; }
    public Computer Computer { get; set; }
    public Device Device { get; }
    public System.Action<Device> OnDeviceRemoved { get; set; }
    public System.Action<Device> OnDeviceGranted { get; set; }
}